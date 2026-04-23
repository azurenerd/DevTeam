using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("project")]
    public ProjectInfo Project { get; set; } = new();

    [JsonPropertyName("timeline")]
    public TimelineData Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapData Heatmap { get; set; } = new();
}

public class ProjectInfo
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    [JsonPropertyName("backlogUrl")]
    public string? BacklogUrl { get; set; }

    [JsonPropertyName("currentDate")]
    public string CurrentDate { get; set; } = string.Empty;
}