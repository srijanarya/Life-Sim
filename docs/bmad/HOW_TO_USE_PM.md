# How to Use BMAD PM Agent - Step-by-Step Guide

## Quick Start (5 Minutes)

### Option 1: Use BMAD in Your IDE (Recommended)

BMAD works best when integrated with your IDE. Here's how:

**For VS Code / Cursor / Windsurf / Claude Code:**

1. **Open LifeCraft project**
   ```
   /Users/srijan/Desktop/LIFE SIMULATOR/
   ```

2. **Create a new file** for brainstorming

   Create a file named something like `brainstorm-with-pm.md`

3. **Type the BMAD command** to invoke PM agent

   ```markdown
   *load-agent pm
   ```

4. **Add your request**

   Below the command, type your brainstorming request, for example:

   ```markdown
   *load-agent pm

   As a BMAD Product Manager for LifeCraft...

   I want to brainstorm engagement and monetization features for the core loop.

   Focus on:
   - Daily challenges and retention mechanics
   - Fair IAP design
   - Social features that drive virality
   ```

5. **Save the file**

   The PM agent will respond directly in your IDE!

---

### Option 2: Use Pre-Created Brainstorming Document

I've already created a brainstorming document at:
- `docs/bmad/brainstorming-output.md`

You can:
1. **Review the 12 feature ideas** I brainstormed
2. **Pick features** to implement
3. **Use PM agent** to create PRDs and user stories

---

### Option 3: Use BMAD Workflows Directly

Use the pre-created workflow files:
- `docs/bmad/sprint-1-core-loop.yaml` - For Sprint 1 work
- `docs/bmad/sprint-2-event-engine.yaml` - For Sprint 2 work
- `docs/bmad/quick-spec-workflow.yaml` - For bug fixes

---

## BMAD PM Agent Commands

| Command | What It Does | Example Use |
|---------|---------------|-------------|
| `*create-prd` | Create Product Requirements Document | Start of feature planning |
| `*create-epics-and-stories` | Break into user stories | For development backlog |
| `*brainstorm-project` | Guided brainstorming | For ideation phase |
| `*implementation-readiness` | Validate planning artifacts | Before implementation |

---

## Example: Creating a PRD

**1. Create file:** `create-prd.md`

**2. Type BMAD command:**
   ```markdown
   *load-agent pm

   As a BMAD Product Manager for LifeCraft...

   I want to create a PRD for the Daily Challenge System feature.

   Target KPIs:
   - D1 Retention >40%
   - D7 Retention >25%
   - Increase daily active users
   ```

**3. Save and wait for response**

The PM agent will respond with:
- Problem statement
- User stories with acceptance criteria
- RICE prioritization
- Technical requirements

---

## Example: Brainstorming with PM

**1. Create file:** `brainstorm-features.md`

**2. Type BMAD command:**
   ```markdown
   *load-agent pm

   As a BMAD Product Manager for LifeCraft...

   I want to brainstorm features that will improve D1 retention.
   Include:
   - Feature ideas
   - Impact assessment
   - Implementation complexity
   ```

**3. Save and wait for response**

The PM agent will:
- Ask clarifying questions
- Suggest feature categories
- Provide structured output

---

## What BMAD PM Agent Can Help With

### 1. Requirements Analysis
- Identify user needs
- Define feature scope
- Set success metrics

### 2. Product Planning
- Create PRDs (Product Requirements Documents)
- Generate user stories
- Prioritize with RICE scoring
- Create sprint backlogs

### 3. Stakeholder Management
- Define target audience
- Competitive analysis
- Market research

### 4. Feature Definition
- Epic breakdown
- Story point estimation
- Acceptance criteria definition

### 5. KPI Alignment
- Map features to KPIs
- Define success metrics
- Impact forecasting

---

## Ready to Start!

Choose one of these options:

### A. Try Interactive BMAD Now
1. Open your IDE (VS Code / Cursor / Windsurf / Claude Code)
2. Create a new file
3. Type: `*load-agent pm`
4. Add your brainstorming request below
5. Save and interact with PM agent!

### B. Review Pre-Created Ideas
1. Open: `docs/bmad/brainstorming-output.md`
2. Review 12 features I already brainstormed
3. Pick features to develop
4. Use PM agent to create PRDs

### C. Use Workflow Files
1. Use: `docs/bmad/sprint-1-core-loop.yaml`
2. Follow structured workflow phases
3. Generate artifacts automatically

---

## Troubleshooting

### BMAD Command Doesn't Trigger

If typing `*load-agent pm` doesn't show BMAD options:

1. **Check `.bmadrc.json` exists** in project root
2. **Ensure BMAD is installed**: `npm list -g | grep bmad`
3. **Restart IDE** if necessary
4. **Create a new file** and try the command again

### "Agent Not Found" Error

This means BMAD isn't configured. Run:
```bash
npx bmad-method@alpha install
```

---

**Choose option A, B, or C and let me know!**

## Updated Strategic Direction

### IMPORTANT UPDATE: Inspired-By Approach (Not Cloning)

The brainstorming document has been updated to reflect a **new strategic direction**:

**Core Philosophy:**
- Build LifeCraft as a unique game inspired by successful titles (Side Hustle Sim, BitLife, Idle Business Tycoon)
- Study proven mechanics from reference games, adapt and innovate
- Avoid direct cloning of any single game (legal/IP risk)

**Expanded Vision:**
LifeCraft is now a **Life + Business Tycoon hybrid** with three interconnected systems:
1. Core life simulation (events, careers, relationships)
2. Side hustle businesses (employees, automation)
3. Stock market (live trading, IPOs, real companies)
4. Cross-promotion system (YouTube integration, partner games)

**New Sprint 4 Workflow Available:**
- `docs/bmad/sprint-4-partnership-launch.yaml` - Partnership launch workflow

**Key Benefits of This Approach:**
- ✅ Legal/IP safety (inspired-by, not cloning)
- ✅ Your brand and platform ownership
- ✅ Full data and analytics
- ✅ Unique differentiation (stock market, business IPOs don't exist in reference games)
- ✅ Strategic partnership opportunities (control your cross-promotion)
- ✅ Faster development time (build on proven concepts)

This updated approach is now the foundation for the BMAD planning phase.

