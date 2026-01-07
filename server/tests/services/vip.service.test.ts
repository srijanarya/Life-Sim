import { VipService } from '../../src/services/vip.service';

describe('VipService', () => {
  let service: VipService;

  beforeEach(() => {
    service = new VipService();
  });

  describe('getAvailableTiers', () => {
    it('should return VIP_MONTHLY and VIP_YEARLY tiers', () => {
      const tiers = service.getAvailableTiers();
      
      expect(tiers).toHaveProperty('VIP_MONTHLY');
      expect(tiers).toHaveProperty('VIP_YEARLY');
      expect(tiers.VIP_MONTHLY).toHaveProperty('price');
      expect(tiers.VIP_MONTHLY).toHaveProperty('duration');
      expect(tiers.VIP_MONTHLY).toHaveProperty('benefits');
    });

    it('should have correct pricing', () => {
      const tiers = service.getAvailableTiers();
      
      expect(tiers.VIP_MONTHLY.price).toBe(999);
      expect(tiers.VIP_MONTHLY.duration).toBe(30);
      expect(tiers.VIP_YEARLY.price).toBe(7999);
      expect(tiers.VIP_YEARLY.duration).toBe(365);
    });
  });

  describe('getBenefitsForTier', () => {
    it('should return benefits for VIP_MONTHLY', () => {
      const benefits = service.getBenefitsForTier('VIP_MONTHLY');
      
      expect(benefits.adsRemoved).toBe(true);
      expect(benefits.dailyBonusMultiplier).toBe(2);
      expect(benefits.exclusiveEvents).toBe(true);
      expect(benefits.prioritySupport).toBe(true);
      expect(benefits.extraDailyAds).toBe(5);
      expect(benefits.iapDiscount).toBeUndefined();
    });

    it('should return benefits for VIP_YEARLY', () => {
      const benefits = service.getBenefitsForTier('VIP_YEARLY');
      
      expect(benefits.adsRemoved).toBe(true);
      expect(benefits.dailyBonusMultiplier).toBe(3);
      expect(benefits.iapDiscount).toBe(0.2);
    });

    it('should return default benefits for FREE tier', () => {
      const benefits = service.getBenefitsForTier('FREE');
      
      expect(benefits.adsRemoved).toBe(false);
      expect(benefits.dailyBonusMultiplier).toBe(1);
      expect(benefits.extraDailyAds).toBe(0);
    });
  });
});
