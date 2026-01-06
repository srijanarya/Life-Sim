import { Router, Request, Response } from 'express';
import { prisma } from '@config/database';
import { playerService } from '@services/player.service';
import { rewardedAdsService } from '@services/rewarded-ads.service';
import { logger } from '@config/logger';
import { authenticateToken } from '@middleware/auth';
import { z } from 'zod';

const router = Router();

router.post('/transactions', async (req: Request, res: Response) => {
  try {
    const { playerId, type, amount, currency, receiptId, productId, metadata } =
      req.body;

    const transaction = await prisma.economyTransaction.create({
      data: {
        playerId,
        type,
        amount,
        currency,
        receiptId,
        productId,
        metadata,
      },
    });

    res.status(201).json(transaction);
  } catch (error) {
    logger.error('Failed to create transaction:', error);
    res.status(500).json({ error: 'Failed to process transaction' });
  }
});

router.get('/players/:playerId/transactions', async (req: Request, res: Response) => {
  try {
    const { playerId } = req.params;
    const transactions = await prisma.economyTransaction.findMany({
      where: { playerId },
      orderBy: { createdAt: 'desc' },
    });

    res.json(transactions);
  } catch (error) {
    logger.error('Failed to get transactions:', error);
    res.status(500).json({ error: 'Failed to fetch transactions' });
  }
});

router.post('/iap/verify', async (req: Request, res: Response) => {
  try {
    const { playerId, receiptData } = req.body;

    logger.info(`Verifying IAP for player ${playerId}`);

    await prisma.economyTransaction.create({
      data: {
        playerId,
        type: 'IAP_PURCHASE',
        amount: 1000,
        currency: 'premium',
        receiptId: receiptData.transactionId,
        productId: receiptData.productId,
        metadata: receiptData,
      },
    });

    res.json({ verified: true });
  } catch (error) {
    logger.error('IAP verification failed:', error);
    res.status(500).json({ error: 'IAP verification failed' });
  }
});

const adWatchSchema = z.object({
  adType: z.string(),
});

router.post('/rewards/ad-watch', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = adWatchSchema.parse(req.body);
    const result = await rewardedAdsService.recordAdWatch(req.player!.id, data.adType, req.ip);
    res.json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error && error.message.includes('limit')) {
      return res.status(429).json({ error: error.message });
    }
    logger.error('Ad watch error:', error);
    res.status(500).json({ error: 'Failed to process ad watch' });
  }
});

router.get('/rewards/ad-status', authenticateToken, async (req: Request, res: Response) => {
  try {
    const status = await rewardedAdsService.getAdStatus(req.player!.id);
    res.json(status);
  } catch (error) {
    logger.error('Get ad status error:', error);
    res.status(500).json({ error: 'Failed to get ad status' });
  }
});

router.get('/rewards/types', authenticateToken, async (req: Request, res: Response) => {
  try {
    const types = rewardedAdsService.getAdTypes();
    res.json(types);
  } catch (error) {
    logger.error('Get ad types error:', error);
    res.status(500).json({ error: 'Failed to get ad types' });
  }
});

router.get('/rewards/history', authenticateToken, async (req: Request, res: Response) => {
  try {
    const limit = Math.min(parseInt(req.query.limit as string) || 20, 50);
    const history = await rewardedAdsService.getAdHistory(req.player!.id, limit);
    res.json({ history });
  } catch (error) {
    logger.error('Get ad history error:', error);
    res.status(500).json({ error: 'Failed to get ad history' });
  }
});

export default router;
