import { PlayerService } from '../src/services/player.service';

describe('PlayerService', () => {
  let playerService: PlayerService;

  beforeEach(() => {
    playerService = new PlayerService();
  });

  describe('createPlayer', () => {
    it('should create a new player with profile', async () => {
      const data = {
        email: 'test@example.com',
        username: 'testplayer',
        passwordHash: 'hashedpassword',
      };

      const player = await playerService.createPlayer(data);

      expect(player).toBeDefined();
      expect(player.email).toBe(data.email);
      expect(player.username).toBe(data.username);
      expect(player.profile).toBeDefined();
    });
  });

  describe('incrementStat', () => {
    it('should increment player stat', async () => {
      const playerId = 'test-player-id';

      const profile = await playerService.incrementStat(playerId, 'health', 10);

      expect(profile).toBeDefined();
      expect(profile.health).toBeGreaterThan(0);
    });
  });
});
