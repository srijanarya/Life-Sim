import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';

interface Milestone {
  key: string;
  title: string;
  description: string;
  rewardPremium: number;
  rewardCurrency?: number;
}

interface UnlockedMilestone {
  key: string;
  title: string;
  description: string;
  rewardPremium: number;
  rewardCurrency: number;
  unlockedAt: string;
}

export class MilestoneService {
  private readonly MILESTONES: Record<string, Milestone> = {
    AGE_18: { key: 'AGE_18', title: 'Coming of Age', description: 'You turned 18!', rewardPremium: 100, rewardCurrency: 500 },
    AGE_21: { key: 'AGE_21', title: 'Legal Adult', description: 'You turned 21!', rewardPremium: 150, rewardCurrency: 1000 },
    AGE_30: { key: 'AGE_30', title: 'Thirty and Thriving', description: 'You turned 30!', rewardPremium: 200, rewardCurrency: 2000 },
    AGE_40: { key: 'AGE_40', title: 'Fabulous at 40', description: 'You turned 40!', rewardPremium: 300, rewardCurrency: 3000 },
    AGE_50: { key: 'AGE_50', title: 'Half Century', description: 'You turned 50!', rewardPremium: 500, rewardCurrency: 5000 },
    FIRST_JOB: { key: 'FIRST_JOB', title: 'First Job', description: 'You got your first job!', rewardPremium: 150, rewardCurrency: 1000 },
    FIRST_PROMOTION: { key: 'FIRST_PROMOTION', title: 'Promoted!', description: 'You got your first promotion!', rewardPremium: 200, rewardCurrency: 1500 },
    MILLIONAIRE: { key: 'MILLIONAIRE', title: 'Millionaire!', description: 'You have 1,000,000+ wealth!', rewardPremium: 1000, rewardCurrency: 10000 },
    MULTI_MILLIONAIRE: { key: 'MULTI_MILLIONAIRE', title: 'Multi-Millionaire!', description: 'You have 10,000,000+ wealth!', rewardPremium: 5000, rewardCurrency: 50000 },
    MARRIAGE: { key: 'MARRIAGE', title: 'Happily Married', description: 'You got married!', rewardPremium: 250, rewardCurrency: 2000 },
    FIRST_CHILD: { key: 'FIRST_CHILD', title: 'New Parent', description: 'You had your first child!', rewardPremium: 300, rewardCurrency: 2500 },
    PERFECT_STATS: { key: 'PERFECT_STATS', title: 'Perfect Specimen', description: 'All stats at 100!', rewardPremium: 500, rewardCurrency: 5000 },
    SOCIAL_BUTTERFLY: { key: 'SOCIAL_BUTTERFLY', title: 'Social Butterfly', description: 'Charisma at 100!', rewardPremium: 200, rewardCurrency: 1500 },
    GENIUS: { key: 'GENIUS', title: 'Genius', description: 'Intelligence at 100!', rewardPremium: 200, rewardCurrency: 1500 },
    ATHLETE: { key: 'ATHLETE', title: 'Athlete', description: 'Physical at 100!', rewardPremium: 200, rewardCurrency: 1500 },
    ARTIST: { key: 'ARTIST', title: 'Artist', description: 'Creativity at 100!', rewardPremium: 200, rewardCurrency: 1500 },
    EVENT_VETERAN: { key: 'EVENT_VETERAN', title: 'Event Veteran', description: 'Completed 100 events!', rewardPremium: 300, rewardCurrency: 3000 },
    DECISION_MASTER: { key: 'DECISION_MASTER', title: 'Decision Master', description: 'Made 200 decisions!', rewardPremium: 400, rewardCurrency: 4000 },
  };

  async checkMilestones(gameId: string): Promise<UnlockedMilestone[]> {
    const gameState = await prisma.gameState.findUnique({
      where: { id: gameId },
      include: {
        player: { include: { profile: true, achievements: true } },
        decisions: true,
      },
    });

    if (!gameState) {
      throw new Error('Game state not found');
    }

    const { player } = gameState;
    const profile = player.profile;
    const unlockedMilestones = player.achievements.map(a => a.achievementId);
    const newlyUnlocked: UnlockedMilestone[] = [];

    if (profile.age >= 18 && !unlockedMilestones.includes('AGE_18')) {
      await this.unlockMilestone(player.id, 'AGE_18');
      newlyUnlocked.push(this.MILESTONES.AGE_18);
    }

    if (profile.age >= 21 && !unlockedMilestones.includes('AGE_21')) {
      await this.unlockMilestone(player.id, 'AGE_21');
      newlyUnlocked.push(this.MILESTONES.AGE_21);
    }

    if (profile.age >= 30 && !unlockedMilestones.includes('AGE_30')) {
      await this.unlockMilestone(player.id, 'AGE_30');
      newlyUnlocked.push(this.MILESTONES.AGE_30);
    }

    if (profile.age >= 40 && !unlockedMilestones.includes('AGE_40')) {
      await this.unlockMilestone(player.id, 'AGE_40');
      newlyUnlocked.push(this.MILESTONES.AGE_40);
    }

    if (profile.age >= 50 && !unlockedMilestones.includes('AGE_50')) {
      await this.unlockMilestone(player.id, 'AGE_50');
      newlyUnlocked.push(this.MILESTONES.AGE_50);
    }

    if (gameState.careerId && gameState.careerLevel > 0 && !unlockedMilestones.includes('FIRST_JOB')) {
      await this.unlockMilestone(player.id, 'FIRST_JOB');
      newlyUnlocked.push(this.MILESTONES.FIRST_JOB);
    }

    if (gameState.careerLevel >= 2 && !unlockedMilestones.includes('FIRST_PROMOTION')) {
      await this.unlockMilestone(player.id, 'FIRST_PROMOTION');
      newlyUnlocked.push(this.MILESTONES.FIRST_PROMOTION);
    }

    if (profile.wealth >= 1000000 && !unlockedMilestones.includes('MILLIONAIRE')) {
      await this.unlockMilestone(player.id, 'MILLIONAIRE');
      newlyUnlocked.push(this.MILESTONES.MILLIONAIRE);
    }

    if (profile.wealth >= 10000000 && !unlockedMilestones.includes('MULTI_MILLIONAIRE')) {
      await this.unlockMilestone(player.id, 'MULTI_MILLIONAIRE');
      newlyUnlocked.push(this.MILESTONES.MULTI_MILLIONAIRE);
    }

    if (gameState.isInRelationship && !unlockedMilestones.includes('MARRIAGE')) {
      await this.unlockMilestone(player.id, 'MARRIAGE');
      newlyUnlocked.push(this.MILESTONES.MARRIAGE);
    }

    const decisionCount = gameState.decisions.length;
    if (decisionCount >= 1 && !unlockedMilestones.includes('FIRST_CHILD')) {
      await this.unlockMilestone(player.id, 'FIRST_CHILD');
      newlyUnlocked.push(this.MILESTONES.FIRST_CHILD);
    }

    if (profile.health >= 100 && profile.happiness >= 100 && 
        profile.intelligence >= 100 && profile.charisma >= 100 && 
        profile.physical >= 100 && profile.creativity >= 100 && 
        !unlockedMilestones.includes('PERFECT_STATS')) {
      await this.unlockMilestone(player.id, 'PERFECT_STATS');
      newlyUnlocked.push(this.MILESTONES.PERFECT_STATS);
    }

    if (profile.charisma >= 100 && !unlockedMilestones.includes('SOCIAL_BUTTERFLY')) {
      await this.unlockMilestone(player.id, 'SOCIAL_BUTTERFLY');
      newlyUnlocked.push(this.MILESTONES.SOCIAL_BUTTERFLY);
    }

    if (profile.intelligence >= 100 && !unlockedMilestones.includes('GENIUS')) {
      await this.unlockMilestone(player.id, 'GENIUS');
      newlyUnlocked.push(this.MILESTONES.GENIUS);
    }

    if (profile.physical >= 100 && !unlockedMilestones.includes('ATHLETE')) {
      await this.unlockMilestone(player.id, 'ATHLETE');
      newlyUnlocked.push(this.MILESTONES.ATHLETE);
    }

    if (profile.creativity >= 100 && !unlockedMilestones.includes('ARTIST')) {
      await this.unlockMilestone(player.id, 'ARTIST');
      newlyUnlocked.push(this.MILESTONES.ARTIST);
    }

    const eventsCompleted = await prisma.playerEvent.count({ where: { gameId } });
    if (eventsCompleted >= 100 && !unlockedMilestones.includes('EVENT_VETERAN')) {
      await this.unlockMilestone(player.id, 'EVENT_VETERAN');
      newlyUnlocked.push(this.MILESTONES.EVENT_VETERAN);
    }

    if (decisionCount >= 200 && !unlockedMilestones.includes('DECISION_MASTER')) {
      await this.unlockMilestone(player.id, 'DECISION_MASTER');
      newlyUnlocked.push(this.MILESTONES.DECISION_MASTER);
    }

    await redis.del(`milestones:${player.id}`);

    return newlyUnlocked;
  }

  async getUnlockedMilestones(playerId: string): Promise<UnlockedMilestone[]> {
    const achievements = await prisma.playerAchievement.findMany({
      where: { playerId },
      include: { achievement: true },
      orderBy: { unlockedAt: 'desc' },
    });

    return achievements.map(a => ({
      key: a.achievementId,
      title: this.MILESTONES[a.achievementId]?.title || a.achievementId,
      description: this.MILESTONES[a.achievementId]?.description || '',
      rewardPremium: this.MILESTONES[a.achievementId]?.rewardPremium || 0,
      rewardCurrency: this.MILESTONES[a.achievementId]?.rewardCurrency || 0,
      unlockedAt: a.unlockedAt.toISOString(),
    }));
  }

  async getAllMilestones(playerId: string): Promise<{ milestones: Milestone[]; unlocked: string[] }> {
    const achievements = await prisma.playerAchievement.findMany({
      where: { playerId },
      select: { achievementId: true },
    });

    const unlocked = achievements.map(a => a.achievementId);

    return {
      milestones: Object.values(this.MILESTONES),
      unlocked,
    };
  }

  private async unlockMilestone(playerId: string, key: string): Promise<void> {
    const milestone = this.MILESTONES[key];

    await prisma.playerAchievement.create({
      data: {
        playerId,
        achievementId: key,
      },
    });

    if (milestone.rewardPremium > 0) {
      await prisma.economyTransaction.create({
        data: {
          playerId,
          type: 'SYSTEM_GIFT',
          amount: milestone.rewardPremium,
          currency: 'premium',
          metadata: { source: 'milestone', key },
        },
      });
    }

    if (milestone.rewardCurrency && milestone.rewardCurrency > 0) {
      await prisma.playerProfile.update({
        where: { playerId },
        data: { wealth: { increment: milestone.rewardCurrency } },
      });
    }

    logger.info(`Milestone unlocked for player ${playerId}: ${key}`);
  }
}

export const milestoneService = new MilestoneService();
