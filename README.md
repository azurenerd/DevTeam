<div align="center">

# 🤖 AgentSquad

**An AI-powered autonomous development team**

*Orchestrate multiple AI agents with distinct roles to collaboratively develop software — coordinated through GitHub PRs and Issues.*

</div>

<p align="center">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-purple" />
  <img alt="C#" src="https://img.shields.io/badge/C%23-12-blue" />
  <img alt="Blazor" src="https://img.shields.io/badge/Blazor-Server-orange" />
  <img alt="License" src="https://img.shields.io/badge/license-MIT-green" />
</p>

---

AgentSquad is a C# .NET 8 system that creates and manages a team of specialized AI agents — each with a distinct role, model tier, and set of responsibilities — that work together to build software projects. The agents coordinate entirely through GitHub PRs and Issues, with an in-process message bus for real-time orchestration and a Blazor dashboard for monitoring.

## Architecture

```
┌──────────────────────────────────────────────────────────────────────────┐
│                   AgentSquad.Runner (Host, port 5050)                    │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │                   AgentSquad.Orchestrator                         │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────────────────┐  │  │
│  │  │ AgentRegistry │  │ SpawnManager │  │ WorkflowStateMachine   │  │  │
│  │  └──────┬───────┘  └──────┬───────┘  │  Init → Research →    │  │  │
│  │         │                 │           │  Arch → Plan → Dev →  │  │  │
│  │  ┌──────┴───────┐  ┌─────┴────────┐  │  Test → Review → Fin  │  │  │
│  │  │HealthMonitor │  │DeadlockDetect│  └────────────────────────┘  │  │
│  │  └──────────────┘  └──────────────┘                              │  │
│  └──────────────────────────┬───────────────────────────────────────┘  │
│                             │                                          │
│  ┌──────────────────────────┴───────────────────────────────────────┐  │
│  │                  InProcessMessageBus (Channels)                  │  │
│  │              pub/sub: TaskAssignment, StatusUpdate,               │  │
│  │              HelpRequest, ResourceRequest, ReviewRequest          │  │
│  └───┬──────┬──────┬──────────┬──────────┬──────────┬───────────┬──┘  │
│      │      │      │          │          │          │           │      │
│  ┌───┴──┐┌──┴───┐┌─┴────┐┌───┴────┐┌────┴───┐┌────┴───┐┌─────┴──┐   │
│  │  PM  ││Rsrchr││Archt ││Prncpl  ││Senior  ││Junior  ││  Test  │   │
│  │Agent ││Agent ││Agent ││Eng.    ││Eng.(n) ││Eng.(n) ││  Eng.  │   │
│  └───┬──┘└──┬───┘└─┬────┘└───┬────┘└────┬───┘└────┬───┘└────┬───┘   │
│      └──────┴──────┴─────────┴──────────┴─────────┴──────────┘       │
│                    GitHubService (60s TTL cache)                       │
│                    REST API (/api/dashboard/*)                         │
│                    CopilotCliChatCompletionService                     │
└─────────────────────────┬───────────────┬───────────────────────────┘
                          │               │
           ┌──────────────┴──────┐  ┌─────┴──────────────────────────┐
           │   GitHub (Remote)   │  │  Dashboard.Host (port 5051)    │
           │  PRs │ Issues │ Files│  │  Blazor Server (standalone)    │
           │  Research.md        │  │  HttpDashboardDataService      │
           │  PMSpec.md          │  │  → calls Runner REST API       │
           │  Architecture.md    │  │  Pages: Overview, Timeline,    │
           │  EngineeringPlan.md │  │  Metrics, PRs, Issues, Team    │
           └─────────────────────┘  └────────────────────────────────┘
```

## Features

- **7 Specialized Agent Roles** — Program Manager, Researcher, Architect, Principal Engineer, Senior Engineer, Junior Engineer, and Test Engineer — each with distinct responsibilities and AI behaviors
- **Copilot CLI Integration** — Default AI provider routes all tiers through the GitHub Copilot CLI (`copilot` binary) with automatic fallback to direct API keys. Process-per-request model with concurrency limiting.
- **Multi-Model Support** — Anthropic Claude, OpenAI GPT, Azure OpenAI, and local Ollama models with configurable tier assignments (premium / standard / budget / local)
- **GitHub-Native Coordination** — Agents communicate and deliver work through real GitHub PRs and Issues with structured conventions for titles, labels, and branches
- **Dynamic Agent Scaling** — The PM can request additional Senior/Junior Engineers at runtime; the Orchestrator enforces configurable limits
- **Standalone Dashboard** — Blazor Server monitoring UI that can run as a separate process (port 5051) from the Runner (port 5050), allowing UI iteration without disrupting running agents
- **Project Timeline** — Visual workflow timeline with PM and Engineering views, PR/Issue type indicators, phase-based grouping, and silent background refresh
- **GitHub API Rate Limit Management** — 60-second TTL in-process cache reduces API calls by ~90%, combined with proactive throttling and smart reset-timestamp pausing
- **SQLite State Persistence** — Checkpoint agent state and activity logs for graceful shutdown and recovery
- **Deadlock Detection** — Wait-for graph analysis detects circular agent dependencies
- **Health Monitoring** — Background service detects stuck agents, tracks task duration, and reports system health
- **Phase-Gated Workflow** — State machine enforces project progression: Initialization → Research → Architecture → Engineering Planning → Parallel Development → Testing → Review → Finalization
- **Vision-Based PR Review** — AI reviewers download and analyze actual screenshots from PR comments using base64-embedded images, catching broken UIs that text-only reviews would miss
- **Human Gate Checkpoints** — Configurable gates pause workflow at key points for human review. Hot-reloadable configuration via `IOptionsMonitor`. Gate presets: Full Auto, Supervised, Full Control.

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A [GitHub Personal Access Token](https://github.com/settings/tokens) with `repo` scope
- [GitHub Copilot CLI](https://github.com/features/copilot) v1.0.18+ (default AI provider — routes all model tiers through `copilot` binary)
- **Or** at least one AI provider API key as fallback:
  - [Anthropic API key](https://console.anthropic.com/) (recommended for premium tier)
  - [OpenAI API key](https://platform.openai.com/api-keys)
  - [Ollama](https://ollama.ai/) installed locally (for local/free tier)

### 1. Clone and Build

```bash
git clone <repository-url>
cd AgentSquad
dotnet build
```

### 2. Configure

Edit `src/AgentSquad.Runner/appsettings.json` with your settings, or run the interactive wizard on first launch:

```json
{
  "AgentSquad": {
    "Project": {
      "Name": "my-project",
      "Description": "A brief description of what to build",
      "GitHubRepo": "owner/repo",
      "GitHubToken": "ghp_...",
      "DefaultBranch": "main"
    },
    "Models": {
      "premium":  { "Provider": "Anthropic", "Model": "claude-opus-4-20250514", "ApiKey": "sk-ant-..." },
      "standard": { "Provider": "Anthropic", "Model": "claude-sonnet-4-20250514", "ApiKey": "sk-ant-..." },
      "budget":   { "Provider": "OpenAI",    "Model": "gpt-4o-mini",            "ApiKey": "sk-..." },
      "local":    { "Provider": "Ollama",    "Model": "deepseek-coder-v2",      "Endpoint": "http://localhost:11434" }
    }
  }
}
```

### 3. Run

```bash
cd src/AgentSquad.Runner
dotnet run
```

### 4. Open the Dashboard

The dashboard runs embedded in the Runner at `http://localhost:5050`.

**Standalone mode** (recommended for UI development):

```bash
cd src/AgentSquad.Dashboard.Host
dotnet run
```

This launches the dashboard on `http://localhost:5051` as an independent process. The Runner continues on port 5050 — restarting the dashboard won't affect running agents.

## Configuration

Configuration lives in `src/AgentSquad.Runner/appsettings.json` under the `AgentSquad` section:

| Section | Description |
|---------|-------------|
| `Project` | GitHub repo, PAT, project name/description, default branch |
| `Models` | Model tier definitions — provider, model name, API key, endpoint, temperature, max tokens |
| `Agents` | Per-role model tier assignments and token limits |
| `Limits` | Max additional engineers, daily token budget, poll intervals, timeouts, concurrency |
| `Dashboard` | Dashboard port and SignalR toggle |

See [docs/setup-guide.md](docs/setup-guide.md) for a detailed walkthrough of every configuration option.

## Agent Roles

| Role | Default Model Tier | Responsibilities |
|------|-------------------|------------------|
| **Program Manager** | `premium` | Orchestrates team, manages resources, triages blockers, reviews PR alignment, updates tracking |
| **Researcher** | `standard` | Conducts multi-turn technical research, produces Research.md with findings and recommendations |
| **Architect** | `premium` | Designs system architecture (5-turn AI conversation), produces Architecture.md, reviews PRs for alignment |
| **Principal Engineer** | `premium` | Creates engineering plan, assigns tasks to team, handles high-complexity work, reviews engineer PRs |
| **Senior Engineer** | `standard` | Implements medium-complexity tasks with 3-turn AI (plan → implement → self-review) |
| **Junior Engineer** | `budget` | Implements low-complexity tasks with self-validation retries, escalates when task exceeds capability |
| **Test Engineer** | `standard` | Scans for untested PRs, generates test plans, creates test PRs with coverage documentation |

See [docs/agent-behaviors.md](docs/agent-behaviors.md) for detailed behavior documentation for each agent.

## Dashboard

The Blazor Server dashboard provides real-time visibility into the agent team. It can run embedded in the Runner or as a standalone process.

| Page | Route | Description |
|------|-------|-------------|
| **Agent Overview** | `/` | Grid of all agents with status badges, model selectors, chat, error tracking, and deadlock alerts |
| **Project Timeline** | `/timeline` | Visual workflow timeline with PM/Engineering views, phase grouping, PR/Issue type indicators |
| **Metrics** | `/metrics` | System health, utilization ring chart, status breakdown, longest-running tasks |
| **Health Monitor** | `/health` | Real-time health checks, stuck agent detection, system diagnostics |
| **Pull Requests** | `/pullrequests` | GitHub PR browser with state filters, labels, and branch info |
| **Issues** | `/issues` | GitHub issue browser with label/assignee filters and sorting |
| **Engineering Plan** | `/engineering-plan` | Interactive Cytoscape.js dependency graph of engineering tasks |
| **Team View** | `/team` | Visual office-metaphor layout with agent desks and connection lines |
| **Director CLI** | `/director-cli` | Terminal interface for issuing executive directives to agents |
| **Approvals** | `/approvals` | Human gate approval management with filter buttons |
| **Configuration** | `/configuration` | Settings editor, gate presets, GitHub cleanup (embedded mode only) |
| **Agent Detail** | `/agent/{id}` | Deep dive into a single agent with pause/resume/terminate controls |

<!-- TODO: Add dashboard screenshots here -->

## Project Structure

```
AgentSquad/
├── AgentSquad.sln
├── src/
│   ├── AgentSquad.Core/              # Shared abstractions and infrastructure
│   │   ├── Agents/                   # AgentBase, IAgent, AgentRole, AgentStatus, AgentMessage
│   │   ├── AI/                       # CopilotCliChatCompletionService, ProcessManager, Watchdog
│   │   ├── Configuration/            # Config models, validation, wizard, ModelRegistry
│   │   ├── GitHub/                   # GitHubService (60s TTL cache), rate limiting, PR/Issue workflows
│   │   ├── Messaging/                # IMessageBus, InProcessMessageBus (Channels-based)
│   │   └── Persistence/              # AgentStateStore (SQLite), ProjectFileManager
│   │
│   ├── AgentSquad.Agents/            # Concrete agent implementations
│   │   ├── ProgramManagerAgent.cs    # Team orchestration and blocker triage
│   │   ├── ResearcherAgent.cs        # Multi-turn technical research
│   │   ├── ArchitectAgent.cs         # System architecture design
│   │   ├── PrincipalEngineerAgent.cs # Engineering planning and task assignment
│   │   ├── SeniorEngineerAgent.cs    # Medium-complexity implementation
│   │   ├── JuniorEngineerAgent.cs    # Low-complexity with self-validation
│   │   ├── TestEngineerAgent.cs      # Test plan generation and execution
│   │   └── AgentFactory.cs           # DI-based agent creation
│   │
│   ├── AgentSquad.Orchestrator/      # Runtime coordination
│   │   ├── AgentRegistry.cs          # Thread-safe agent lifecycle registry
│   │   ├── AgentSpawnManager.cs      # Dynamic agent spawning with limits
│   │   ├── WorkflowStateMachine.cs   # Phase-gated project progression
│   │   ├── DeadlockDetector.cs       # Wait-for graph cycle detection
│   │   ├── HealthMonitor.cs          # Stuck agent detection and health snapshots
│   │   └── GracefulShutdownHandler.cs# Clean shutdown with state persistence
│   │
│   ├── AgentSquad.Dashboard/         # Real-time monitoring UI (shared library)
│   │   ├── Components/Pages/         # Blazor pages (Overview, Timeline, Metrics, etc.)
│   │   ├── Hubs/AgentHub.cs          # SignalR hub for push updates
│   │   └── Services/                 # IDashboardDataService, HttpDashboardDataService
│   │
│   ├── AgentSquad.Dashboard.Host/    # Standalone dashboard process (port 5051)
│   │   ├── Program.cs                # Independent Blazor host with IHttpClientFactory
│   │   └── StandaloneServiceRegistration.cs  # Stub services for standalone mode
│   │
│   └── AgentSquad.Runner/            # Application host (port 5050)
│       ├── Program.cs                # DI setup, REST API endpoints, service registration
│       ├── AgentSquadWorker.cs       # Bootstrap: spawns core agents in phased sequence
│       └── appsettings.json          # Configuration file (gitignored)
│
├── tests/
│   ├── AgentSquad.Core.Tests/
│   ├── AgentSquad.Agents.Tests/
│   └── AgentSquad.Integration.Tests/
│
├── scripts/
│   └── fresh-reset.ps1               # Clean all GitHub artifacts for fresh run
│
└── docs/
    ├── Requirements.md                # Detailed requirements with workflow scenarios
    ├── MonitorPrompt.md               # Dashboard monitoring expectations
    └── LessonsLearned.md              # Operational lessons from 70+ runs
```

## Development

### Build

```bash
dotnet build AgentSquad.sln
```

### Test

```bash
dotnet test AgentSquad.sln
```

### Run in Development

```bash
cd src/AgentSquad.Runner
dotnet run --environment Development
```

The dashboard runs on the configured port (default `5050`). The Runner bootstraps the core agents and enters a steady-state loop where the PM manages all further coordination.

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make your changes with tests
4. Run `dotnet build && dotnet test` to verify
5. Submit a pull request

## Technology Stack

| Component | Technology |
|-----------|-----------|
| Runtime | .NET 8 / C# 12 |
| AI Integration | Microsoft Semantic Kernel |
| AI Providers | GitHub Copilot CLI (default), Anthropic Claude, OpenAI GPT, Azure OpenAI, Ollama |
| GitHub Integration | Octokit.net |
| Dashboard | Blazor Server + SignalR (embedded or standalone) |
| Persistence | SQLite via Microsoft.Data.Sqlite |
| Message Bus | System.Threading.Channels (in-process pub/sub) |
| Dependency Injection | Microsoft.Extensions.DependencyInjection |
| Hosting | Microsoft.Extensions.Hosting (Generic Host) |

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
