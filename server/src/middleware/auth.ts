import { Request, Response, NextFunction } from 'express';
import jwt from 'jsonwebtoken';
import { config } from '@config/index';
import { logger } from '@config/logger';
import { prisma } from '@config/database';

declare global {
  namespace Express {
    interface Request {
      player?: {
        id: string;
        email: string;
        username: string;
        subscriptionTier: string;
      };
    }
  }
}

export interface JwtPayload {
  playerId: string;
  email: string;
  username: string;
  subscriptionTier: string;
  iat?: number;
  exp?: number;
}

export function generateToken(payload: Omit<JwtPayload, 'iat' | 'exp'>): string {
  return jwt.sign(payload, config.jwt.secret, {
    expiresIn: config.jwt.expiresIn,
  });
}

export function verifyToken(token: string): JwtPayload {
  return jwt.verify(token, config.jwt.secret) as JwtPayload;
}

export async function authenticateToken(
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> {
  try {
    const authHeader = req.headers.authorization;
    const token = authHeader?.split(' ')[1];

    if (!token) {
      res.status(401).json({ error: 'Authentication required' });
      return;
    }

    const payload = verifyToken(token);

    const player = await prisma.player.findUnique({
      where: { id: payload.playerId },
      select: {
        id: true,
        email: true,
        username: true,
        status: true,
        subscriptionTier: true,
      },
    });

    if (!player) {
      res.status(401).json({ error: 'Player not found' });
      return;
    }

    if (player.status !== 'ACTIVE') {
      res.status(403).json({ error: 'Account suspended or banned' });
      return;
    }

    req.player = {
      id: player.id,
      email: player.email,
      username: player.username,
      subscriptionTier: player.subscriptionTier,
    };

    next();
  } catch (error) {
    if (error instanceof jwt.TokenExpiredError) {
      res.status(401).json({ error: 'Token expired' });
      return;
    }
    if (error instanceof jwt.JsonWebTokenError) {
      res.status(401).json({ error: 'Invalid token' });
      return;
    }
    logger.error('Authentication error:', error);
    res.status(500).json({ error: 'Authentication failed' });
  }
}

export async function optionalAuth(
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> {
  try {
    const authHeader = req.headers.authorization;
    const token = authHeader?.split(' ')[1];

    if (token) {
      const payload = verifyToken(token);
      const player = await prisma.player.findUnique({
        where: { id: payload.playerId },
        select: {
          id: true,
          email: true,
          username: true,
          status: true,
          subscriptionTier: true,
        },
      });

      if (player && player.status === 'ACTIVE') {
        req.player = {
          id: player.id,
          email: player.email,
          username: player.username,
          subscriptionTier: player.subscriptionTier,
        };
      }
    }

    next();
  } catch {
    next();
  }
}

export async function requireVip(
  req: Request,
  res: Response,
  next: NextFunction
): Promise<void> {
  if (!req.player) {
    res.status(401).json({ error: 'Authentication required' });
    return;
  }

  if (req.player.subscriptionTier === 'FREE') {
    res.status(403).json({ error: 'VIP subscription required' });
    return;
  }

  next();
}
