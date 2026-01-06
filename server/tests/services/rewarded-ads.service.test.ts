import { RewardedAdsService } from '../../src/services/rewarded-ads.service';

describe('RewardedAdsService', () => {
  let service: RewardedAdsService;

  beforeEach(() => {
    service = new RewardedAdsService();
  });

  describe('getMaxAdsForPlayer', () => {
    it('should return 3 ads for FREE players', () => {
      const max = service['getMaxAdsForPlayer']('FREE');
      expect(max).toBe(3);
    });

    it('should return 8 ads for VIP_MONTHLY players', () => {
      const max = service['getMaxAdsForPlayer']('VIP_MONTHLY');
      expect(max).toBe(8);
    });

    it('should return 13 ads for VIP_YEARLY players', () => {
      const max = service['getMaxAdsForPlayer']('VIP_YEARLY');
      expect(max).toBe(13);
    });
  });

  describe('getAdTypes', () => {
    it('should return all ad types with rewards', () => {
      const types = service.getAdTypes();
      
      expect(types).toHaveProperty('DAILY_AD');
      expect(types).toHaveProperty('EXTRA_LIFE');
      expect(types).toHaveProperty('BOOST_STATS');
      expect(types.DAILY_AD).toHaveProperty('premium');
      expect(types.DAILY_AD).toHaveProperty('gameCurrency');
    });
  });
});
