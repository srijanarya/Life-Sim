import { Router, Request, Response } from 'express';
import { z } from 'zod';
import { logger } from '@config/logger';
import { authService } from '@services/auth.service';
import { authenticateToken } from '@middleware/auth';

const router = Router();

const registerSchema = z.object({
  email: z.string().email(),
  username: z.string().min(3).max(20).regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  password: z.string().min(8).max(100),
});

const loginSchema = z.object({
  email: z.string().email(),
  password: z.string(),
});

const changePasswordSchema = z.object({
  currentPassword: z.string(),
  newPassword: z.string().min(8).max(100),
});

router.post('/register', async (req: Request, res: Response) => {
  try {
    const data = registerSchema.parse(req.body);
    const result = await authService.register(data);

    res.status(201).json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error) {
      if (error.message.includes('already')) {
        return res.status(409).json({ error: error.message });
      }
    }
    logger.error('Registration error:', error);
    res.status(500).json({ error: 'Registration failed' });
  }
});

router.post('/login', async (req: Request, res: Response) => {
  try {
    const data = loginSchema.parse(req.body);
    const result = await authService.login(data);

    res.json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error) {
      if (error.message.includes('Invalid') || error.message.includes('suspended')) {
        return res.status(401).json({ error: error.message });
      }
    }
    logger.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
});

router.post('/refresh', authenticateToken, async (req: Request, res: Response) => {
  try {
    const result = await authService.refreshToken(req.player!.id);
    res.json(result);
  } catch (error) {
    logger.error('Token refresh error:', error);
    res.status(500).json({ error: 'Token refresh failed' });
  }
});

router.post('/change-password', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = changePasswordSchema.parse(req.body);
    await authService.changePassword(req.player!.id, data.currentPassword, data.newPassword);
    res.json({ message: 'Password changed successfully' });
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error && error.message.includes('incorrect')) {
      return res.status(401).json({ error: error.message });
    }
    logger.error('Change password error:', error);
    res.status(500).json({ error: 'Password change failed' });
  }
});

router.get('/me', authenticateToken, async (req: Request, res: Response) => {
  res.json(req.player);
});

export default router;
