<#
Import-HazardReportsToAPI_V2.ps1

Usage:
.\Import-HazardReportsToAPI_V2.ps1 `
  -CsvFilePath "Shared\HazardReportSubmissionForm.csv" `
  -ApiBaseUrl "http://localhost:5115" `
  -ApiKey "SMS-DEV-12345-ABCDEF" `
  -BatchSize 5 `
  -WhatIf

Expected CSV headers:
Date Created
Hazard ID
PIN # (Concat)
Detailed Description
Date and Time of Incident, if Applicable
Upload an Image or File
Location
Location (AOA)
Location (Bag Road or Baggage Make-up Area)
Hazard Report Status
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$CsvFilePath,

    [Parameter(Mandatory = $true)]
    [string]$ApiBaseUrl,

    [Parameter(Mandatory = $true)]
    [string]$ApiKey,

    [Parameter(Mandatory = $false)]
    [switch]$WhatIf = $false,

    [Parameter(Mandatory = $false)]
    [int]$BatchSize = 10
)

Set-StrictMode -Version Latest

function Write-ColorOutput {
    param(
        [Parameter(Mandatory = $true)][string]$Message,
        [Parameter(Mandatory = $false)][string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Parse-Coordinates {
    param([string]$CoordinateString)

    if ([string]::IsNullOrWhiteSpace($CoordinateString)) {
        return $null, $null
    }

    try {
        # Expected format: "45.5805564831504,-122.5842046737671"
        $parts = $CoordinateString.Split(',')
        if ($parts.Length -eq 2) {
            $lat = [decimal]::Parse($parts[0].Trim(), [System.Globalization.CultureInfo]::InvariantCulture)
            $lon = [decimal]::Parse($parts[1].Trim(), [System.Globalization.CultureInfo]::InvariantCulture)
            return $lat, $lon
        }
    }
    catch {
        Write-ColorOutput "Could not parse coordinates: $CoordinateString" "Yellow"
    }

    return $null, $null
}

function Parse-IsoDateOrDefault {
    param(
        [string]$TextDate,
        [datetime]$DefaultDate = (Get-Date).ToUniversalTime()
    )

    if ([string]::IsNullOrWhiteSpace($TextDate)) {
        return $DefaultDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
    }

    try {
        return ([datetime]::Parse($TextDate)).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
    catch {
        return $DefaultDate.ToString("yyyy-MM-ddTHH:mm:ssZ")
    }
}

function Clean-ApiText {
    param(
        [AllowNull()][string]$Text,
        [int]$MaxLength = 10000
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $Text = $Text -replace '\\n', ' '
    $Text = $Text -replace "`r`n", ' '
    $Text = $Text -replace "`r", ' '
    $Text = $Text -replace "`n", ' '
    $Text = $Text -replace ([char]0xFFFD), ''
    $Text = [regex]::Replace($Text, '[\x00-\x08\x0B\x0C\x0E-\x1F]', '')
    $Text = [regex]::Replace($Text, '\s{2,}', ' ')
    $Text = $Text.Trim()

    if ($Text.Length -gt $MaxLength) {
        $Text = $Text.Substring(0, $MaxLength)
    }

    return $Text
}

function Get-ContentTypeFromFileName {
    param([string]$FileName)

    $ext = [System.IO.Path]::GetExtension($FileName).ToLowerInvariant()
    switch ($ext) {
        ".jpg"  { "image/jpeg" }
        ".jpeg" { "image/jpeg" }
        ".png"  { "image/png" }
        ".gif"  { "image/gif" }
        ".pdf"  { "application/pdf" }
        ".csv"  { "text/csv" }
        ".txt"  { "text/plain" }
        ".xlsx" { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
        ".docx" { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" }
        ".mp4"  { "video/mp4" }
        ".wav"  { "audio/wav" }
        default { "application/octet-stream" }
    }
}

function Parse-UriList {
    param([string]$RawText)

    if ([string]::IsNullOrWhiteSpace($RawText)) {
        return @()
    }

    # Supports one or multiple URIs separated by | or ;
    $parts = $RawText -split '[|;]'
    $uris = @()

    foreach ($p in $parts) {
        $candidate = $p.Trim()
        if ([string]::IsNullOrWhiteSpace($candidate)) { continue }

        if ([System.Uri]::IsWellFormedUriString($candidate, [System.UriKind]::Absolute)) {
            $uris += $candidate
        }
        else {
            Write-ColorOutput "Skipping invalid URI: $candidate" "Yellow"
        }
    }

    return $uris
}

function Build-ReportAttachments {
    param($CsvRow)

    $raw = $CsvRow."Upload an Image or File"
    $uris = Parse-UriList -RawText $raw
    $attachments = @()

    foreach ($u in $uris) {
        $uriObj = [System.Uri]$u
        $fileName = [System.IO.Path]::GetFileName($uriObj.AbsolutePath)
        if ([string]::IsNullOrWhiteSpace($fileName)) {
            $fileName = "attachment-$([guid]::NewGuid().ToString('N')).bin"
        }

        $attachments += @{
            fileUri = $u
            fileName = $fileName
            base64Content = $null   # URI-only mode
            contentType = Get-ContentTypeFromFileName -FileName $fileName
        }
    }

    return $attachments
}

function Create-ApiRequest {
    param($CsvRow)

    # Coordinates
    $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (AOA)"
    if ($null -eq $lat -or $null -eq $lon) {
        $lat, $lon = Parse-Coordinates -CoordinateString $CsvRow."Location (Bag Road or Baggage Make-up Area)"
    }

    $defaultLat = 45.5887
    $defaultLon = -122.5950

    if ($null -eq $lat -or $lat -lt -90 -or $lat -gt 90) { $lat = $defaultLat }
    if ($null -eq $lon -or $lon -lt -180 -or $lon -gt 180) { $lon = $defaultLon }

    # Dates
    $submittedDate = Parse-IsoDateOrDefault -TextDate $CsvRow."Date Created"
    $incidentDate = if ([string]::IsNullOrWhiteSpace($CsvRow."Date and Time of Incident, if Applicable")) {
        $submittedDate
    } else {
        Parse-IsoDateOrDefault -TextDate $CsvRow."Date and Time of Incident, if Applicable"
    }

    # Description and location
    $hazardDescription = Clean-ApiText -Text $CsvRow."Detailed Description" -MaxLength 10000
    $locationDescription = if ([string]::IsNullOrWhiteSpace($CsvRow.Location)) {
        "Not Provided"
    } else {
        Clean-ApiText -Text $CsvRow.Location -MaxLength 500
    }

    # URI attachments from "Upload an Image or File"
    $attachments = Build-ReportAttachments -CsvRow $CsvRow

    # V2 request shape
    # Anonymous/source-only import because this CSV does not include contact identity fields.
    $apiRequest = @{
        hazardDescription = $hazardDescription
        reportSubmittedBy = "EXTERNAL_API_SOURCE"
        reportSubmittedDate = $submittedDate
        reportSubmittingDepartment = "EXTERNAL_API_SOURCE"
        reportSubmittingDepartmentJobFunction = "EXTERNAL_API_SOURCE"
        reportIsAnonymous = $true
        reportContactName = ""
        reportContactCell = ""
        reportContactEmail = ""
        reportContactCompany = ""
        locationDescription = $locationDescription
        locationLatitude = [decimal]::Round([decimal]$lat, 8)
        locationLongitude = [decimal]::Round([decimal]$lon, 8)
        hazardIncidentDateTime = $incidentDate
        reportAttachments = $attachments
    }

    return $apiRequest
}

function Submit-Report {
    param(
        [Parameter(Mandatory = $true)]$ApiRequest,
        [Parameter(Mandatory = $true)][int]$RowIndex
    )

    if ($WhatIf) {
        Write-ColorOutput "WHATIF: Would submit row $RowIndex" "Cyan"
        Write-Host ($ApiRequest | ConvertTo-Json -Depth 10)
        return @{
            Success = $true
            Response = $null
            Message = "WHATIF mode"
        }
    }

    try {
        $headers = @{
            "Content-Type" = "application/json"
            "X-API-Key"    = $ApiKey
        }

        $body = $ApiRequest | ConvertTo-Json -Depth 10 -Compress
        $response = Invoke-RestMethod -Uri "$ApiBaseUrl/api/v2/pdxsms?api-version=2" -Method POST -Headers $headers -Body $body

        return @{
            Success = $true
            Response = $response
            Message = "Successfully submitted - Tracking ID: $($response.TrackingId)"
        }
    }
    catch {
        $detail = if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            $_.ErrorDetails.Message
        } else {
            $_.Exception.Message
        }

        return @{
            Success = $false
            Response = $null
            Message = "Exception processing row $RowIndex : $detail"
        }
    }
}

# --------------------------
# Main
# --------------------------
try {
    Write-ColorOutput "Starting V2 Hazard Report Import" "Green"
    Write-ColorOutput "CSV File: $CsvFilePath" "White"
    Write-ColorOutput "API Base URL: $ApiBaseUrl" "White"
    Write-ColorOutput "API Version: v2" "White"
    if ($WhatIf) { Write-ColorOutput "Mode: WHATIF (no actual submissions)" "Yellow" }

    if (-not (Test-Path $CsvFilePath)) {
        throw "CSV file not found: $CsvFilePath"
    }

    $csvData = Import-Csv -Path $CsvFilePath
    $totalRows = $csvData.Count
    Write-ColorOutput "Rows found: $totalRows" "Cyan"

    $successCount = 0
    $failureCount = 0
    $results = @()

    for ($i = 0; $i -lt $totalRows; $i++) {
        $row = $csvData[$i]
        $rowIndex = $i + 1

        Write-ColorOutput "Processing row $rowIndex/$totalRows " "White"

        $apiRequest = $null
        try {
            $apiRequest = Create-ApiRequest -CsvRow $row
            $result = Submit-Report -ApiRequest $apiRequest -RowIndex $rowIndex

            if ($result.Success) {
                $successCount++
                Write-ColorOutput $result.Message "Green"
            } else {
                $failureCount++
                Write-ColorOutput $result.Message "Red"
                Write-ColorOutput "Payload for failed row $rowIndex :" "Yellow"
                Write-Host ($apiRequest | ConvertTo-Json -Depth 10)
            }

            $results += @{
                Row = $rowIndex
                HazardId = $row.'Hazard ID'
                PinConcat = $row.'PIN # (Concat)'
                Success = $result.Success
                Message = $result.Message
                ApiRequest = $apiRequest
                ApiResponse = $result.Response
            }

            if (($rowIndex % $BatchSize -eq 0) -and ($rowIndex -lt $totalRows)) {
                Write-ColorOutput "Batch pause (2s)..." "Yellow"
                Start-Sleep -Seconds 2
            }
        }
        catch {
            $failureCount++
            $errorMsg = "Exception processing row $rowIndex : $($_.Exception.Message)"
            Write-ColorOutput $errorMsg "Red"

            $results += @{
                Row = $rowIndex
                HazardId = $row.'Hazard ID'
                PinConcat = $row.'PIN # (Concat)'
                Success = $false
                Message = $errorMsg
                ApiRequest = $apiRequest
                ApiResponse = $null
            }
        }
    }

    Write-ColorOutput "`nImport Summary" "Green"
    Write-ColorOutput "  Total Rows: $totalRows" "White"
    Write-ColorOutput "  Success:    $successCount" "Green"
    Write-ColorOutput "  Failed:     $failureCount" "Red"

    $successRate = if ($totalRows -gt 0) { [math]::Round(($successCount / $totalRows) * 100, 2) } else { 0 }
    Write-ColorOutput "  Success %:  $successRate" "Cyan"

    $resultsFile = "Import-Results-V2-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $results | ConvertTo-Json -Depth 10 | Out-File -FilePath $resultsFile -Encoding UTF8
    Write-ColorOutput "Results file: $resultsFile" "Cyan"

    if ($failureCount -gt 0) {
        Write-ColorOutput "Some rows failed. Review the results file." "Yellow"
    }
}
catch {
    Write-ColorOutput "Fatal error: $($_.Exception.Message)" "Red"
    exit 1
}

Write-ColorOutput "Import process completed." "Green"