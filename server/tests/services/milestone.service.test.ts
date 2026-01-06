import { MilestoneService } from '../src/services/milestone.service';

describe('MilestoneService', () => {
  let service: MilestoneService;

  beforeEach(() => {
    service = new MilestoneService();
  });

  describe('MILESTONES constant', () => {
    it('should have 19 defined milestones', () => {
      const milestones = service['MILESTONES'];
      
      expect(Object.keys(milestones).length).toBe(19);
    });

    it('should include age milestones', () => {
      const milestones = service['MILESTONES'];
      
      expect(milestones).toHaveProperty('AGE_18');
      expect(milestones).toHaveProperty('AGE_21');
      expect(milestones).toHaveProperty('AGE_30');
      expect(milestones).toHaveProperty('AGE_40');
      expect(milestones).toHaveProperty('AGE_50');
    });

    it('should include wealth milestones', () => {
      const milestones = service['MILESTONES'];
      
      expect(milestones).toHaveProperty('MILLIONAIRE');
      expect(milestones).toHaveProperty('MULTI_MILLIONAIRE');
    });

    it('should have positive rewards', () => {
      const milestones = service['MILESTONES'];
      
      Object.values(milestones).forEach(milestone => {
        expect(milestone.rewardPremium).toBeGreaterThan(0);
      });
    });
  });

  describe('COMPLETION_BONUS', () => {
    it('should be positive', () => {
      const bonus = service['COMPLETION_BONUS'];
      expect(bonus).toBe(150);
    });
  });
});
