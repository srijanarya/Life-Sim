import { LeaderboardService } from '../src/services/leaderboard.service';

describe('LeaderboardService', () => {
  let service: LeaderboardService;

  beforeEach(() => {
    service = new LeaderboardService();
  });

  describe('calculateScore', () => {
    const profile = {
      wealth: 1000,
      totalPlaytime: 1000,
      intelligence: 50,
      charisma: 50,
      physical: 50,
      creativity: 50,
      gamesPlayed: 10,
    };

    it('should calculate score correctly', () => {
      const score = service['calculateScore'](profile);
      
      expect(score).toBe(
        1000 * 1 +
        1000 * 0.1 +
        50 * 10 * 4 +
        10 * 50
      );
    });

    it('should give weight to gamesPlayed', () => {
      const profile = {
        wealth: 0,
        totalPlaytime: 0,
        intelligence: 0,
        charisma: 0,
        physical: 0,
        creativity: 0,
        gamesPlayed: 10,
      };

      const score = service['calculateScore'](profile);
      expect(score).toBe(500);
    });

    it('should give weight to stats', () => {
      const profile = {
        wealth: 0,
        totalPlaytime: 0,
        intelligence: 100,
        charisma: 100,
        physical: 100,
        creativity: 100,
        gamesPlayed: 0,
      };

      const score = service['calculateScore'](profile);
      expect(score).toBe(4000);
    });
  });

  describe('getTopPlayers', () => {
    it('should limit to 10 players by default', async () => {
      const result = await service.getTopPlayers(10);
      
      expect(result.length).toBeLessThanOrEqual(10);
    });

    it('should sort by score descending', async () => {
      const result = await service.getTopPlayers(10);
      
      if (result.length > 1) {
        for (let i = 0; i < result.length - 1; i++) {
          expect(result[i].score).toBeGreaterThanOrEqual(result[i + 1].score);
        }
      }
    });
  });
});
