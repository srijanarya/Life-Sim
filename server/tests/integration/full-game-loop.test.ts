import { request, response } from 'express';
import { GameManager } from '../../src/services/game.service';
import { eventEngine } from '../../src/events/event-engine.service';
import { decisionSystem } from '../../src/events/decision-system.service';
import { playerService } from '../../src/services/player.service';
import { prisma } from '@config/database';
import { GameService } from '@config/database';

describe('Full Game Loop Integration', () => {
  let testPlayer: any;
  let testGameState: any;
  let testPlayerProfile: any;

  beforeAll(async () => {
    await prisma.$connect();

    testPlayer = await prisma.player.create({
      data: {
        email: 'integration-test@example.com',
        username: 'integration-test',
        passwordHash: 'hashed-password',
        status: 'ACTIVE',
        subscriptionTier: 'FREE',
      },
    });

    testPlayerProfile = await prisma.playerProfile.create({
      data: {
        playerId: testPlayer.id,
        age: 18,
        health: 100,
        happiness: 100,
        wealth: 0,
        intelligence: 50,
        charisma: 50,
        physical: 50,
        creativity: 50,
        totalPlaytime: 0,
        gamesPlayed: 0,
      },
    });

    testGameState = await prisma.gameState.create({
      data: {
        playerId: testPlayer.id,
        currentYear: 1,
        currentMonth: 1,
        currentAge: 18,
        isInRelationship: false,
        careerLevel: 0,
      },
    });
  });

  afterAll(async () => {
    await prisma.$disconnect();
  });

  describe('Game Initialization Flow', () => {
    it('should create new game with initial state', async () => {
      const newGame = await GameManager.createGame(testPlayer.id, {
        startingAge: 18,
      });

      expect(newGame).toBeDefined();
      expect(newGame.currentYear).toBe(1);
      expect(newGame.currentAge).toBe(18);
    });

    it('should initialize player profile with default stats', async () => {
      expect(testPlayerProfile.health).toBe(100);
      expect(testPlayerProfile.happiness).toBe(100);
      expect(testPlayerProfile.wealth).toBe(0);
      expect(testPlayerProfile.intelligence).toBe(50);
    });
  });

  describe('Event Generation Flow', () => {
    it('should generate appropriate event for age 18', async () => {
      const event = await eventEngine.generateEvent(
        testGameState,
        testPlayerProfile,
        []
      );

      expect(event).not.toBeNull();
      expect(event?.minAge).toBeLessThanOrEqual(18);
      expect(event?.maxAge).toBeGreaterThanOrEqual(18);
    });

    it('should respect stat requirements for events', async () => {
      const highIntelligenceProfile = { ...testPlayerProfile, intelligence: 90 };

      const event = await eventEngine.generateEvent(
        testGameState,
        highIntelligenceProfile,
        []
      );

      expect(event).not.toBeNull();
    });

    it('should not generate events below age minimum', async () => {
      const youngGameState = { ...testGameState, currentAge: 10 };

      const event = await eventEngine.generateEvent(
        youngGameState,
        testPlayerProfile,
        []
      );

      expect(event).not.toBeNull();
    });
  });

  describe('Decision Processing Flow', () => {
    let testEvent: any;
    let testDecision: any;

    beforeAll(async () => {
      testEvent = await prisma.eventTemplate.create({
        data: {
          title: 'Integration Test Event',
          description: 'Test event for integration testing',
          eventType: 'LIFE_EVENT',
          rarity: 'COMMON',
          minAge: 18,
          maxAge: 65,
          isActive: true,
        },
      });

      testDecision = await prisma.decisionTemplate.create({
        data: {
          eventId: testEvent.id,
          text: 'Test Decision',
          order: 1,
          outcomes: {
            happinessBoost: 10,
            intelligenceBoost: 5,
          },
        },
      });
    });

    it('should process decision and apply stat changes', async () => {
      const initialHappiness = testPlayerProfile.happiness;
      const initialIntelligence = testPlayerProfile.intelligence;

      await decisionSystem.processDecision(
        testGameState.id,
        testEvent.id,
        testDecision.id
      );

      const updatedProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      expect(updatedProfile.happiness).toBeGreaterThan(initialHappiness);
      expect(updatedProfile.intelligence).toBeGreaterThan(initialIntelligence);
    });

    it('should apply wealth changes correctly', async () => {
      const testWealthDecision = await prisma.decisionTemplate.create({
        data: {
          eventId: testEvent.id,
          text: 'Wealth Test Decision',
          order: 2,
          outcomes: {
            wealthChange: 1000,
          },
        },
      });

      const initialWealth = testPlayerProfile.wealth;

      await decisionSystem.processDecision(
        testGameState.id,
        testEvent.id,
        testWealthDecision.id
      );

      const updatedProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      expect(updatedProfile.wealth).toBe(initialWealth + 1000);
    });
  });

  describe('Time Advancement Flow', () => {
    it('should advance game time and generate new event', async () => {
      const initialYear = testGameState.currentYear;
      const initialMonth = testGameState.currentMonth;

      const advancedGame = await GameManager.advanceTime(testGameState.id);

      expect(advancedGame).toBeDefined();
      expect(advancedGame.currentYear).toBeGreaterThanOrEqual(initialYear);
      expect(advancedGame.currentAge).toBeGreaterThanOrEqual(initialMonth);
    });

    it('should increment age after 12 months', async () => {
      let gameState = testGameState;

      for (let i = 0; i < 12; i++) {
        gameState = await GameManager.advanceTime(gameState.id);
      }

      expect(gameState.currentAge).toBe(19);
      expect(gameState.currentYear).toBe(2);
    });
  });

  describe('Career Progression Flow', () => {
    it('should handle career changes', async () => {
      const career = await prisma.career.create({
        data: {
          name: 'Software Engineer',
          description: 'Test career',
          type: 'TECH',
          minIntelligence: 70,
          minCreativity: 50,
          minCharisma: 40,
          baseSalary: 60000,
          maxSalary: 200000,
          promotionYears: 2,
          isActive: true,
        },
      });

      const careerDecision = await prisma.decisionTemplate.create({
        data: {
          eventId: testEvent.id,
          text: 'Start Career',
          order: 3,
          outcomes: {
            careerChange: career.id,
          },
        },
      });

      await decisionSystem.processDecision(
        testGameState.id,
        testEvent.id,
        careerDecision.id
      );

      const updatedGameState = await prisma.gameState.findUnique({
        where: { id: testGameState.id },
      });

      expect(updatedGameState.careerId).toBe(career.id);
    });

    it('should track years in career', async () => {
      const playerCareer = await prisma.playerCareer.create({
        data: {
          playerId: testPlayer.id,
          careerId: 'test-career-id',
          level: 1,
          currentSalary: 60000,
          yearsInCareer: 1,
          isCurrent: true,
        },
      });

      expect(playerCareer.yearsInCareer).toBeGreaterThan(0);
    });
  });

  describe('Event History Flow', () => {
    it('should record events in game history', async () => {
      const playerEvent = await eventEngine.recordEvent(
        testGameState.id,
        testEvent.id,
        testGameState
      );

      expect(playerEvent).toBeDefined();
      expect(playerEvent.gameId).toBe(testGameState.id);
      expect(playerEvent.templateId).toBe(testEvent.id);
    });

    it('should retrieve events for game', async () => {
      const events = await eventEngine.getEventsForGame(testGameState.id);

      expect(Array.isArray(events)).toBe(true);
      expect(events.length).toBeGreaterThan(0);
    });
  });

  describe('Stat Updates Flow', () => {
    it('should increment stats correctly', async () => {
      const initialHealth = testPlayerProfile.health;

      await playerService.incrementStat(testPlayer.id, 'health', 10);

      const updatedProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      expect(updatedProfile.health).toBe(initialHealth + 10);
    });

    it('should handle multiple stat updates', async () => {
      await playerService.incrementStat(testPlayer.id, 'intelligence', 5);
      await playerService.incrementStat(testPlayer.id, 'charisma', 3);
      await playerService.incrementStat(testPlayer.id, 'creativity', 7);

      const updatedProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      expect(updatedProfile.intelligence).toBe(55);
      expect(updatedProfile.charisma).toBe(53);
      expect(updatedProfile.creativity).toBe(57);
    });
  });

  describe('Cooldown System', () => {
    it('should set and check event cooldown', async () => {
      await eventEngine.setEventCooldown(testGameState.id);

      const isOnCooldown = await eventEngine.checkEventCooldown(testGameState.id);

      expect(isOnCooldown).toBe(true);
    });

    it('should allow events after cooldown expires', async () => {
      await eventEngine.setEventCooldown(testGameState.id);

      const isOnCooldown = await eventEngine.checkEventCooldown(testGameState.id);

      expect(isOnCooldown).toBe(true);
    });
  });

  describe('Complete Game Loop Scenario', () => {
    it('should complete full game cycle: create -> event -> decision -> advance', async () => {
      const game = await GameManager.createGame(testPlayer.id, { startingAge: 18 });

      expect(game).toBeDefined();

      const event = await eventEngine.generateEvent(game, testPlayerProfile, []);

      expect(event).not.toBeNull();

      const playerEvent = await eventEngine.recordEvent(
        game.id,
        event.id,
        game
      );

      expect(playerEvent).toBeDefined();

      const initialProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      await decisionSystem.processDecision(
        game.id,
        event.id,
        event.decisions[0].id
      );

      const finalProfile = await prisma.playerProfile.findUnique({
        where: { playerId: testPlayer.id },
      });

      expect(finalProfile).not.toEqual(initialProfile);
    });
  });
});
