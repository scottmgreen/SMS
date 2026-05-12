# Import Hazard Reports to SMS API (Default Category/Type Version)
# This script reads the CSV file and maps the data to your SMS API endpoints
# using DEFAULT_CATEGORY and DEFAULT_TYPE for all hazard categorization
# Import-HazardReportsToAPI_V1.ps1 -CsvFilePath "Shared\HazardReportSubmissionForm.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 5
param(
    [Parameter(Mandatory=$true)]
    [string]$CsvFilePath,

    [Parameter(Mandatory=$true)]
    [string]$ApiBaseUrl,

    [Parameter(Mandatory=$true)]
    [string]$ApiKey,

    [Parameter(Mandatory=$false)]
    [switch]$WhatIf = $false,

    [Parameter(Mandatory=$false)]
    [int]$BatchSize = 10
)

# Function to write colored output
function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

# Function to parse coordinates from CSV
function Parse-Coordinates {
    param([string]$CoordinateString)

    if ([string]::IsNullOrWhiteSpace($CoordinateString)) {
        return $null, $null
    }

    try {
        # Handle format: "45.5805564831504,-122.5842046737671"
        $parts = $CoordinateString.Split(',')
        if ($parts.Length -eq 2) {
            $lat = [decimal]::Parse($parts[0].Trim())
            $lon = [decimal]::Parse($parts[1].Trim())
            return $lat, $lon
        }
    }
    catch {
        Write-ColorOutput "⚠️  Could not parse coordinates: $CoordinateString" "Yellow"
    }

    return $null, $null
}

# Function to create API request object with default category/type
function Create-ApiRequest {
    param($CsvRow)

    # ALWAYS use default values for category and type
    $mappedCategory = "DEFAULT_CATEGORY"
    $mappedType = "DEFAULT_TYPE"

    # Parse coordinates - try both AOA and Baggage area fields
    $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (AOA)"
    if (-not $lat -or -not $lon) {
        $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (Bag Road or Baggage Make-up Area)"
    }

    # Determine location description
    $locationDesc = if ($CsvRow.Location) { $CsvRow.Location } else { "Unknown Location" }

    # Parse incident date/time
    $incidentDateTime = $null
    if ($CsvRow."Date and Time of Incident") {
        try {
            $incidentDateTime = [DateTime]::Parse($CsvRow."Date and Time of Incident").ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
        catch {
            Write-ColorOutput "⚠️  Could not parse incident date: $($CsvRow.'Date and Time of Incident')" "Yellow"
        }
    }

    # Create the API request object with default values
    $apiRequest = @{
        HazardCategory = $mappedCategory
        HazardType = $mappedType
        HazardDescription = $CsvRow."Detailed Description"
        LocationDescription = $locationDesc
        HazardIncidentDateTime = $incidentDateTime
    }

    # Add coordinates if available
    if ($lat -and $lon) {
        $apiRequest.LocationLatitude = $lat
        $apiRequest.LocationLongitude = $lon
    }

    # Add contact information if available
    if ($CsvRow."First Name" -or $CsvRow."Last Name") {
        $apiRequest.ReportContactName = "$($CsvRow.'First Name') $($CsvRow.'Last Name')".Trim()
    }
    if ($CsvRow."Email Address") {
        $apiRequest.ReportContactEmail = $CsvRow."Email Address"
    }
    if ($CsvRow."Phone Number") {
        $apiRequest.ReportContactCell = $CsvRow."Phone Number"
    }
    if ($CsvRow.Company) {
        $apiRequest.ReportContactCompany = $CsvRow.Company
    }

    return $apiRequest
}

# Function to submit report to API
function Submit-Report {
    param($ApiRequest, $RowIndex)

    if ($WhatIf) {
        Write-ColorOutput "🔍 WHATIF: Would submit row $RowIndex" "Cyan"
        Write-Host ($ApiRequest | ConvertTo-Json -Depth 3)
        return @{ Success = $true; Message = "WHATIF mode" }
    }

    try {
        $headers = @{
            'Content-Type' = 'application/json'
            'X-API-Key' = $ApiKey
        }

        $body = $ApiRequest | ConvertTo-Json -Depth 3
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v1/pdxsms?api-version=1" -Method POST -Headers $headers -Body $body

        return @{
            Success = $true
            Response = $response
            Message = "Successfully submitted - Tracking ID: $($response.TrackingId)"
        }
    }
    catch {
        return @{
            Success = $false
            Error = $_.Exception.Message
            Message = "Failed to submit: $($_.Exception.Message)"
        }
    }
}

# Main execution
try {
    Write-ColorOutput "🚀 Starting Hazard Report Import Process (DEFAULT VALUES)" "Green"
    Write-ColorOutput "📁 CSV File: $CsvFilePath" "White"
    Write-ColorOutput "🌐 API Base URL: $ApiBaseUrl" "White"
    Write-ColorOutput "🔑 API Key: $($ApiKey.Substring(0, 8))..." "White"
    Write-ColorOutput "📋 Using API VERSION 1" "Yellow"
    Write-ColorOutput "📋 Using DEFAULT_CATEGORY and DEFAULT_TYPE for all reports" "Yellow"
    

    if ($WhatIf) {
        Write-ColorOutput "🔍 Running in WHATIF mode - no actual submissions will be made" "Yellow"
    }

    # Test CSV file exists
    if (-not (Test-Path $CsvFilePath)) {
        throw "CSV file not found: $CsvFilePath"
    }

    # Load CSV data
    Write-ColorOutput "📊 Loading CSV data..." "Cyan"
    $csvData = Import-Csv -Path $CsvFilePath
    $totalRows = $csvData.Count
    Write-ColorOutput "📈 Found $totalRows rows to process" "Green"

    # Initialize counters
    $successCount = 0
    $failureCount = 0
    $results = @()

    # Process each row
    for ($i = 0; $i -lt $totalRows; $i++) {
        $row = $csvData[$i]
        $rowIndex = $i + 1

        Write-ColorOutput "🔄 Processing row $rowIndex/$totalRows - Hazard ID: $($row.'Hazard ID')" "White"

        try {
            # Create API request with default values
            $apiRequest = Create-ApiRequest -CsvRow $row

            # Submit to API
            $result = Submit-Report -ApiRequest $apiRequest -RowIndex $rowIndex

            if ($result.Success) {
                $successCount++
                Write-ColorOutput "✅ $($result.Message)" "Green"
            }
            else {
                $failureCount++
                Write-ColorOutput "❌ $($result.Message)" "Red"
            }

            # Add to results
            $results += @{
                Row = $rowIndex
                HazardId = $row.'Hazard ID'
                Success = $result.Success
                Message = $result.Message
                ApiRequest = $apiRequest
                ApiResponse = $result.Response
            }

            # Batch pause
            if ($rowIndex % $BatchSize -eq 0 -and $rowIndex -lt $totalRows) {
                Write-ColorOutput "⏸️  Pausing for 2 seconds (batch of $BatchSize completed)..." "Yellow"
                Start-Sleep -Seconds 2
            }
        }
        catch {
            $failureCount++
            $errorMsg = "Exception processing row $rowIndex`: $($_.Exception.Message)"
            Write-ColorOutput "❌ $errorMsg" "Red"

            $results += @{
                Row = $rowIndex
                HazardId = $row.'Hazard ID'
                Success = $false
                Message = $errorMsg
                ApiRequest = $null
                ApiResponse = $null
            }
        }
    }

    # Final summary
    Write-ColorOutput "`n🎯 Import Summary:" "Green"
    Write-ColorOutput "  Total Rows Processed: $totalRows" "White"
    Write-ColorOutput "  Successful Imports: $successCount" "Green"
    Write-ColorOutput "  Failed Imports: $failureCount" "Red"
    Write-ColorOutput "  Success Rate: $([math]::Round(($successCount / $totalRows) * 100, 2))%" "Cyan"

    # Save results to file
    $resultsFile = "Import-Results-DefaultValues-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $results | ConvertTo-Json -Depth 5 | Out-File -FilePath $resultsFile
    Write-ColorOutput "📄 Detailed results saved to: $resultsFile" "Cyan"

    if ($failureCount -gt 0) {
        Write-ColorOutput "`n❌ Some imports failed. Check the results file for details." "Yellow"
    }
}
catch {
    Write-ColorOutput "💥 Fatal error: $($_.Exception.Message)" "Red"
    exit 1
}

Write-ColorOutput "🏁 Import process completed!" "Green"