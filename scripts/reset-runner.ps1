<#
.SYNOPSIS
    Full reset: stop runner, clean GitHub repo, delete local workspaces and DB.
.DESCRIPTION
    Performs a complete cleanup so the next runner start begins from a fresh state:
    1. Stops any running AgentSquad runner process
    2. Closes all open issues and PRs on the target GitHub repo
    3. Deletes all agent branches (keeps main/master only)
    4. Deletes agent-generated markdown docs from the repo (Research.md, PMSpec.md, etc.)
    5. Deletes local agent workspace directories (C:\Agents by default)
    6. Deletes the SQLite checkpoint/state DB
.EXAMPLE
    .\scripts\reset-runner.ps1
    .\scripts\reset-runner.ps1 -WorkspaceRoot "D:\AgentWorkspaces" -SkipGitHub
#>
param(
    [string]$WorkspaceRoot = "C:\Agents",
    [string]$SettingsPath = (Join-Path $PSScriptRoot ".." "src" "AgentSquad.Runner" "appsettings.json"),
    [switch]$SkipGitHub,
    [switch]$SkipLocal
)

$ErrorActionPreference = "Continue"

Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  AgentSquad Full Reset" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Cyan

# ── Phase 1: Stop runner ────────────────────────────
Write-Host "`n▶ Phase 1/4: Stopping runner..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "stop-runner.ps1") -ErrorAction SilentlyContinue

# ── Phase 2: Clean GitHub ───────────────────────────
if (-not $SkipGitHub) {
    Write-Host "`n▶ Phase 2/4: Cleaning GitHub repository..." -ForegroundColor Yellow

    # Read PAT and repo from appsettings.json
    if (-not (Test-Path $SettingsPath)) {
        Write-Host "  ⚠ appsettings.json not found at $SettingsPath — skipping GitHub cleanup" -ForegroundColor Red
    } else {
        $settings = Get-Content $SettingsPath -Raw | ConvertFrom-Json
        $pat = $settings.AgentSquad.Project.GitHubToken
        $repo = $settings.AgentSquad.Project.GitHubRepo
        $headers = @{ Authorization = "token $pat"; Accept = "application/vnd.github+json" }

        Write-Host "  Repo: $repo" -ForegroundColor Gray

        # Close all open issues
        $page = 1
        $closedIssues = 0
        do {
            $issues = Invoke-RestMethod "https://api.github.com/repos/$repo/issues?state=open&per_page=100&page=$page" -Headers $headers -ErrorAction SilentlyContinue
            foreach ($i in $issues) {
                if ($i.pull_request) { continue }
                Invoke-RestMethod "https://api.github.com/repos/$repo/issues/$($i.number)" -Method Patch -Headers $headers -Body '{"state":"closed"}' -ContentType 'application/json' -ErrorAction SilentlyContinue | Out-Null
                $closedIssues++
            }
            $page++
        } while ($issues.Count -eq 100)
        Write-Host "  Closed $closedIssues issues" -ForegroundColor Green

        # Close all open PRs
        $closedPrs = 0
        $prs = Invoke-RestMethod "https://api.github.com/repos/$repo/pulls?state=open&per_page=100" -Headers $headers -ErrorAction SilentlyContinue
        foreach ($pr in $prs) {
            Invoke-RestMethod "https://api.github.com/repos/$repo/pulls/$($pr.number)" -Method Patch -Headers $headers -Body '{"state":"closed"}' -ContentType 'application/json' -ErrorAction SilentlyContinue | Out-Null
            $closedPrs++
        }
        Write-Host "  Closed $closedPrs PRs" -ForegroundColor Green

        # Delete all non-main branches
        $deletedBranches = 0
        $branches = Invoke-RestMethod "https://api.github.com/repos/$repo/branches?per_page=100" -Headers $headers -ErrorAction SilentlyContinue
        foreach ($b in $branches) {
            if ($b.name -eq "main" -or $b.name -eq "master") { continue }
            Invoke-RestMethod "https://api.github.com/repos/$repo/git/refs/heads/$($b.name)" -Method Delete -Headers $headers -ErrorAction SilentlyContinue | Out-Null
            $deletedBranches++
        }
        Write-Host "  Deleted $deletedBranches branches" -ForegroundColor Green

        # Reset repo to baseline if BaselineCommitSha is configured
        $baselineCommit = $settings.AgentSquad.Project.BaselineCommitSha
        if ($baselineCommit) {
            Write-Host "  Resetting repo to baseline commit $($baselineCommit.Substring(0,8))..." -ForegroundColor Cyan
            try {
                $baseline = Invoke-RestMethod "https://api.github.com/repos/$repo/commits/$baselineCommit" -Headers $headers
                $baseTreeSha = $baseline.commit.tree.sha

                $mainRef = Invoke-RestMethod "https://api.github.com/repos/$repo/git/ref/heads/main" -Headers $headers
                $newCommit = @{
                    message = "Reset repo to baseline for fresh agent run"
                    tree = $baseTreeSha
                    parents = @($mainRef.object.sha)
                } | ConvertTo-Json -Depth 5
                $commit = Invoke-RestMethod "https://api.github.com/repos/$repo/git/commits" -Method Post -Headers $headers -Body $newCommit -ContentType 'application/json'

                $updateRef = @{ sha = $commit.sha; force = $false } | ConvertTo-Json
                Invoke-RestMethod "https://api.github.com/repos/$repo/git/refs/heads/main" -Method Patch -Headers $headers -Body $updateRef -ContentType 'application/json' | Out-Null
                Write-Host "  Repo reset to baseline $($commit.sha.Substring(0,8))" -ForegroundColor Green
            } catch {
                Write-Host "  ⚠ Baseline reset failed: $_" -ForegroundColor Red
            }
        } else {
            Write-Host "  ⚠ No BaselineCommitSha configured — skipping repo file reset" -ForegroundColor Yellow
            Write-Host "    Set AgentSquad.Project.BaselineCommitSha in appsettings.json to enable" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "`n▶ Phase 2/4: Skipping GitHub cleanup (--SkipGitHub)" -ForegroundColor Gray
}

# ── Phase 3: Clean local workspaces ─────────────────
if (-not $SkipLocal) {
    Write-Host "`n▶ Phase 3/4: Cleaning local agent workspaces..." -ForegroundColor Yellow

    if (Test-Path $WorkspaceRoot) {
        $dirs = Get-ChildItem -Path $WorkspaceRoot -Directory -ErrorAction SilentlyContinue
        foreach ($d in $dirs) {
            Remove-Item -Path $d.FullName -Recurse -Force -ErrorAction SilentlyContinue
        }
        Write-Host "  Deleted $($dirs.Count) workspace directories from $WorkspaceRoot" -ForegroundColor Green
    } else {
        Write-Host "  No workspace directory at $WorkspaceRoot" -ForegroundColor Gray
    }
} else {
    Write-Host "`n▶ Phase 3/4: Skipping local cleanup (--SkipLocal)" -ForegroundColor Gray
}

# ── Phase 4: Delete checkpoint DB ────────────────────
Write-Host "`n▶ Phase 4/4: Deleting checkpoint database..." -ForegroundColor Yellow
$runnerDir = Join-Path $PSScriptRoot ".." "src" "AgentSquad.Runner"
$dbFiles = Get-ChildItem -Path $runnerDir -Filter "agentsquad_*.db*" -ErrorAction SilentlyContinue
if ($dbFiles) {
    foreach ($db in $dbFiles) {
        Remove-Item -Path $db.FullName -Force -ErrorAction SilentlyContinue
        Write-Host "  Deleted $($db.Name)" -ForegroundColor Green
    }
} else {
    Write-Host "  No DB files found" -ForegroundColor Gray
}

Write-Host "`n═══════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ Reset complete. Run start-runner.ps1 to begin fresh." -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Green
