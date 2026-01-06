import { Router, Request, Response } from 'express';
import { logger } from '@config/logger';
import { milestoneService } from '@services/milestone.service';
import { authenticateToken } from '@middleware/auth';

const router = Router();

router.get('/', authenticateToken, async (req: Request, res: Response) => {
  try {
    const result = await milestoneService.getAllMilestones(req.player!.id);
    res.json(result);
  } catch (error) {
    logger.error('Get milestones error:', error);
    res.status(500).json({ error: 'Failed to get milestones' });
  }
});

router.get('/unlocked', authenticateToken, async (req: Request, res: Response) => {
  try {
    const milestones = await milestoneService.getUnlockedMilestones(req.player!.id);
    res.json({ milestones });
  } catch (error) {
    logger.error('Get unlocked milestones error:', error);
    res.status(500).json({ error: 'Failed to get unlocked milestones' });
  }
});

router.post('/check', authenticateToken, async (req: Request, res: Response) => {
  try {
    const { gameId } = req.body;
    
    if (!gameId) {
      return res.status(400).json({ error: 'gameId is required' });
    }

    const newlyUnlocked = await milestoneService.checkMilestones(gameId);
    res.json({ newlyUnlocked, count: newlyUnlocked.length });
  } catch (error) {
    logger.error('Check milestones error:', error);
    res.status(500).json({ error: 'Failed to check milestones' });
  }
});

export default router;
