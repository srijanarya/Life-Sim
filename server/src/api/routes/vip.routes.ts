import { Router, Request, Response } from 'express';
import { z } from 'zod';
import { logger } from '@config/logger';
import { vipService } from '@services/vip.service';
import { authenticateToken, requireVip } from '@middleware/auth';

const router = Router();

const activateSchema = z.object({
  tier: z.enum(['VIP_MONTHLY', 'VIP_YEARLY']),
  receiptData: z.string().min(10),
});

router.get('/status', authenticateToken, async (req: Request, res: Response) => {
  try {
    const status = await vipService.getVipStatus(req.player!.id);
    res.json(status);
  } catch (error) {
    logger.error('Get VIP status error:', error);
    res.status(500).json({ error: 'Failed to get VIP status' });
  }
});

router.get('/tiers', async (req: Request, res: Response) => {
  try {
    const tiers = vipService.getAvailableTiers();
    res.json(tiers);
  } catch (error) {
    logger.error('Get VIP tiers error:', error);
    res.status(500).json({ error: 'Failed to get VIP tiers' });
  }
});

router.post('/activate', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = activateSchema.parse(req.body);
    const result = await vipService.activateSubscription(req.player!.id, data.tier, data.receiptData);
    res.json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error && error.message.includes('Invalid')) {
      return res.status(400).json({ error: error.message });
    }
    logger.error('VIP activation error:', error);
    res.status(500).json({ error: 'Failed to activate VIP' });
  }
});

router.post('/cancel', authenticateToken, async (req: Request, res: Response) => {
  try {
    const result = await vipService.cancelSubscription(req.player!.id);
    res.json(result);
  } catch (error) {
    if (error instanceof Error && error.message.includes('No active')) {
      return res.status(400).json({ error: error.message });
    }
    logger.error('VIP cancellation error:', error);
    res.status(500).json({ error: 'Failed to cancel VIP' });
  }
});

export default router;
