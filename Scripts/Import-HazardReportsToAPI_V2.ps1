# Import Hazard Reports to SMS API (Default Category/Type Version)
# This script reads the CSV file and maps the data to your SMS API endpoints
# Import-HazardReportsToAPI_V2.ps1 -CsvFilePath "Shared\HazardReportSubmissionForm.csv" -ApiBaseUrl "http://localhost:5115" -ApiKey "SMS-DEV-12345-ABCDEF" -BatchSize 5
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


# In your Create-ApiRequest function, replace this:
# $apiRequest.ReportContactCell = $CsvRow."Phone Number"

function Clean-PhoneNumber {
    param([string]$Phone)
    if ($null -eq $Phone) { return "" }
    # Remove +1 and all non-digit characters, keep last 10 digits
    $digits = ($Phone -replace '[^0-9]', '')
    if ($digits.Length -gt 10) { $digits = $digits.Substring($digits.Length - 10) }
    return $digits
}

# Function to create API request object with default category/type
function Create-ApiRequest {
    param($CsvRow)

    $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (AOA)"
    if (-not $lat -or -not $lon) {
        $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (Bag Road or Baggage Make-up Area)"
    }

    $defaultBagRoadLat = 45.5887
    $defaultBagRoadLon = -122.5950

    # Validate coordinates
    if ($null -eq $lat -or $lat -lt -90 -or $lat -gt 90) {
        $lat = $defaultBagRoadLat
    }
    if ($null -eq $lon -or $lon -lt -180 -or $lon -gt 180) {
        $lon = $defaultBagRoadLon
    }



    $locationDesc = if ($CsvRow.Location) { $CsvRow.Location } else { "Not Provided" }

    $incidentDateTime = $null
    if ($CsvRow."Date Created") {
        try {
            $incidentDateTime = [DateTime]::Parse($CsvRow."Date Created").ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
        catch {
            $incidentDateTime = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        }
    } else {
        $incidentDateTime = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    }

    $submittedDate = $null
    if ($CsvRow."Date Created") {
        try {
            $submittedDate = [DateTime]::Parse($CsvRow."Date Created").ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
        catch {
            $submittedDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
        }
    } else {
        $submittedDate = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    }

    $fullName = if ($CsvRow."First Name" -or $CsvRow."Last Name") { "$($CsvRow.'First Name') $($CsvRow.'Last Name')".Trim() } else { "" }

    $reportIsAnonymous = [string]::IsNullOrWhiteSpace($fullName)
    
    
    $apiRequest = @{
        hazardDescription =  $CsvRow."Detailed Description"
        reportSubmittedBy = $fullName
        reportSubmittedDate = $submittedDate
        reportSubmittingDepartment = $CsvRow.Company
        reportSubmittingDepartmentJobFunction = ""
        reportIsAnonymous = $reportIsAnonymous
        reportContactName = $fullName
        reportContactEmail = $CsvRow."Email Address"
        reportContactCompany = $CsvRow.Company
        locationDescription = $locationDesc
        locationLatitude = if ($lat) { $lat } else { 0 }
        locationLongitude = if ($lon) { $lon } else { 0 }
        hazardIncidentDateTime = $incidentDateTime
        reportAttachments = @()
    }
    #TOUCH UPS
    $apiRequest.reportContactCell = Clean-PhoneNumber $CsvRow."Phone Number"
    $apiRequest.locationLatitude = if ($lat) { [decimal]::Round([decimal]$lat, 8) } else { 0 }
    $apiRequest.locationLongitude = if ($lon) { [decimal]::Round([decimal]$lon, 8) } else { 0 }
    $apiRequest.hazardDescription = $CsvRow."Detailed Description".Replace("`r", "\r").Replace("`n", "\n")
    
    return $apiRequest
}

function Clean-ApiText {
    param(
        [AllowNull()][string]$Text,
        [int]$MaxLength = 10000
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    # Convert literal \n text into real newlines
    $Text = $Text -replace '\\n', " "

    # Normalize line endings
    $Text = $Text -replace "`r`n", " "
    $Text = $Text -replace "`r", " "
    $Text = $Text -replace "`n", " "  # REMOVE ALL NEWLINES

    # Remove hidden/Unicode line returns (line/paragraph separators)
    #$Text = $Text -replace "([`u2028`u2029])", ''

    # Remove Unicode replacement char
    $Text = $Text -replace ([char]0xFFFD), ''

    # Remove all non-printable/control/hidden Unicode characters except tab
    #$Text = [regex]::Replace($Text, '[^\P{C}\t]', '')

    # Remove control chars except tab (redundant but safe)
    $Text = [regex]::Replace($Text, '[\x00-\x08\x0B\x0C\x0E-\x1F]', '')

    # Collapse excessive spaces
    $Text = [regex]::Replace($Text, '[ ]{2,}', ' ')

    $Text = $Text.Trim()

    # HARD LIMIT — likely needed for this API
    if ($Text.Length -gt $MaxLength) {
        $Text = $Text.Substring(0, $MaxLength) + " [TRUNCATED FOR API SUBMISSION]"
    }

    return $Text
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

        # Clean only problem long-text fields before JSON serialization
        $apiRequest.hazardDescription = Clean-ApiText -Text $apiRequest.hazardDescription -MaxLength 10000
        $apiRequest.locationDescription = Clean-ApiText -Text $apiRequest.locationDescription -MaxLength 500
        
        # 1. Convert to JSON with -Compress (removes structural layout breaks natively)
        $body = $apiRequest | ConvertTo-Json -Depth 10 -Compress

        Write-Host "Hazard description length: $($apiRequest.hazardDescription.Length)"
        Write-Host "JSON byte count: $([System.Text.Encoding]::UTF8.GetByteCount($body))"

        
                
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v2/pdxsms?api-version=2" -Method POST -Headers $headers -Body $body

        return @{
            Success = $true
            Response = $response
            Message = "Successfully submitted - Tracking ID: $($response.TrackingId)"
        }
    }
    catch {
    $failureCount++
    $errorMsg = "Exception processing row $rowIndex`: $($_.Exception.Message)"
    Write-ColorOutput "❌ $errorMsg" "Red"
    Write-Host "Payload for failed row $rowIndex`:" -ForegroundColor Yellow
    Write-Host ($apiRequest | ConvertTo-Json -Depth 5)
    $results += @{
        Row = $rowIndex
        HazardId = $row.'Hazard ID'
        Success = $false
        Message = $errorMsg
        ApiRequest = $apiRequest
        ApiResponse = $null
    }
}
}

# Main execution
try {
    Write-ColorOutput "🚀 Starting Hazard Report Import Process (DEFAULT VALUES)" "Green"
    Write-ColorOutput "📁 CSV File: $CsvFilePath" "White"
    Write-ColorOutput "🌐 API Base URL: $ApiBaseUrl" "White"
    Write-ColorOutput "🔑 API Key: $($ApiKey.Substring(0, 8))..." "White"
    Write-ColorOutput "📋 Using API VERSION 2" "Yellow"

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