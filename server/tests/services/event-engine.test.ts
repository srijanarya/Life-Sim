import { eventEngine } from '../../src/events/event-engine.service';
import { decisionSystem } from '../../src/events/decision-system.service';
import { playerService } from '../../src/services/player.service';
import { prisma } from '../../src/config/database';
import { EventTemplate, GameState, PlayerProfile } from '../../src/types/game';

describe('Event Engine', () => {
  let gameState: GameState;
  let playerProfile: PlayerProfile;

  beforeAll(async () => {
    await prisma.$connect();
  });

  afterAll(async () => {
    await prisma.$disconnect();
  });

  beforeEach(() => {
    gameState = {
      id: 'test-game-id',
      playerId: 'test-player-id',
      currentYear: 1,
      currentMonth: 1,
      currentAge: 25,
      isInRelationship: false,
      careerId: 'test-career-id',
      careerLevel: 1,
      lastEventTime: new Date(),
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    playerProfile = {
      id: 'test-profile-id',
      playerId: 'test-player-id',
      age: 25,
      health: 80,
      happiness: 70,
      wealth: 50000,
      intelligence: 70,
      charisma: 60,
      physical: 65,
      creativity: 55,
      totalPlaytime: 1000,
      gamesPlayed: 5,
      createdAt: new Date(),
      updatedAt: new Date(),
    };
  });

  describe('generateEvent', () => {
    it('should generate event matching age range', async () => {
      gameState.currentAge = 30;
      const event = await eventEngine.generateEvent(
        gameState,
        playerProfile,
        []
      );

      expect(event).not.toBeNull();
      expect(event?.minAge).toBeLessThanOrEqual(30);
      expect(event?.maxAge).toBeGreaterThanOrEqual(30);
    });

    it('should filter events by required career', async () => {
      gameState.careerId = 'specific-career-id';
      const event = await eventEngine.generateEvent(
        gameState,
        playerProfile,
        []
      );

      expect(event).not.toBeNull();
    });

    it('should filter events by relationship status', async () => {
      gameState.isInRelationship = true;
      const event = await eventEngine.generateEvent(
        gameState,
        playerProfile,
        []
      );

      expect(event).not.toBeNull();
    });

    it('should apply stat-based adjustments to event weights', async () => {
      playerProfile.intelligence = 90;
      playerProfile.happiness = 30;

      const event = await eventEngine.generateEvent(
        gameState,
        playerProfile,
        []
      );

      expect(event).not.toBeNull();
    });

    it('should return null when no events available', async () => {
      gameState.currentAge = 120;
      const event = await eventEngine.generateEvent(
        gameState,
        playerProfile,
        []
      );

      expect(event).toBeNull();
    });

    it('should respect event cooldowns', async () => {
      await eventEngine.setEventCooldown(gameState.id);
      const isOnCooldown = await eventEngine.checkEventCooldown(gameState.id);

      expect(isOnCooldown).toBe(true);
    });
  });

  describe('statBasedAdjustments', () => {
    it('should boost events matching player stats', () => {
      playerProfile.intelligence = 80;

      const highStatEvent: any = {
        rarity: 'COMMON',
        minStats: { intelligence: 70 },
      };

      const lowStatEvent: any = {
        rarity: 'COMMON',
        minStats: { charisma: 70 },
      };

      const highWeight = eventEngine['applyStatBasedAdjustments'](
        70,
        highStatEvent,
        playerProfile
      );
      const lowWeight = eventEngine['applyStatBasedAdjustments'](
        70,
        lowStatEvent,
        playerProfile
      );

      expect(highWeight).toBeGreaterThan(lowWeight);
    });

    it('should prioritize career events for employed players', () => {
      const careerEvent: any = {
        eventType: 'CAREER_EVENT',
        requiredCareer: 'test-career-id',
        rarity: 'COMMON',
      };

      const weight = eventEngine['applyStatBasedAdjustments'](
        70,
        careerEvent,
        playerProfile
      );

      expect(weight).toBeGreaterThan(70);
    });

    it('should adjust happiness-based events', () => {
      playerProfile.happiness = 20;

      const lifeEvent: any = {
        eventType: 'LIFE_EVENT',
        rarity: 'COMMON',
      };

      const weight = eventEngine['applyStatBasedAdjustments'](
        70,
        lifeEvent,
        playerProfile
      );

      expect(weight).toBeGreaterThan(70);
    });
  });

  describe('recordEvent', () => {
    it('should record event in database', async () => {
      const mockEvent: EventTemplate = {
        id: 'test-event-id',
        title: 'Test Event',
        description: 'Test description',
        eventType: 'LIFE_EVENT' as any,
        rarity: 'COMMON' as any,
        minAge: 0,
        maxAge: 100,
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date(),
      };

      const playerEvent = await eventEngine.recordEvent(
        gameState.id,
        mockEvent.id,
        gameState
      );

      expect(playerEvent).toBeDefined();
      expect(playerEvent.gameId).toBe(gameState.id);
      expect(playerEvent.templateId).toBe(mockEvent.id);
    });
  });

  describe('getEventsForGame', () => {
    it('should retrieve events for game', async () => {
      const events = await eventEngine.getEventsForGame('test-game-id');

      expect(Array.isArray(events)).toBe(true);
    });

    it('should cache events in Redis', async () => {
      const firstCall = await eventEngine.getEventsForGame('test-cache-game-id');
      const secondCall = await eventEngine.getEventsForGame('test-cache-game-id');

      expect(secondCall).toEqual(firstCall);
    });
  });
});

describe('Decision System', () => {
  let gameState: GameState;

  beforeAll(async () => {
    await prisma.$connect();
  });

  afterAll(async () => {
    await prisma.$disconnect();
  });

  beforeEach(() => {
    gameState = {
      id: 'test-game-id',
      playerId: 'test-player-id',
      currentYear: 1,
      currentMonth: 1,
      currentAge: 25,
      isInRelationship: false,
      careerId: 'test-career-id',
      careerLevel: 1,
      lastEventTime: new Date(),
      createdAt: new Date(),
      updatedAt: new Date(),
    };
  });

  describe('processDecision', () => {
    it('should apply stat boosts from decision outcomes', async () => {
      const mockEventId = 'test-event-id';
      const mockDecisionId = 'test-decision-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: mockDecisionId,
        outcomes: {
          healthBoost: 10,
          happinessBoost: 5,
          intelligenceBoost: 8,
        },
      } as any);

      jest.spyOn(prisma.playerDecision, 'create').mockResolvedValueOnce({} as any);

      const decision = await decisionSystem.processDecision(
        gameState.id,
        mockEventId,
        mockDecisionId
      );

      expect(decision).toBeDefined();
    });

    it('should apply wealth changes', async () => {
      const mockEventId = 'test-event-id';
      const mockDecisionId = 'test-decision-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: mockDecisionId,
        outcomes: {
          wealthChange: 1000,
        },
      } as any);

      jest.spyOn(prisma.playerDecision, 'create').mockResolvedValueOnce({} as any);

      const decision = await decisionSystem.processDecision(
        gameState.id,
        mockEventId,
        mockDecisionId
      );

      expect(decision).toBeDefined();
    });

    it('should handle relationship changes', async () => {
      const mockEventId = 'test-event-id';
      const mockDecisionId = 'test-decision-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: mockDecisionId,
        outcomes: {
          relationshipChange: true,
        },
      } as any);

      jest.spyOn(prisma.gameState, 'update').mockResolvedValueOnce({} as any);
      jest.spyOn(prisma.playerDecision, 'create').mockResolvedValueOnce({} as any);

      const decision = await decisionSystem.processDecision(
        gameState.id,
        mockEventId,
        mockDecisionId
      );

      expect(decision).toBeDefined();
    });

    it('should handle career changes', async () => {
      const mockEventId = 'test-event-id';
      const mockDecisionId = 'test-decision-id';
      const newCareerId = 'new-career-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: mockDecisionId,
        outcomes: {
          careerChange: newCareerId,
        },
      } as any);

      jest.spyOn(prisma.gameState, 'update').mockResolvedValueOnce({} as any);
      jest.spyOn(prisma.playerDecision, 'create').mockResolvedValueOnce({} as any);

      const decision = await decisionSystem.processDecision(
        gameState.id,
        mockEventId,
        mockDecisionId
      );

      expect(decision).toBeDefined();
    });
  });

  describe('validateDecision', () => {
    it('should return true for valid decision', async () => {
      const eventId = 'test-event-id';
      const decisionId = 'test-decision-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: decisionId,
        eventId: eventId,
      } as any);

      const isValid = await decisionSystem.validateDecision(
        eventId,
        decisionId
      );

      expect(isValid).toBe(true);
    });

    it('should return false for invalid decision', async () => {
      const eventId = 'test-event-id';
      const decisionId = 'test-decision-id';

      jest.spyOn(prisma.decisionTemplate, 'findUnique').mockResolvedValueOnce({
        id: decisionId,
        eventId: 'different-event-id',
      } as any);

      const isValid = await decisionSystem.validateDecision(
        eventId,
        decisionId
      );

      expect(isValid).toBe(false);
    });
  });

  describe('getDecisionsForEvent', () => {
    it('should return decisions sorted by order', async () => {
      const eventId = 'test-event-id';

      jest.spyOn(prisma.decisionTemplate, 'findMany').mockResolvedValueOnce([
        { id: 'dec-1', order: 2, text: 'Option 2' },
        { id: 'dec-2', order: 1, text: 'Option 1' },
        { id: 'dec-3', order: 3, text: 'Option 3' },
      ] as any);

      const decisions = await decisionSystem.getDecisionsForEvent(eventId);

      expect(decisions).toHaveLength(3);
      expect(decisions[0].order).toBe(1);
      expect(decisions[1].order).toBe(2);
      expect(decisions[2].order).toBe(3);
    });
  });
});
