import { Router, Request, Response } from 'express';
import { z } from 'zod';
import { logger } from '@config/logger';
import { dailyChallengeService } from '@services/daily-challenge.service';
import { authenticateToken } from '@middleware/auth';

const router = Router();

const completeSchema = z.object({
  challengeId: z.string(),
});

router.get('/', authenticateToken, async (req: Request, res: Response) => {
  try {
    const challenges = await dailyChallengeService.getPlayerChallenges(req.player!.id);
    res.json(challenges);
  } catch (error) {
    logger.error('Get daily challenges error:', error);
    res.status(500).json({ error: 'Failed to get daily challenges' });
  }
});

router.post('/complete', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = completeSchema.parse(req.body);
    const result = await dailyChallengeService.completeChallenge(req.player!.id, data.challengeId);
    res.json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error && error.message.includes('already completed')) {
      return res.status(400).json({ error: error.message });
    }
    logger.error('Complete challenge error:', error);
    res.status(500).json({ error: 'Failed to complete challenge' });
  }
});

router.post('/claim-bonus', authenticateToken, async (req: Request, res: Response) => {
  try {
    const result = await dailyChallengeService.claimCompletionBonus(req.player!.id);
    res.json(result);
  } catch (error) {
    if (error instanceof Error && error.message.includes('not available')) {
      return res.status(400).json({ error: error.message });
    }
    logger.error('Claim completion bonus error:', error);
    res.status(500).json({ error: 'Failed to claim bonus' });
  }
});

export default router;
