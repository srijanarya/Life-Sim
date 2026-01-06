import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';

interface AdReward {
  premium?: number;
  gameCurrency?: number;
  health?: number;
  happiness?: number;
}

interface AdWatchResult {
  success: boolean;
  rewards: AdReward;
  adsRemaining: number;
  message?: string;
}

interface AdStatus {
  adsWatchedToday: number;
  adsRemaining: number;
  maxAdsPerDay: number;
  nextResetTime: string;
  canWatchAd: boolean;
  cooldownRemaining: number;
}

export class RewardedAdsService {
  private readonly AD_TYPES: Record<string, AdReward> = {
    DAILY_AD: { premium: 50, gameCurrency: 1000 },
    EXTRA_LIFE: { premium: 30 },
    BOOST_STATS: { health: 10, happiness: 10 },
    DOUBLE_REWARD: { premium: 100 },
    SKIP_WAIT: { premium: 20 },
  };

  private readonly MAX_DAILY_ADS = 3;
  private readonly VIP_MONTHLY_EXTRA_ADS = 5;
  private readonly VIP_YEARLY_EXTRA_ADS = 10;
  private readonly AD_COOLDOWN_SECONDS = 30;
  private readonly CACHE_TTL = 300;

  private getMaxAdsForPlayer(subscriptionTier: string): number {
    switch (subscriptionTier) {
      case 'VIP_YEARLY':
        return this.MAX_DAILY_ADS + this.VIP_YEARLY_EXTRA_ADS;
      case 'VIP_MONTHLY':
        return this.MAX_DAILY_ADS + this.VIP_MONTHLY_EXTRA_ADS;
      default:
        return this.MAX_DAILY_ADS;
    }
  }

  private getTodayRange(): { start: Date; end: Date } {
    const now = new Date();
    const start = new Date(now);
    start.setHours(0, 0, 0, 0);
    
    const end = new Date(now);
    end.setHours(23, 59, 59, 999);
    
    return { start, end };
  }

  async recordAdWatch(
    playerId: string,
    adType: string,
    ipAddress?: string
  ): Promise<AdWatchResult> {
    const rewards = this.AD_TYPES[adType];
    if (!rewards) {
      throw new Error(`Invalid ad type: ${adType}`);
    }

    const player = await prisma.player.findUnique({
      where: { id: playerId },
      select: { subscriptionTier: true },
    });

    if (!player) {
      throw new Error('Player not found');
    }

    const maxAds = this.getMaxAdsForPlayer(player.subscriptionTier);

    const { start, end } = this.getTodayRange();
    const adsWatchedToday = await prisma.economyTransaction.count({
      where: {
        playerId,
        type: 'REWARDED_AD',
        createdAt: { gte: start, lte: end },
      },
    });

    if (adsWatchedToday >= maxAds) {
      throw new Error(`Daily ad limit reached (${maxAds} ads per day)`);
    }

    const cooldownKey = `ad-cooldown:${playerId}`;
    const cooldown = await redis.get(cooldownKey);
    if (cooldown) {
      const remaining = await redis.ttl(cooldownKey);
      throw new Error(`Please wait ${remaining} seconds before watching another ad`);
    }

    if (rewards.premium && rewards.premium > 0) {
      await prisma.economyTransaction.create({
        data: {
          playerId,
          type: 'REWARDED_AD',
          amount: rewards.premium,
          currency: 'premium',
          metadata: {
            adType,
            timestamp: Date.now(),
            ipAddress: ipAddress || 'unknown',
          },
        },
      });
    }

    if (rewards.gameCurrency && rewards.gameCurrency > 0) {
      await prisma.playerProfile.update({
        where: { playerId },
        data: { wealth: { increment: rewards.gameCurrency } },
      });
    }

    if (rewards.health || rewards.happiness) {
      const statUpdates: Record<string, { increment: number }> = {};
      
      if (rewards.health) {
        statUpdates.health = { increment: Math.min(rewards.health, 100) };
      }
      if (rewards.happiness) {
        statUpdates.happiness = { increment: Math.min(rewards.happiness, 100) };
      }

      await prisma.playerProfile.update({
        where: { playerId },
        data: statUpdates,
      });
    }

    await redis.setex(cooldownKey, this.AD_COOLDOWN_SECONDS, '1');
    await redis.del(`ad-status:${playerId}`);

    logger.info(`Player ${playerId} watched ${adType} ad, rewards granted`);

    return {
      success: true,
      rewards,
      adsRemaining: maxAds - adsWatchedToday - 1,
      message: `Earned ${rewards.premium || 0} premium currency!`,
    };
  }

  async getAdStatus(playerId: string): Promise<AdStatus> {
    const cacheKey = `ad-status:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for ad status', error);
    }

    const player = await prisma.player.findUnique({
      where: { id: playerId },
      select: { subscriptionTier: true },
    });

    if (!player) {
      throw new Error('Player not found');
    }

    const maxAds = this.getMaxAdsForPlayer(player.subscriptionTier);

    const { start, end } = this.getTodayRange();
    const adsWatchedToday = await prisma.economyTransaction.count({
      where: {
        playerId,
        type: 'REWARDED_AD',
        createdAt: { gte: start, lte: end },
      },
    });

    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(0, 0, 0, 0);

    const cooldownKey = `ad-cooldown:${playerId}`;
    const cooldownRemaining = await redis.ttl(cooldownKey);

    const adsRemaining = Math.max(0, maxAds - adsWatchedToday);

    const status: AdStatus = {
      adsWatchedToday,
      adsRemaining,
      maxAdsPerDay: maxAds,
      nextResetTime: tomorrow.toISOString(),
      canWatchAd: adsRemaining > 0 && cooldownRemaining <= 0,
      cooldownRemaining: Math.max(0, cooldownRemaining),
    };

    await redis.setex(cacheKey, 60, JSON.stringify(status));

    return status;
  }

  getAdTypes(): Record<string, AdReward> {
    return { ...this.AD_TYPES };
  }

  async getAdHistory(
    playerId: string,
    limit: number = 20
  ): Promise<Array<{ adType: string; reward: number; timestamp: Date }>> {
    const transactions = await prisma.economyTransaction.findMany({
      where: {
        playerId,
        type: 'REWARDED_AD',
      },
      orderBy: { createdAt: 'desc' },
      take: limit,
      select: {
        amount: true,
        metadata: true,
        createdAt: true,
      },
    });

    return transactions.map((t) => ({
      adType: (t.metadata as Record<string, unknown>)?.adType as string || 'UNKNOWN',
      reward: t.amount,
      timestamp: t.createdAt,
    }));
  }
}

export const rewardedAdsService = new RewardedAdsService();
