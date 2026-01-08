import { Router, Request, Response } from 'express';
import { prisma } from '@config/database';
import { eventEngine } from '@events/event-engine.service';
import { decisionSystem } from '@events/decision-system.service';
import { logger } from '@config/logger';

const router = Router();

router.post('/', async (req: Request, res: Response) => {
  try {
    const { playerId, initialTraits } = req.body;

    const gameState = await prisma.gameState.create({
      data: {
        playerId,
        currentAge: initialTraits?.startingAge || 18,
        currentYear: 1,
        currentMonth: 1,
      },
    });

    res.status(201).json(gameState);
  } catch (error) {
    logger.error('Failed to create game:', error);
    res.status(500).json({ error: 'Failed to create game' });
  }
});

router.get('/:id', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const gameState = await prisma.gameState.findUnique({
      where: { id },
    });

    if (!gameState) {
      return res.status(404).json({ error: 'Game not found' });
    }

    res.json(gameState);
  } catch (error) {
    logger.error('Failed to get game:', error);
    res.status(500).json({ error: 'Failed to fetch game' });
  }
});

router.post('/:id/advance', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;

    const onCooldown = await eventEngine.checkEventCooldown(id);
    if (onCooldown) {
      return res.status(429).json({ error: 'Event cooldown active' });
    }

    const gameState = await prisma.gameState.findUnique({
      where: { id },
    });

    if (!gameState) {
      return res.status(404).json({ error: 'Game not found' });
    }

    gameState.currentMonth++;
    if (gameState.currentMonth > 12) {
      gameState.currentMonth = 1;
      gameState.currentYear++;
      gameState.currentAge++;
    }

    const updatedGame = await prisma.gameState.update({
      where: { id },
      data: {
        currentYear: gameState.currentYear,
        currentMonth: gameState.currentMonth,
        currentAge: gameState.currentAge,
      },
    });

    const player = await prisma.player.findUnique({
      where: { id: gameState.playerId },
      include: { profile: true },
    });

    if (!player?.profile) {
      return res.status(404).json({ error: 'Player profile not found' });
    }

    const previousEvents = await prisma.playerEvent.findMany({
      where: { gameId: id },
      select: { templateId: true },
    });
    const previousEventIds = previousEvents.map((e) => e.templateId);

    await eventEngine.setEventCooldown(id);

    const eventTemplate = await eventEngine.generateEvent(
      updatedGame,
      player.profile,
      previousEventIds
    );
    if (eventTemplate) {
      const playerEvent = await eventEngine.recordEvent(
        id,
        eventTemplate.id,
        updatedGame
      );

      return res.json({
        gameState: updatedGame,
        event: playerEvent,
        eventTemplate,
      });
    }

    res.json({ gameState: updatedGame, event: null });
  } catch (error) {
    logger.error('Failed to advance game:', error);
    res.status(500).json({ error: 'Failed to advance game' });
  }
});

router.get('/:id/history', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const events = await eventEngine.getEventsForGame(id);
    res.json(events);
  } catch (error) {
    logger.error('Failed to get game history:', error);
    res.status(500).json({ error: 'Failed to fetch history' });
  }
});

router.post('/:id/decisions', async (req: Request, res: Response) => {
  try {
    const { id: gameId } = req.params;
    const { eventId, decisionId } = req.body;

    const isValid = await decisionSystem.validateDecision(
      eventId,
      decisionId
    );
    if (!isValid) {
      return res.status(400).json({ error: 'Invalid decision for this event' });
    }

    const decision = await decisionSystem.processDecision(
      gameId,
      eventId,
      decisionId
    );

    res.status(201).json(decision);
  } catch (error) {
    logger.error('Failed to process decision:', error);
    res.status(500).json({ error: 'Failed to process decision' });
  }
});

router.get('/:id/decisions', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const decisions = await decisionSystem.getPlayerDecisions(id);
    res.json(decisions);
  } catch (error) {
    logger.error('Failed to get decisions:', error);
    res.status(500).json({ error: 'Failed to fetch decisions' });
  }
});

router.get('/:id/daily-challenge', async (req: Request, res: Response) => {
  try {
    const { id } = req.params;
    const challenge = await eventEngine.getDailyChallenge(id);
    res.json(challenge);
  } catch (error) {
    logger.error('Failed to get daily challenge:', error);
    res.status(500).json({ error: 'Failed to fetch daily challenge' });
  }
});

export default router;
