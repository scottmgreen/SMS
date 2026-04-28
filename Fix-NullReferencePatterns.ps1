# ================================================================================
# ?? Fix-NullReferencePatterns.ps1
# Systematically fixes C# null reference warnings in Blazor project
# ================================================================================

Write-Host "?? Starting Null Reference Pattern Fix" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Get baseline count
Write-Host "`n?? Getting baseline warning count..." -ForegroundColor Yellow
$output = dotnet build --verbosity normal 2>&1 | Out-String
$allLines = $output -split "`n"

# Extract null reference warnings
$cs8602 = $allLines | Where-Object { $_ -match "CS8602" }  # Dereference of possibly null reference
$cs8604 = $allLines | Where-Object { $_ -match "CS8604" }  # Possible null reference argument
$cs8625 = $allLines | Where-Object { $_ -match "CS8625" }  # Cannot convert null literal
$cs8601 = $allLines | Where-Object { $_ -match "CS8601" }  # Possible null reference assignment
$cs8619 = $allLines | Where-Object { $_ -match "CS8619" }  # Nullability mismatch
$cs8618 = $allLines | Where-Object { $_ -match "CS8618" }  # Non-nullable property

Write-Host "?? BASELINE NULL REFERENCE WARNINGS:" -ForegroundColor Cyan
Write-Host "   CS8602 (Dereference): $($cs8602.Count)" -ForegroundColor White
Write-Host "   CS8604 (Null argument): $($cs8604.Count)" -ForegroundColor White  
Write-Host "   CS8625 (Null literal): $($cs8625.Count)" -ForegroundColor White
Write-Host "   CS8601 (Null assignment): $($cs8601.Count)" -ForegroundColor White
Write-Host "   CS8619 (Nullability mismatch): $($cs8619.Count)" -ForegroundColor White
Write-Host "   CS8618 (Non-nullable property): $($cs8618.Count)" -ForegroundColor White

$totalWarnings = $cs8602.Count + $cs8604.Count + $cs8625.Count + $cs8601.Count + $cs8619.Count + $cs8618.Count
Write-Host "   ?? TOTAL: $totalWarnings" -ForegroundColor Yellow

if ($totalWarnings -eq 0) {
    Write-Host "`n?? No null reference warnings found! Project is already clean." -ForegroundColor Green
    exit 0
}

# Function to show top files with most warnings
function Show-TopWarningFiles($warnings, $type) {
    if ($warnings.Count -eq 0) { return }

    Write-Host "`n?? Top files with $type warnings:" -ForegroundColor Yellow
    $fileGroups = $warnings | ForEach-Object {
        if ($_ -match "([^>]+\.(razor|cs))\(") {
            $Matches[1]
        }
    } | Group-Object | Sort-Object Count -Descending | Select-Object -First 5

    foreach ($group in $fileGroups) {
        Write-Host "   ?? $($group.Name): $($group.Count) warnings" -ForegroundColor White
    }
}

Show-TopWarningFiles $cs8602 "CS8602"
Show-TopWarningFiles $cs8604 "CS8604" 
Show-TopWarningFiles $cs8625 "CS8625"

Write-Host "`n???  NULL REFERENCE PATTERNS TO FIX:" -ForegroundColor Green

# Common null reference patterns and their fixes
$patterns = @(
    @{
        Description = "string?.Method() -> string?.Method() with null check"
        Pattern = '(\w+)\.(\w+)\('
        Condition = 'CS8602'
        Fix = 'Add null check or use null-conditional operator'
    },
    @{
        Description = "new SomeClass(possiblyNull) -> new SomeClass(possiblyNull ?? default)"  
        Pattern = 'new \w+\([^)]*\w+[^)]*\)'
        Condition = 'CS8604'
        Fix = 'Add null check or default value'
    },
    @{
        Description = 'assignment = null -> assignment = null!'
        Pattern = '= null;'
        Condition = 'CS8625'
        Fix = 'Use null forgiving operator or make type nullable'
    }
)

foreach ($pattern in $patterns) {
    Write-Host "?? $($pattern.Description)" -ForegroundColor Cyan
    Write-Host "   ?? Fix: $($pattern.Fix)" -ForegroundColor White
}

Write-Host "`n??  RECOMMENDATIONS:" -ForegroundColor Yellow
Write-Host "1. ?? Add null checks before dereferencing potentially null objects" -ForegroundColor White
Write-Host "2. ?? Use null-conditional operators (?.) where appropriate" -ForegroundColor White  
Write-Host "3. ???  Use null forgiving operator (!) only when you're certain value isn't null" -ForegroundColor White
Write-Host "4. ???  Consider making properties nullable (Type?) if they can legitimately be null" -ForegroundColor White
Write-Host "5. ?? Use string.IsNullOrEmpty() for string checks" -ForegroundColor White

Write-Host "`n?? NEXT STEPS:" -ForegroundColor Green
Write-Host "1. Run this script to identify patterns" -ForegroundColor White
Write-Host "2. Use GitHub Copilot to fix individual files systematically" -ForegroundColor White
Write-Host "3. Focus on high-impact files first (files with most warnings)" -ForegroundColor White
Write-Host "4. Test thoroughly after each batch of changes" -ForegroundColor White

Write-Host "`n?? Null reference pattern analysis complete!" -ForegroundColor Green

# Optional: Create a detailed report file
$reportFile = "NullReferenceAnalysis_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
@"
NULL REFERENCE WARNING ANALYSIS
Generated: $(Get-Date)

TOTALS:
- CS8602 (Dereference): $($cs8602.Count)
- CS8604 (Null argument): $($cs8604.Count)  
- CS8625 (Null literal): $($cs8625.Count)
- CS8601 (Null assignment): $($cs8601.Count)
- CS8619 (Nullability mismatch): $($cs8619.Count)
- CS8618 (Non-nullable property): $($cs8618.Count)

TOTAL WARNINGS: $totalWarnings

CS8602 WARNINGS (Dereference):
$($cs8602 -join "`n")

CS8604 WARNINGS (Null argument):
$($cs8604 -join "`n")

CS8625 WARNINGS (Null literal):
$($cs8625 -join "`n")

"@ | Out-File $reportFile -Encoding UTF8

Write-Host "`n?? Detailed report saved to: $reportFile" -ForegroundColor Cyan