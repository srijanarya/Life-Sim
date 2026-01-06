import Redis from 'ioredis';
import { config } from './index';

class RedisClient {
  private client: Redis | null = null;

  public connect(): Redis {
    if (this.client) {
      return this.client;
    }

    this.client = new Redis(config.redis.url, {
      maxRetriesPerRequest: 3,
      retryDelayOnFailover: 100,
      lazyConnect: true,
    });

    this.client.on('connect', () => {
      console.log('Redis connected successfully');
    });

    this.client.on('error', (err) => {
      console.error('Redis connection error:', err);
    });

    return this.client;
  }

  public async disconnect(): Promise<void> {
    if (this.client) {
      await this.client.quit();
      this.client = null;
      console.log('Redis disconnected');
    }
  }

  public getClient(): Redis {
    if (!this.client) {
      throw new Error('Redis client not connected. Call connect() first.');
    }
    return this.client;
  }
}

export const redisClient = new RedisClient();
export const redis = redisClient.connect();
