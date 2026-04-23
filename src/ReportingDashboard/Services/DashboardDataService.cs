using System.Text.Json;
using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDashboardDataService : IDisposable
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}

public class DashboardDataService : IDashboardDataService
{
    private readonly string _filePath;
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardData? _data;
    private string? _error;
    private FileSystemWatcher? _watcher;
    private Timer? _pollTimer;
    private Timer? _debounceTimer;
    private DateTime _lastWriteTime = DateTime.MinValue;

    public event Action? OnDataChanged;

    public DashboardDataService(IConfiguration config, ILogger<DashboardDataService> logger)
    {
        _logger = logger;
        var configPath = config["DashboardDataFile"] ?? "wwwroot/data/dashboard-data.json";
        _filePath = Path.GetFullPath(configPath);

        LoadData();
        SetupWatcher();
        SetupPolling();
    }

    public DashboardData? GetData() => _data;
    public string? GetError() => _error;

    private void LoadData()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _data = null;
                _error = $"Dashboard data file not found. Expected location: {_filePath}";
                _logger.LogWarning("Dashboard data file not found: {Path}", _filePath);
                return;
            }

            var json = ReadFileWithRetry();
            _data = JsonSerializer.Deserialize<DashboardData>(json);
            _error = null;
            _lastWriteTime = File.GetLastWriteTimeUtc(_filePath);
        }
        catch (JsonException ex)
        {
            _data = null;
            _error = $"Error reading dashboard data: {ex.Message}";
            _logger.LogError(ex, "Failed to deserialize dashboard data from {Path}", _filePath);
        }
        catch (Exception ex)
        {
            _data = null;
            _error = $"Error reading dashboard data: {ex.Message}";
            _logger.LogError(ex, "Failed to load dashboard data from {Path}", _filePath);
        }
    }

    private string ReadFileWithRetry()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                return File.ReadAllText(_filePath);
            }
            catch (IOException) when (i < 2)
            {
                Thread.Sleep(200);
            }
        }
        return File.ReadAllText(_filePath);
    }

    private void SetupWatcher()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            var file = Path.GetFileName(_filePath);
            if (string.IsNullOrEmpty(dir)) return;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime
            };
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Renamed += (s, e) => DebouncedReload();
            _watcher.EnableRaisingEvents = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set up FileSystemWatcher, relying on polling fallback");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e) => DebouncedReload();

    private void DebouncedReload()
    {
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(_ =>
        {
            LoadData();
            OnDataChanged?.Invoke();
        }, null, 300, Timeout.Infinite);
    }

    private void SetupPolling()
    {
        _pollTimer = new Timer(_ =>
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var currentWriteTime = File.GetLastWriteTimeUtc(_filePath);
                    if (currentWriteTime != _lastWriteTime)
                    {
                        DebouncedReload();
                    }
                }
                else if (_data != null)
                {
                    LoadData();
                    OnDataChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Polling check failed for {Path}", _filePath);
            }
        }, null, 5000, 5000);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
        _pollTimer?.Dispose();
        _debounceTimer?.Dispose();
    }
}
