# BMAD Method Setup for LifeCraft

BMAD Method is now configured for LifeCraft project!

## What Was Created

- `.bmadrc.json` - BMAD configuration file
- `_bmad-output/` - Output directory for BMAD artifacts
- `_bmad-core/` - BMAD core data storage

## BMAD Configuration

Your `.bmadrc.json` contains:
- **Output Paths**:
  - Planning artifacts → `_bmad-output/planning-artifacts`
  - Implementation artifacts → `_bmad-output/implementation-artifacts`
  - Long-term knowledge → `docs/`
- **Modules**:
  - BMM (BMad Method) - Enabled for greenfield development
  - BMB (BMad Builder) - Disabled (custom modules)
  - CIS (Creative Intelligence) - Disabled
- **Project Info**:
  - Name: LifeCraft - Real Life Simulator
  - Platform: iOS
  - Stack: Unity + Node.js/TypeScript

## How to Use BMAD Agents

Since the automated installer had issues, here's how to use BMAD manually:

### Option 1: In Your IDE (Recommended)

BMAD works with IDE integration. Load the PM agent workflow:

1. In VS Code / Cursor / Windsurf / Claude Code
2. Load any agent file and type: `*workflow-init`
3. The system will analyze your project and recommend a workflow track
4. Follow the agent's guidance through phases

### Option 2: Direct Agent Invocation

Load specific agents and use their commands:

| Agent | Command | What It Does |
|--------|---------|---------------|
| PM | `*create-prd` | Create Product Requirements Document |
| PM | `*implementation-readiness` | Validate planning artifacts |
| Architect | `*create-architecture` | Create system architecture |
| UX Designer | `*create-ux-design` | Generate UX design |
| Developer | `*dev-story` | Implement a user story |
| Test Architect | `*test-design` | Create test strategy |

### Option 3: Workflow Files

Use the pre-built BMAD workflows from `docs/bmad/`:

- `sprint-1-core-loop.yaml` - Core Loop Prototype workflow
- `sprint-2-event-engine.yaml` - Event Engine workflow
- `quick-spec-workflow.yaml` - Bug fixes & small features

## BMAD Phases for LifeCraft

### Phase 1: Analysis (Optional)
- Brainstorm feature ideas
- Research competitors (BitLife, The Sims Mobile)
- Market analysis

### Phase 2: Planning
- Create PRD with user stories
- RICE prioritization
- Epics breakdown

### Phase 3: Solutioning
- System architecture design
- UX/UI wireframes
- Technical specifications

### Phase 4: Implementation
- Sprint planning
- Story-driven development
- Code review
- Testing

## Available Workflows

### BMad Method Track (Full Planning)
For new features and major work:
1. Load PM agent: `*workflow-init`
2. Run `*create-prd` to get Product Requirements Document
3. Load Architect agent: `*create-architecture`
4. Load UX Designer: `*create-ux-design`
5. Validate: `*implementation-readiness`
6. Load SM agent: `*sprint-planning`
7. Load Developer: `*dev-story` (repeat per story)

### Quick Flow Track (Bug Fixes / Small Features)
For quick changes:
1. Load Quick Flow Solo Dev: `*create-tech-spec`
2. Implement: `*quick-dev`
3. Review: `*code-review`

## Recommended First Steps

### 1. Initialize BMAD Workflow

Open your IDE and run:
```
*workflow-init
```

This will:
- Analyze your project structure
- Recommend the best BMAD track
- Set up workflow tracking

### 2. Start with PM Agent

Load PM agent and create a PRD:
```
As a Product Manager for LifeCraft

I want to create a PRD for [feature name]

Focus on:
- User stories with acceptance criteria
- RICE prioritization
- Epic breakdown
```

### 3. Architecture Design

Load Architect agent:
```
As a Technical Architect for LifeCraft

I want to design the architecture for [feature]

Focus on:
- System components
- Data flow
- API design
- Database schema impact
```

## Project-Specific BMAD Guidelines

### For Unity iOS Client
- Use Unity-specific patterns (ScriptableObjects, Coroutines)
- Consider mobile constraints (performance, battery, memory)
- Design for touch interactions

### For Node.js Backend
- Follow REST API best practices
- Use TypeScript for type safety
- Leverage existing services (Player, Event Engine, Decision System)

### For Life Simulation Game
- Event-driven architecture
- Stat system integration
- Decision outcome tracking
- Progression mechanics

## File Structure with BMAD

```
LIFE SIMULATOR/
├── .bmadrc.json                    # BMAD config
├── _bmad-output/                    # BMAD artifacts
│   ├── planning-artifacts/            # PRD, UX, Architecture
│   └── implementation-artifacts/        # Sprint logs, story tracking
├── _bmad-core/                       # BMAD runtime data
├── docs/
│   └── bmad/                        # BMAD workflows
├── server/
│   └── src/
├── client/
│   └── Assets/
└── shared/
    └── types/
```

## Troubleshooting

### If Agents Don't Appear in IDE

BMAD may need IDE-specific setup. Check:
- VS Code: Ensure BMAD extension is installed
- Cursor: BMAD is built-in
- Windsurf: BMAD is built-in
- Claude Code: BMAD is built-in

### If You Get "Agent Not Found" Errors

Ensure:
1. `.bmadrc.json` exists in project root
2. Project path is correct
3. BMAD is installed globally or via npx

### Manual Agent Loading

If IDE integration doesn't work, you can:
1. Download agent files from: https://github.com/bmad-code-org/BMAD-METHOD/tree/main/src/modules
2. Load agent file directly in your IDE
3. Type agent commands (e.g., `*create-prd`)

## Next Steps

1. Open project in your preferred IDE
2. Run `*workflow-init` to get started
3. Choose your first feature to work on
4. Follow BMAD phases to completion

## Resources

- **BMAD Documentation**: https://github.com/bmad-code-org/BMAD-METHOD
- **PM Agent Guide**: See `docs/bmad/` for workflow examples
- **Agent Commands**: Load any agent and type `*menu` for options

---

**BMAD initialized for LifeCraft! Ready for agile AI-driven development.**
