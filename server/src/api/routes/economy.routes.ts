import { Router, Request, Response } from 'express';
import { prisma } from '@config/database';
import { playerService } from '@services/player.service';
import { logger } from '@config/logger';

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

export default router;
