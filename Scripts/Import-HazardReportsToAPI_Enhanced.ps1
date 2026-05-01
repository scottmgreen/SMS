# Process each row (Enhanced Error Reporting)
for ($i = 0; $i -lt $totalRows; $i++) {
    $row = $csvData[$i]
    $rowIndex = $i + 1

    Write-ColorOutput "🔄 Processing row $rowIndex/$totalRows - Hazard ID: $($row.'Hazard ID')" "White"

    try {
        # Create API request
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
            
            # Show detailed error information
            if ($result.StatusCode) {
                Write-ColorOutput "   HTTP Status: $($result.StatusCode) - $($result.StatusDescription)" "Yellow"
            }
            
            if ($result.ResponseBody -and $result.ResponseBody -ne "") {
                Write-ColorOutput "   Server Response:" "Yellow"
                Write-ColorOutput "   $($result.ResponseBody)" "Magenta"
            }
            
            if ($result.RequestBody) {
                Write-ColorOutput "   Request Body that failed:" "Yellow"
                Write-ColorOutput "   $($result.RequestBody)" "Gray"
            }
            
            # Pause on errors to make them more visible
            Write-ColorOutput "   ⏸️  Pausing 3 seconds for error review..." "Red"
            Start-Sleep -Seconds 3
        }

        # Add to results with detailed error info
        $resultEntry = @{
            Row = $rowIndex
            HazardId = $row.'Hazard ID'
            Success = $result.Success
            Message = $result.Message
            ApiRequest = $apiRequest
            ApiResponse = $result.Response
        }

        # Add error details if present
        if (-not $result.Success) {
            $resultEntry.ErrorDetails = @{
                StatusCode = $result.StatusCode
                StatusDescription = $result.StatusDescription
                ResponseBody = $result.ResponseBody
                RequestBody = $result.RequestBody
                ExceptionType = $result.ExceptionType
                Error = $result.Error
            }
        }

        $results += $resultEntry

        # Batch pause (only if no error occurred)
        if ($result.Success -and $rowIndex % $BatchSize -eq 0 -and $rowIndex -lt $totalRows) {
            Write-ColorOutput "⏸️  Pausing for 2 seconds (batch of $BatchSize completed)..." "Yellow"
            Start-Sleep -Seconds 2
        }
    }
    catch {
        $failureCount++
        $errorMsg = "Exception processing row $rowIndex`: $($_.Exception.Message)"
        Write-ColorOutput "❌ $errorMsg" "Red"
        
        # Show the problematic row data
        Write-ColorOutput "   Problematic row data:" "Yellow"
        $row | Format-Table | Out-String | Write-Host -ForegroundColor Gray

        $results += @{
            Row = $rowIndex
            HazardId = $row.'Hazard ID'
            Success = $false
            Message = $errorMsg
            ApiRequest = $null
            ApiResponse = $null
            ErrorDetails = @{
                Exception = $_.Exception.Message
                StackTrace = $_.Exception.StackTrace
                RowData = $row
            }
        }
    }
}