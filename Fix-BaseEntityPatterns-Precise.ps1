# Precise BaseEntity Pattern Fixer for SMS3 Project
# This script fixes ONLY the specific BaseEntity comparison patterns causing CS8604/CS8625 warnings

param(
    [string]$ProjectPath = "Presentation\SMS3",
    [switch]$WhatIf = $false
)

Write-Host "?? SMS3 Precise BaseEntity Pattern Fixer" -ForegroundColor Green

# Define VERY specific patterns to avoid false positives
$patterns = @(
    # Entity comparisons - be very specific about context
    @{ Pattern = '\s+if\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*!=\s*null\s*\)'; Replacement = '        if ($1 is not null)' }
    @{ Pattern = '\s+if\s*\(\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*==\s*null\s*\)'; Replacement = '        if ($1 is null)' }
    @{ Pattern = '([a-zA-Z_][a-zA-Z0-9_]*)\s*!=\s*null\s*\&\&'; Replacement = '$1 is not null &&' }
    @{ Pattern = '([a-zA-Z_][a-zA-Z0-9_]*)\s*==\s*null\s*\&\&'; Replacement = '$1 is null &&' }
    @{ Pattern = '([a-zA-Z_][a-zA-Z0-9_]*)\s*!=\s*null\s*\|\|'; Replacement = '$1 is not null ||' }
    @{ Pattern = '([a-zA-Z_][a-zA-Z0-9_]*)\s*==\s*null\s*\|\|'; Replacement = '$1 is null ||' }
    @{ Pattern = '\?\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*!=\s*null\s*:'; Replacement = '? $1 is not null :' }
    @{ Pattern = '\?\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*==\s*null\s*:'; Replacement = '? $1 is null :' }
    @{ Pattern = 'return\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*!=\s*null'; Replacement = 'return $1 is not null' }
    @{ Pattern = 'return\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*==\s*null'; Replacement = 'return $1 is null' }
)

# Get all C# files in the project (excluding problematic ones)
$excludeFiles = @("ExternalReportSearchResults.razor.cs", "TechnicalAssessment.razor.cs")
$files = Get-ChildItem -Path $ProjectPath -Recurse -Include "*.cs" -Exclude "*obj*", "*bin*" | 
    Where-Object { $excludeFiles -notcontains $_.Name }

Write-Host "Found $($files.Count) C# files to process" -ForegroundColor Yellow

$totalReplacements = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileReplacements = 0

    # Apply each pattern replacement
    foreach ($patternInfo in $patterns) {
        $pattern = $patternInfo.Pattern
        $replacement = $patternInfo.Replacement

        $matches = [regex]::Matches($content, $pattern)
        if ($matches.Count -gt 0) {
            $content = $content -replace $pattern, $replacement
            $fileReplacements += $matches.Count
        }
    }

    # Write back if changes were made
    if ($content -ne $originalContent) {
        if ($WhatIf) {
            Write-Host "  WOULD FIX: $($file.Name) - $fileReplacements replacements" -ForegroundColor Yellow
        } else {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            Write-Host "  FIXED: $($file.Name) - $fileReplacements replacements" -ForegroundColor Green
        }
        $totalReplacements += $fileReplacements
    }
}

Write-Host "`n? Complete! Total replacements: $totalReplacements" -ForegroundColor Green

if ($WhatIf) {
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Cyan
} else {
    Write-Host "Run 'dotnet build' to verify fixes" -ForegroundColor Cyan
}