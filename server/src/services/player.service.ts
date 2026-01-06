import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { PlayerProfile as PlayerProfileType } from '@types/game';
import { logger } from '@config/logger';
import { Player, Prisma } from '@prisma/client';

type PlayerWithProfile = Player & { profile: PlayerProfileType };

export class PlayerService {
  private static readonly CACHE_TTL = 3600;

  async getPlayerById(id: string): Promise<PlayerWithProfile | null> {
    const cacheKey = `player:${id}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        logger.debug(`Cache hit for player: ${id}`);
        return JSON.parse(cached);
      }

      const player = await prisma.player.findUnique({
        where: { id },
        include: { profile: true },
      });

      if (player) {
        await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(player));
      }

      return player as PlayerWithProfile;
    } catch (error) {
      logger.error(`Failed to get player ${id}:`, error);
      throw error;
    }
  }

  async getPlayerByEmail(email: string): Promise<Player | null> {
    try {
      return await prisma.player.findUnique({
        where: { email },
      });
    } catch (error) {
      logger.error(`Failed to get player by email ${email}:`, error);
      throw error;
    }
  }

  async createPlayer(
    data: Prisma.PlayerCreateInput
  ): Promise<PlayerWithProfile> {
    try {
      const player = await prisma.player.create({
        data: {
          ...data,
          profile: {
            create: {},
          },
        },
        include: { profile: true },
      });

      logger.info(`Created new player: ${player.id}`);
      return player as PlayerWithProfile;
    } catch (error) {
      logger.error('Failed to create player:', error);
      throw error;
    }
  }

  async updatePlayer(
    id: string,
    data: Prisma.PlayerUpdateInput
  ): Promise<Player> {
    try {
      const player = await prisma.player.update({
        where: { id },
        data,
      });

      await redis.del(`player:${id}`);
      logger.info(`Updated player: ${id}`);
      return player;
    } catch (error) {
      logger.error(`Failed to update player ${id}:`, error);
      throw error;
    }
  }

  async updateProfile(
    playerId: string,
    updates: Partial<PlayerProfileType>
  ): Promise<PlayerProfileType> {
    try {
      const profile = await prisma.playerProfile.update({
        where: { playerId },
        data: updates,
      });

      await redis.del(`player:${playerId}`);
      logger.info(`Updated profile for player: ${playerId}`);
      return profile;
    } catch (error) {
      logger.error(`Failed to update profile for player ${playerId}:`, error);
      throw error;
    }
  }

  async incrementStat(
    playerId: string,
    stat: keyof PlayerProfileType,
    amount: number
  ): Promise<PlayerProfileType> {
    const validStats: Array<keyof PlayerProfileType> = [
      'health',
      'happiness',
      'wealth',
      'intelligence',
      'charisma',
      'physical',
      'creativity',
      'totalPlaytime',
      'gamesPlayed',
    ];

    if (!validStats.includes(stat)) {
      throw new Error(`Invalid stat: ${stat}`);
    }

    try {
      const profile = await prisma.playerProfile.update({
        where: { playerId },
        data: {
          [stat]: {
            increment: amount,
          },
        },
      });

      await redis.del(`player:${playerId}`);
      return profile;
    } catch (error) {
      logger.error(
        `Failed to increment ${stat} for player ${playerId}:`,
        error
      );
      throw error;
    }
  }

  async checkAchievements(
    playerId: string
  ): Promise<PlayerProfileType['achievements']> {
    try {
      const achievements = await prisma.playerAchievement.findMany({
        where: { playerId },
        include: { achievement: true },
      });

      return achievements as unknown as PlayerProfileType['achievements'];
    } catch (error) {
      logger.error(`Failed to check achievements for player ${playerId}:`, error);
      throw error;
    }
  }
}

export const playerService = new PlayerService();
