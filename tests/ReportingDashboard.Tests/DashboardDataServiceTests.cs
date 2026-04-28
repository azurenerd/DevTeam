using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "dashboard-tests-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "dashboard-data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private IDashboardDataService CreateService(string? filePath = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DashboardDataFile"] = filePath ?? _tempFile
            })
            .Build();
        var logger = NullLogger<DashboardDataService>.Instance;
        return new DashboardDataService(config, logger);
    }

    private static string GetValidJson() => """
    {
        "project": {
            "title": "Test Project",
            "subtitle": "Test Subtitle",
            "backlogUrl": "https://example.com",
            "currentDate": "2025-04-15"
        },
        "timeline": {
            "startDate": "2024-11-01",
            "endDate": "2025-08-31",
            "tracks": []
        },
        "heatmap": {
            "months": ["Jan", "Feb"],
            "highlightMonth": "Jan",
            "rows": []
        }
    }
    """;

    [Fact]
    public void LoadsValidJson_ReturnsData()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        var data = service.GetData();

        Assert.NotNull(data);
        Assert.Equal("Test Project", data!.Project.Title);
        Assert.Equal("Test Subtitle", data.Project.Subtitle);
        Assert.Null(service.GetError());
    }

    [Fact]
    public void MissingFile_ReturnsError()
    {
        using var service = CreateService(Path.Combine(_tempDir, "nonexistent.json"));

        Assert.Null(service.GetData());
        Assert.Contains("not found", service.GetError()!);
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        File.WriteAllText(_tempFile, "{ invalid json }}}");
        using var service = CreateService();

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Error parsing", service.GetError()!);
    }

    [Fact]
    public void JsonWithComments_LoadsSuccessfully()
    {
        var jsonWithComments = """
        {
            // Project metadata
            "project": {
                "title": "Comment Test",
                "subtitle": "Sub",
                "currentDate": "2025-04-01"
            },
            "timeline": {
                "startDate": "2025-01-01",
                "endDate": "2025-12-31",
                "tracks": []
            },
            "heatmap": {
                "months": [],
                "highlightMonth": "",
                "rows": []
            }
        }
        """;
        File.WriteAllText(_tempFile, jsonWithComments);
        using var service = CreateService();

        var data = service.GetData();
        Assert.NotNull(data);
        Assert.Equal("Comment Test", data!.Project.Title);
    }

    [Fact]
    public void LiveReload_PreservesPreviousDataOnInvalidUpdate()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        Assert.NotNull(service.GetData());
        Assert.Equal("Test Project", service.GetData()!.Project.Title);

        // Overwrite with invalid JSON
        File.WriteAllText(_tempFile, "not valid json {{{{");
        Thread.Sleep(600); // Wait for debounce + reload

        // Previous valid data should be preserved
        Assert.NotNull(service.GetData());
        Assert.Equal("Test Project", service.GetData()!.Project.Title);
    }

    [Fact]
    public void TimelineModel_ParsesTracksAndMilestones()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "currentDate": "2025-04-01" },
            "timeline": {
                "startDate": "2025-01-01",
                "endDate": "2025-12-31",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Track One",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2025-03-01", "label": "Mar 1", "type": "poc" },
                            { "date": "2025-06-15", "label": "Jun 15", "type": "production" }
                        ]
                    }
                ]
            },
            "heatmap": { "months": [], "highlightMonth": "", "rows": [] }
        }
        """;
        File.WriteAllText(_tempFile, json);
        using var service = CreateService();

        var data = service.GetData();
        Assert.NotNull(data);
        Assert.Single(data!.Timeline.Tracks);
        Assert.Equal("M1", data.Timeline.Tracks[0].Id);
        Assert.Equal(2, data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal("poc", data.Timeline.Tracks[0].Milestones[0].Type);
    }

    [Fact]
    public void HeatmapModel_ParsesCategoriesAndItems()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "currentDate": "2025-04-01" },
            "timeline": { "startDate": "2025-01-01", "endDate": "2025-12-31", "tracks": [] },
            "heatmap": {
                "months": ["Jan", "Feb", "Mar"],
                "highlightMonth": "Feb",
                "rows": [
                    {
                        "category": "Shipped",
                        "items": { "Jan": ["Item A", "Item B"], "Feb": ["Item C"], "Mar": [] }
                    },
                    {
                        "category": "Blockers",
                        "items": { "Jan": [], "Feb": ["Blocker 1"], "Mar": [] }
                    }
                ]
            }
        }
        """;
        File.WriteAllText(_tempFile, json);
        using var service = CreateService();

        var data = service.GetData();
        Assert.NotNull(data);
        Assert.Equal(3, data!.Heatmap.Months.Count);
        Assert.Equal("Feb", data.Heatmap.HighlightMonth);
        Assert.Equal(2, data.Heatmap.Rows.Count);
        Assert.Equal("Shipped", data.Heatmap.Rows[0].Category);
        Assert.Equal(2, data.Heatmap.Rows[0].Items["Jan"].Count);
    }

    [Fact]
    public void OnDataChanged_FiresOnFileUpdate()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        var changed = false;
        service.OnDataChanged += () => changed = true;

        // Update with new valid data
        var updatedJson = GetValidJson().Replace("Test Project", "Updated Project");
        File.WriteAllText(_tempFile, updatedJson);
        Thread.Sleep(600); // debounce delay

        Assert.True(changed);
    }
}