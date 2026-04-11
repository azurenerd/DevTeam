using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class HeatmapData
{
    [JsonPropertyName("shipped")]
    public Dictionary<string, List<string>> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public Dictionary<string, List<string>> InProgress { get; set; } = new();

    [JsonPropertyName("carryover")]
    public Dictionary<string, List<string>> Carryover { get; set; } = new();

    [JsonPropertyName("blockers")]
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}