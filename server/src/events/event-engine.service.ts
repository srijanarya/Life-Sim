import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';
import { config } from '@config/index';
import {
  GameState,
  PlayerEvent,
  EventTemplate,
  EventType,
  PlayerProfile,
} from '@types/game';
import { randomInt } from 'crypto';

export class EventEngine {
  private static readonly CACHE_TTL = 1800;
  private readonly EVENT_WEIGHTS: Record<EventRarity, number> = {
    COMMON: 70,
    UNCOMMON: 20,
    RARE: 7,
    EPIC: 2,
    LEGENDARY: 1,
  };

  async generateEvent(
    gameState: GameState,
    playerProfile: PlayerProfile,
    previousEvents: string[] = []
  ): Promise<EventTemplate | null> {
    try {
      const age = gameState.currentAge;
      const careerId = gameState.careerId;

      const events = await prisma.eventTemplate.findMany({
        where: {
          isActive: true,
          minAge: { lte: age },
          maxAge: { gte: age },
          requiredCareer: careerId || undefined,
          requiredRelationship: gameState.isInRelationship ? true : undefined,
          id: { notIn: previousEvents },
        },
        include: { decisions: true },
      });

      const filteredEvents = await this.filterEventsByStatsAndCooldowns(
        events,
        gameState,
        playerProfile
      );

      if (filteredEvents.length === 0) {
        logger.warn(`No events available for age ${age}`);
        return null;
      }

      const selectedEvent = this.selectWeightedEvent(
        filteredEvents,
        playerProfile
      );
      logger.debug(
        `Generated event "${selectedEvent.title}" for player at age ${age}`
      );

      return selectedEvent as unknown as EventTemplate;
    } catch (error) {
      logger.error('Failed to generate event:', error);
      throw error;
    }
  }

  private async filterEventsByStatsAndCooldowns(
    events: any[],
    gameState: GameState,
    playerProfile: PlayerProfile
  ): Promise<any[]> {
    const eligibleEvents: any[] = [];

    for (const event of events) {
      if (!this.meetsStatThresholds(event, playerProfile)) {
        continue;
      }

      if (await this.isOnCooldown(event, gameState)) {
        continue;
      }

      eligibleEvents.push(event);
    }

    return filteredEvents;
  }

  private selectWeightedEvent(
    events: any[],
    playerProfile: PlayerProfile
  ): any {
    const weightedEvents: Array<{ event: any; weight: number }> = [];

    for (const event of events) {
      let weight = this.EVENT_WEIGHTS[event.rarity];

      weight = this.applyStatBasedAdjustments(weight, event, playerProfile);

      if (event.weightMultiplier) {
        weight *= event.weightMultiplier;
      }

      weightedEvents.push({ event, weight });
    }

    const totalWeight = weightedEvents.reduce((sum, { weight }) => sum + weight, 0);
    let randomWeight = randomInt(1, totalWeight + 1);

    for (const { event, weight } of weightedEvents) {
      randomWeight -= weight;
      if (randomWeight <= 0) {
        return event;
      }
    }

    return weightedEvents[0].event;
  }

  private meetsStatThresholds(event: any, profile: PlayerProfile): boolean {
    if (!event.minStats) {
      return true;
    }

    const minStats = event.minStats as any;
    const stats = {
      health: profile.health,
      happiness: profile.happiness,
      wealth: profile.wealth,
      intelligence: profile.intelligence,
      charisma: profile.charisma,
      physical: profile.physical,
      creativity: profile.creativity,
    };

    for (const [stat, min] of Object.entries(minStats)) {
      if (stats[stat as keyof typeof stats] < (min as number)) {
        return false;
      }
    }

    return true;
  }

  private async isOnCooldown(event: any, gameState: GameState): Promise<boolean> {
    if (!event.cooldownYears) {
      return false;
    }

    const cooldownKey = `game:${gameState.id}:event-cooldown:${event.id}`;
    return await redis.get(cooldownKey) !== null;
  }

  private applyStatBasedAdjustments(
    baseWeight: number,
    event: any,
    profile: PlayerProfile
  ): number {
    let adjustedWeight = baseWeight;
    const stats = {
      health: profile.health,
      happiness: profile.happiness,
      wealth: profile.wealth,
      intelligence: profile.intelligence,
      charisma: profile.charisma,
      physical: profile.physical,
      creativity: profile.creativity,
    };

    if (event.minStats) {
      const minStats = event.minStats as any;
      const matchingStats = Object.entries(minStats).filter(
        ([stat, min]) => stats[stat as keyof typeof stats] >= (min as number)
      ).length;

      const missingStats = Object.entries(minStats).filter(
        ([stat, min]) => stats[stat as keyof typeof stats] < (min as number)
      ).length;

      if (matchingStats > 0) {
        adjustedWeight *= 1 + matchingStats * 0.1;
      }

      if (missingStats > 0) {
        adjustedWeight *= 0.5 * missingStats;
      }
    }

    if (event.eventType === 'CAREER_EVENT' && event.requiredCareer) {
      adjustedWeight *= 1.3;
    }

    if (profile.happiness < 30 && event.eventType === 'LIFE_EVENT') {
      adjustedWeight *= 1.2;
    } else if (profile.happiness > 80 && event.eventType === 'RANDOM_EVENT') {
      adjustedWeight *= 1.15;
    }

    if (profile.wealth > 1000000) {
      const eventTitle = event.title?.toLowerCase() || '';
      if (eventTitle.includes('invest') || eventTitle.includes('business')) {
        adjustedWeight *= 1.25;
      }
    }

    return Math.round(adjustedWeight);
  }

  async recordEvent(
    gameId: string,
    templateId: string,
    gameState: GameState
  ): Promise<PlayerEvent> {
    try {
      const event = await prisma.playerEvent.create({
        data: {
          gameId,
          templateId,
          yearOccurred: gameState.currentYear,
          monthOccurred: gameState.currentMonth,
          ageOccurred: gameState.currentAge,
        },
      });

      logger.info(`Recorded event ${event.id} for game ${gameId}`);
      return event;
    } catch (error) {
      logger.error(`Failed to record event for game ${gameId}:`, error);
      throw error;
    }
  }

  async getEventsForGame(gameId: string): Promise<PlayerEvent[]> {
    const cacheKey = `game:${gameId}:events`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }

      const events = await prisma.playerEvent.findMany({
        where: { gameId },
        include: { template: true },
        orderBy: { createdAt: 'desc' },
      });

      await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(events));
      return events as unknown as PlayerEvent[];
    } catch (error) {
      logger.error(`Failed to get events for game ${gameId}:`, error);
      throw error;
    }
  }

  async getEventTemplate(id: string): Promise<EventTemplate | null> {
    const cacheKey = `event-template:${id}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }

      const template = await prisma.eventTemplate.findUnique({
        where: { id },
        include: { decisions: true },
      });

      if (template) {
        await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(template));
      }

      return template as unknown as EventTemplate;
    } catch (error) {
      logger.error(`Failed to get event template ${id}:`, error);
      throw error;
    }
  }

  async checkEventCooldown(gameId: string): Promise<boolean> {
    const cacheKey = `game:${gameId}:event-cooldown`;

    try {
      const cooldown = await redis.get(cacheKey);
      return cooldown !== null;
    } catch (error) {
      logger.error('Failed to check event cooldown:', error);
      return true;
    }
  }

  async setEventCooldown(gameId: string): Promise<void> {
    const cacheKey = `game:${gameId}:event-cooldown`;

    try {
      await redis.setex(
        cacheKey,
        Math.floor(config.game.eventCooldownMs / 1000),
        '1'
      );
    } catch (error) {
      logger.error('Failed to set event cooldown:', error);
    }
  }

  async getDailyChallenge(gameId: string): Promise<EventTemplate | null> {
    const today = new Date().toISOString().split('T')[0];
    const cacheKey = `daily-challenge:${today}:${gameId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }

      const challenge = await prisma.eventTemplate.findFirst({
        where: {
          eventType: 'DAILY_CHALLENGE',
          isActive: true,
        },
        include: { decisions: true },
      });

      if (challenge) {
        await redis.setex(cacheKey, 86400, JSON.stringify(challenge));
      }

      return challenge as unknown as EventTemplate;
    } catch (error) {
      logger.error('Failed to get daily challenge:', error);
      throw error;
    }
  }
}

type EventRarity = 'COMMON' | 'UNCOMMON' | 'RARE' | 'EPIC' | 'LEGENDARY';

export const eventEngine = new EventEngine();
