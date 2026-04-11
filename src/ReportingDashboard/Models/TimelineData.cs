using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class TimelineData
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("nowDate")]
    public string NowDate { get; set; } = "";

    [JsonPropertyName("tracks")]
    public List<TimelineTrack> Tracks { get; set; } = new();
}

public class TimelineTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#0078D4";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";
}