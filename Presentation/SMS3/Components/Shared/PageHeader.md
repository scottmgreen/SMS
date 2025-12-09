# PageHeader Component Usage Guide

## Overview
The `PageHeader` component provides a consistent, reusable SMS page header across all pages in the application.

## Location
`Components\Shared\PageHeader.razor`

## Basic Usage

### Simple Page Header
```razor
<PageHeader Title="Your Page Title" />
```

### With Subtitle
```razor
<PageHeader Title="Your Page Title"
           Subtitle="Your page description or subtitle" />
```

### With Icon
```razor
<PageHeader Title="Your Page Title"
           Subtitle="Your page description"
           Icon="fas fa-your-icon" />
```

### With Additional Content (Buttons, etc.)
```razor
<PageHeader Title="Your Page Title"
           Subtitle="Your page description"
           Icon="fas fa-your-icon">
    <AdditionalContent>
        <div class="d-flex gap-2">
            <RadzenButton Text="Action 1" Icon="add" Size="ButtonSize.Small" />
            <RadzenButton Text="Action 2" Icon="edit" Size="ButtonSize.Small" />
        </div>
    </AdditionalContent>
</PageHeader>
```

## Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Title` | string | ? Yes | The main title text for the page |
| `Subtitle` | string? | ? No | Optional subtitle/description text |
| `Icon` | string? | ? No | Optional FontAwesome icon class |
| `AdditionalContent` | RenderFragment? | ? No | Optional content for the right side |

## Examples in Codebase

### HazardReportingBlazor.razor
```razor
<PageHeader Title="Submit Hazard Report"
           Subtitle="Report safety hazards and incidents for SMS processing and risk assessment"
           Icon="fas fa-exclamation-triangle" />
```

### Hazards.razor
```razor
<PageHeader Title="SMS Hazards Management"
           Subtitle="Comprehensive listing and management of all SMS hazards and safety risks"
           Icon="fas fa-exclamation-triangle" />
```

### Reports.razor
```razor
<PageHeader Title="SMS Reports Management"
           Subtitle="Comprehensive listing of all SMS reports with expandable hazard details"
           Icon="fas fa-file-alt" />
```

### ReportProcessing.razor (with AdditionalContent)
```razor
<PageHeader Title="Report Processing Workflow"
           Subtitle="Process reports and manage SMS risk validation workflow"
           Icon="fas fa-cogs">
    <AdditionalContent>
        <div class="d-flex gap-2">
            <RadzenButton ButtonStyle="ButtonStyle.Light" 
                         Text="Refresh All" 
                         Icon="refresh"
                         Size="ButtonSize.Small" />
            <RadzenButton ButtonStyle="ButtonStyle.Info" 
                         Text="Export" 
                         Icon="download"
                         Size="ButtonSize.Small" />
        </div>
    </AdditionalContent>
</PageHeader>
```

## Migration from Old Headers

### Before (Old Manual Header)
```razor
<div class="sms-page-header mb-4">
    <div class="row align-items-center">
        <div class="col">
            <h4 class="mb-1">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Submit Hazard Report
            </h4>
            <p class="mb-0 text-white-50">
                Report safety hazards and incidents for SMS processing
            </p>
        </div>
    </div>
</div>
```

### After (New Component)
```razor
<PageHeader Title="Submit Hazard Report"
           Subtitle="Report safety hazards and incidents for SMS processing"
           Icon="fas fa-exclamation-triangle" />
```

## Benefits
- ? **Consistency** - All pages use the same header styling
- ? **Maintainability** - Single place to update header styling
- ? **Flexibility** - Supports icons, subtitles, and additional content
- ? **Simplicity** - Easy to use with clear parameters
- ? **Type Safety** - EditorRequired attribute ensures Title is provided

## Common FontAwesome Icons
- `fas fa-exclamation-triangle` - Hazards/Safety
- `fas fa-file-alt` - Reports/Documents
- `fas fa-cogs` - Processing/Settings
- `fas fa-chart-line` - Analytics/Reports
- `fas fa-users` - People/Teams
- `fas fa-shield-alt` - Security/Safety
- `fas fa-database` - Data/Storage