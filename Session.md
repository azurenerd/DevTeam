# AgentSquad Session Handoff

> **Purpose:** Everything a new Copilot CLI session needs to get up to speed quickly. Read this file first, then follow the steps below.

---

## 1. Essential Context Documents

Read these in order to understand the project and expectations:

```
Read C:\Git\AgentSquad\Session.md           (this file — session setup)
Read C:\Git\AgentSquad\docs\MonitorPrompt.md (monitoring checklist & failure modes)
Read C:\Git\AgentSquad\docs\Requirements.md  (project requirements)
Read C:\Git\AgentSquad\LessonsLearned.md     (hard-won operational knowledge)
```

Also read the `.github/copilot-instructions.md` (auto-loaded) for architecture, conventions, and build/test commands.

---

## 2. GitHub Reset (Fresh Run)

Before starting a new agent workflow run, fully reset the target GitHub repo:

### Option A: Use the reset script (recommended)
```powershell
# Full reset — closes issues/PRs, deletes branches, resets repo files
.\scripts\fresh-reset.ps1 -GitHubToken "<PAT>"

# Preserve the HTML design reference (default preserves .gitignore + OriginalDesignConcept.html)
.\scripts\fresh-reset.ps1 -GitHubToken "<PAT>" -PreserveFiles "OriginalDesignConcept.html,.gitignore"
```

### Option B: Manual reset steps
1. Close ALL open issues: `gh issue list -s open --json number -q '.[].number' | ForEach-Object { gh issue close $_ }`
2. Close ALL open PRs: `gh pr list -s open --json number -q '.[].number' | ForEach-Object { gh pr close $_ }`
3. Delete ALL non-main branches via GitHub API
4. Reset repo files to only preserved files (`OriginalDesignConcept.html`, `.gitignore`)
5. Delete local SQLite DB: `Remove-Item src\AgentSquad.Runner\agentsquad_default.db* -Force`
6. Delete agent workspaces: `Remove-Item C:\AgentWorkspaces\* -Recurse -Force`

### Verify reset before proceeding
```powershell
# Must all return 0 / empty
gh issue list -s open -R azurenerd/ReportingDashboard
gh pr list -s open -R azurenerd/ReportingDashboard
```

**Important:** The PAT is in `src/AgentSquad.Runner/appsettings.json` under `AgentSquad.Project.GitHubToken`. Note: this user is an Enterprise Managed User (EMU) — `gh issue create` may fail with 403. Use the runner's Octokit integration or direct REST API with the PAT instead.

---

## 3. Building & Running

```powershell
# Build
dotnet build AgentSquad.sln

# Run the runner (dashboard at http://localhost:5050)
cd src\AgentSquad.Runner
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5050"
.\bin\Debug\net8.0\AgentSquad.Runner.exe
```

### Critical runner rules
- **NEVER** use `dotnet run | Tee-Object` — it kills the runner during Copilot CLI subprocess calls
- **NEVER** kill processes by name (`Stop-Process -Name`, `taskkill /IM`) — it kills your own CLI session
- **Always** run the exe directly in async mode: `.\bin\Debug\net8.0\AgentSquad.Runner.exe`
- **Always** stop the runner before building (file locks on DLLs)
- Find runner PID: `Get-Process AgentSquad.Runner`
- Stop runner: `Stop-Process -Id <PID>`

---

## 4. Monitoring Expectations

Read `docs/MonitorPrompt.md` for the full checklist. Key points:

### What to watch
1. **Phase progression**: Research → PM Spec → Architecture → Engineering Planning → Development → Testing → Review → Complete
2. **Agent status cycles**: Idle → Working → Idle is normal. Idle → Idle → Idle with open work = stuck.
3. **PR pipeline per engineering PR**: created → `ready-for-review` → Architect review → `architect-approved` → TE tests → `tests-added` → PM review → `pm-approved` → PE merge
4. **Rate limiting**: GitHub API limit is 5000/hr. Runner has `RateLimitManager` (SemaphoreSlim 10 slots). Watch for `Rate limit exceeded` in logs.

### SQL monitoring tables
```sql
-- Track PRs through review pipeline
CREATE TABLE IF NOT EXISTS pr_monitor (
    pr_number INTEGER PRIMARY KEY, title TEXT, author TEXT,
    phase TEXT, status TEXT, last_checked TEXT
);

-- Track overall run progress
CREATE TABLE IF NOT EXISTS run_monitor (
    id INTEGER PRIMARY KEY, phase TEXT, started_at TEXT,
    agents_active INTEGER, issues_open INTEGER, prs_open INTEGER
);
```

### Red flags (investigate immediately)
- Agent Idle with open work in their phase
- Agent Working >10 minutes on same item
- `RateLimitExceededException` — all API calls pause until reset
- `OperationCanceledException` outside of shutdown — possible deadlock
- TE in "API-only mode" — tests committed without building/running

---

## 5. Dashboard Features

The Blazor Server dashboard at http://localhost:5050 includes:

- **Home**: Agent cards with real-time status, activity logs
- **Project Timeline**: Phase-grouped view of all issues/PRs with:
  - Auto-refresh every 30 seconds
  - Smart run detection (filters to latest run only)
  - Clickable issue/PR numbers (open GitHub in new tab)
  - Detail panel with child issues and linked PRs
- **Workflow**: Phase gate status and signals
- **Notifications**: Gate approval requests

### Timeline data flow
- Issues/PRs fetched from GitHub via `DashboardDataService` (cached, 30s PR / 60s issue expiry)
- `BuildTimeline()` pipeline: run detection → filter → dedup → synthetic doc PRs → parent-child → phase grouping
- Doc phases (Research, PM Spec, Architecture) appear as synthetic nodes from PRs
- Engineering tasks filtered to latest burst (30-min window from newest)

---

## 6. Key Configuration

Config is in `src/AgentSquad.Runner/appsettings.json` (gitignored). Template: `appsettings.template.json`.

Key settings:
- `AgentSquad.Project.GitHubRepo`: `"azurenerd/ReportingDashboard"`
- `AgentSquad.Project.GitHubToken`: PAT for GitHub API
- `AgentSquad.CopilotCli.Enabled`: `true` (routes all AI through `copilot` CLI)
- `AgentSquad.CopilotCli.SinglePassMode`: `true` (single AI call per doc, not multi-turn)
- `AgentSquad.CopilotCli.MaxConcurrentRequests`: `5`
- `AgentSquad.Models`: Per-tier model definitions (premium/standard/budget/local)
- `AgentSquad.Limits.MaxAdditionalEngineers`: `3`

### Model tier strategy
| Tier | Used By | Default Model |
|------|---------|---------------|
| premium | PM, Architect, PE | claude-opus-4.6 |
| standard | Researcher, Senior Engineers, TE | claude-sonnet-4.6 |
| budget | Junior Engineers | gpt-5.2 |

---

## 7. Known Issues & Workarounds

1. **GitHub EMU restrictions**: `gh issue create` fails with 403. Use Octokit via the runner or REST API with PAT.
2. **Rate limiting**: Heavy runs can exhaust the 5000/hr GitHub API limit. The `RateLimitManager` auto-pauses and retries. Dashboard shows rate-limit status.
3. **Stale checkpoint recovery**: Runner uses `WorkflowStateMachine` checkpoint. If resuming an old run, the phase may be wrong. Delete the DB for a fresh start.
4. **Agent workspaces**: TE and engineers clone repos to `C:\AgentWorkspaces\`. These persist across runs — delete for fresh start.
5. **PM issue ordering**: The PM extraction prompt instructs dependency-ordered issue creation (scaffolding first). If issues come out in wrong order, check the extraction prompt in `ProgramManagerAgent.CreateUserStoryIssuesAsync()`.

---

## 8. Quick Start Prompt for New Session

Copy-paste this to start a new Copilot CLI session with full context:

```
Read C:\Git\AgentSquad\Session.md for project context and session setup instructions.
Then read C:\Git\AgentSquad\docs\MonitorPrompt.md for monitoring expectations.
Then read C:\Git\AgentSquad\LessonsLearned.md for operational knowledge.

I want to [DESCRIBE YOUR GOAL HERE — e.g., "do a fresh GitHub reset and start a new agent workflow run and monitor it", "fix a bug in the timeline page", "review the latest run's output"].
```

---

*Last updated: 2026-04-13*
