import { prisma } from '@config/database';
import { logger } from '@config/logger';
import * as fs from 'fs/promises';
import * as path from 'path';

export interface EventDefinition {
  title: string;
  description: string;
  eventType: string;
  rarity: string;
  minAge: number;
  maxAge: number;
  requiredCareer?: string;
  requiredRelationship?: boolean;
  minStats?: {
    health?: number;
    happiness?: number;
    wealth?: number;
    intelligence?: number;
    charisma?: number;
    physical?: number;
    creativity?: number;
  };
  weightMultiplier?: number;
  followUpEvent?: string;
  cooldownYears?: number;
  decisions: Array<{
    text: string;
    order: number;
    outcomes: Record<string, unknown>;
  }>;
}

export class EventCMS {
  private readonly CMS_DIR = path.join(__dirname, '../../cms/events');

  async loadEventsFromFile(filename: string): Promise<void> {
    try {
      const filePath = path.join(this.CMS_DIR, filename);
      const fileContent = await fs.readFile(filePath, 'utf-8');
      const eventData = JSON.parse(fileContent);

      if (Array.isArray(eventData)) {
        for (const eventDef of eventData) {
          await this.syncEventToDatabase(eventDef);
        }
      } else {
        await this.syncEventToDatabase(eventData);
      }

      logger.info(`Loaded events from ${filename}`);
    } catch (error) {
      logger.error(`Failed to load events from ${filename}:`, error);
      throw error;
    }
  }

  async loadAllEvents(): Promise<void> {
    try {
      const files = await fs.readdir(this.CMS_DIR);
      const eventFiles = files.filter((file) => file.endsWith('.json'));

      logger.info(`Found ${eventFiles.length} event files to load`);

      for (const file of eventFiles) {
        await this.loadEventsFromFile(file);
      }

      logger.info('All events loaded successfully');
    } catch (error) {
      logger.error('Failed to load all events:', error);
      throw error;
    }
  }

  private async syncEventToDatabase(eventDef: EventDefinition): Promise<void> {
    try {
      const existingEvent = await prisma.eventTemplate.findFirst({
        where: { title: eventDef.title },
      });

      if (existingEvent) {
        await prisma.eventTemplate.update({
          where: { id: existingEvent.id },
          data: {
            description: eventDef.description,
            eventType: eventDef.eventType as any,
            rarity: eventDef.rarity as any,
            minAge: eventDef.minAge,
            maxAge: eventDef.maxAge,
            requiredCareer: eventDef.requiredCareer,
            requiredRelationship: eventDef.requiredRelationship,
            minStats: eventDef.minStats as any,
            weightMultiplier: eventDef.weightMultiplier,
            followUpEvent: eventDef.followUpEvent,
            cooldownYears: eventDef.cooldownYears,
            isActive: true,
          },
        });

        await this.syncDecisions(existingEvent.id, eventDef.decisions);
        logger.debug(`Updated event: ${eventDef.title}`);
      } else {
        const newEvent = await prisma.eventTemplate.create({
          data: {
            title: eventDef.title,
            description: eventDef.description,
            eventType: eventDef.eventType as any,
            rarity: eventDef.rarity as any,
            minAge: eventDef.minAge,
            maxAge: eventDef.maxAge,
            requiredCareer: eventDef.requiredCareer,
            requiredRelationship: eventDef.requiredRelationship,
            minStats: eventDef.minStats as any,
            weightMultiplier: eventDef.weightMultiplier,
            followUpEvent: eventDef.followUpEvent,
            cooldownYears: eventDef.cooldownYears,
            isActive: true,
          },
        });

        await this.syncDecisions(newEvent.id, eventDef.decisions);
        logger.debug(`Created event: ${eventDef.title}`);
      }
    } catch (error) {
      logger.error(`Failed to sync event ${eventDef.title}:`, error);
      throw error;
    }
  }

  private async syncDecisions(
    eventId: string,
    decisions: EventDefinition['decisions']
  ): Promise<void> {
    const existingDecisions = await prisma.decisionTemplate.findMany({
      where: { eventId },
    });

    for (const decisionDef of decisions) {
      const existingDecision = existingDecisions.find(
        (d) => d.order === decisionDef.order
      );

      if (existingDecision) {
        await prisma.decisionTemplate.update({
          where: { id: existingDecision.id },
          data: {
            text: decisionDef.text,
            order: decisionDef.order,
            outcomes: decisionDef.outcomes as any,
          },
        });
      } else {
        await prisma.decisionTemplate.create({
          data: {
            eventId,
            text: decisionDef.text,
            order: decisionDef.order,
            outcomes: decisionDef.outcomes as any,
          },
        });
      }
    }
  }

  async exportEvent(eventId: string): Promise<EventDefinition | null> {
    try {
      const event = await prisma.eventTemplate.findUnique({
        where: { id: eventId },
        include: { decisions: true },
      });

      if (!event) {
        return null;
      }

      const eventDef: EventDefinition = {
        title: event.title,
        description: event.description,
        eventType: event.eventType,
        rarity: event.rarity,
        minAge: event.minAge,
        maxAge: event.maxAge,
        requiredCareer: event.requiredCareer || undefined,
        requiredRelationship: event.requiredRelationship || undefined,
        minStats: event.minStats as any || undefined,
        weightMultiplier: event.weightMultiplier || undefined,
        followUpEvent: event.followUpEvent || undefined,
        cooldownYears: event.cooldownYears || undefined,
        decisions: event.decisions.map((d) => ({
          text: d.text,
          order: d.order,
          outcomes: d.outcomes as Record<string, unknown>,
        })),
      };

      return eventDef;
    } catch (error) {
      logger.error(`Failed to export event ${eventId}:`, error);
      throw error;
    }
  }

  async exportEventToFile(eventId: string, filename: string): Promise<void> {
    try {
      const eventDef = await this.exportEvent(eventId);

      if (!eventDef) {
        throw new Error(`Event not found: ${eventId}`);
      }

      const filePath = path.join(this.CMS_DIR, filename);
      await fs.writeFile(filePath, JSON.stringify(eventDef, null, 2), 'utf-8');

      logger.info(`Exported event to ${filename}`);
    } catch (error) {
      logger.error(`Failed to export event ${eventId}:`, error);
      throw error;
    }
  }

  async deactivateEvent(eventTitle: string): Promise<void> {
    try {
      await prisma.eventTemplate.updateMany({
        where: { title: eventTitle },
        data: { isActive: false },
      });

      logger.info(`Deactivated event: ${eventTitle}`);
    } catch (error) {
      logger.error(`Failed to deactivate event ${eventTitle}:`, error);
      throw error;
    }
  }

  async activateEvent(eventTitle: string): Promise<void> {
    try {
      await prisma.eventTemplate.updateMany({
        where: { title: eventTitle },
        data: { isActive: true },
      });

      logger.info(`Activated event: ${eventTitle}`);
    } catch (error) {
      logger.error(`Failed to activate event ${eventTitle}:`, error);
      throw error;
    }
  }
}

export const eventCMS = new EventCMS();
