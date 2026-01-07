#!/usr/bin/env ts-node
import { eventCMS } from '../src/services/event-cms.service';

const args = process.argv.slice(2);
const command = args[0];

async function main() {
  try {
    switch (command) {
      case 'load':
        const file = args[1];
        if (file) {
          await eventCMS.loadEventsFromFile(file);
          console.log(`✓ Loaded events from ${file}`);
        } else {
          await eventCMS.loadAllEvents();
          console.log('✓ Loaded all events from CMS');
        }
        break;

      case 'export':
        const exportEventId = args[1];
        const exportFilename = args[2] || 'event-export.json';
        if (exportEventId) {
          await eventCMS.exportEventToFile(exportEventId, exportFilename);
          console.log(`✓ Exported event to ${exportFilename}`);
        } else {
          console.error('Error: Event ID required for export');
          process.exit(1);
        }
        break;

      case 'activate':
        const activateTitle = args[1];
        if (activateTitle) {
          await eventCMS.activateEvent(activateTitle);
          console.log(`✓ Activated event: ${activateTitle}`);
        } else {
          console.error('Error: Event title required for activation');
          process.exit(1);
        }
        break;

      case 'deactivate':
        const deactivateTitle = args[1];
        if (deactivateTitle) {
          await eventCMS.deactivateEvent(deactivateTitle);
          console.log(`✓ Deactivated event: ${deactivateTitle}`);
        } else {
          console.error('Error: Event title required for deactivation');
          process.exit(1);
        }
        break;

      default:
        console.log(`
Event CMS Commands:

  load [file]         Load events from JSON file (or all files if no file specified)
  export <id> [file]  Export event to JSON file
  activate <title>     Activate event by title
  deactivate <title>   Deactivate event by title

Examples:
  npm run cms:load                    # Load all events
  npm run cms:load life-events.json  # Load specific file
  npm run cms:export <event-id>       # Export event
  npm run cms:activate "Job Interview" # Activate event
  npm run cms:deactivate "Job Interview" # Deactivate event
        `);
        process.exit(0);
    }
  } catch (error) {
    console.error('Error:', error);
    process.exit(1);
  }
}

main();
