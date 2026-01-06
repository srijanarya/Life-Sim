import express, { Request, Response, NextFunction } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import rateLimit from 'express-rate-limit';
import { config } from '@config/index';
import { logger } from '@config/logger';
import { connectDatabase } from '@config/database';
import { redisClient } from '@config/redis';

import authRoutes from './api/routes/auth.routes';
import playerRoutes from './api/routes/player.routes';
import gameRoutes from './api/routes/game.routes';
import eventRoutes from './api/routes/event.routes';
import economyRoutes from './api/routes/economy.routes';
import vipRoutes from './api/routes/vip.routes';
import leaderboardRoutes from './api/routes/leaderboard.routes';
import dailyChallengeRoutes from './api/routes/daily-challenge.routes';
import milestoneRoutes from './api/routes/milestone.routes';

const app = express();

app.use(helmet());

app.use(cors({
  origin: config.isDev ? '*' : ['https://lifecraft.app', 'https://api.lifecraft.app'],
  credentials: true,
}));

const limiter = rateLimit({
  windowMs: config.rateLimit.windowMs,
  max: config.rateLimit.maxRequests,
  message: 'Too many requests from this IP, please try again later.',
});

app.use('/api/', limiter);

app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

app.use((req: Request, res: Response, next: NextFunction) => {
  logger.info(`${req.method} ${req.path}`);
  next();
});

app.get('/health', (req: Request, res: Response) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

app.use('/api/auth', authRoutes);
app.use('/api/player', playerRoutes);
app.use('/api/game', gameRoutes);
app.use('/api/events', eventRoutes);
app.use('/api/economy', economyRoutes);
app.use('/api/vip', vipRoutes);
app.use('/api/leaderboard', leaderboardRoutes);
app.use('/api/daily-challenges', dailyChallengeRoutes);
app.use('/api/milestones', milestoneRoutes);

app.use((req: Request, res: Response) => {
  res.status(404).json({ error: 'Not found' });
});

app.use((err: Error, req: Request, res: Response, next: NextFunction) => {
  logger.error('Unhandled error:', err);
  res.status(500).json({ error: 'Internal server error' });
});

async function startServer() {
  try {
    await connectDatabase();
    await redisClient.connect();

    app.listen(config.server.port, config.server.host, () => {
      logger.info(
        `Server running on http://${config.server.host}:${config.server.port}`
      );
    });
  } catch (error) {
    logger.error('Failed to start server:', error);
    process.exit(1);
  }
}

if (require.main === module) {
  startServer();
}

export default app;
