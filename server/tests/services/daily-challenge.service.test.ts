import { DailyChallengeService } from '../src/services/daily-challenge.service';

describe('DailyChallengeService', () => {
  let service: DailyChallengeService;

  beforeEach(() => {
    service = new DailyChallengeService();
  });

  describe('getDailyChallenges', () => {
    it('should return exactly 3 challenges', () => {
      const challenges = service['getDailyChallenges']();
      
      expect(challenges.length).toBe(3);
    });

    it('should include one of each difficulty', () => {
      const challenges = service['getDailyChallenges']();
      
      const difficulties = challenges.map(c => c.difficulty);
      expect(difficulties).toContain('EASY');
      expect(difficulties).toContain('MEDIUM');
      expect(difficulties).toContain('HARD');
    });

    it('should have positive rewards', () => {
      const challenges = service['getDailyChallenges']();
      
      challenges.forEach(challenge => {
        expect(challenge.rewardPremium).toBeGreaterThan(0);
        expect(challenge.rewardCurrency).toBeGreaterThan(0);
      });
    });
  });

  describe('COMPLETION_BONUS', () => {
    it('should be positive', () => {
      expect(service['COMPLETION_BONUS']).toBe(150);
    });
  });
});
