using System.Collections.Concurrent;
using AgentSquad.Core.Agents;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.GitHub;
using AgentSquad.Core.GitHub.Models;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentSquad.Agents;

public class JuniorEngineerAgent : AgentBase
{
    private readonly IMessageBus _messageBus;
    private readonly IGitHubService _github;
    private readonly PullRequestWorkflow _prWorkflow;
    private readonly IssueWorkflow _issueWorkflow;
    private readonly ProjectFileManager _projectFiles;
    private readonly ModelRegistry _modelRegistry;
    private readonly AgentSquadConfig _config;

    private const int MaxSelfReviewRetries = 2;

    private readonly HashSet<int> _processedIssueIds = new();
    private readonly ConcurrentQueue<ReworkItem> _reworkQueue = new();
    private readonly ConcurrentQueue<IssueAssignmentMessage> _assignmentQueue = new();
    private readonly ConcurrentQueue<ClarificationResponseMessage> _clarificationResponses = new();
    private readonly List<IDisposable> _subscriptions = new();
    private int? _currentIssueNumber;
    private int? _currentPrNumber;

    public JuniorEngineerAgent(
        AgentIdentity identity,
        IMessageBus messageBus,
        IGitHubService github,
        PullRequestWorkflow prWorkflow,
        IssueWorkflow issueWorkflow,
        ProjectFileManager projectFiles,
        ModelRegistry modelRegistry,
        IOptions<AgentSquadConfig> config,
        ILogger<JuniorEngineerAgent> logger)
        : base(identity, logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _github = github ?? throw new ArgumentNullException(nameof(github));
        _prWorkflow = prWorkflow ?? throw new ArgumentNullException(nameof(prWorkflow));
        _issueWorkflow = issueWorkflow ?? throw new ArgumentNullException(nameof(issueWorkflow));
        _projectFiles = projectFiles ?? throw new ArgumentNullException(nameof(projectFiles));
        _modelRegistry = modelRegistry ?? throw new ArgumentNullException(nameof(modelRegistry));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    protected override Task OnInitializeAsync(CancellationToken ct)
    {
        _subscriptions.Add(_messageBus.Subscribe<TaskAssignmentMessage>(
            Identity.Id, HandleTaskAssignmentAsync));

        _subscriptions.Add(_messageBus.Subscribe<IssueAssignmentMessage>(
            Identity.Id, HandleIssueAssignmentAsync));

        _subscriptions.Add(_messageBus.Subscribe<ChangesRequestedMessage>(
            Identity.Id, HandleChangesRequestedAsync));

        _subscriptions.Add(_messageBus.Subscribe<ClarificationResponseMessage>(
            Identity.Id, HandleClarificationResponseAsync));

        Logger.LogInformation("Junior Engineer {Name} initialized, awaiting task assignments",
            Identity.DisplayName);
        return Task.CompletedTask;
    }

    protected override async Task RunAgentLoopAsync(CancellationToken ct)
    {
        UpdateStatus(AgentStatus.Idle, "Ready for task assignments");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Priority 1: Process rework feedback from reviewers
                if (_reworkQueue.TryDequeue(out var rework))
                {
                    await HandleReworkAsync(rework, ct);
                    continue;
                }

                // Priority 2: Process new issue assignments from PE
                if (_assignmentQueue.TryDequeue(out var assignment))
                {
                    await WorkOnIssueAsync(assignment, ct);
                    continue;
                }

                // Priority 3: Check for existing PR work (recovery after restart)
                if (_currentPrNumber is null)
                {
                    var myTasks = await _prWorkflow.GetAgentTasksAsync(Identity.DisplayName, ct);
                    var activePR = myTasks.FirstOrDefault(pr =>
                        string.Equals(pr.State, "open", StringComparison.OrdinalIgnoreCase));

                    if (activePR != null && Identity.AssignedPullRequest != activePR.Number.ToString())
                    {
                        await WorkOnTaskAsync(activePR, ct);
                    }
                    else if (activePR == null)
                    {
                        UpdateStatus(AgentStatus.Idle, "Waiting for task assignment");
                    }
                }

                await CheckForIssuesAsync(ct);

                await Task.Delay(
                    TimeSpan.FromSeconds(_config.Limits.GitHubPollIntervalSeconds), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Junior Engineer {Name} loop error", Identity.DisplayName);
                RecordError($"Loop error: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);
                UpdateStatus(AgentStatus.Error, ex.Message);
                try { await Task.Delay(10_000, ct); }
                catch (OperationCanceledException) { break; }
                UpdateStatus(AgentStatus.Idle, "Recovered from error");
            }
        }

        UpdateStatus(AgentStatus.Offline, "Junior Engineer loop exited");
    }

    protected override Task OnStopAsync(CancellationToken ct)
    {
        foreach (var sub in _subscriptions)
            sub.Dispose();
        _subscriptions.Clear();
        return Task.CompletedTask;
    }

    #region Task Execution

    private async Task WorkOnTaskAsync(AgentPullRequest pr, CancellationToken ct)
    {
        UpdateStatus(AgentStatus.Working, $"Working on PR #{pr.Number}: {pr.Title}");
        Identity.AssignedPullRequest = pr.Number.ToString();

        Logger.LogInformation("Junior Engineer {Name} starting work on PR #{Number}: {Title}",
            Identity.DisplayName, pr.Number, pr.Title);

        try
        {
            // Keep context smaller for local/budget models
            var architectureDoc = await _projectFiles.GetArchitectureDocAsync(ct);
            var pmSpecDoc = await _projectFiles.GetPMSpecAsync(ct);
            var taskTitle = PullRequestWorkflow.ParseTaskTitleFromTitle(pr.Title) ?? pr.Title;
            var techStack = _config.Project.TechStack;

            var kernel = _modelRegistry.GetKernel(Identity.ModelTier, Identity.Id);
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            // Step 1: Break down the task into smaller steps
            var history = new ChatHistory();
            history.AddSystemMessage(
                $"You are a Junior Engineer working on a low-complexity task. " +
                $"The project uses {techStack} as its technology stack. " +
                "Focus on producing correct, simple, and readable code. " +
                "Follow the established patterns in the project architecture " +
                "and ensure your work aligns with the business requirements. " +
                "If the task seems too complex, say so clearly.");

            history.AddUserMessage(
                $"## Business Context (key points)\n{TruncateForContext(pmSpecDoc)}\n\n" +
                $"## Architecture (key sections)\n{TruncateForContext(architectureDoc)}\n\n" +
                $"## Task: {taskTitle}\n{pr.Body}\n\n" +
                "Break this task into small, concrete implementation steps. " +
                "List each step clearly. If this task seems too complex for straightforward " +
                "implementation, start your response with 'COMPLEXITY_WARNING'.");

            var planResponse = await chat.GetChatMessageContentAsync(
                history, cancellationToken: ct);
            var planContent = planResponse.Content ?? "";
            history.AddAssistantMessage(planContent);

            // Check if the model thinks the task is too complex
            if (planContent.Contains("COMPLEXITY_WARNING", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning(
                    "Junior Engineer {Name} detected complex task in PR #{Number}, escalating",
                    Identity.DisplayName, pr.Number);
                await EscalateComplexityAsync(pr, ct);
                return;
            }

            Logger.LogDebug("Junior Engineer {Name} created plan for PR #{Number}",
                Identity.DisplayName, pr.Number);

            // Step 2: Implement with structured file output
            history.AddUserMessage(
                "Now implement the task following your plan. " +
                "Output each file using this exact format:\n\n" +
                "FILE: path/to/file.ext\n```language\n<file content>\n```\n\n" +
                $"Use the {techStack} technology stack. " +
                "Produce complete code for each file. " +
                "Every file MUST use the FILE: marker format so it can be parsed and committed. " +
                "Keep it simple and correct.");

            var implResponse = await chat.GetChatMessageContentAsync(
                history, cancellationToken: ct);
            var implementation = implResponse.Content?.Trim() ?? "";
            history.AddAssistantMessage(implementation);

            // Step 3: Self-validate with retries
            var isValid = false;
            var attempt = 0;

            while (!isValid && attempt < MaxSelfReviewRetries)
            {
                attempt++;
                Logger.LogDebug(
                    "Junior Engineer {Name} self-validation attempt {Attempt} for PR #{Number}",
                    Identity.DisplayName, attempt, pr.Number);

                var (valid, feedback) = await SelfValidateAsync(
                    chat, history, implementation, taskTitle, pr.Body, ct);

                if (valid)
                {
                    isValid = true;
                }
                else
                {
                    // Iterate: ask model to fix its own issues
                    history.AddUserMessage(
                        $"Self-review found issues:\n{feedback}\n\n" +
                        "Please fix these issues and provide the corrected implementation. " +
                        "Use the FILE: marker format for all files.");

                    var fixResponse = await chat.GetChatMessageContentAsync(
                        history, cancellationToken: ct);
                    implementation = fixResponse.Content?.Trim() ?? implementation;
                    history.AddAssistantMessage(implementation);
                }
            }

            if (!isValid)
            {
                Logger.LogWarning(
                    "Junior Engineer {Name} could not resolve self-review issues on PR #{Number} " +
                    "after {MaxRetries} retries, proceeding with best effort",
                    Identity.DisplayName, pr.Number, MaxSelfReviewRetries);
            }

            // Step 4: Parse and commit code files
            var codeFiles = AgentSquad.Core.AI.CodeFileParser.ParseFiles(implementation);

            if (codeFiles.Count > 0)
            {
                Logger.LogInformation(
                    "Junior Engineer {Name} parsed {Count} code files from AI output for PR #{Number}",
                    Identity.DisplayName, codeFiles.Count, pr.Number);

                await _prWorkflow.CommitCodeFilesToPRAsync(
                    pr.Number, codeFiles, "Implement task", ct);
            }
            else
            {
                Logger.LogWarning(
                    "Junior Engineer {Name} could not parse structured files for PR #{Number}, " +
                    "committing raw output",
                    Identity.DisplayName, pr.Number);

                await _prWorkflow.CommitFixesToPRAsync(
                    pr.Number,
                    $"src/{taskTitle}-implementation.md",
                    $"## Implementation\n\n{implementation}",
                    "Add implementation",
                    ct);
            }

            // Post summary as PR comment
            var fileSummary = codeFiles.Count > 0
                ? $"**Files committed:** {codeFiles.Count}\n" + string.Join("\n", codeFiles.Select(f => $"- `{f.Path}`"))
                : "Raw implementation committed (could not parse structured files)";

            var comment = $"## Implementation Complete\n\n" +
                          $"**Junior Engineer:** {Identity.DisplayName}\n" +
                          $"**Self-Validation:** {(isValid ? "✅ Passed" : "⚠️ Best effort (review carefully)")}\n\n" +
                          $"{fileSummary}";

            await _github.AddPullRequestCommentAsync(pr.Number, comment, ct);

            // Step 5: Mark ready for review
            await _prWorkflow.MarkReadyForReviewAsync(pr.Number, Identity.DisplayName, ct);

            // Notify PM and PE to review this PR
            await _messageBus.PublishAsync(new ReviewRequestMessage
            {
                FromAgentId = Identity.Id,
                ToAgentId = "*",
                MessageType = "ReviewRequest",
                PrNumber = pr.Number,
                PrTitle = pr.Title,
                ReviewType = "CodeReview"
            }, ct);

            // Notify Principal Engineer that the task is complete
            await _messageBus.PublishAsync(new StatusUpdateMessage
            {
                FromAgentId = Identity.Id,
                ToAgentId = "*",
                MessageType = "TaskComplete",
                NewStatus = AgentStatus.Online,
                CurrentTask = taskTitle,
                Details = $"PR #{pr.Number} implementation complete. " +
                          $"Self-validation: {(isValid ? "passed" : "needs careful review")}."
            }, ct);

            Logger.LogInformation(
                "Junior Engineer {Name} completed PR #{Number}, marked ready for review " +
                "(self-validation: {Valid})",
                Identity.DisplayName, pr.Number, isValid);

            UpdateStatus(AgentStatus.Idle, $"Completed PR #{pr.Number}, awaiting next task");
            Identity.AssignedPullRequest = null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Junior Engineer {Name} failed working on PR #{Number}",
                Identity.DisplayName, pr.Number);
            RecordError($"Failed on PR #{pr.Number}: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Error, ex);

            await ReportBlockerAsync(
                $"Implementation failure on PR #{pr.Number}",
                $"Failed while working on PR #{pr.Number}: {pr.Title}\n\nError: {ex.Message}",
                ct);
        }
    }

    private async Task<(bool IsValid, string? Feedback)> SelfValidateAsync(
        IChatCompletionService chat,
        ChatHistory history,
        string implementation,
        string taskTitle,
        string requirements,
        CancellationToken ct)
    {
        try
        {
            var validationHistory = new ChatHistory();
            validationHistory.AddSystemMessage(
                "You are a code reviewer checking if an implementation meets its requirements. " +
                "Be concise. Check for:\n" +
                "1. Does it meet the stated requirements?\n" +
                "2. Are there obvious bugs or missing error handling?\n" +
                "3. Does it follow reasonable coding patterns?\n\n" +
                "Respond with exactly one of these on the first line:\n" +
                "VALIDATION: PASS\n" +
                "VALIDATION: FAIL\n\n" +
                "If FAIL, list the specific issues below.");

            validationHistory.AddUserMessage(
                $"## Task: {taskTitle}\n{requirements}\n\n" +
                $"## Implementation\n{implementation}");

            var response = await chat.GetChatMessageContentAsync(
                validationHistory, cancellationToken: ct);
            var result = response.Content?.Trim() ?? "";

            var passed = result.Contains("VALIDATION: PASS", StringComparison.OrdinalIgnoreCase);
            var feedback = passed ? null : result;

            return (passed, feedback);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Self-validation failed with exception, treating as pass");
            return (true, null);
        }
    }

    #endregion

    #region Issue-Driven Work

    /// <summary>
    /// Processes a new Issue assignment. Reads the Issue, creates a PR linking to it,
    /// and implements the solution. Supports clarification loop with the PM.
    /// </summary>
    private async Task WorkOnIssueAsync(IssueAssignmentMessage assignment, CancellationToken ct)
    {
        try
        {
            _currentIssueNumber = assignment.IssueNumber;
            UpdateStatus(AgentStatus.Working, $"Starting issue #{assignment.IssueNumber}: {assignment.IssueTitle}");

            var issue = await _github.GetIssueAsync(assignment.IssueNumber, ct);
            if (issue is null)
            {
                Logger.LogWarning("Cannot find issue #{Number}", assignment.IssueNumber);
                _currentIssueNumber = null;
                return;
            }

            Logger.LogInformation("Junior Engineer {Name} starting work on issue #{Number}: {Title}",
                Identity.DisplayName, issue.Number, issue.Title);

            var pmSpecDoc = await _projectFiles.GetPMSpecAsync(ct);
            var architectureDoc = await _projectFiles.GetArchitectureDocAsync(ct);
            var techStack = _config.Project.TechStack;

            var kernel = _modelRegistry.GetKernel(Identity.ModelTier, Identity.Id);
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            // Plan approach with possible clarification loop
            var planHistory = new ChatHistory();
            planHistory.AddSystemMessage(
                $"You are a Junior Engineer analyzing a GitHub Issue before starting work. " +
                $"The project uses {techStack}. " +
                "Read the Issue carefully and produce:\n" +
                "1. A summary of what you understand needs to be built\n" +
                "2. The acceptance criteria\n" +
                "3. Your planned approach\n" +
                "4. Any questions — if requirements are UNCLEAR, list them. " +
                "If you understand everything, say 'NO_QUESTIONS'.");

            planHistory.AddUserMessage(
                $"## PM Specification\n{TruncateForContext(pmSpecDoc)}\n\n" +
                $"## Architecture\n{TruncateForContext(architectureDoc)}\n\n" +
                $"## Issue #{issue.Number}: {issue.Title}\n{issue.Body}");

            var planResponse = await chat.GetChatMessageContentAsync(planHistory, cancellationToken: ct);
            var planContent = planResponse.Content?.Trim() ?? "";

            // Clarification loop
            if (!planContent.Contains("NO_QUESTIONS", StringComparison.OrdinalIgnoreCase) &&
                planContent.Contains("?"))
            {
                var maxRounds = _config.Limits.MaxClarificationRoundTrips;
                var clarificationRounds = 0;

                while (clarificationRounds < maxRounds)
                {
                    var questions = ExtractQuestions(planContent);
                    if (string.IsNullOrWhiteSpace(questions))
                        break;

                    Logger.LogInformation(
                        "Junior Engineer {Name} asking clarification on issue #{Number} (round {Round}/{Max})",
                        Identity.DisplayName, issue.Number, clarificationRounds + 1, maxRounds);

                    await _github.AddIssueCommentAsync(issue.Number,
                        $"**{Identity.DisplayName}** has questions before starting work:\n\n{questions}",
                        ct);

                    await _messageBus.PublishAsync(new ClarificationRequestMessage
                    {
                        FromAgentId = Identity.Id,
                        ToAgentId = "*",
                        MessageType = "ClarificationRequest",
                        IssueNumber = issue.Number,
                        Question = questions
                    }, ct);

                    UpdateStatus(AgentStatus.Blocked, $"Waiting for clarification on issue #{issue.Number}");

                    var responseReceived = false;
                    for (var i = 0; i < 60; i++)
                    {
                        if (_clarificationResponses.TryDequeue(out var resp) &&
                            resp.IssueNumber == issue.Number)
                        {
                            responseReceived = true;
                            planHistory.AddAssistantMessage(planContent);
                            planHistory.AddUserMessage(
                                $"The PM has responded:\n\n{resp.Response}\n\n" +
                                "Update your understanding. List remaining questions or say 'NO_QUESTIONS'.");

                            var updatedPlan = await chat.GetChatMessageContentAsync(
                                planHistory, cancellationToken: ct);
                            planContent = updatedPlan.Content?.Trim() ?? "";
                            break;
                        }
                        await Task.Delay(5000, ct);
                    }

                    if (!responseReceived)
                    {
                        Logger.LogWarning("No clarification response for issue #{Number}, proceeding",
                            issue.Number);
                        break;
                    }

                    clarificationRounds++;

                    if (planContent.Contains("NO_QUESTIONS", StringComparison.OrdinalIgnoreCase))
                        break;
                }
            }

            // Create PR linking to the Issue
            var prDescription = $"Closes #{issue.Number}\n\n" +
                $"## Understanding\n{ExtractSection(planContent, "summary", "understand")}\n\n" +
                $"## Acceptance Criteria\n{ExtractSection(planContent, "acceptance", "criteria")}\n\n" +
                $"## Planned Approach\n{ExtractSection(planContent, "task", "plan", "approach")}";

            var branchName = await _prWorkflow.CreateTaskBranchAsync(
                Identity.DisplayName,
                $"issue-{issue.Number}-{Slugify(issue.Title)}",
                ct);

            var pr = await _prWorkflow.CreateTaskPullRequestAsync(
                Identity.DisplayName,
                issue.Title,
                prDescription,
                assignment.Complexity,
                "Architecture.md",
                "PMSpec.md",
                branchName,
                ct);

            _currentPrNumber = pr.Number;
            Identity.AssignedPullRequest = pr.Number.ToString();

            Logger.LogInformation(
                "Junior Engineer {Name} created PR #{PrNumber} for issue #{IssueNumber}",
                Identity.DisplayName, pr.Number, issue.Number);

            // Implement
            await ImplementAndCommitAsync(pr, issue, ct);

            _currentIssueNumber = null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Junior Engineer {Name} failed on issue #{Number}",
                Identity.DisplayName, assignment.IssueNumber);
            RecordError($"Failed on issue #{assignment.IssueNumber}: {ex.Message}",
                Microsoft.Extensions.Logging.LogLevel.Error, ex);
            _currentIssueNumber = null;
        }
    }

    /// <summary>
    /// Core implementation logic for issue-driven work.
    /// </summary>
    private async Task ImplementAndCommitAsync(AgentPullRequest pr, AgentIssue issue, CancellationToken ct)
    {
        var architectureDoc = await _projectFiles.GetArchitectureDocAsync(ct);
        var pmSpecDoc = await _projectFiles.GetPMSpecAsync(ct);
        var techStack = _config.Project.TechStack;

        var kernel = _modelRegistry.GetKernel(Identity.ModelTier, Identity.Id);
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage(
            $"You are a Junior Engineer implementing a task from a GitHub Issue. " +
            $"The project uses {techStack}. " +
            "Follow the architecture closely. Write clean, well-commented code. " +
            "Include proper error handling and basic unit tests. " +
            "Ask for help if something is too complex.");

        history.AddUserMessage(
            $"## PM Specification\n{TruncateForContext(pmSpecDoc)}\n\n" +
            $"## Architecture\n{TruncateForContext(architectureDoc)}\n\n" +
            $"## Issue #{issue.Number}: {issue.Title}\n{issue.Body}\n\n" +
            $"## PR Description\n{pr.Body}\n\n" +
            "Produce a complete implementation. Output each file using this format:\n\n" +
            "FILE: path/to/file.ext\n```language\n<file content>\n```\n\n" +
            $"Use the {techStack} technology stack. " +
            "Include all source code files, configuration, and tests. " +
            "Every file MUST use the FILE: marker format.");

        var implResponse = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
        var implementation = implResponse.Content?.Trim() ?? "";

        // Parse code files and commit
        var codeFiles = AgentSquad.Core.AI.CodeFileParser.ParseFiles(implementation);

        if (codeFiles.Count > 0)
        {
            Logger.LogInformation(
                "Junior Engineer {Name} parsed {Count} code files for PR #{Number}",
                Identity.DisplayName, codeFiles.Count, pr.Number);

            await _prWorkflow.CommitCodeFilesToPRAsync(
                pr.Number, codeFiles, $"Implement issue #{issue.Number}: {issue.Title}", ct);
        }
        else
        {
            Logger.LogWarning(
                "Junior Engineer {Name} could not parse files for PR #{Number}, committing raw",
                Identity.DisplayName, pr.Number);

            await _prWorkflow.CommitFixesToPRAsync(
                pr.Number,
                $"src/issue-{issue.Number}-implementation.md",
                $"## Implementation\n\n{implementation}",
                "Add implementation",
                ct);
        }

        // Mark ready for review
        await _prWorkflow.MarkReadyForReviewAsync(pr.Number, Identity.DisplayName, ct);

        // Notify PM and PE to review
        await _messageBus.PublishAsync(new ReviewRequestMessage
        {
            FromAgentId = Identity.Id,
            ToAgentId = "*",
            MessageType = "ReviewRequest",
            PrNumber = pr.Number,
            PrTitle = pr.Title,
            ReviewType = "CodeReview"
        }, ct);

        await _messageBus.PublishAsync(new StatusUpdateMessage
        {
            FromAgentId = Identity.Id,
            ToAgentId = "*",
            MessageType = "TaskComplete",
            NewStatus = AgentStatus.Online,
            CurrentTask = issue.Title,
            Details = $"PR #{pr.Number} implementation complete and ready for review."
        }, ct);

        Logger.LogInformation(
            "Junior Engineer {Name} completed PR #{Number}, marked ready for review",
            Identity.DisplayName, pr.Number);

        UpdateStatus(AgentStatus.Idle, $"Completed PR #{pr.Number}, awaiting next task");
        Identity.AssignedPullRequest = null;
        _currentPrNumber = null;
    }

    #endregion

    #region Complexity Escalation

    private async Task EscalateComplexityAsync(AgentPullRequest pr, CancellationToken ct)
    {
        var title = $"Task #{pr.Number} exceeds Junior Engineer capability";
        var body = $"## Complexity Escalation\n\n" +
                   $"**Junior Engineer:** {Identity.DisplayName}\n" +
                   $"**PR:** #{pr.Number} — {pr.Title}\n\n" +
                   $"This task appears too complex for a Junior Engineer. " +
                   $"The implementation requires deeper expertise or spans multiple " +
                   $"complex subsystems.\n\n" +
                   $"Requesting reassignment to a Senior or Principal Engineer.";

        try
        {
            var issue = await _issueWorkflow.AskAgentAsync(
                Identity.DisplayName,
                "Principal Engineer",
                $"{title}\n\n{body}",
                ct);

            UpdateStatus(AgentStatus.Blocked, $"Escalated PR #{pr.Number} — too complex");
            Identity.AssignedPullRequest = null;

            Logger.LogWarning(
                "Junior Engineer {Name} escalated PR #{PrNumber} via issue #{IssueNumber}",
                Identity.DisplayName, pr.Number, issue.Number);

            // Notify Principal Engineer via message bus
            await _messageBus.PublishAsync(new HelpRequestMessage
            {
                FromAgentId = Identity.Id,
                ToAgentId = "PrincipalEngineer",
                MessageType = "ComplexityEscalation",
                IssueTitle = title,
                IssueBody = body,
                IsBlocker = true
            }, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Junior Engineer {Name} failed to escalate complexity",
                Identity.DisplayName);
            RecordError($"Escalation failed: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning, ex);
        }
    }

    #endregion

    #region Issue Handling

    private async Task CheckForIssuesAsync(CancellationToken ct)
    {
        try
        {
            var issues = await _issueWorkflow.GetIssuesForAgentAsync(Identity.DisplayName, ct);

            foreach (var issue in issues)
            {
                if (_processedIssueIds.Contains(issue.Number))
                    continue;

                _processedIssueIds.Add(issue.Number);

                Logger.LogInformation(
                    "Junior Engineer {Name} processing issue #{Number}: {Title}",
                    Identity.DisplayName, issue.Number, issue.Title);

                if (issue.Body.Contains("REQUEST_CHANGES", StringComparison.OrdinalIgnoreCase)
                    || issue.Body.Contains("feedback", StringComparison.OrdinalIgnoreCase))
                {
                    await _issueWorkflow.ResolveIssueAsync(
                        issue.Number,
                        $"Acknowledged. {Identity.DisplayName} will address the feedback.",
                        ct);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Junior Engineer {Name} failed to check issues",
                Identity.DisplayName);
        }
    }

    private async Task ReportBlockerAsync(string title, string details, CancellationToken ct)
    {
        try
        {
            var issue = await _issueWorkflow.ReportBlockerAsync(
                Identity.DisplayName, title, details, ct);
            UpdateStatus(AgentStatus.Blocked, title);

            Logger.LogWarning("Junior Engineer {Name} reported blocker issue #{Number}: {Title}",
                Identity.DisplayName, issue.Number, title);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Junior Engineer {Name} failed to report blocker",
                Identity.DisplayName);
            RecordError($"Blocker report failed: {ex.Message}", Microsoft.Extensions.Logging.LogLevel.Warning, ex);
        }
    }

    #endregion

    #region Message Handlers

    private Task HandleTaskAssignmentAsync(TaskAssignmentMessage message, CancellationToken ct)
    {
        Logger.LogInformation(
            "Junior Engineer {Name} received task assignment: {Title} (Complexity: {Complexity})",
            Identity.DisplayName, message.Title, message.Complexity);

        return Task.CompletedTask;
    }

    private Task HandleIssueAssignmentAsync(IssueAssignmentMessage message, CancellationToken ct)
    {
        Logger.LogInformation(
            "Junior Engineer {Name} received issue assignment: #{IssueNumber} {Title}",
            Identity.DisplayName, message.IssueNumber, message.IssueTitle);
        _assignmentQueue.Enqueue(message);
        return Task.CompletedTask;
    }

    private Task HandleChangesRequestedAsync(ChangesRequestedMessage message, CancellationToken ct)
    {
        // Only handle feedback for PRs assigned to this agent
        if (Identity.AssignedPullRequest != message.PrNumber.ToString() &&
            _currentPrNumber != message.PrNumber)
            return Task.CompletedTask;

        Logger.LogInformation(
            "Junior Engineer {Name} received change request from {Reviewer} on PR #{PrNumber}",
            Identity.DisplayName, message.ReviewerAgent, message.PrNumber);

        _reworkQueue.Enqueue(new ReworkItem(message.PrNumber, message.PrTitle, message.Feedback, message.ReviewerAgent));
        return Task.CompletedTask;
    }

    private Task HandleClarificationResponseAsync(ClarificationResponseMessage message, CancellationToken ct)
    {
        Logger.LogInformation(
            "Junior Engineer {Name} received clarification response for issue #{IssueNumber}",
            Identity.DisplayName, message.IssueNumber);
        _clarificationResponses.Enqueue(message);
        return Task.CompletedTask;
    }

    #endregion

    private async Task HandleReworkAsync(ReworkItem rework, CancellationToken ct)
    {
        var pr = await _github.GetPullRequestAsync(rework.PrNumber, ct);
        if (pr is null || !string.Equals(pr.State, "open", StringComparison.OrdinalIgnoreCase))
            return;

        UpdateStatus(AgentStatus.Working, $"Addressing feedback on PR #{rework.PrNumber}");
        Logger.LogInformation(
            "Junior Engineer {Name} reworking PR #{PrNumber} based on feedback from {Reviewer}",
            Identity.DisplayName, rework.PrNumber, rework.Reviewer);

        try
        {
            var kernel = _modelRegistry.GetKernel(Identity.ModelTier, Identity.Id);
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var architectureDoc = await _projectFiles.GetArchitectureDocAsync(ct);
            var pmSpecDoc = await _projectFiles.GetPMSpecAsync(ct);
            var techStack = _config.Project.TechStack;

            var history = new ChatHistory();
            history.AddSystemMessage(
                $"You are a Junior Software Engineer addressing review feedback on a pull request. " +
                $"The project uses {techStack}. " +
                "The reviewer has requested changes. Carefully read the feedback, understand what needs " +
                "to be fixed, and produce an updated implementation that addresses ALL the feedback points. " +
                "Be thorough — every feedback item must be resolved. Ask for help if truly stuck.");

            history.AddUserMessage(
                $"## PR #{rework.PrNumber}: {rework.PrTitle}\n" +
                $"## Original PR Description\n{pr.Body}\n\n" +
                $"## Architecture (Summary)\n{TruncateForContext(architectureDoc)}\n\n" +
                $"## PM Specification (Summary)\n{TruncateForContext(pmSpecDoc)}\n\n" +
                $"## Review Feedback from {rework.Reviewer}\n{rework.Feedback}\n\n" +
                "Please provide the corrected files that address all the feedback. " +
                "Output each file using this exact format:\n\n" +
                "FILE: path/to/file.ext\n```language\n<file content>\n```\n\n" +
                "Include the COMPLETE content of each changed file.");

            var response = await chat.GetChatMessageContentAsync(history, cancellationToken: ct);
            var updatedImpl = response.Content?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(updatedImpl))
            {
                var codeFiles = AgentSquad.Core.AI.CodeFileParser.ParseFiles(updatedImpl);
                if (codeFiles.Count > 0)
                {
                    await _prWorkflow.CommitCodeFilesToPRAsync(
                        pr.Number, codeFiles, "Address review feedback", ct);
                }
                else
                {
                    var taskTitle = PullRequestWorkflow.ParseTaskTitleFromTitle(pr.Title);
                    await _prWorkflow.CommitFixesToPRAsync(
                        pr.Number,
                        $"src/{taskTitle}-rework.md",
                        $"## Rework: Addressing Review Feedback\n\n" +
                        $"**Reviewer:** {rework.Reviewer}\n\n" +
                        $"### Changes Made\n{updatedImpl}",
                        $"Address review feedback from {rework.Reviewer}",
                        ct);
                }

                await _prWorkflow.MarkReadyForReviewAsync(pr.Number, Identity.DisplayName, ct);

                await _messageBus.PublishAsync(new ReviewRequestMessage
                {
                    FromAgentId = Identity.Id,
                    ToAgentId = "*",
                    MessageType = "ReviewRequest",
                    PrNumber = pr.Number,
                    PrTitle = pr.Title,
                    ReviewType = "Rework"
                }, ct);

                Logger.LogInformation(
                    "Junior Engineer {Name} submitted rework for PR #{PrNumber}, re-requesting review",
                    Identity.DisplayName, pr.Number);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Junior Engineer {Name} failed rework on PR #{PrNumber}",
                Identity.DisplayName, rework.PrNumber);
        }
    }

    #region Helpers

    /// <summary>
    /// Truncate architecture doc to keep context within local/budget model limits.
    /// </summary>
    private static string TruncateForContext(string content, int maxLength = 4000)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;

        return content[..maxLength] + "\n\n[... truncated for context window ...]";
    }

    /// <summary>Extract question lines from AI plan output.</summary>
    private static string ExtractQuestions(string content)
    {
        var lines = content.Split('\n');
        var questions = lines.Where(l => l.TrimStart().Contains('?')).ToList();
        return questions.Count > 0 ? string.Join("\n", questions) : "";
    }

    /// <summary>Extract a rough section from plan content by keyword match.</summary>
    private static string ExtractSection(string content, params string[] keywords)
    {
        var lines = content.Split('\n');
        var collecting = false;
        var result = new List<string>();

        foreach (var line in lines)
        {
            var lower = line.ToLowerInvariant();
            if (keywords.Any(k => lower.Contains(k)))
            {
                collecting = true;
                result.Add(line);
                continue;
            }

            if (collecting)
            {
                if (line.TrimStart().StartsWith('#') || line.TrimStart().StartsWith("**"))
                {
                    if (result.Count > 1) break;
                }
                result.Add(line);
            }
        }

        return result.Count > 0 ? string.Join('\n', result).Trim() : content[..Math.Min(500, content.Length)];
    }

    /// <summary>Create a URL-safe slug from a title.</summary>
    private static string Slugify(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(' ', '-')
            .Replace(':', '-');
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        return slug.Length > 40 ? slug[..40] : slug;
    }

    #endregion
}
