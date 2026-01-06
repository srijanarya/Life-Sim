import { prisma } from '@config/database';
import { redis } from '@config/redis';
import { logger } from '@config/logger';

interface ChallengeTemplate {
  id: string;
  title: string;
  description: string;
  objectiveType: string;
  targetValue: number;
  difficulty: 'EASY' | 'MEDIUM' | 'HARD';
  rewardPremium: number;
  rewardCurrency: number;
}

interface PlayerChallenge {
  challengeId: string;
  title: string;
  description: string;
  difficulty: string;
  progress: number;
  targetValue: number;
  completed: boolean;
  rewardPremium: number;
  rewardCurrency: number;
}

interface DailyChallengesResponse {
  challenges: PlayerChallenge[];
  allCompleted: boolean;
  bonusAvailable: boolean;
  bonusReward: number;
}

export class DailyChallengeService {
  private readonly CHALLENGE_TEMPLATES: ChallengeTemplate[] = [
    { id: 'earn_wealth_easy', title: 'Quick Earner', description: 'Earn 500 wealth', objectiveType: 'EARN_WEALTH', targetValue: 500, difficulty: 'EASY', rewardPremium: 20, rewardCurrency: 200 },
    { id: 'earn_wealth_med', title: 'Money Maker', description: 'Earn 2000 wealth', objectiveType: 'EARN_WEALTH', targetValue: 2000, difficulty: 'MEDIUM', rewardPremium: 50, rewardCurrency: 500 },
    { id: 'earn_wealth_hard', title: 'Wealth Builder', description: 'Earn 5000 wealth', objectiveType: 'EARN_WEALTH', targetValue: 5000, difficulty: 'HARD', rewardPremium: 100, rewardCurrency: 1000 },
    { id: 'play_events_easy', title: 'Event Explorer', description: 'Complete 3 events', objectiveType: 'PLAY_EVENTS', targetValue: 3, difficulty: 'EASY', rewardPremium: 15, rewardCurrency: 150 },
    { id: 'play_events_med', title: 'Event Master', description: 'Complete 7 events', objectiveType: 'PLAY_EVENTS', targetValue: 7, difficulty: 'MEDIUM', rewardPremium: 40, rewardCurrency: 400 },
    { id: 'play_events_hard', title: 'Event Champion', description: 'Complete 15 events', objectiveType: 'PLAY_EVENTS', targetValue: 15, difficulty: 'HARD', rewardPremium: 80, rewardCurrency: 800 },
    { id: 'make_decision_easy', title: 'Decision Maker', description: 'Make 5 decisions', objectiveType: 'MAKE_DECISIONS', targetValue: 5, difficulty: 'EASY', rewardPremium: 15, rewardCurrency: 150 },
    { id: 'make_decision_med', title: 'Thoughtful Player', description: 'Make 12 decisions', objectiveType: 'MAKE_DECISIONS', targetValue: 12, difficulty: 'MEDIUM', rewardPremium: 35, rewardCurrency: 350 },
    { id: 'boost_stats_easy', title: 'Self Improvement', description: 'Increase any stat by 10', objectiveType: 'BOOST_STATS', targetValue: 10, difficulty: 'EASY', rewardPremium: 20, rewardCurrency: 200 },
    { id: 'boost_stats_hard', title: 'Peak Performance', description: 'Increase any stat by 50', objectiveType: 'BOOST_STATS', targetValue: 50, difficulty: 'HARD', rewardPremium: 90, rewardCurrency: 900 },
    { id: 'watch_ads_easy', title: 'Ad Watcher', description: 'Watch 2 rewarded ads', objectiveType: 'WATCH_ADS', targetValue: 2, difficulty: 'EASY', rewardPremium: 25, rewardCurrency: 250 },
    { id: 'login_streak', title: 'Loyal Player', description: 'Login today', objectiveType: 'LOGIN', targetValue: 1, difficulty: 'EASY', rewardPremium: 10, rewardCurrency: 100 },
  ];

  private readonly COMPLETION_BONUS = 150;

  private getTodayKey(): string {
    const today = new Date();
    return `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
  }

  private getDailyChallenges(): ChallengeTemplate[] {
    const todayKey = this.getTodayKey();
    const seed = todayKey.split('-').reduce((acc, num) => acc + parseInt(num), 0);
    
    const shuffled = [...this.CHALLENGE_TEMPLATES].sort((a, b) => {
      const hashA = (seed * a.id.charCodeAt(0)) % 100;
      const hashB = (seed * b.id.charCodeAt(0)) % 100;
      return hashA - hashB;
    });

    const easy = shuffled.find(c => c.difficulty === 'EASY')!;
    const medium = shuffled.find(c => c.difficulty === 'MEDIUM')!;
    const hard = shuffled.find(c => c.difficulty === 'HARD')!;

    return [easy, medium, hard];
  }

  async getPlayerChallenges(playerId: string): Promise<DailyChallengesResponse> {
    const cacheKey = `daily-challenges:${playerId}:${this.getTodayKey()}`;

    try {
      const cached = await redis.get(cacheKey);
      if (cached) {
        return JSON.parse(cached);
      }
    } catch (error) {
      logger.warn('Cache read failed for daily challenges', error);
    }

    const todayChallenges = this.getDailyChallenges();
    const todayKey = this.getTodayKey();

    const progressRecords = await prisma.economyTransaction.findMany({
      where: {
        playerId,
        type: 'SYSTEM_GIFT',
        metadata: { path: ['source'], string_contains: 'daily_challenge' },
        createdAt: { gte: new Date(todayKey), lt: new Date(new Date(todayKey).getTime() + 86400000) },
      },
    });

    const completedChallengeIds = new Set(
      progressRecords.map(r => (r.metadata as Record<string, unknown>)?.challengeId as string)
    );

    const challenges: PlayerChallenge[] = todayChallenges.map(template => {
      const isCompleted = completedChallengeIds.has(template.id);
      return {
        challengeId: template.id,
        title: template.title,
        description: template.description,
        difficulty: template.difficulty,
        progress: isCompleted ? template.targetValue : 0,
        targetValue: template.targetValue,
        completed: isCompleted,
        rewardPremium: template.rewardPremium,
        rewardCurrency: template.rewardCurrency,
      };
    });

    const allCompleted = challenges.every(c => c.completed);
    const bonusClaimed = progressRecords.some(r => (r.metadata as Record<string, unknown>)?.type === 'completion_bonus');

    const response: DailyChallengesResponse = {
      challenges,
      allCompleted,
      bonusAvailable: allCompleted && !bonusClaimed,
      bonusReward: this.COMPLETION_BONUS,
    };

    await redis.setex(cacheKey, 60, JSON.stringify(response));

    return response;
  }

  async completeChallenge(playerId: string, challengeId: string): Promise<{ success: boolean; rewards: { premium: number; currency: number } }> {
    const todayChallenges = this.getDailyChallenges();
    const challenge = todayChallenges.find(c => c.id === challengeId);

    if (!challenge) {
      throw new Error('Challenge not found or not available today');
    }

    const todayKey = this.getTodayKey();
    const existing = await prisma.economyTransaction.findFirst({
      where: {
        playerId,
        type: 'SYSTEM_GIFT',
        metadata: { path: ['challengeId'], equals: challengeId },
        createdAt: { gte: new Date(todayKey) },
      },
    });

    if (existing) {
      throw new Error('Challenge already completed');
    }

    await prisma.economyTransaction.create({
      data: {
        playerId,
        type: 'SYSTEM_GIFT',
        amount: challenge.rewardPremium,
        currency: 'premium',
        metadata: { source: 'daily_challenge', challengeId, type: 'challenge_reward' },
      },
    });

    if (challenge.rewardCurrency > 0) {
      await prisma.playerProfile.update({
        where: { playerId },
        data: { wealth: { increment: challenge.rewardCurrency } },
      });
    }

    await redis.del(`daily-challenges:${playerId}:${todayKey}`);

    logger.info(`Player ${playerId} completed challenge ${challengeId}`);

    return {
      success: true,
      rewards: { premium: challenge.rewardPremium, currency: challenge.rewardCurrency },
    };
  }

  async claimCompletionBonus(playerId: string): Promise<{ success: boolean; reward: number }> {
    const status = await this.getPlayerChallenges(playerId);

    if (!status.bonusAvailable) {
      throw new Error('Completion bonus not available');
    }

    await prisma.economyTransaction.create({
      data: {
        playerId,
        type: 'SYSTEM_GIFT',
        amount: this.COMPLETION_BONUS,
        currency: 'premium',
        metadata: { source: 'daily_challenge', type: 'completion_bonus' },
      },
    });

    await redis.del(`daily-challenges:${playerId}:${this.getTodayKey()}`);

    logger.info(`Player ${playerId} claimed daily challenge completion bonus`);

    return { success: true, reward: this.COMPLETION_BONUS };
  }

  async updateProgress(playerId: string, objectiveType: string, value: number): Promise<void> {
    logger.debug(`Progress update for ${playerId}: ${objectiveType} +${value}`);
  }
}

export const dailyChallengeService = new DailyChallengeService();
