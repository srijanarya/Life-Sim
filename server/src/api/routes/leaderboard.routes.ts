import { Router, Request, Response } from 'express';
import { z } from 'zod';
import { logger } from '@config/logger';
import { leaderboardService } from '@services/leaderboard.service';
import { authenticateToken, optionalAuth } from '@middleware/auth';

const router = Router();

router.get('/', optionalAuth, async (req: Request, res: Response) => {
  try {
    const page = parseInt(req.query.page as string) || 1;
    const limit = Math.min(parseInt(req.query.limit as string) || 50, 100);
    
    const result = await leaderboardService.getLeaderboard(page, limit);
    res.json(result);
  } catch (error) {
    logger.error('Get leaderboard error:', error);
    res.status(500).json({ error: 'Failed to get leaderboard' });
  }
});

router.get('/top', async (req: Request, res: Response) => {
  try {
    const count = Math.min(parseInt(req.query.count as string) || 10, 50);
    const topPlayers = await leaderboardService.getTopPlayers(count);
    res.json({ players: topPlayers });
  } catch (error) {
    logger.error('Get top players error:', error);
    res.status(500).json({ error: 'Failed to get top players' });
  }
});

router.get('/my-rank', authenticateToken, async (req: Request, res: Response) => {
  try {
    const rank = await leaderboardService.getPlayerRank(req.player!.id);
    res.json(rank);
  } catch (error) {
    logger.error('Get player rank error:', error);
    res.status(500).json({ error: 'Failed to get player rank' });
  }
});

router.post('/update', authenticateToken, async (req: Request, res: Response) => {
  try {
    const score = await leaderboardService.updatePlayerScore(req.player!.id);
    res.json({ success: true, score });
  } catch (error) {
    logger.error('Update leaderboard score error:', error);
    res.status(500).json({ error: 'Failed to update score' });
  }
});

export default router;
