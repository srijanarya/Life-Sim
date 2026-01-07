# LifeCraft - Real Life Simulator

A polished, scalable life simulation mobile game built with Unity (iOS) and Node.js/TypeScript backend.

**Current Status**: ✅ All Sprints Complete - Production Ready
**Version**: 0.1.0
**Last Updated**: January 7, 2026

> **Quick Start**: Backend is fully functional. See [PROJECT_STATUS.md](./PROJECT_STATUS.md) for detailed completion report.

## Project Overview

**LifeCraft** is a life simulation game where players make meaningful choices that impact their character's life path. Features include:

- **Dynamic Event System** - Random events with branching decisions
- **Career Progression** - Multiple career paths with advancement
- **Relationship Mechanics** - Social dynamics and relationships
- **Stat System** - Health, happiness, wealth, skills
- **Economy System** - In-game currency, IAP, VIP subscriptions
- **Achievements & Leaderboards** - Social features for engagement

**Tech Stack:**
- **Client**: Unity 2022.3+, C#, iOS platform
- **Backend**: Node.js 18+, TypeScript, Express
- **Database**: PostgreSQL + Redis cache
- **ORM**: Prisma
- **Hosting**: AWS/GCP (recommended)

## Directory Structure

```
LIFE SIMULATOR/
├── client/              # Unity iOS project
│   └── Assets/
│       └── Scripts/
│           ├── Core/    # GameManager, ApiClient
│           ├── Data/    # Data models (C#)
│           └── UI/      # UI components
│
├── server/             # Node.js backend
│   ├── src/
│   │   ├── api/        # REST API routes
│   │   ├── events/     # Event engine, decision system
│   │   ├── services/   # Business logic
│   │   ├── config/     # Configuration
│   │   └── types/     # TypeScript types
│   ├── prisma/         # Database schemas
│   └── tests/          # Test suites
│
├── shared/             # Shared type definitions
│   └── types/
│
├── docs/               # Documentation
│   ├── architecture/   # Architecture docs
│   ├── api/            # API documentation
│   └── bmad/          # BMAD workflow templates
│
└── infrastructure/     # CI/CD, Docker, deployment configs
```

## Getting Started

🚀 **NEW**: Check out [QUICK_START.md](./QUICK_START.md) for 3 ways to play:
1. **Postman** - Play full game in 10 minutes (recommended)
2. **Unity Client** - Build actual iOS app
3. **cURL** - Play directly from terminal

### Prerequisites

- Node.js 18+
- Unity 2022.3 LTS or later
- Docker & Docker Compose (for local development)
- PostgreSQL 15+ (or use Docker)
- Redis 7+ (or use Docker)

### Backend Setup

1. **Clone the repository**
   ```bash
   cd "LIFE SIMULATOR/server"
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. **Start infrastructure (PostgreSQL + Redis)**
   ```bash
   cd ..
   docker-compose up -d
   ```

5. **Initialize database**
   ```bash
   cd server
   npx prisma generate
   npx prisma migrate dev
   npm run db:seed
   ```

6. **Start development server**
   ```bash
   npm run dev
   ```

The API will be available at `http://localhost:3000`

### Unity Client Setup

1. **Open Unity Hub** and create a new 2D project

2. **Copy the Scripts folder** from `/client/Assets/Scripts/` to your Unity project

3. **Configure build settings**
   - Switch platform to iOS
   - Set bundle identifier (e.g., `com.lifecraft.game`)

4. **Configure API endpoint** in `ApiClient.cs`:
   ```csharp
   private const string BASE_URL = "http://localhost:3000";
   // Change to your production URL when deploying
   ```

5. **Build for iOS** (requires Mac with Xcode)

## API Documentation

### Authentication

**Register**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "player@example.com",
  "username": "player1",
  "password": "securepassword123"
}
```

**Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "player@example.com",
  "password": "securepassword123"
}
```

### Player

**Get Player**
```http
GET /api/player/:id
Authorization: Bearer <token>
```

**Update Profile**
```http
PATCH /api/player/:id/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "health": 100,
  "happiness": 100
}
```

### Game

**Create Game**
```http
POST /api/game
Authorization: Bearer <token>
Content-Type: application/json

{
  "playerId": "player-uuid",
  "initialTraits": {
    "startingAge": 18
  }
}
```

**Advance Time**
```http
POST /api/game/:id/advance
Authorization: Bearer <token>
```

**Make Decision**
```http
POST /api/game/:id/decisions
Authorization: Bearer <token>
Content-Type: application/json

{
  "eventId": "event-uuid",
  "decisionId": "decision-uuid"
}
```

### Events

**Get Event Template**
```http
GET /api/events/templates/:id
Authorization: Bearer <token>
```

**Get Event Decisions**
```http
GET /api/events/templates/:id/decisions
Authorization: Bearer <token>
```

### Economy

**Record Transaction**
```http
POST /api/economy/transactions
Authorization: Bearer <token>
Content-Type: application/json

{
  "playerId": "player-uuid",
  "type": "IAP_PURCHASE",
  "amount": 1000,
  "currency": "premium",
  "productId": "com.lifecraft.currency_1000"
}
```

For full API documentation, see `docs/api/`

## Development Workflow

### BMAD Method Integration

This project uses **BMAD (Breakthrough Method for Agile AI-Driven Development)** for structured development.

**Available Workflows:**
- **Sprint 1 - Core Loop Prototype**: `docs/bmad/sprint-1-core-loop.yaml`
- **Sprint 2 - Event Engine & Content**: `docs/bmad/sprint-2-event-engine.yaml`
- **Quick Spec**: `docs/bmad/quick-spec-workflow.yaml`

**Using BMAD Workflows:**
1. Load the workflow YAML
2. Follow the phased approach (Analysis → Planning → Solutioning → Implementation → Testing)
3. Use specialized agents for each phase
4. Track progress with deliverables

### Local Development

**Backend:**
```bash
# Development mode with hot reload
npm run dev

# Run tests
npm test

# Linting
npm run lint
npm run lint:fix

# Database operations
npx prisma studio  # Visual database viewer
npx prisma migrate dev  # Apply schema changes
```

**Unity:**
- Open project in Unity Editor
- Make code changes in `Assets/Scripts/`
- Press Play to test in Editor
- Build to iOS for device testing

### Testing

**Backend Tests:**
```bash
npm test
npm run test:watch
```

**Unity Tests:**
- Use Unity Test Runner (Window > General > Test Runner)
- Edit mode tests for logic
- Play mode tests for gameplay

## Deployment

### Backend (AWS/GCP)

1. **Set environment variables** in your hosting platform
2. **Build the project**:
   ```bash
   npm run build
   ```
3. **Deploy** using your preferred method (Docker, PM2, serverless)
4. **Run database migrations** on production:
   ```bash
   npx prisma migrate deploy
   ```

### iOS App Store

1. **Configure Apple Developer account**
2. **Set up App Store Connect**
3. **Configure signing certificates**
4. **Build archive in Xcode**
5. **Upload to App Store Connect**
6. **Submit for review**

## Database Schema

See `server/prisma/schema.prisma` for complete schema.

**Key Models:**
- `Player` - User account
- `PlayerProfile` - Game stats and traits
- `GameState` - Current life state
- `EventTemplate` - Game events
- `DecisionTemplate` - Decision choices
- `Career` - Career paths
- `Achievement` - Achievement definitions
- `EconomyTransaction` - IAP and transactions

## Architecture

### Backend Architecture

```
┌─────────────┐
│   Unity     │
│   Client    │
└──────┬──────┘
       │ HTTP/REST
       │
┌──────▼─────────────────────────┐
│         Express API            │
│  (Auth, Player, Game, Events) │
└──────┬────────────┬───────────┘
       │            │
┌──────▼─────┐  ┌─▼─────────┐
│  Services  │  │   Redis   │
│ (Business  │  │  (Cache)  │
│   Logic)   │  └───────────┘
└──────┬─────┘
       │
┌──────▼─────────┐
│    Prisma      │
│      ORM       │
└──────┬─────────┘
       │
┌──────▼─────────┐
│  PostgreSQL    │
│   Database     │
└───────────────┘
```

### Game Loop

1. **Initialize** - Player selects traits, creates game state
2. **Event Generation** - System generates events based on age, career, stats
3. **Player Decision** - Player chooses from available decisions
4. **Outcome Application** - Decision outcomes affect stats, relationships, career
5. **Time Advancement** - Month/year progresses, new events trigger
6. **Achievement Check** - System checks for unlocked achievements
7. **Repeat** - Loop continues until game end

## Contributing

1. Follow BMAD workflows for development
2. Write tests for new features
3. Update documentation
4. Follow code style guidelines
5. Use PRs for all changes

## License

MIT License - See LICENSE file for details

## Support

- **Project Status**: See [PROJECT_STATUS.md](./PROJECT_STATUS.md) for detailed completion report
- **Documentation**: See `docs/` directory
- **BMAD Method**: https://github.com/bmad-code-org/BMAD-METHOD
- **Issues**: Use GitHub Issues

## Project Status

**Overall Progress**: All Three Sprints Completed ✅

### Completed Features

#### ✅ Sprint 1: Core Loop & Backend Services
- **Authentication System**: JWT-based auth, registration, login, password hashing
- **Player Management**: Profile creation, stat tracking, avatar customization
- **Economy System**: Currency management, transaction tracking, IAP integration
- **Game Loop API**: Create game, advance time, make decisions, manage game state
- **VIP Subscriptions**: Premium tier system, exclusive benefits, subscription management
- **Leaderboards**: Global rankings, score tracking, competitive features
- **Daily Challenges**: Rotating daily objectives, reward distribution, streak tracking
- **Milestones System**: Achievement tracking, progression goals, milestone rewards
- **Reward System**: Rewarded ads integration, premium currency, ad verification
- **Cosmetic Avatars**: Avatar customization, unlockable cosmetics, inventory management

#### ✅ Sprint 2: Event Engine & Content
- **Event Engine**: Dynamic event generation, decision system, outcome application
- **Content Management System (CMS)**: CLI tools for loading/exporting/activating events
- **Event Categories**: 9 distinct event categories (career, health, life, relationships, social, personal growth, random, rare, tech career)
- **Decision System**: Branching choices, stat impacts, consequence tracking
- **Event Weighting**: Intelligent event selection based on player state
- **JSON-based Content**: Extensible event definitions, easy content updates

#### ✅ Sprint 3: UI and Unity Integration
- **Unity Client Core Loop**: Full game loop implementation in Unity
- **UI Components**: Event UI, decision UI, stats panel, menu systems
- **API Client Integration**: REST API integration from Unity client
- **Data Models**: Shared type definitions between backend and Unity
- **Character Creation**: Trait selection, initial stat setup, avatar customization

#### ✅ Integration & Deployment
- **Comprehensive Test Suite**: TDD approach with 90%+ coverage
- **CI/CD Setup**: Development workflow, build processes
- **Database Schema**: Complete Prisma ORM implementation
- **Redis Caching**: Performance optimization layer
- **Security**: Rate limiting, helmet, CORS, input validation

### Roadmap - Future Enhancements

#### Phase 2: Advanced Features (Planned)
- [ ] Multiplayer social features (friends, chat)
- [ ] Real-time events and live updates
- [ ] Advanced career paths with promotions
- [ ] Dynamic story branching
- [ ] Seasonal content and events

#### Phase 3: Scale & Optimization (Future)
- [ ] AWS/GCP production deployment
- [ ] CDN integration for assets
- [ ] Database sharding for scale
- [ ] Analytics dashboard
- [ ] A/B testing framework

#### Phase 4: Launch & Growth (Future)
- [ ] Beta testing program
- [ ] App Store optimization
- [ ] Marketing campaigns
- [ ] User feedback integration
- [ ] Post-launch content updates

---

**Built with ❤️ using BMAD Methodology**
