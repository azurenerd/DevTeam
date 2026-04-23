using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("highlightMonth")]
    public string HighlightMonth { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<StatusRow> Rows { get; set; } = new();
}

public class StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}