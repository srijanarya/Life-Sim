# Quick Start - Using BMAD PM Agent in Your IDE

## BMAD is Ready for LifeCraft! ✅

BMAD Method v6.0.0-alpha.22 has been configured:
- **Project**: LifeCraft - Real Life Simulator
- **Track**: Greenfield (new project development)
- **Module**: BMM (BMad Method) - Enabled
- **Output**: Planning artifacts → `_bmad-output/planning-artifacts`

---

## How to Use BMAD in Your IDE

### Step 1: Open Project

Open the LifeCraft project in your preferred IDE:
- **VS Code** / **Cursor** / **Windsurf** / **Claude Code**
- Navigate to: `/Users/srijan/Desktop/LIFE SIMULATOR/`

### Step 2: Initialize BMAD Workflow

Create a new file in your IDE or use the built-in BMAD command:

**Option A: Load PM Agent (Recommended)**

Create a file with content:
```
*workflow-init
```

Then **Save** the file. BMAD will:
1. Analyze your project structure
2. Recommend the best BMAD track
3. Display workflow options
4. Guide you through phases

**Option B: Direct PM Agent Load**

Create a file with content:
```
*load-agent pm
```

Then the PM agent will guide you through:
- User story creation
- RICE prioritization
- Epic breakdown
- PRD creation

---

## PM Agent Capabilities

The BMAD PM Agent can help you with:

| Command | What It Does |
|---------|---------------|
| `*create-prd` | Create Product Requirements Document |
| `*create-epics-and-stories` | Break PRD into epics and user stories |
| `*implementation-readiness` | Validate planning artifacts |
| `*brainstorm-project` | Guided brainstorming session |
| `*product-brief` | Create a product brief |
| `*correct-course` | Course correction during development |

---

## Recommended Workflow for LifeCraft

### Phase 1: Planning (Start Here)

1. **Load PM Agent** with `*load-agent pm` or `*workflow-init`
2. **Run `*create-prd`** to create a Product Requirements Document
3. **Run `*brainstorm-project`** for feature ideation
4. **Review output** in `_bmad-output/planning-artifacts/`

### Phase 2: Solutioning

5. **Load Architect Agent** with `*load-agent architect`
6. **Run `*create-architecture`** for system design
7. **Load UX Designer Agent** with `*load-agent ux-designer`
8. **Run `*create-ux-design`** for UI/UX planning
9. **Run `*implementation-readiness`** to validate all planning

### Phase 3: Implementation

10. **Load SM Agent** with `*load-agent sm`
11. **Run `*sprint-planning`** to create sprint backlog
12. **Load DEV Agent** with `*load-agent dev`
13. **Run `*create-story`** and `*dev-story`** for each story

---

## Using the Brainstorming Output

I've already created:
- `docs/bmad/brainstorming-output.md` - 12 feature ideas with RICE scores
- `docs/BMAD_SETUP.md` - Setup guide

You can reference these when using the PM agent!

---

## Quick Test: Try It Now

**In your IDE, create a new file named `test-bmad.md` and paste:**

```
*load-agent pm

As the BMAD Product Manager for LifeCraft...

I want to brainstorm engagement and monetization features for a mobile life simulation game.
```

**Then save the file.** The PM agent will respond with ideas and questions!

---

## Troubleshooting

### BMAD Commands Don't Work

If `*workflow-init` doesn't trigger BMAD:
1. Check that `.bmadrc.json` exists in project root
2. Ensure BMAD is installed (`npm list -g | grep bmad`)
3. Try creating a file and typing BMAD commands

### Want More Agents?

The PM agent can collaborate with other agents:
- **Architect** (`*load-agent architect`)
- **UX Designer** (`*load-agent ux-designer`)
- **Test Architect** (`*load-agent tea`)
- **Developer** (`*load-agent dev`)

Load multiple agents for **Party Mode** collaboration!

---

## Next Steps

1. **Initialize workflow**: Create a file and type `*workflow-init`
2. **Brainstorm features**: Let PM agent guide you through ideation
3. **Review output**: Check `_bmad-output/planning-artifacts/` for generated documents
4. **Start development**: Use created user stories to guide implementation

---

**Ready to build LifeCraft with BMAD! 🚀**
