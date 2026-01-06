import { Router, Request, Response } from 'express';
import { playerService } from '@services/player.service';
import { z } from 'zod';
import { logger } from '@config/logger';

const router = Router();

const registerSchema = z.object({
  email: z.string().email(),
  username: z.string().min(3).max(20),
  password: z.string().min(8),
});

const loginSchema = z.object({
  email: z.string().email(),
  password: z.string(),
});

router.post('/register', async (req: Request, res: Response) => {
  try {
    const data = registerSchema.parse(req.body);

    const existingPlayer = await playerService.getPlayerByEmail(data.email);
    if (existingPlayer) {
      return res.status(409).json({ error: 'Email already registered' });
    }

    const player = await playerService.createPlayer({
      email: data.email,
      username: data.username,
      passwordHash: data.password,
    });

    res.status(201).json({
      id: player.id,
      email: player.email,
      username: player.username,
      createdAt: player.createdAt,
    });
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    logger.error('Registration error:', error);
    res.status(500).json({ error: 'Registration failed' });
  }
});

router.post('/login', async (req: Request, res: Response) => {
  try {
    const data = loginSchema.parse(req.body);

    const player = await playerService.getPlayerByEmail(data.email);
    if (!player) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    res.json({
      id: player.id,
      email: player.email,
      username: player.username,
      subscriptionTier: player.subscriptionTier,
    });
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    logger.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
});

export default router;
