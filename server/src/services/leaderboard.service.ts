import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';

interface LeaderboardEntry {
  rank: number;
  playerId: string;
  username: string;
  score: number;
  wealth: number;
  gamesPlayed: number;
}

interface PlayerRank {
  rank: number;
  score: number;
  percentile: number;
  totalPlayers: number;
}

export class LeaderboardService {
  private readonly CACHE_TTL = 300;
  private readonly PAGE_SIZE = 50;

  async updatePlayerScore(playerId: string): Promise<number> {
    const profile = await prisma.playerProfile.findUnique({
      where: { playerId },
    });

    if (!profile) {
      throw new Error('Player profile not found');
    }

    const score = this.calculateScore(profile);

    await prisma.leaderboardEntry.upsert({
      where: { playerId },
      create: { playerId, score, season: 'current' },
      update: { score },
    });

    await redis.del('leaderboard:global');
    await redis.del(`leaderboard:rank:${playerId}`);

    return score;
  }

  private calculateScore(profile: {
    wealth: number;
    totalPlaytime: number;
    intelligence: number;
    charisma: number;
    physical: number;
    creativity: number;
    gamesPlayed: number;
  }): number {
    return Math.floor(
      profile.wealth * 1 +
      profile.totalPlaytime * 0.1 +
      profile.intelligence * 10 +
      profile.charisma * 10 +
      profile.physical * 10 +
      profile.creativity * 10 +
      profile.gamesPlayed * 50
    );
  }

  async getLeaderboard(page: number = 1, limit: number = this.PAGE_SIZE): Promise<{
    entries: LeaderboardEntry[];
    totalPages: number;
    currentPage: number;
  }> {
    const cacheKey = `leaderboard:global:${page}:${limit}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for leaderboard', error);
    }

    const offset = (page - 1) * limit;

    const [entries, totalCount] = await Promise.all([
      prisma.leaderboardEntry.findMany({
        where: { season: 'current' },
        orderBy: { score: 'desc' },
        skip: offset,
        take: limit,
        include: {
          player: {
            select: {
              username: true,
              profile: {
                select: { wealth: true, gamesPlayed: true },
              },
            },
          },
        },
      }),
      prisma.leaderboardEntry.count({ where: { season: 'current' } }),
    ]);

    const leaderboardEntries: LeaderboardEntry[] = entries.map((entry, index) => ({
      rank: offset + index + 1,
      playerId: entry.playerId,
      username: entry.player.username,
      score: entry.score,
      wealth: entry.player.profile?.wealth || 0,
      gamesPlayed: entry.player.profile?.gamesPlayed || 0,
    }));

    const result = {
      entries: leaderboardEntries,
      totalPages: Math.ceil(totalCount / limit),
      currentPage: page,
    };

    await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(result));

    return result;
  }

  async getPlayerRank(playerId: string): Promise<PlayerRank> {
    const cacheKey = `leaderboard:rank:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for player rank', error);
    }

    const entry = await prisma.leaderboardEntry.findUnique({
      where: { playerId },
    });

    if (!entry) {
      await this.updatePlayerScore(playerId);
      return this.getPlayerRank(playerId);
    }

    const [betterPlayers, totalPlayers] = await Promise.all([
      prisma.leaderboardEntry.count({
        where: { score: { gt: entry.score }, season: 'current' },
      }),
      prisma.leaderboardEntry.count({ where: { season: 'current' } }),
    ]);

    const rank = betterPlayers + 1;
    const percentile = Math.round((1 - betterPlayers / totalPlayers) * 100);

    const result: PlayerRank = {
      rank,
      score: entry.score,
      percentile,
      totalPlayers,
    };

    await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(result));

    return result;
  }

  async getTopPlayers(count: number = 10): Promise<LeaderboardEntry[]> {
    const result = await this.getLeaderboard(1, count);
    return result.entries;
  }

  async resetSeasonLeaderboard(): Promise<void> {
    const currentSeason = `season_${new Date().getFullYear()}_${Math.ceil((new Date().getMonth() + 1) / 3)}`;

    await prisma.leaderboardEntry.updateMany({
      where: { season: 'current' },
      data: { season: currentSeason },
    });

    const players = await prisma.player.findMany({
      select: { id: true },
    });

    for (const player of players) {
      await prisma.leaderboardEntry.create({
        data: { playerId: player.id, score: 0, season: 'current' },
      });
    }

    const keys = await redis.keys('leaderboard:*');
    if (keys.length > 0) {
      await redis.del(...keys);
    }

    logger.info(`Leaderboard reset complete. Previous season: ${currentSeason}`);
  }
}

export const leaderboardService = new LeaderboardService();
