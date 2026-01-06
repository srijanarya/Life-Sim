import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';

interface AvatarItem {
  id: string;
  name: string;
  category: 'HAIR' | 'FACE' | 'OUTFIT' | 'ACCESSORY';
  imageUrl: string;
  rarity: 'COMMON' | 'UNCOMMON' | 'RARE' | 'EPIC' | 'LEGENDARY';
  price: number;
  priceCurrency: 'premium' | 'game';
  isPremiumOnly: boolean;
  isActive: boolean;
}

interface PlayerInventoryItem {
  avatarItemId: string;
  name: string;
  category: string;
  imageUrl: string;
  rarity: string;
  price: number;
  priceCurrency: string;
  purchasedAt: string;
}

interface CurrentAvatar {
  hairId: string | null;
  faceId: string | null;
  outfitId: string | null;
  accessoryId: string | null;
}

interface AvailableAvatarsResponse {
  categories: Record<string, AvatarItem[]>;
  inventory: PlayerInventoryItem[];
  equipped: CurrentAvatar;
}

export class AvatarService {
  private readonly AVATAR_CATEGORIES = ['HAIR', 'FACE', 'OUTFIT', 'ACCESSORY'] as const;
  private readonly RARITY_WEIGHTS = {
    COMMON: 70,
    UNCOMMON: 20,
    RARE: 7,
    EPIC: 2,
    LEGENDARY: 1,
  };
  private readonly CACHE_TTL = 600;

  async getAllAvailableAvatars(playerId: string): Promise<AvailableAvatarsResponse> {
    const cacheKey = `avatars:available:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for available avatars', error);
    }

    const [items, inventory, equipped] = await Promise.all([
      this.getAvailableItems(),
      this.getPlayerInventory(playerId),
      this.getPlayerEquippedAvatar(playerId),
    ]);

    const categories: Record<string, AvatarItem[]> = {};
    for (const category of this.AVATAR_CATEGORIES) {
      categories[category] = items
        .filter(item => item.category === category)
        .map(item => ({
          id: item.id,
          name: item.name,
          category: item.category,
          imageUrl: item.imageUrl,
          rarity: item.rarity,
          price: item.price,
          priceCurrency: item.priceCurrency,
          isPremiumOnly: item.isPremiumOnly,
          isActive: item.isActive,
        }));
    }

    const response: AvailableAvatarsResponse = {
      categories,
      inventory,
      equipped,
    };

    await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(response));

    return response;
  }

  async getAvailableItems(): Promise<Array<{
    id: string;
    name: string;
    category: string;
    imageUrl: string;
    rarity: string;
    price: number;
    priceCurrency: string;
    isPremiumOnly: boolean;
    isActive: boolean;
  }>> {
    const items = await prisma.avatarItem.findMany({
      where: { isActive: true },
      orderBy: { rarity: 'asc' },
    });

    return items;
  }

  async getPlayerInventory(playerId: string): Promise<PlayerInventoryItem[]> {
    const cacheKey = `avatars:inventory:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for avatar inventory', error);
    }

    const inventoryItems = await prisma.playerAvatarInventory.findMany({
      where: { playerId },
      include: { avatarItem: true },
      orderBy: { purchasedAt: 'desc' },
    });

    const inventory: PlayerInventoryItem[] = inventoryItems.map(item => ({
      avatarItemId: item.avatarItemId,
      name: item.avatarItem.name,
      category: item.avatarItem.category,
      imageUrl: item.avatarItem.imageUrl,
      rarity: item.avatarItem.rarity,
      price: item.avatarItem.price,
      priceCurrency: item.avatarItem.priceCurrency,
      purchasedAt: item.purchasedAt.toISOString(),
    }));

    await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(inventory));

    return inventory;
  }

  async getPlayerEquippedAvatar(playerId: string): Promise<CurrentAvatar> {
    const cacheKey = `avatars:equipped:${playerId}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for equipped avatar', error);
    }

    const equipped = await prisma.playerAvatar.findUnique({
      where: { playerId },
    });

    const result: CurrentAvatar = {
      hairId: equipped?.hairId || null,
      faceId: equipped?.faceId || null,
      outfitId: equipped?.outfitId || null,
      accessoryId: equipped?.accessoryId || null,
    };

    await redis.setex(cacheKey, this.CACHE_TTL, JSON.stringify(result));

    return result;
  }

  async purchaseAvatar(
    playerId: string,
    avatarItemId: string
  ): Promise<{ success: boolean; equipped: CurrentAvatar }> {
    const [player, avatarItem, alreadyOwned] = await Promise.all([
      prisma.player.findUnique({
        where: { id: playerId },
        select: { subscriptionTier: true },
      }),
      prisma.avatarItem.findUnique({ where: { id: avatarItemId } }),
      prisma.playerAvatarInventory.findFirst({
        where: { playerId, avatarItemId },
      }),
    ]);

    if (!player || !avatarItem) {
      throw new Error('Player or avatar item not found');
    }

    if (alreadyOwned) {
      throw new Error('Avatar already owned');
    }

    if (avatarItem.priceCurrency === 'premium') {
      const profile = await prisma.playerProfile.findUnique({
        where: { playerId },
        select: { wealth: true },
      });

      if (!profile || profile.wealth < avatarItem.price) {
        throw new Error(`Insufficient ${avatarItem.priceCurrency} currency`);
      }
    } else {
      const premiumBalance = await this.getPremiumBalance(playerId);
      if (premiumBalance < avatarItem.price) {
        throw new Error('Insufficient premium currency');
      }
    }

    if (avatarItem.priceCurrency === 'premium') {
      await prisma.economyTransaction.create({
        data: {
          playerId,
          type: 'SYSTEM_GIFT',
          amount: -avatarItem.price,
          currency: 'premium',
          metadata: { source: 'avatar_purchase', avatarItemId },
        },
      });
    } else {
      await prisma.playerProfile.update({
        where: { playerId },
        data: { wealth: { decrement: avatarItem.price } },
      });
    }

    await prisma.playerAvatarInventory.create({
      data: { playerId, avatarItemId },
    });

    await redis.del(`avatars:inventory:${playerId}`);

    logger.info(`Player ${playerId} purchased avatar ${avatarItemId}`);

    const equipped = await this.getPlayerEquippedAvatar(playerId);

    return { success: true, equipped };
  }

  async equipAvatar(
    playerId: string,
    avatarItemId: string,
    category: string
  ): Promise<CurrentAvatar> {
    const [alreadyOwned, avatarItem] = await Promise.all([
      prisma.playerAvatarInventory.findFirst({
        where: { playerId, avatarItemId },
      }),
      prisma.avatarItem.findUnique({ where: { id: avatarItemId } }),
    ]);

    if (!alreadyOwned || !avatarItem) {
      throw new Error('Avatar not owned');
    }

    if (avatarItem.category !== category) {
      throw new Error('Avatar category mismatch');
    }

    const updateData: Record<string, string> = {};
    switch (category) {
      case 'HAIR':
        updateData.hairId = avatarItemId;
        break;
      case 'FACE':
        updateData.faceId = avatarItemId;
        break;
      case 'OUTFIT':
        updateData.outfitId = avatarItemId;
        break;
      case 'ACCESSORY':
        updateData.accessoryId = avatarItemId;
        break;
    }

    await prisma.playerAvatar.upsert({
      where: { playerId },
      create: { playerId, ...updateData },
      update: updateData,
    });

    await redis.del(`avatars:equipped:${playerId}`);

    logger.info(`Player ${playerId} equipped avatar ${avatarItemId}`);

    return this.getPlayerEquippedAvatar(playerId);
  }

  async unequipAvatar(playerId: string, category: string): Promise<CurrentAvatar> {
    const updateData: Record<string, { set: null }> = {};
    switch (category) {
      case 'HAIR':
        updateData.hairId = { set: null };
        break;
      case 'FACE':
        updateData.faceId = { set: null };
        break;
      case 'OUTFIT':
        updateData.outfitId = { set: null };
        break;
      case 'ACCESSORY':
        updateData.accessoryId = { set: null };
        break;
    }

    await prisma.playerAvatar.update({
      where: { playerId },
      data: updateData,
    });

    await redis.del(`avatars:equipped:${playerId}`);

    return this.getPlayerEquippedAvatar(playerId);
  }

  private async getPremiumBalance(playerId: string): Promise<number> {
    const [income, expense] = await Promise.all([
      prisma.economyTransaction.aggregate({
        where: {
          playerId,
          type: 'REWARDED_AD',
          currency: 'premium',
        },
        _sum: { amount: true },
      }),
      prisma.economyTransaction.aggregate({
        where: {
          playerId,
          type: 'SYSTEM_GIFT',
          currency: 'premium',
        },
        _sum: { amount: true },
      }),
    ]);

    const totalEarned = income._sum.amount || 0;
    const totalSpent = Math.abs(expense._sum.amount || 0);

    return totalEarned - totalSpent;
  }

  async getStarterAvatars(playerId: string): Promise<AvatarItem[]> {
    const starterItems = await prisma.avatarItem.findMany({
      where: {
        isActive: true,
        OR: [
          { price: { equals: 0 } },
          { isPremiumOnly: { equals: false } },
        ],
      },
      orderBy: { category: 'asc' },
    });

    return starterItems.map(item => ({
      id: item.id,
      name: item.name,
      category: item.category,
      imageUrl: item.imageUrl,
      rarity: item.rarity,
      price: item.price,
      priceCurrency: item.priceCurrency,
      isPremiumOnly: item.isPremiumOnly,
      isActive: item.isActive,
    }));
  }

  async grantStarterAvatars(playerId: string): Promise<void> {
    const existing = await prisma.playerAvatarInventory.findMany({
      where: { playerId },
      select: { avatarItemId: true },
    });

    const ownedIds = new Set(existing.map(e => e.avatarItemId));

    const starters = await this.getStarterAvatars(playerId);

    for (const starter of starters) {
      if (!ownedIds.has(starter.id)) {
        await prisma.playerAvatarInventory.create({
          data: { playerId, avatarItemId: starter.id },
        });
      }
    }

    await redis.del(`avatars:inventory:${playerId}`);

    logger.info(`Granted starter avatars to player ${playerId}`);
  }
}

export const avatarService = new AvatarService();
