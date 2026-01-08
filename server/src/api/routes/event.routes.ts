import { Router, Request, Response } from 'express';
import { eventEngine } from '@events/event-engine.service';
import { decisionSystem } from '@events/decision-system.service';
import { logger } from '@config/logger';

const router = Router();

router.get('/templates/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const template = await eventEngine.getEventTemplate(id);

    if (!template) {
      return res.status(404).json({ error: 'Event template not found' });
    }

    res.json(template);
  } catch (error) {
    logger.error('Failed to get event template:', error);
    res.status(500).json({ error: 'Failed to fetch event template' });
  }
});

router.get('/templates/:id/decisions', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const decisions = await decisionSystem.getDecisionsForEvent(id);
    res.json(decisions);
  } catch (error) {
    logger.error('Failed to get event decisions:', error);
    res.status(500).json({ error: 'Failed to fetch decisions' });
  }
});

router.get('/templates', async (req: Request, res: Response) => {
  try {
    const { type, rarity } = req.query;

    const events = await eventEngine.queryEventTemplates({
      type: typeof type === 'string' ? type : undefined,
      rarity: typeof rarity === 'string' ? rarity : undefined,
    });

    res.json(events);
  } catch (error) {
    logger.error('Failed to get event templates:', error);
    res.status(500).json({ error: 'Failed to fetch templates' });
  }
});

export default router;
