import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';
import { config } from '@config/index';
import { GameState, PlayerEvent, EventTemplate, EventType } from '@types/game';
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
          requiredCareer: careerId || null,
          id: { notIn: previousEvents },
        },
        orderBy: { rarity: 'asc' },
      });

      if (events.length === 0) {
        logger.warn(`No events available for age ${age}`);
        return null;
      }

      const selectedEvent = this.selectWeightedEvent(events);
      logger.debug(
        `Generated event "${selectedEvent.title}" for player at age ${age}`
      );

      return selectedEvent;
    } catch (error) {
      logger.error('Failed to generate event:', error);
      throw error;
    }
  }

  private selectWeightedEvent(events: EventTemplate[]): EventTemplate {
    const weightedEvents: Array<{ event: EventTemplate; weight: number }> = [];

    for (const event of events) {
      const weight = this.EVENT_WEIGHTS[event.rarity];
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
