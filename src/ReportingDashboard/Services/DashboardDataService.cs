using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DashboardData? Data { get; private set; }
    public bool IsError { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    public DashboardDataService(IWebHostEnvironment env, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        var filePath = Path.Combine(_env.WebRootPath, "data.json");

        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("data.json not found at {Path}", filePath);
                IsError = true;
                ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);
            Data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            if (Data is null)
            {
                _logger.LogError("data.json deserialized to null");
                IsError = true;
                ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Data.Title))
            {
                _logger.LogWarning("data.json is missing required field: title");
                IsError = true;
                ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
                return;
            }

            _logger.LogInformation("Dashboard data loaded successfully: {Title}", Data.Title);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse data.json: {Message}", ex.Message);
            IsError = true;
            ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading data.json: {Message}", ex.Message);
            IsError = true;
            ErrorMessage = "Dashboard data could not be loaded. Check data.json for errors.";
        }
    }
}