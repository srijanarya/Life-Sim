# Unity Client Setup Guide

**Version**: 1.0
**Last Updated**: January 7, 2026
**Prerequisites**: Unity 2022.3 LTS+, Xcode (for iOS), Mac computer

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Creation](#project-creation)
3. [Import Scripts](#import-scripts)
4. [Create Scenes](#create-scenes)
5. [Create UI Prefabs](#create-ui-prefabs)
6. [Connect Scripts](#connect-scripts)
7. [Configure Build Settings](#configure-build-settings)
8. [Test in Editor](#test-in-editor)
9. [Build for iOS](#build-for-ios)

---

## Prerequisites

### Software Requirements
- ✅ Unity Hub installed
- ✅ Unity 2022.3 LTS or later
- ✅ Xcode 14+ (for iOS builds)
- ✅ Git (optional, for version control)

### Hardware Requirements
- Mac with Apple Silicon (M1/M2/M3) or Intel-based Mac
- At least 16GB RAM recommended

---

## Project Creation

### Step 1: Create New Unity Project

1. Open **Unity Hub**
2. Click **New Project**
3. Choose **2D Core** template
4. Set project name: `LifeCraft`
5. Choose location (e.g., `/Users/yourname/Projects/LifeCraft`)
6. Click **Create Project**

**Important**: Make sure to select **2D** template, not 3D.

### Step 2: Project Settings

Once project opens:

1. **File → Build Settings**
   - Platform: iOS
   - Click **Switch Platform** (if not already iOS)
   - Target Device: iPhone + iPad

2. **Edit → Project Settings**
   - Player → Other Settings
   - Bundle Identifier: `com.lifecraft.game`
   - Product Name: `LifeCraft`
   - Default Orientation: Landscape Left (or Portrait depending on design)

---

## Import Scripts

### Step 1: Copy Scripts to Project

**Option A: Manual Copy**
```bash
cp -r "LIFE SIMULATOR/client/Assets/Scripts" /path/to/your/unity/project/Assets/
```

**Option B: Drag & Drop**
1. Open your Unity project
2. In Finder, navigate to `LIFE SIMULATOR/client/Assets/Scripts`
3. Drag the entire `Scripts` folder into Unity's **Assets** folder
4. Unity will import the files

### Step 2: Verify Import

In Unity Project window, you should see:
```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs
│   │   └── ApiClient.cs
│   ├── Data/
│   │   └── Models.cs
│   ├── Services/
│   │   ├── AvatarManager.cs
│   │   └── RewardedAdsManager.cs
│   └── UI/
│       ├── UIManager.cs
│       ├── MainMenuPanel.cs
│       ├── CharacterCreationPanel.cs
│       ├── EventPopupPanel.cs
│       ├── StatsPanel.cs
│       ├── TimelinePanel.cs
│       ├── AvatarPanel.cs
│       └── CareerDashboard.cs
```

Unity should automatically compile the scripts. Check the **Console** window for any compilation errors.

---

## Create Scenes

### Scene 1: MainMenu

**Purpose**: Main entry point with navigation to character creation, settings, and quit.

#### Create Scene
1. **File → New Scene**
2. Name it: `MainMenu`
3. **File → Save As** → Save to `Assets/Scenes/MainMenu.unity`

#### Setup Scene Objects

1. **Canvas** (for UI)
   - **GameObject → UI → Canvas**
   - In Inspector:
     - **Render Mode**: Screen Space - Overlay
     - **UI Scale Mode**: Scale With Screen Size
     - **Reference Resolution**: X=1920, Y=1080

2. **EventSystem** (empty GameObject)
   - **GameObject → Create Empty**
   - Name it: `EventSystem`
   - Attach `GameManager.cs` script (Scripts/Core/)

3. **Main Camera**
   - Select existing Main Camera
   - **Inspector**:
     - **Background**: Black (0, 0, 0, 255)
     - **Orthographic Size**: 5
     - Change Projection to Orthographic (if not already)

#### Add UI Panel

1. **GameObject → UI → Panel**
2. Name it: `MainMenuPanel`
3. In Hierarchy, make it child of Canvas
4. **Inspector → RectTransform**:
   - Anchor: Center
   - Pos X: 0, Pos Y: 0
   - Width: 1920, Height: 1080
5. Attach `MainMenuPanel.cs` script

---

### Scene 2: CharacterCreation

**Purpose**: Player creates character with traits and avatar.

#### Create Scene
1. **File → New Scene**
2. Name it: `CharacterCreation`
3. Save to `Assets/Scenes/CharacterCreation.unity`

#### Setup Scene Objects

1. **Canvas** (same as MainMenu setup)
2. **EventSystem** (empty GameObject with GameManager.cs)
3. **CharacterCreationPanel** (UI Panel)
   - **GameObject → UI → Panel**
   - Name: `CharacterCreationPanel`
   - Attach `CharacterCreationPanel.cs`

---

### Scene 3: Gameplay

**Purpose**: Main game loop with events, decisions, stats, and timeline.

#### Create Scene
1. **File → New Scene**
2. Name it: `Gameplay`
3. Save to `Assets/Scenes/Gameplay.unity`

#### Setup Scene Objects

1. **Canvas** (same as MainMenu)
2. **EventSystem** (empty GameObject with GameManager.cs)

3. **EventPopupPanel** (for displaying events)
   - **GameObject → UI → Panel**
   - Name: `EventPopupPanel`
   - Initially set **Active** to false (checkbox unchecked)
   - Attach `EventPopupPanel.cs`

4. **StatsPanel** (for displaying player stats)
   - **GameObject → UI → Panel**
   - Name: `StatsPanel`
   - Position: Top-right corner
   - Attach `StatsPanel.cs`

5. **TimelinePanel** (for game history)
   - **GameObject → UI → Panel**
   - Name: `TimelinePanel`
   - Position: Bottom
   - Attach `TimelinePanel.cs`

---

## Create UI Prefabs

### Prefab 1: UIButton

**Purpose**: Reusable button component for all UI interactions.

#### Create Button
1. **GameObject → UI → Button**
2. Name it: `UIButton`
3. In Inspector:
   - Add `UIButton.cs` script (if not auto-added)
   - Configure button properties:
     - Button Type: Button
     - Interactable: checked

#### Configure Visual Elements
1. Expand Button in Hierarchy
2. Select **Button → Text (TMP)** or **Button → Text**
3. Change text content as needed

#### Save as Prefab
1. Drag `UIButton` from Hierarchy to **Project → Assets/Prefabs**
2. A blue prefab icon will appear

### Prefab 2: EventPopupItem

**Purpose**: Individual event item in timeline.

#### Create Item
1. **GameObject → UI → Panel**
2. Name it: `EventPopupItem`
3. Add **TimelineItemPanel.cs` script

#### Save as Prefab
1. Drag to `Assets/Prefabs/EventPopupItem`

---

## Connect Scripts

### GameManager Setup

The `GameManager.cs` needs to be configured as a **Singleton** and initialized.

#### Configure GameManager

1. Select **EventSystem** GameObject in MainMenu scene
2. Attach **GameManager.cs** (if not already attached)
3. In Inspector → GameManager Script:
   - **API URL**: Enter your backend URL
     - Local dev: `http://localhost:3000`
     - Production: `https://your-api-url.com`
   - **Auto Initialize**: Checked

4. Make sure **DontDestroyOnLoad** is working:
   - In `GameManager.cs`, verify `DontDestroyOnLoad(gameObject)` is in Awake()

### UI Manager Setup

The `UIManager.cs` manages transitions between scenes/panels.

#### Create UIManager GameObject

1. In MainMenu scene:
   - **GameObject → Create Empty**
   - Name it: `UIManager`
   - Attach `UIManager.cs`

2. Connect References (in Inspector):
   - **Main Menu Panel**: Drag MainMenuPanel object
   - **Character Creation Panel**: (will be set at runtime)
   - **Event Popup Panel**: (will be set at runtime)
   - **Stats Panel**: (will be set at runtime)
   - **Timeline Panel**: (will be set at runtime)

---

## Configure Build Settings

### Step 1: Add Scenes to Build

1. **File → Build Settings**
2. **Scenes In Build** section:
   - Drag these scenes from Project window in this order:
     1. `MainMenu.unity` (Index 0 - first scene)
     2. `CharacterCreation.unity`
     3. `Gameplay.unity`

### Step 2: iOS-Specific Settings

1. In Build Settings, select **iOS** platform
2. Click **Player Settings...**
3. **Player → iOS → Other Settings**:
   - **Bundle Identifier**: `com.lifecraft.game`
   - **Target Minimum iOS Version**: 12.0
   - **Architecture**: ARM64

4. **Player → iOS → Identification**:
   - **Display Name**: `LifeCraft`

---

## Test in Editor

### Step 1: Test Main Menu

1. Open **MainMenu** scene
2. Click **Play** button (top center of Unity)
3. Expected behavior:
   - Main menu panel appears
   - Buttons are clickable
   - GameManager initializes

### Step 2: Test Character Creation

1. Open **CharacterCreation** scene
2. Click **Play**
3. Expected behavior:
   - Character creation panel appears
   - Can select starting age
   - Can choose traits
   - Create character button works

### Step 3: Test Gameplay

1. Open **Gameplay** scene
2. Click **Play**
3. Expected behavior:
   - Stats panel shows in top-right
   - Timeline shows at bottom
   - Event popup appears (manually trigger via GameManager test)
   - Decision buttons are clickable

### Testing API Connection

1. Make sure backend is running:
   ```bash
   cd "LIFE SIMULATOR/server"
   npm run dev
   ```

2. In Unity, run Gameplay scene
3. Watch Console for API calls:
   - Should see "API connected" message
   - Should see events being fetched

---

## Build for iOS

### Step 1: Prepare Build

1. Open Unity
2. **File → Build Settings**
3. Ensure iOS is selected as platform
4. Ensure all 3 scenes are in "Scenes In Build"
5. Click **Build**
6. Choose folder for Xcode project (e.g., `Builds/iOS`)

### Step 2: Open in Xcode

1. Unity will open Xcode automatically (or you can open manually)
2. In Xcode:
   - Select your development team (signing)
   - Select target device (iPhone or iPad)

### Step 3: Build and Run

1. In Xcode top bar:
   - Select your device from the list
   - Click **Run** (▶) button
   - App will install on device and launch

---

## Common Issues & Solutions

### Issue 1: Scripts Not Compiling

**Symptom**: Console shows red errors

**Solution**:
1. Check console for specific error messages
2. Common issues:
   - Missing `using` statements at top of script
   - Missing namespace
   - Typo in class name

### Issue 2: UI Elements Not Visible

**Symptom**: Panels are created but nothing shows

**Solution**:
1. Check Canvas settings (Screen Space - Overlay)
2. Check RectTransform positioning
3. Ensure Panel has **Image** component or child elements

### Issue 3: Buttons Not Clickable

**Symptom**: Buttons don't respond to clicks

**Solution**:
1. Ensure Button has **CanvasRenderer** component
2. Check that there's an **EventSystem** in scene
3. Verify Button's **Interactable** is checked

### Issue 4: API Connection Fails

**Symptom**: No events or data loading

**Solution**:
1. Check backend is running:
   ```bash
   curl http://localhost:3000/api/health
   ```
2. Verify GameManager's **API URL** is correct
3. Check Unity Console for specific error messages
4. Ensure macOS firewall isn't blocking localhost:3000

### Issue 5: Scene Transitions Not Working

**Symptom**: Clicking play doesn't change scenes

**Solution**:
1. Verify `SceneManager.LoadScene()` is being called
2. Check scenes are in Build Settings
3. Verify scene names match exactly (case-sensitive)

---

## Next Steps

### Recommended UI Enhancements

1. **Add Visual Assets**
   - Background images for each scene
   - Event icons (career, health, relationships)
   - Avatar sprites

2. **Add Animations**
   - Button hover effects
   - Scene transitions (fade in/out)
   - Event popup animations

3. **Add Audio**
   - Background music
   - Sound effects (button clicks, event notifications)
   - Decision confirmation sounds

4. **Add Polish**
   - Loading spinners for API calls
   - Error messages for failed requests
   - Success animations for rewards

### Testing Checklist

Before submitting to App Store:

- [ ] All scenes load without errors
- [ ] All buttons are responsive
- [ ] API calls work in production
- [ ] Game loop works end-to-end
- [ ] Stats update correctly
- [ ] Timeline displays events
- [ ] Character creation saves data
- [ ] Build runs on physical iOS device
- [ ] No memory leaks
- [ ] Performance is smooth (60 FPS)

---

## Resources

### Unity Documentation
- [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/index.html)
- [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)

### Project-Specific
- [Postman Collection](../postman/LifeCraft-API-Collection.json)
- [API Documentation](../../README.md#api-documentation)
- [PROJECT_STATUS.md](../../PROJECT_STATUS.md)

### Troubleshooting
- [Unity Forum](https://forum.unity.com/)
- [Stack Overflow - Unity Tag](https://stackoverflow.com/questions/tagged/unity)

---

## Quick Reference: API Endpoints

Use these in Unity's `ApiClient.cs`:

```
Base URL: http://localhost:3000

Auth:
  POST /api/auth/register
  POST /api/auth/login

Player:
  GET /api/player/:id
  PATCH /api/player/:id/profile

Game:
  POST /api/game
  GET /api/game/:id
  POST /api/game/:id/advance
  POST /api/game/:id/decisions

Events:
  GET /api/events/random
  GET /api/events/templates/:id
  GET /api/events/templates/:id/decisions

Economy:
  GET /api/economy/:playerId
  POST /api/economy/transactions

Leaderboard:
  GET /api/leaderboard/global
  GET /api/leaderboard/:playerId

Daily Challenges:
  GET /api/daily-challenge/current
  POST /api/daily-challenge/:id/complete

Milestones:
  GET /api/milestones/:playerId
  POST /api/milestones/:id/claim

VIP:
  GET /api/vip/status/:playerId
  POST /api/vip/subscribe

Avatars:
  GET /api/avatars/:playerId
  GET /api/avatars/shop
  POST /api/avatars/:playerId/equip
```

---

**Happy Building! 🎮**

For issues or questions, refer to the main README.md or PROJECT_STATUS.md.
