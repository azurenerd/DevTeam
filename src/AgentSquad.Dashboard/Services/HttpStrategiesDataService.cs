using System.Net.Http.Json;
using AgentSquad.Core.Strategies;

namespace AgentSquad.Dashboard.Services;

public sealed class HttpStrategiesDataService : IStrategiesDataService
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpStrategiesDataService> _logger;

    public HttpStrategiesDataService(HttpClient http, ILogger<HttpStrategiesDataService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskSnapshot>> GetActiveTasksAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<TaskSnapshot>>("/api/strategies/active", ct).ConfigureAwait(false);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GET /api/strategies/active failed");
            return [];
        }
    }

    public async Task<IReadOnlyList<TaskSnapshot>> GetRecentTasksAsync(int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<TaskSnapshot>>($"/api/strategies/recent?limit={limit}", ct).ConfigureAwait(false);
            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GET /api/strategies/recent failed");
            return [];
        }
    }

    public async Task<EnabledStrategiesInfo> GetEnabledAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<EnabledStrategiesInfo>("/api/strategies/enabled", ct).ConfigureAwait(false);
            return result ?? new EnabledStrategiesInfo(false, []);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "GET /api/strategies/enabled failed");
            return new EnabledStrategiesInfo(false, []);
        }
    }
}
