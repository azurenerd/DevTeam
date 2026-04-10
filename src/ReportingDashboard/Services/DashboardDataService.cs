using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public class DashboardDataService
{
    private readonly string _dataFilePath;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DashboardDataService(IWebHostEnvironment env, IConfiguration config, ILogger<DashboardDataService> logger)
    {
        _env = env;
        _logger = logger;
        _dataFilePath = config.GetValue<string>("Dashboard:DataFilePath") ?? "wwwroot/data/data.json";
    }

    public async Task<DashboardData> LoadDataAsync()
    {
        try
        {
            var fullPath = Path.Combine(_env.ContentRootPath, _dataFilePath);
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Data file not found at {Path}", fullPath);
                return new DashboardData
                {
                    ErrorMessage = $"Data file not found: {_dataFilePath}. Please create it from data.template.json."
                };
            }

            var json = await File.ReadAllTextAsync(fullPath);
            var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

            return data ?? new DashboardData
            {
                ErrorMessage = "Data file was empty or could not be parsed."
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize data.json");
            return new DashboardData
            {
                ErrorMessage = $"Invalid JSON in data file: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading dashboard data");
            return new DashboardData
            {
                ErrorMessage = $"Error loading dashboard data: {ex.Message}"
            };
        }
    }

    public string GetFullPath()
    {
        return Path.Combine(_env.ContentRootPath, _dataFilePath);
    }
}