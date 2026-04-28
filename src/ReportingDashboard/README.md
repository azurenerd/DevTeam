# Executive Reporting Dashboard

A single-page executive reporting dashboard that visualizes project milestone timelines, monthly execution status, and key deliverables in a screenshot-friendly 1920x1080 format optimized for PowerPoint slides.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Start

```bash
# Run from repository root
dotnet run --project src/ReportingDashboard

# Or with hot-reload for development
dotnet watch --project src/ReportingDashboard
```

The dashboard opens at **http://localhost:5000**.

## Editing Dashboard Data

All display content is driven by a single JSON file:

```
src/ReportingDashboard/wwwroot/data/dashboard-data.json
```

Edit this file and save — the dashboard automatically reloads via FileSystemWatcher (no restart needed).

### JSON Structure

```json
{
  "project": {
    "title": "Project Name",
    "subtitle": "Org · Workstream · Period",
    "backlogUrl": "https://dev.azure.com/...",
    "currentDate": "2025-04-15"
  },
  "timeline": {
    "startDate": "2024-11-01",
    "endDate": "2025-08-31",
    "tracks": [
      {
        "id": "M1",
        "name": "Milestone Name",
        "color": "#0078D4",
        "milestones": [
          { "date": "2025-01-31", "label": "Jan 31", "type": "poc" }
        ]
      }
    ]
  },
  "heatmap": {
    "months": ["Jan", "Feb", "Mar", "Apr"],
    "highlightMonth": "Apr",
    "rows": [
      {
        "category": "Shipped",
        "items": { "Jan": ["Item 1"], "Feb": ["Item 2"] }
      }
    ]
  }
}
```

### Milestone Types

| Type | Marker | Color |
|------|--------|-------|
| `checkpoint` | Open circle | Workstream color |
| `poc` | Gold diamond | #F4B400 |
| `production` | Green diamond | #34A853 |

### Heatmap Categories

| Category | Emoji | Header Color |
|----------|-------|-------------|
| Shipped | ✅ | Green |
| In Progress | 🔄 | Blue |
| Carryover | 🔁 | Amber |
| Blockers | 🚫 | Red |

### Notes

- The `currentDate` field positions the red "NOW" line on the timeline. If omitted, the system date is used.
- The `highlightMonth` field highlights the current month column in the heatmap with a gold background.
- JSON comments (`// ...`) are supported for inline documentation.
- If the JSON is malformed, the previous valid state continues to display and the error is logged to the console.

## Taking Screenshots

1. Open the dashboard at http://localhost:5000
2. Set your browser window to **1920×1080** (or use DevTools device mode)
3. Take a full-page screenshot (e.g., `Ctrl+Shift+S` in Firefox, or use a browser extension)
4. The screenshot is ready to paste directly into PowerPoint — no cropping needed

### Browser Tips

- **Chrome**: DevTools → Toggle device toolbar → set to 1920×1080 → capture screenshot
- **Firefox**: `Ctrl+Shift+S` → "Save full page"
- **Edge**: DevTools → same as Chrome workflow

## Project Structure

```
src/ReportingDashboard/
├── Components/
│   ├── App.razor                 # Root HTML shell
│   ├── Routes.razor              # Router configuration
│   ├── Header.razor              # Project title, subtitle, legend
│   ├── Timeline.razor            # SVG milestone timeline
│   ├── Heatmap.razor             # Execution status grid
│   ├── HeatmapCell.razor         # Individual grid cell
│   ├── Layout/
│   │   └── MainLayout.razor      # Minimal layout wrapper
│   └── Pages/
│       └── Dashboard.razor       # Main page (route: /)
├── Models/
│   ├── DashboardData.cs          # Root data model
│   ├── TimelineData.cs           # Timeline tracks and milestones
│   └── HeatmapData.cs           # Heatmap rows and items
├── Services/
│   └── DashboardDataService.cs   # JSON loading, caching, live reload
├── wwwroot/
│   ├── css/app.css               # Global styles (1920x1080 fixed layout)
│   └── data/dashboard-data.json  # Dashboard configuration data
├── Properties/launchSettings.json
├── Program.cs
├── appsettings.json
└── ReportingDashboard.csproj
```

## Running Tests

```bash
dotnet test ReportingDashboard.sln
```

## Architecture

- **Blazor Server** with Interactive Server render mode for real-time SignalR updates
- **FileSystemWatcher** + polling fallback monitors JSON changes and pushes updates to connected clients
- **Fixed 1920×1080** layout with `overflow: hidden` ensures pixel-perfect screenshots
- **Zero external dependencies** beyond the default Blazor Server template
- **No cloud services**, no database, no authentication — runs entirely local