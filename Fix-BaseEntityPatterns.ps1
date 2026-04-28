# Comprehensive BaseEntity Pattern Fixer for SMS3 Project
# This script fixes the most common BaseEntity comparison patterns causing CS8604/CS8625 warnings

param(
    [string]$ProjectPath = "Presentation\SMS3",
    [switch]$WhatIf = $false
)

Write-Host "?? SMS3 BaseEntity Pattern Fixer" -ForegroundColor Green
Write-Host "Target: $ProjectPath" -ForegroundColor Cyan

# Define pattern replacements
$patterns = @{
    # Basic entity null checks
    ' != null'        = ' is not null'
    ' == null'        = ' is null'
    '!= null\)'      = 'is not null)'
    '== null\)'      = 'is null)'
    '(.*) != null'    = '$1 is not null'
    '(.*) == null'    = '$1 is null'
}

# Get all C# files in the project
$files = Get-ChildItem -Path $ProjectPath -Recurse -Include "*.cs" -Exclude "*obj*", "*bin*"

Write-Host "Found $($files.Count) C# files to process" -ForegroundColor Yellow

$totalReplacements = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileReplacements = 0

    # Apply each pattern replacement
    foreach ($pattern in $patterns.GetEnumerator()) {
        $oldPattern = $pattern.Key
        $newPattern = $pattern.Value

        $matches = [regex]::Matches($content, $oldPattern)
        if ($matches.Count -gt 0) {
            $content = $content -replace [regex]::Escape($oldPattern), $newPattern
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