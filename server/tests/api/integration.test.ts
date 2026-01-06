import request from 'supertest';
import app from '../src/index';

const TEST_PLAYER = {
  id: 'test-player-id',
  email: 'test@example.com',
  username: 'testplayer',
  password: 'SecurePass123!',
};

let authToken: string;

describe('API Integration Tests', () => {
  beforeAll(async () => {
    const response = await request(app)
      .post('/api/auth/register')
      .send(TEST_PLAYER);
    
    authToken = response.body.token;
  });

  describe('Authentication', () => {
    it('should register new player', async () => {
      const newPlayer = {
        email: `new-${Date.now()}@example.com`,
        username: `newuser${Date.now()}`,
        password: 'AnotherPass123!',
      };

      const response = await request(app)
        .post('/api/auth/register')
        .send(newPlayer)
        .expect(201);

      expect(response.body).toHaveProperty('token');
      expect(response.body).toHaveProperty('player');
    });

    it('should not allow duplicate email', async () => {
      const response = await request(app)
        .post('/api/auth/register')
        .send(TEST_PLAYER)
        .expect(409);

      expect(response.body.error).toContain('already registered');
    });

    it('should login with valid credentials', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: TEST_PLAYER.email,
          password: TEST_PLAYER.password,
        })
        .expect(200);

      expect(response.body).toHaveProperty('token');
    });

    it('should reject invalid credentials', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: TEST_PLAYER.email,
          password: 'wrongpassword',
        })
        .expect(401);

      expect(response.body.error).toContain('Invalid');
    });

    it('should require authentication for protected routes', async () => {
      const response = await request(app)
        .get('/api/vip/status')
        .expect(401);

      expect(response.body.error).toContain('required');
    });
  });

  describe('VIP Status', () => {
    it('should get VIP status with auth', async () => {
      const response = await request(app)
        .get('/api/vip/status')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body).toHaveProperty('tier');
      expect(response.body).toHaveProperty('isActive');
      expect(response.body).toHaveProperty('benefits');
    });

    it('should get available VIP tiers', async () => {
      const response = await request(app)
        .get('/api/vip/tiers')
        .expect(200);

      expect(response.body).toHaveProperty('VIP_MONTHLY');
      expect(response.body).toHaveProperty('VIP_YEARLY');
    });
  });

  describe('Leaderboards', () => {
    it('should get leaderboard', async () => {
      const response = await request(app)
        .get('/api/leaderboard/')
        .expect(200);

      expect(response.body).toHaveProperty('entries');
      expect(response.body).toHaveProperty('totalPages');
      expect(response.body).toHaveProperty('currentPage');
      expect(Array.isArray(response.body.entries)).toBe(true);
    });

    it('should get top players', async () => {
      const response = await request(app)
        .get('/api/leaderboard/top?count=10')
        .expect(200);

      expect(response.body.players).toBeDefined();
      expect(response.body.players.length).toBeLessThanOrEqual(10);
    });
  });

  describe('Daily Challenges', () => {
    it('should get daily challenges with auth', async () => {
      const response = await request(app)
        .get('/api/daily-challenges/')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body).toHaveProperty('challenges');
      expect(response.body).toHaveProperty('allCompleted');
      expect(response.body).toHaveProperty('bonusAvailable');
      expect(Array.isArray(response.body.challenges)).toBe(true);
      expect(response.body.challenges.length).toBe(3);
    });

    it('should not allow accessing challenges without auth', async () => {
      const response = await request(app)
        .get('/api/daily-challenges/')
        .expect(401);

      expect(response.body.error).toContain('required');
    });
  });

  describe('Milestones', () => {
    it('should get all milestones with auth', async () => {
      const response = await request(app)
        .get('/api/milestones/')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body).toHaveProperty('milestones');
      expect(response.body).toHaveProperty('unlocked');
      expect(Array.isArray(response.body.milestones)).toBe(true);
    });

    it('should get unlocked milestones only', async () => {
      const response = await request(app)
        .get('/api/milestones/unlocked')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body.milestones).toBeDefined();
    });
  });

  describe('Rewarded Ads', () => {
    it('should get ad types with auth', async () => {
      const response = await request(app)
        .get('/api/economy/rewards/types')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body).toHaveProperty('DAILY_AD');
      expect(response.body).toHaveProperty('EXTRA_LIFE');
      expect(response.body).toHaveProperty('BOOST_STATS');
    });

    it('should get ad status with auth', async () => {
      const response = await request(app)
        .get('/api/economy/rewards/ad-status')
        .set('Authorization', `Bearer ${authToken}`)
        .expect(200);

      expect(response.body).toHaveProperty('adsWatchedToday');
      expect(response.body).toHaveProperty('adsRemaining');
      expect(response.body).toHaveProperty('canWatchAd');
      expect(response.body).toHaveProperty('nextResetTime');
    });
  });
});
