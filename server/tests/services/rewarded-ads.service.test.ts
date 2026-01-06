import { RewardedAdsService } from '../src/services/rewarded-ads.service';
import { avatarService } from '../src/services/avatar.service';

describe('RewardedAdsService', () => {
  let service: RewardedAdsService;

  beforeEach(() => {
    service = new RewardedAdsService();
  });

  afterEach(() => {
    jest.clearAllMocks();
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

    it('should default to 3 ads for unknown tiers', () => {
      const max = service['getMaxAdsForPlayer']('UNKNOWN');
      expect(max).toBe(3);
    });
  });

  describe('getAdTypes', () => {
    it('should return all ad types with rewards', () => {
      const types = service.getAdTypes();
      
      expect(types).toHaveProperty('DAILY_AD');
      expect(types).toHaveProperty('EXTRA_LIFE');
      expect(types).toHaveProperty('BOOST_STATS');
      expect(types).toHaveProperty('DOUBLE_REWARD');
      expect(types).toHaveProperty('SKIP_WAIT');
      
      expect(types.DAILY_AD).toHaveProperty('premium');
      expect(types.DAILY_AD).toHaveProperty('gameCurrency');
      expect(types.DAILY_AD.premium).toBeGreaterThan(0);
    });

    it('should have positive reward amounts', () => {
      const types = service.getAdTypes();
      
      Object.values(types).forEach(type => {
        if (type.premium) expect(type.premium).toBeGreaterThan(0);
        if (type.gameCurrency) expect(type.gameCurrency).toBeGreaterThan(0);
      });
    });
  });
});
