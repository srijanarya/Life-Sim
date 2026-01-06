import { Router, Request, Response } from 'express';
import { z } from 'zod';
import { logger } from '@config/logger';
import { avatarService } from '@services/avatar.service';
import { authenticateToken } from '@middleware/auth';

const router = Router();

const purchaseSchema = z.object({
  avatarItemId: z.string().uuid(),
});

const equipSchema = z.object({
  avatarItemId: z.string().uuid(),
  category: z.enum(['HAIR', 'FACE', 'OUTFIT', 'ACCESSORY']),
});

router.get('/', authenticateToken, async (req: Request, res: Response) => {
  try {
    const avatars = await avatarService.getAllAvailableAvatars(req.player!.id);
    res.json(avatars);
  } catch (error) {
    logger.error('Get avatars error:', error);
    res.status(500).json({ error: 'Failed to get avatars' });
  }
});

router.get('/starter', authenticateToken, async (req: Request, res: Response) => {
  try {
    const starters = await avatarService.getStarterAvatars(req.player!.id);
    res.json({ starters });
  } catch (error) {
    logger.error('Get starter avatars error:', error);
    res.status(500).json({ error: 'Failed to get starter avatars' });
  }
});

router.post('/purchase', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = purchaseSchema.parse(req.body);
    const result = await avatarService.purchaseAvatar(req.player!.id, data.avatarItemId);
    res.json(result);
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error) {
      if (error.message.includes('already owned')) {
        return res.status(400).json({ error: error.message });
      }
      if (error.message.includes('Insufficient')) {
        return res.status(400).json({ error: error.message });
      }
    }
    logger.error('Purchase avatar error:', error);
    res.status(500).json({ error: 'Failed to purchase avatar' });
  }
});

router.post('/equip', authenticateToken, async (req: Request, res: Response) => {
  try {
    const data = equipSchema.parse(req.body);
    const equipped = await avatarService.equipAvatar(
      req.player!.id,
      data.avatarItemId,
      data.category
    );
    res.json({ success: true, equipped });
  } catch (error) {
    if (error instanceof z.ZodError) {
      return res.status(400).json({ error: error.errors });
    }
    if (error instanceof Error && error.message.includes('not owned')) {
      return res.status(400).json({ error: error.message });
    }
    logger.error('Equip avatar error:', error);
    res.status(500).json({ error: 'Failed to equip avatar' });
  }
});

router.post('/unequip', authenticateToken, async (req: Request, res: Response) => {
  try {
    const { category } = req.body;
    if (!['HAIR', 'FACE', 'OUTFIT', 'ACCESSORY'].includes(category)) {
      return res.status(400).json({ error: 'Invalid category' });
    }
    const equipped = await avatarService.unequipAvatar(req.player!.id, category);
    res.json({ success: true, equipped });
  } catch (error) {
    logger.error('Unequip avatar error:', error);
    res.status(500).json({ error: 'Failed to unequip avatar' });
  }
});

router.post('/claim-starters', authenticateToken, async (req: Request, res: Response) => {
  try {
    await avatarService.grantStarterAvatars(req.player!.id);
    res.json({ success: true, message: 'Starter avatars granted' });
  } catch (error) {
    logger.error('Claim starter avatars error:', error);
    res.status(500).json({ error: 'Failed to grant starter avatars' });
  }
});

export default router;
