using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Core.Configuration;

namespace AgentSquad.Core.Workspace;

/// <summary>
/// Background service that validates Playwright at startup and periodically re-checks.
/// Ensures browser binaries are installed and Chromium can launch before any agent needs them.
/// Re-validates every 5 minutes to catch disk cleanup, AV quarantine, or corruption.
/// </summary>
public class PlaywrightHealthService : BackgroundService
{
    private readonly PlaywrightRunner _runner;
    private readonly IOptions<AgentSquadConfig> _config;
    private readonly ILogger<PlaywrightHealthService> _logger;
    private static readonly TimeSpan RecheckInterval = TimeSpan.FromMinutes(5);

    public PlaywrightHealthService(
        PlaywrightRunner runner,
        IOptions<AgentSquadConfig> config,
        ILogger<PlaywrightHealthService> logger)
    {
        _runner = runner;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run initial validation at startup — give DI a moment to finish
        await Task.Delay(2000, stoppingToken);

        _logger.LogInformation("PlaywrightHealthService: running startup validation...");

        var wsConfig = _config.Value.Workspace;
        var workspacePath = wsConfig.RootPath ?? @"C:\Agents";
        Directory.CreateDirectory(workspacePath);

        var ok = await _runner.ValidateAsync(wsConfig, workspacePath, stoppingToken);
        if (ok)
            _logger.LogInformation("PlaywrightHealthService: startup check passed ✓");
        else
            _logger.LogWarning("PlaywrightHealthService: startup check FAILED — {Reason}. UI tests and screenshots will be skipped until resolved.",
                _runner.NotReadyReason);

        // Periodic re-validation
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(RecheckInterval, stoppingToken);
                await _runner.ValidateAsync(wsConfig, workspacePath, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PlaywrightHealthService: periodic check failed");
            }
        }
    }
}
