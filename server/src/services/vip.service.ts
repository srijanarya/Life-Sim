import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';
import { SubscriptionTier } from '@prisma/client';

interface VipBenefits {
  adsRemoved: boolean;
  dailyBonusMultiplier: number;
  exclusiveEvents: boolean;
  prioritySupport: boolean;
  extraDailyAds: number;
  iapDiscount?: number;
}

interface VipTierConfig {
  price: number;
  duration: number;
  benefits: VipBenefits;
}

interface VipStatus {
  tier: string;
  isActive: boolean;
  daysRemaining: number;
  expiresAt: string | null;
  benefits: VipBenefits;
}

export class VipService {
  private readonly VIP_TIERS: Record<string, VipTierConfig> = {
    VIP_MONTHLY: {
      price: 999,
      duration: 30,
      benefits: {
        adsRemoved: true,
        dailyBonusMultiplier: 2,
        exclusiveEvents: true,
        prioritySupport: true,
        extraDailyAds: 5,
      },
    },
    VIP_YEARLY: {
      price: 7999,
      duration: 365,
      benefits: {
        adsRemoved: true,
        dailyBonusMultiplier: 3,
        exclusiveEvents: true,
        prioritySupport: true,
        extraDailyAds: 10,
        iapDiscount: 0.2,
      },
    },
  };

  private readonly FREE_BENEFITS: VipBenefits = {
    adsRemoved: false,
    dailyBonusMultiplier: 1,
    exclusiveEvents: false,
    prioritySupport: false,
    extraDailyAds: 0,
  };

  async activateSubscription(
    playerId: string,
    tier: 'VIP_MONTHLY' | 'VIP_YEARLY',
    receiptData: string
  ): Promise<{ success: boolean; status: VipStatus }> {
    const tierConfig = this.VIP_TIERS[tier];
    if (!tierConfig) {
      throw new Error(`Invalid subscription tier: ${tier}`);
    }

    const isValid = await this.validateReceipt(receiptData);
    if (!isValid) {
      throw new Error('Invalid purchase receipt');
    }

    const endDate = new Date();
    endDate.setDate(endDate.getDate() + tierConfig.duration);

    await prisma.player.update({
      where: { id: playerId },
      data: {
        subscriptionTier: tier as SubscriptionTier,
        subscriptionEnds: endDate,
      },
    });

    await prisma.economyTransaction.create({
      data: {
        playerId,
        type: 'VIP_SUBSCRIPTION',
        amount: tierConfig.price,
        currency: 'USD',
        receiptId: receiptData,
        productId: `com.lifecraft.${tier.toLowerCase()}`,
        metadata: { tier, duration: tierConfig.duration },
      },
    });

    await redis.del(`vip-status:${playerId}`);

    logger.info(`VIP subscription activated for player ${playerId}: ${tier}`);

    const status = await this.getVipStatus(playerId);
    return { success: true, status };
  }

  async getVipStatus(playerId: string): Promise<VipStatus> {
    const cacheKey = `vip-status:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for VIP status', error);
    }

    const player = await prisma.player.findUnique({
      where: { id: playerId },
      select: { subscriptionTier: true, subscriptionEnds: true },
    });

    if (!player) {
      throw new Error('Player not found');
    }

    const now = new Date();
    const isExpired = player.subscriptionEnds && player.subscriptionEnds < now;
    const effectiveTier = isExpired ? 'FREE' : player.subscriptionTier;

    let daysRemaining = 0;
    if (player.subscriptionEnds && !isExpired) {
      const diffTime = player.subscriptionEnds.getTime() - now.getTime();
      daysRemaining = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    }

    const benefits = this.VIP_TIERS[effectiveTier]?.benefits || this.FREE_BENEFITS;

    const status: VipStatus = {
      tier: effectiveTier,
      isActive: effectiveTier !== 'FREE',
      daysRemaining,
      expiresAt: player.subscriptionEnds?.toISOString() || null,
      benefits,
    };

    await redis.setex(cacheKey, 300, JSON.stringify(status));

    return status;
  }

  async cancelSubscription(playerId: string): Promise<{ success: boolean; message: string }> {
    const player = await prisma.player.findUnique({
      where: { id: playerId },
      select: { subscriptionTier: true, subscriptionEnds: true },
    });

    if (!player || player.subscriptionTier === 'FREE') {
      throw new Error('No active subscription to cancel');
    }

    logger.info(`VIP subscription cancelled for player ${playerId}`);

    await redis.del(`vip-status:${playerId}`);

    return {
      success: true,
      message: `Subscription cancelled. Benefits active until ${player.subscriptionEnds?.toISOString()}`,
    };
  }

  async checkAndExpireSubscriptions(): Promise<number> {
    const now = new Date();

    const expiredPlayers = await prisma.player.updateMany({
      where: {
        subscriptionTier: { not: 'FREE' },
        subscriptionEnds: { lt: now },
      },
      data: {
        subscriptionTier: 'FREE',
      },
    });

    if (expiredPlayers.count > 0) {
      logger.info(`Expired ${expiredPlayers.count} VIP subscriptions`);
    }

    return expiredPlayers.count;
  }

  getBenefitsForTier(tier: string): VipBenefits {
    return this.VIP_TIERS[tier]?.benefits || this.FREE_BENEFITS;
  }

  getAvailableTiers(): Record<string, { price: number; duration: number; benefits: VipBenefits }> {
    return { ...this.VIP_TIERS };
  }

  private async validateReceipt(receiptData: string): Promise<boolean> {
    if (!receiptData || receiptData.length < 10) {
      return false;
    }
    return true;
  }
}

export const vipService = new VipService();
