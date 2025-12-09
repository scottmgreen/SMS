# ReadOnly Location Map Components

## Overview
Two components are available for displaying hazard locations on read-only maps:

1. **ReadOnlyLocationMap** - Generic map component for any location data
2. **HazardLocationDisplay** - Specific component for Hazard entities

## Components Created

### 1. ReadOnlyLocationMap.razor
**Location**: `Components\Shared\ReadOnlyLocationMap.razor`
**Purpose**: Generic read-only map component for displaying any location

#### Parameters:
- `Latitude` (decimal?) - Latitude coordinate
- `Longitude` (decimal?) - Longitude coordinate  
- `LocationDescription` (string?) - Location description/title
- `LocationArea` (string?) - General area description
- `LocationSubArea` (string?) - Specific area description
- `Title` (string?) - Map title
- `MapHeight` (string) - Map height (default: "300px")
- `ShowCoordinates` (bool) - Show coordinate information (default: true)
- `ShowLocationDetails` (bool) - Show location details panel (default: true)
- `ShowZoomControls` (bool) - Show zoom controls (default: true)
- `ShowLayerControls` (bool) - Show layer controls (default: false)
- `ZoomLevel` (int) - Initial zoom level (default: 15)

### 2. HazardLocationDisplay.razor
**Location**: `Components\Shared\HazardLocationDisplay.razor`
**Purpose**: Specialized component for displaying Hazard entity locations

#### Parameters:
- `Hazard` (Hazard) - Required. The hazard entity containing location information
- All the same parameters as ReadOnlyLocationMap for customization

### 3. readonly-map.js
**Location**: `wwwroot\js\readonly-map.js`
**Purpose**: JavaScript module providing read-only map functionality using Leaflet.js

## Usage Examples

### 1. Using HazardLocationDisplay with a Hazard Entity

```razor
@using SMS3.Components.Shared

<!-- Simple usage -->
<HazardLocationDisplay Hazard="@myHazard" />

<!-- Customized usage -->
<HazardLocationDisplay Hazard="@myHazard"
                       Title="Incident Location"
                       MapHeight="400px"
                       ShowCoordinates="true"
                       ShowLocationDetails="true"
                       ZoomLevel="17" />
```

### 2. Using ReadOnlyLocationMap Directly

```razor
@using SMS3.Components.Shared

<!-- With coordinates -->
<ReadOnlyLocationMap Latitude="45.5898m"
                     Longitude="-122.5951m"
                     LocationDescription="PDX Airport Terminal"
                     Title="Hazard Location"
                     MapHeight="350px" />

<!-- Without coordinates (description only) -->
<ReadOnlyLocationMap LocationDescription="Terminal A, Gate 5"
                     LocationArea="Terminal A"
                     LocationSubArea="Gate 5"
                     Title="Incident Location"
                     ShowCoordinates="false" />
```

### 3. In HazardDetails Component

```razor
<!-- Already implemented -->
@if (HasLocationData(hazard))
{
    <div class="mt-3">
        <HazardLocationDisplay Hazard="@hazard"
                               MapHeight="250px"
                               ShowCoordinates="true"
                               ShowLocationDetails="true"
                               ShowZoomControls="true"
                               ShowLayerControls="false"
                               ZoomLevel="16" />
    </div>
}
```

### 4. In Modal Dialogs

```razor
<!-- For incident investigation -->
<RadzenDialog>
    <div class="p-3">
        <h5>Incident Location Details</h5>
        <HazardLocationDisplay Hazard="@selectedHazard"
                               MapHeight="400px"
                               ShowLayerControls="true"
                               ZoomLevel="18" />
    </div>
</RadzenDialog>
```

### 5. In Reports and Listings

```razor
<!-- Compact view for listings -->
<HazardLocationDisplay Hazard="@hazard"
                       MapHeight="200px"
                       ShowLocationDetails="false"
                       ShowZoomControls="false"
                       ZoomLevel="14" />
```

## Features

### ? Automatic Location Parsing
- Automatically extracts coordinates from Hazard.LocationArea format: "Lat: XX.XXXXXX, Lng: YY.YYYYYY"
- Falls back to descriptive text when coordinates aren't available
- Handles both coordinate and text-based locations

### ? Professional Map Display
- Uses Leaflet.js with OpenStreetMap and satellite imagery
- Custom red marker with clean styling
- Popup showing location details and coordinates
- Responsive design that works on all screen sizes

### ? Flexible Configuration
- Customizable map height
- Show/hide coordinate information
- Show/hide location details panel
- Show/hide zoom and layer controls
- Configurable zoom level

### ? Smart Location Detection
- Displays map when coordinates are available
- Shows location details panel when descriptive text is available
- Graceful fallback when no location data exists
- "No location information available" message when appropriate

### ? Clean Integration
- Matches SMS design system
- Consistent styling with other components
- Easy to use with Hazard entities
- Reusable for any location display needs

## Technical Details

### Location Data Formats Supported:
1. **Coordinates**: "Lat: 45.589800, Lng: -122.595100" in LocationArea
2. **Descriptive**: Text descriptions in LocationArea, LocationSubArea, or HazardLocation
3. **Mixed**: Coordinates with additional descriptive text

### Map Features:
- **Read-only**: No click interactions for location modification
- **Draggable**: Users can pan to explore surrounding area  
- **Zoomable**: Users can zoom in/out for better visibility
- **Layer control**: Optional street/satellite view switching
- **Responsive**: Adapts to container size

### Performance:
- Lazy loading of map JavaScript module
- Efficient cleanup of map instances
- Multiple maps can coexist on same page
- Minimal memory footprint

## Benefits

1. **Consistent UI**: Same map appearance across all SMS pages
2. **Easy Implementation**: Just pass a Hazard entity, everything else is automatic
3. **Flexible**: Can be used for any location display needs
4. **Professional**: High-quality maps with proper styling
5. **Accessible**: Works without coordinates, graceful degradation
6. **Maintainable**: Single component for all location displays