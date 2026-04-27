using AgentSquad.Core.DevPlatform.Capabilities;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Core.DevPlatform;

/// <summary>
/// Handles post-merge cleanup: closes linked work items when a PR is merged.
/// Works across both GitHub and ADO platforms via the abstraction layer.
/// GitHub: "Closes #X" auto-close handles most cases; this is a safety net.
/// ADO: Required — no native auto-close mechanism.
/// </summary>
public sealed class MergeCloseoutService
{
    private readonly IPullRequestService _prService;
    private readonly IWorkItemService _workItemService;
    private readonly ILogger<MergeCloseoutService> _logger;

    public MergeCloseoutService(
        IPullRequestService prService,
        IWorkItemService workItemService,
        ILogger<MergeCloseoutService> logger)
    {
        _prService = prService ?? throw new ArgumentNullException(nameof(prService));
        _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// After a PR is merged, close all linked work items.
    /// Idempotent — safe to call multiple times for the same PR.
    /// </summary>
    public async Task CloseLinkedWorkItemsAsync(int prId, CancellationToken ct = default)
    {
        try
        {
            var linkedIds = await _prService.GetLinkedWorkItemIdsAsync(prId, ct);
            if (linkedIds.Count == 0)
            {
                _logger.LogDebug("No linked work items found for merged PR #{PrId}", prId);
                return;
            }

            _logger.LogInformation("Closing {Count} linked work item(s) for merged PR #{PrId}: [{Ids}]",
                linkedIds.Count, prId, string.Join(", ", linkedIds));

            foreach (var workItemId in linkedIds)
            {
                try
                {
                    var workItem = await _workItemService.GetAsync(workItemId, ct);
                    if (workItem is null)
                    {
                        _logger.LogWarning("Linked work item #{Id} not found — skipping", workItemId);
                        continue;
                    }

                    // Skip if already closed
                    if (string.Equals(workItem.State, "closed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(workItem.State, "Closed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(workItem.State, "Done", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(workItem.State, "Resolved", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("Work item #{Id} already in terminal state '{State}'", workItemId, workItem.State);
                        continue;
                    }

                    await _workItemService.CloseAsync(workItemId, ct);
                    _logger.LogInformation("Closed work item #{Id} after PR #{PrId} merged", workItemId, prId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to close linked work item #{Id} for PR #{PrId}", workItemId, prId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to close linked work items for merged PR #{PrId}", prId);
        }
    }

    /// <summary>
    /// Link a work item to a PR and then close the work item.
    /// Convenience method combining both operations.
    /// </summary>
    public async Task LinkAndCloseAsync(int prId, int workItemId, CancellationToken ct = default)
    {
        await _prService.LinkWorkItemAsync(prId, workItemId, ct);
        await _workItemService.CloseAsync(workItemId, ct);
    }
}
