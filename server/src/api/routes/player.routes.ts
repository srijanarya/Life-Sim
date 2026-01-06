import { Router, Request, Response } from 'express';
import { playerService } from '@services/player.service';
import { logger } from '@config/logger';

const router = Router();

router.get('/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const player = await playerService.getPlayerById(id);

    if (!player) {
      return res.status(404).json({ error: 'Player not found' });
    }

    res.json(player);
  } catch (error) {
    logger.error('Failed to get player:', error);
    res.status(500).json({ error: 'Failed to fetch player' });
  }
});

router.patch('/:id/profile', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const updates = req.body;

    const profile = await playerService.updateProfile(id, updates);
    res.json(profile);
  } catch (error) {
    logger.error('Failed to update profile:', error);
    res.status(500).json({ error: 'Failed to update profile' });
  }
});

router.get('/:id/achievements', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const achievements = await playerService.checkAchievements(id);
    res.json(achievements);
  } catch (error) {
    logger.error('Failed to get achievements:', error);
    res.status(500).json({ error: 'Failed to fetch achievements' });
  }
});

export default router;
