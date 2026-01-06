import { AvatarService } from '../src/services/avatar.service';

describe('AvatarService', () => {
  let service: AvatarService;

  beforeEach(() => {
    service = new AvatarService();
  });

  describe('AVATAR_CATEGORIES', () => {
    it('should have 4 categories', () => {
      const categories = service['AVATAR_CATEGORIES'];
      
      expect(categories).toContain('HAIR');
      expect(categories).toContain('FACE');
      expect(categories).toContain('OUTFIT');
      expect(categories).toContain('ACCESSORY');
      expect(categories.length).toBe(4);
    });
  });

  describe('RARITY_WEIGHTS', () => {
    it('should sum to 100', () => {
      const weights = service['RARITY_WEIGHTS'];
      const total = Object.values(weights).reduce((sum, weight) => sum + weight, 0);
      
      expect(total).toBe(100);
    });

    it('should have descending rarity', () => {
      const weights = service['RARITY_WEIGHTS'];
      
      expect(weights.COMMON).toBeGreaterThan(weights.UNCOMMON);
      expect(weights.UNCOMMON).toBeGreaterThan(weights.RARE);
      expect(weights.RARE).toBeGreaterThan(weights.EPIC);
      expect(weights.EPIC).toBeGreaterThan(weights.LEGENDARY);
    });
  });

  describe('getStarterAvatars', () => {
    it('should return items with price 0 or isPremiumOnly false', async () => {
      const starters = await service['getStarterAvatars']('player-id');
      
      starters.forEach(starter => {
        const isFree = starter.price === 0 || starter.isPremiumOnly === false;
        expect(isFree).toBe(true);
      });
    });
  });
});
