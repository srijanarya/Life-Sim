# Quick Start Guide - Play LifeCraft Now

**Goal**: Get the game playable in 10 minutes
**Last Updated**: January 7, 2026

---

## Option A: Play via Postman (Fastest - 10 min)

### Step 1: Start Backend (5 min)

```bash
# Start database and Redis
cd "LIFE SIMULATOR"
docker-compose up -d

# Wait for services to start (10-15 seconds)
# Check they're running:
docker ps

# Setup and start backend
cd server
cp .env.example .env
npm install
npx prisma generate
npx prisma migrate dev
npm run db:seed
npm run dev
```

Backend will start on **http://localhost:3000**

✅ **Verify it's working**:
```bash
curl http://localhost:3000/api/auth/login
# Should return JSON response with error (expected, body is missing)
```

### Step 2: Import Postman Collection (3 min)

1. Download and install [Postman](https://www.postman.com/downloads/)
2. Open Postman
3. **Import** → Click "Upload Files"
4. Select `docs/postman/LifeCraft-API-Collection.json`
5. Collection will appear in left sidebar

### Step 3: Play Game! (2 min)

Follow this sequence in Postman:

#### 1. Register Player
- Open **Authentication → Register Player**
- Click **Send**
- Note the `player.id` from response

#### 2. Login
- Open **Authentication → Login**
- Click **Send**
- Token is auto-saved to environment variables

#### 3. Create Game
- Open **Game → Create New Game**
- Click **Send**
- Note the `game.id`

#### 4. Get Random Event
- Open **Events → Get Random Event**
- Click **Send**
- Note the `event.id`

#### 5. Get Event Decisions
- Open **Events → Get Event Decisions**
- Click **Send**
- Note the `decisionId` (first one in array)

#### 6. Make Decision
- Open **Game → Make Decision**
- Click **Send**

#### 7. Advance Time
- Open **Game → Advance Time**
- Click **Send**

#### 8. Repeat Steps 4-7
Keep playing through events and advancing time!

---

## Option B: Play via Unity Client (Full Game - 1-2 days)

### Prerequisites
- Unity 2022.3 LTS+
- Xcode 14+ (for iOS)
- Mac computer
- Backend running (see Option A, Step 1)

### Quick Setup

1. **Create Unity Project** (5 min)
   - Open Unity Hub
   - New Project → 2D Core → "LifeCraft"

2. **Import Scripts** (5 min)
   ```bash
   cp -r "LIFE SIMULATOR/client/Assets/Scripts" /path/to/LifeCraft/Assets/
   ```

3. **Follow Unity Setup Guide** (1-2 hours)
   - See `docs/UNITY_SETUP_GUIDE.md`
   - Create scenes (MainMenu, CharacterCreation, Gameplay)
   - Create UI prefabs
   - Connect scripts
   - Test in Editor

4. **Build and Test** (30 min)
   - File → Build Settings → iOS
   - Build → Open Xcode
   - Run on device

---

## Option C: Play via cURL (For Developers)

Use these commands directly in terminal:

### 1. Register
```bash
curl -X POST http://localhost:3000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "player@example.com",
    "username": "testplayer",
    "password": "password123"
  }'
```

### 2. Login (save token)
```bash
TOKEN=$(curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "player@example.com",
    "password": "password123"
  }' | jq -r '.token')

echo "Token: $TOKEN"
```

### 3. Create Game (save game ID)
```bash
GAME_ID=$(curl -X POST http://localhost:3000/api/game \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "playerId": "PLAYER_ID_FROM_REGISTER",
    "initialTraits": {"startingAge": 18}
  }' | jq -r '.game.id')

echo "Game ID: $GAME_ID"
```

### 4. Get Random Event (save event ID)
```bash
EVENT_ID=$(curl -X GET http://localhost:3000/api/events/random \
  -H "Authorization: Bearer $TOKEN" | jq -r '.event.id')

echo "Event ID: $EVENT_ID"
```

### 5. Get Event Decisions (save decision ID)
```bash
DECISION_ID=$(curl -X GET http://localhost:3000/api/events/templates/$EVENT_ID/decisions \
  -H "Authorization: Bearer $TOKEN" | jq -r '.decisions[0].id')

echo "Decision ID: $DECISION_ID"
```

### 6. Make Decision
```bash
curl -X POST http://localhost:3000/api/game/$GAME_ID/decisions \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"eventId\": \"$EVENT_ID\",
    \"decisionId\": \"$DECISION_ID\"
  }"
```

### 7. Advance Time
```bash
curl -X POST http://localhost:3000/api/game/$GAME_ID/advance \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"months": 1}'
```

### 8. Loop
Repeat steps 4-7 to continue playing!

---

## Game Loop Summary

Regardless of how you play, the game works like this:

```
START
  ↓
1. Create/Select Player
  ↓
2. Start New Game
  ↓
3. Get Random Event (based on age, stats)
  ↓
4. View Event Description
  ↓
5. Choose Decision (affects stats, wealth, relationships)
  ↓
6. See Outcome (stats updated)
  ↓
7. Advance Time (month/year progresses)
  ↓
8. Repeat Steps 3-7 until:
  - Character dies (health = 0)
  - Retires (age = 100)
  - Player chooses to end
```

---

## Troubleshooting

### Backend Not Starting

**Problem**: `npm run dev` fails

**Solutions**:
```bash
# 1. Check if Docker is running
docker ps

# 2. Check if ports are in use
lsof -i :3000
lsof -i :5432
lsof -i :6379

# 3. Kill processes using ports
kill -9 <PID>

# 4. Check .env file exists
cat server/.env
```

### Database Connection Error

**Problem**: "Error connecting to database"

**Solutions**:
```bash
# 1. Restart Docker services
docker-compose down
docker-compose up -d

# 2. Check PostgreSQL is healthy
docker logs lifecraft-postgres

# 3. Recreate database
docker-compose down -v
docker-compose up -d
cd server
npm run db:seed
```

### Postman Collection Import Error

**Problem**: "Invalid collection"

**Solutions**:
1. Ensure JSON file is valid:
   ```bash
   cat docs/postman/LifeCraft-API-Collection.json | jq '.'
   ```
2. Update Postman to latest version
3. Try importing each folder manually

### Unity Scripts Not Compiling

**Problem**: Red errors in Console

**Solutions**:
1. Check Unity Console for specific error
2. Verify API URL in GameManager is correct
3. Ensure all required Unity packages are imported:
   - UnityEngine
   - UnityEngine.UI
   - UnityEngine.Networking
   - Newtonsoft.Json (for JSON parsing)

---

## What's Left to Implement

### Backend: ✅ Complete
- Authentication ✅
- Player management ✅
- Game loop ✅
- Event engine ✅
- Economy ✅
- Leaderboards ✅
- Daily challenges ✅
- Milestones ✅
- VIP ✅
- Avatars ✅

### Unity Client: ⚠️ Needs Visual Setup
- Scripts ✅ (all C# code complete)
- Scenes ❌ (needs to be created in Unity Editor)
- UI Prefabs ❌ (needs buttons, panels, text)
- Visual Assets ❌ (images, sprites, fonts)
- Audio ❌ (music, sound effects)

### Next Development Tasks

**Priority 1: Complete Unity Visual Setup**
- Create 3 scenes (MainMenu, CharacterCreation, Gameplay)
- Create UI prefabs (buttons, panels)
- Connect scripts to UI elements
- Test in Unity Editor

**Priority 2: Add Visual Assets**
- Background images for each scene
- Event icons
- Avatar sprites
- Fonts

**Priority 3: Polish**
- Animations
- Sound effects
- Loading screens
- Error handling UI

**Priority 4: Test & Deploy**
- Test on physical iOS device
- Fix bugs
- Submit to App Store

---

## Help & Resources

### Documentation
- **[PROJECT_STATUS.md](./PROJECT_STATUS.md)** - Full completion report
- **[README.md](./README.md)** - Main documentation
- **[UNITY_SETUP_GUIDE.md](./docs/UNITY_SETUP_GUIDE.md)** - Detailed Unity setup
- **[Postman Collection](./docs/postman/LifeCraft-API-Collection.json)** - API testing

### Quick Commands Reference

```bash
# Backend
cd server && npm run dev              # Start backend
cd server && npm test               # Run tests
cd server && npx prisma studio      # Database viewer

# Infrastructure
docker-compose up -d               # Start database + Redis
docker-compose down                 # Stop all services
docker-compose logs -f              # View logs

# Database
npx prisma migrate dev             # Apply schema changes
npx prisma db:seed                # Seed initial data
npx prisma studio                 # Visual database GUI
```

---

**Enjoy playing LifeCraft! 🎮**

For questions or issues, check the documentation files or create an issue on GitHub.
