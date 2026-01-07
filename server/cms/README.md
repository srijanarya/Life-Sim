# Event Content Management System (CMS)

This directory contains JSON files that define game events for the Event Engine.

## File Structure

Events are organized by type/category:
- `career-events.json` - General career-related events
- `relationship-events.json` - Relationship and social events
- `life-events.json` - Life milestones and personal events
- `random-events.json` - Random occurrences
- `rare-events.json` - Legendary and epic events
- `wealth-events.json` - Financial and investment events
- `personal-growth-events.json` - Learning and self-improvement
- `social-events.json` - Community and social gatherings
- `health-events.json` - Health and wellness events
- `tech-career-events.json` - Tech-specific career events

## Event Definition Schema

```json
{
  "title": "Event Title",
  "description": "Event description shown to player",
  "eventType": "LIFE_EVENT|CAREER_EVENT|RELATIONSHIP_EVENT|RANDOM_EVENT|DAILY_CHALLENGE",
  "rarity": "COMMON|UNCOMMON|RARE|EPIC|LEGENDARY",
  "minAge": 0,
  "maxAge": 100,
  "requiredCareer": "career-id-or-null",
  "requiredRelationship": true|false|undefined,
  "minStats": {
    "health": 0-100,
    "happiness": 0-100,
    "wealth": 0+,
    "intelligence": 0-100,
    "charisma": 0-100,
    "physical": 0-100,
    "creativity": 0-100
  },
  "weightMultiplier": 0.0-10.0,
  "followUpEvent": "event-id-or-null",
  "cooldownYears": 0-10,
  "decisions": [
    {
      "text": "Decision text shown to player",
      "order": 1,
      "outcomes": {
        "healthBoost": 0-100,
        "healthPenalty": 0-100,
        "happinessBoost": 0-100,
        "happinessPenalty": 0-100,
        "wealthChange": -infinity to +infinity,
        "intelligenceBoost": 0-100,
        "charismaBoost": 0-100,
        "physicalBoost": 0-100,
        "creativityBoost": 0-100,
        "careerChange": "new-career-id",
        "relationshipChange": true|false,
        "successChance": 0.0-1.0
      }
    }
  ]
}
```

## Event Type Guidelines

### Life Events
- Major life milestones (birthdays, anniversaries)
- Health and wellness
- Personal growth
- Family events
- Age: 0-100

### Career Events
- Job opportunities
- Promotions
- Workplace conflicts
- Career transitions
- Age: 18-70

### Relationship Events
- Dating and romance
- Marriage
- Social connections
- Family dynamics
- Age: 16-80

### Random Events
- Unexpected occurrences
- Lucky discoveries
- Chance encounters
- Age: 0-100

### Daily Challenges
- Time-limited special events
- Bonus opportunities
- Achievements
- Age: 18-80

## Rarity Guidelines

### COMMON (Weight: 70)
- Everyday occurrences
- Minor decisions
- Frequent events
- Example: Getting coffee, casual conversation

### UNCOMMON (Weight: 20)
- Notable but not rare
- Moderate impact decisions
- Occur occasionally
- Example: Job interview, first date

### RARE (Weight: 7)
- Significant events
- Major decisions
- Occur infrequently
- Example: Promotion, major purchase

### EPIC (Weight: 2)
- Very special events
- Life-changing decisions
- Rare occurrences
- Example: Winning contest, major achievement

### LEGENDARY (Weight: 1)
- Extremely rare events
- Defining moments
- Once or twice per game
- Example: Lottery win, royal opportunity

## Using the CMS

### Load Events to Database

```bash
# Load all events from CMS
npm run cms:load

# Load specific file
npm run cms:load career-events.json
```

### Export Events

```bash
# Export event by ID to JSON
npm run cms:export <event-id> exported-event.json
```

### Activate/Deactivate Events

```bash
# Activate event by title
npm run cms:activate "Event Title"

# Deactivate event by title
npm run cms:deactivate "Event Title"
```

## Best Practices

1. **Balance**: Ensure each decision has trade-offs and meaningful consequences
2. **Variety**: Mix of stat boosts, penalties, and wealth changes
3. **Progression**: Higher rarity events should require better stats
4. **Theming**: Match event text and outcomes to event type
5. **Age Appropriate**: Events should match age ranges
6. **Stat Dependencies**: Use minStats to gate challenging events

## Event Chains

Create follow-up events using `followUpEvent`:
```json
{
  "title": "First Date",
  "followUpEvent": "second-date-event-id"
}
```

The follow-up event will trigger automatically after the first event is completed.

## Custom Weighting

Adjust event frequency with `weightMultiplier`:
- < 1.0: Less frequent
- 1.0: Standard frequency
- > 1.0: More frequent

Example:
```json
{
  "weightMultiplier": 0.1,
  "title": "Lottery Win"
}
```

## Testing Events

After adding/modifying events:
1. Load events with CMS: `npm run cms:load`
2. Test in game
3. Verify stat changes work correctly
4. Check event weight distribution
5. Test with different player stats

## Adding New Events

1. Choose appropriate category file
2. Follow event schema
3. Add to JSON array
4. Load to database
5. Test thoroughly

## Version Control

Always commit CMS JSON files to track event changes and enable rollbacks.
