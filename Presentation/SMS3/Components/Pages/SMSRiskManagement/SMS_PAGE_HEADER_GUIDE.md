# SMS Global Page Header Styling

## Overview
The `.sms-page-header` CSS class provides a consistent, professional header styling for all SMS pages that matches the NavMenu design.

## Location
- **Global CSS File**: `wwwroot/app.css`
- **Available to**: All pages in the SMS3 application

## Usage

### Basic Implementation
```razor
<!-- SMS Page Header -->
<div class="sms-page-header mb-4">
    <div class="row align-items-center">
        <div class="col">
            <h4 class="mb-1">
                <i class="fas fa-your-icon me-2"></i>
                Your Page Title
            </h4>
            <p class="mb-0 text-white-50">
                Your page description or subtitle
            </p>
        </div>
    </div>
</div>
```

### With Additional Content (e.g., buttons)
```razor
<!-- SMS Page Header with Actions -->
<div class="sms-page-header mb-4">
    <div class="row align-items-center">
        <div class="col">
            <h4 class="mb-1">
                <i class="fas fa-cogs me-2"></i>
                Page Title
            </h4>
            <p class="mb-0 text-white-50">
                Page description
            </p>
        </div>
        <div class="col-auto">
            <RadzenButton ButtonStyle="ButtonStyle.Light" 
                         Text="Action Button" 
                         Size="ButtonSize.Medium" />
        </div>
    </div>
</div>
```

## Styling Features

### Colors (SMS Compliant)
- **Background**: `#212e61` (SMS Blue Dark Bold) - matches NavMenu
- **Bottom Border**: `#4ac6f7` (SMS Blue Accent)
- **Text**: White for headings, semi-transparent white for descriptions
- **Icons**: `#4ac6f7` (SMS Blue Accent)

### Layout
- **Padding**: `1.5rem 2rem` (desktop), `1rem 1.5rem` (mobile)
- **Border Radius**: `0.375rem`
- **Box Shadow**: Subtle shadow for depth
- **Margin**: Use `mb-4` class for consistent spacing below header

### Typography
- **All heading levels** (h1-h6) are properly styled
- **Font Weight**: 600 for headings
- **Helper Classes**: `.text-white`, `.text-white-50` for proper contrast

## Examples in Codebase
- `ReportProcessing.razor` - Original implementation
- `Reports.razor` - Example with icon and description

## Notes
- Always include `mb-4` class for consistent spacing
- Use FontAwesome icons with `me-2` spacing class
- Follow Bootstrap grid system for layout (row/col structure)
- Icons automatically get the SMS Blue Accent color
- Fully responsive design included