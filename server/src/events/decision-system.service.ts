import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';
import { playerService } from '@services/player.service';
import { PlayerDecision, DecisionTemplate } from '@types/game';

export class DecisionSystem {
  private static readonly CACHE_TTL = 1800;

  async processDecision(
    gameId: string,
    eventId: string,
    decisionId: string
  ): Promise<PlayerDecision> {
    try {
      const decisionTemplate = await prisma.decisionTemplate.findUnique({
        where: { id: decisionId },
      });

      if (!decisionTemplate) {
        throw new Error(`Decision template not found: ${decisionId}`);
      }

      const outcomes = decisionTemplate.outcomes as Record<string, unknown>;

      await this.applyOutcomes(gameId, outcomes);

      const playerDecision = await prisma.playerDecision.create({
        data: {
          gameId,
          eventId,
          templateId: decisionId,
          outcomes: outcomes as unknown as Prisma.InputJsonValue,
        },
      });

      logger.info(
        `Processed decision ${decisionId} for event ${eventId} in game ${gameId}`
      );

      return playerDecision as unknown as PlayerDecision;
    } catch (error) {
      logger.error('Failed to process decision:', error);
      throw error;
    }
  }

  private async applyOutcomes(
    gameId: string,
    outcomes: Record<string, unknown>
  ): Promise<void> {
    const gameState = await prisma.gameState.findUnique({
      where: { id: gameId },
    });

    if (!gameState) {
      throw new Error(`Game state not found: ${gameId}`);
    }

    const statUpdates: Record<string, number> = {};

    if (outcomes.healthBoost) {
      statUpdates.health = Number(outcomes.healthBoost);
    }
    if (outcomes.healthPenalty) {
      statUpdates.health = -Number(outcomes.healthPenalty);
    }
    if (outcomes.happinessBoost) {
      statUpdates.happiness = Number(outcomes.happinessBoost);
    }
    if (outcomes.happinessPenalty) {
      statUpdates.happiness = -Number(outcomes.happinessPenalty);
    }
    if (outcomes.wealthChange) {
      statUpdates.wealth = Number(outcomes.wealthChange);
    }
    if (outcomes.intelligenceBoost) {
      statUpdates.intelligence = Number(outcomes.intelligenceBoost);
    }
    if (outcomes.charismaBoost) {
      statUpdates.charisma = Number(outcomes.charismaBoost);
    }
    if (outcomes.physicalBoost) {
      statUpdates.physical = Number(outcomes.physicalBoost);
    }
    if (outcomes.creativityBoost) {
      statUpdates.creativity = Number(outcomes.creativityBoost);
    }

    for (const [stat, amount] of Object.entries(statUpdates)) {
      await playerService.incrementStat(
        gameState.playerId,
        stat as keyof typeof statUpdates,
        amount
      );
    }

    if (outcomes.careerChange) {
      await prisma.gameState.update({
        where: { id: gameId },
        data: { careerId: outcomes.careerChange as string },
      });
    }

    if (outcomes.relationshipChange !== undefined) {
      const isInRelationship = Boolean(outcomes.relationshipChange);
      await prisma.gameState.update({
        where: { id: gameId },
        data: { isInRelationship },
      });
    }

    await redis.del(`player:${gameState.playerId}`);
  }

  async getDecisionsForEvent(eventId: string): Promise<DecisionTemplate[]> {
    const cacheKey = `event:${eventId}:decisions`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }

      const decisions = await prisma.decisionTemplate.findMany({
        where: { eventId },
        orderBy: { order: 'asc' },
      });

      await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(decisions));
      return decisions as unknown as DecisionTemplate[];
    } catch (error) {
      logger.error(`Failed to get decisions for event ${eventId}:`, error);
      throw error;
    }
  }

  async getPlayerDecisions(gameId: string): Promise<PlayerDecision[]> {
    const cacheKey = `game:${gameId}:decisions`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }

      const decisions = await prisma.playerDecision.findMany({
        where: { gameId },
        orderBy: { createdAt: 'desc' },
      });

      await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(decisions));
      return decisions;
    } catch (error) {
      logger.error(`Failed to get decisions for game ${gameId}:`, error);
      throw error;
    }
  }

  async validateDecision(
    eventId: string,
    decisionId: string
  ): Promise<boolean> {
    try {
      const decision = await prisma.decisionTemplate.findUnique({
        where: { id: decisionId },
      });

      return decision?.eventId === eventId;
    } catch (error) {
      logger.error('Failed to validate decision:', error);
      return false;
    }
  }
}

import { Prisma } from '@prisma/client';

export const decisionSystem = new DecisionSystem();
