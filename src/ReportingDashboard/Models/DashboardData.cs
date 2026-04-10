namespace ReportingDashboard.Models;

public record DashboardData
{
    public ProjectInfo? Project { get; init; }
    public List<Milestone> Milestones { get; init; } = [];
    public List<WorkItem> Shipped { get; init; } = [];
    public List<WorkItem> InProgress { get; init; } = [];
    public List<WorkItem> CarriedOver { get; init; } = [];
    public MonthSummary? CurrentMonth { get; init; }
    public string? ErrorMessage { get; init; }
}

public record ProjectInfo
{
    public string Name { get; init; } = "Untitled Project";
    public string? Lead { get; init; }
    public string Status { get; init; } = "Unknown";
    public string? LastUpdated { get; init; }
    public string? Summary { get; init; }
}

public record Milestone
{
    public string Title { get; init; } = "";
    public string? TargetDate { get; init; }
    public string Status { get; init; } = "Upcoming";
}

public record WorkItem
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int PercentComplete { get; init; }
    public string? CarryOverReason { get; init; }
}

public record MonthSummary
{
    public string? Month { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int CarriedItems { get; init; }
    public string OverallHealth { get; init; } = "Unknown";
}