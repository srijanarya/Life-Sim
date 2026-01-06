import dotenv from 'dotenv';
import { z } from 'zod';

dotenv.config();

const envSchema = z.object({
  NODE_ENV: z.enum(['development', 'production', 'test']).default('development'),
  PORT: z.string().transform(Number).default('3000'),
  HOST: z.string().default('localhost'),
  
  DATABASE_URL: z.string(),
  REDIS_URL: z.string().default('redis://localhost:6379'),
  
  JWT_SECRET: z.string().min(32),
  JWT_EXPIRES_IN: z.string().default('7d'),
  
  RATE_LIMIT_WINDOW_MS: z.string().transform(Number).default('900000'),
  RATE_LIMIT_MAX_REQUESTS: z.string().transform(Number).default('100'),
  
  GAME_TICK_INTERVAL_MS: z.string().transform(Number).default('1000'),
  EVENT_COOLDOWN_MS: z.string().transform(Number).default('5000'),
});

const parsed = envSchema.safeParse(process.env);

if (!parsed.success) {
  console.error('Invalid environment variables:', parsed.error.flatten().fieldErrors);
  process.exit(1);
}

export const config = {
  env: parsed.data.NODE_ENV,
  isDev: parsed.data.NODE_ENV === 'development',
  isProd: parsed.data.NODE_ENV === 'production',
  isTest: parsed.data.NODE_ENV === 'test',
  
  server: {
    port: parsed.data.PORT,
    host: parsed.data.HOST,
  },
  
  database: {
    url: parsed.data.DATABASE_URL,
  },
  
  redis: {
    url: parsed.data.REDIS_URL,
  },
  
  jwt: {
    secret: parsed.data.JWT_SECRET,
    expiresIn: parsed.data.JWT_EXPIRES_IN,
  },
  
  rateLimit: {
    windowMs: parsed.data.RATE_LIMIT_WINDOW_MS,
    maxRequests: parsed.data.RATE_LIMIT_MAX_REQUESTS,
  },
  
  game: {
    tickIntervalMs: parsed.data.GAME_TICK_INTERVAL_MS,
    eventCooldownMs: parsed.data.EVENT_COOLDOWN_MS,
  },
} as const;

export type Config = typeof config;
