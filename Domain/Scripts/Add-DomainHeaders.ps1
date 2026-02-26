# PowerShell Script to Add Headers to Domain Layer Files
# SMS Safety Management System - Domain Layer Header Management
# Author: SMS Development Team
# Date: January 2025

param(
    [string]$ProjectPath = ".",
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

# Define header templates for Domain layer file types
$HeaderTemplates = @{
    "Entity" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------
"@

    "ValueObject" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Enum" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Interface" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Common" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Error" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain error definitions providing structured error handling
//                  with business-meaningful error codes and messages.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Model" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Exception" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Domain-specific exception for business rule violations
//                  and exceptional conditions within the domain layer.
// </copyright>
//-----------------------------------------------------------------------
"@
}

# Define file patterns and their corresponding template types
$FilePatterns = @{
    "Base*.cs" = "Common"
    "*ID.cs" = "ValueObject"
    "*Error*.cs" = "Error"
    "*Exception*.cs" = "Exception"
    "I*.cs" = "Interface"
    "*Enum*.cs" = "Enum"
    "*Statistics.cs" = "Model"
    "*Dashboard.cs" = "Model"
    "*Data.cs" = "Model"
}

# Function to determine file description based on filename and content
function Get-FileDescription {
    param([string]$fileName, [string]$content, [string]$relativePath)
    
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    
    # Entity descriptions (in Entities folder)
    if ($relativePath -match "\\Entities\\" -and $fileName -match "\.cs$" -and $fileName -notmatch "ID\.cs$") {
        if ($baseName -match ".*User.*" -and $baseName -notmatch "ID$") { return "SMS user entity representing $($baseName.ToLower()) with authentication and authorization capabilities." }
        if ($baseName -match ".*Audit.*" -and $baseName -notmatch "ID$") { return "SMS audit entity representing $($baseName.ToLower()) for compliance and regulatory requirements." }
        if ($baseName -match ".*Hazard.*" -and $baseName -notmatch "ID$") { return "SMS hazard entity representing $($baseName.ToLower()) for safety management processes." }
        if ($baseName -match ".*Risk.*" -and $baseName -notmatch "ID$") { return "SMS risk entity representing $($baseName.ToLower()) for risk assessment and management." }
        if ($baseName -match ".*Investigation.*" -and $baseName -notmatch "ID$") { return "SMS investigation entity representing $($baseName.ToLower()) for safety investigation workflows." }
        if ($baseName -match ".*Report.*" -and $baseName -notmatch "ID$") { return "SMS report entity representing $($baseName.ToLower()) for safety reporting processes." }
        if ($baseName -match ".*Mitigation.*" -and $baseName -notmatch "ID$") { return "SMS mitigation entity representing $($baseName.ToLower()) for risk mitigation strategies." }
        if ($baseName -match ".*SPI.*" -or $baseName -match ".*SafetyPerformance.*") { return "SMS safety performance indicator entity for measuring and tracking safety metrics." }
        return "SMS domain entity representing $($baseName.ToLower()) with business rules and lifecycle management."
    }
    
    # Value Object descriptions (in ValueObjects folder)
    if ($relativePath -match "\\ValueObjects\\" -or $fileName -match ".*ID\.cs$") {
        if ($baseName -match ".*Name") { return "Value object representing $($baseName.ToLower()) with validation rules and business constraints." }
        if ($baseName -match ".*Password") { return "Value object representing secure password with hashing, validation, and policy enforcement." }
        if ($baseName -match ".*Permission") { return "Value object representing $($baseName.ToLower()) with access control and authorization logic." }
        if ($baseName -match ".*ID$") { return "Strongly-typed identifier for $($baseName.Replace('ID', '').ToLower()) entities ensuring type safety." }
        if ($baseName -match ".*Assignment") { return "Value object representing $($baseName.ToLower()) with workflow and validation rules." }
        return "Value object representing $($baseName.ToLower()) with immutable properties and business validation."
    }
    
    # Enum descriptions
    if ($relativePath -match "\\Enums\\" -or $fileName -match ".*Enum.*\.cs$") {
        if ($baseName -match ".*Status") { return "Enumeration defining valid status values for SMS $($baseName.Replace('Status', '').ToLower()) workflows." }
        if ($baseName -match ".*Type") { return "Enumeration defining classification types for SMS $($baseName.Replace('Type', '').ToLower()) entities." }
        if ($baseName -match ".*Level") { return "Enumeration defining level classifications for SMS $($baseName.Replace('Level', '').ToLower()) assessment." }
        if ($baseName -match ".*Category") { return "Enumeration defining category classifications for SMS $($baseName.Replace('Category', '').ToLower()) management." }
        if ($baseName -match "SPI.*") { return "Enumeration defining Safety Performance Indicator classifications and measurement types." }
        return "Enumeration defining valid values and classifications for SMS $($baseName.ToLower()) domain concepts."
    }
    
    # Error descriptions
    if ($relativePath -match "\\Errors\\" -or $fileName -match ".*Error.*\.cs$") {
        return "Comprehensive error catalog defining structured error handling for SMS domain operations with business-meaningful error codes and messages."
    }
    
    # Exception descriptions
    if ($relativePath -match "\\Exceptions\\" -or $fileName -match ".*Exception.*\.cs$") {
        return "Domain-specific exception for SMS business rule violations and exceptional conditions requiring special handling."
    }
    
    # Interface descriptions
    if ($fileName -match "^I.*\.cs$") {
        if ($baseName -match ".*Repository") { return "Repository contract defining data access operations for SMS $($baseName.Replace('Repository', '').Replace('I', '').ToLower()) entities." }
        if ($baseName -match ".*Service") { return "Domain service contract defining complex business operations for SMS $($baseName.Replace('Service', '').Replace('I', '').ToLower()) coordination." }
        if ($baseName -match ".*Event") { return "Domain event contract defining event structure for SMS $($baseName.Replace('Event', '').Replace('I', '').ToLower()) notifications." }
        return "Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic."
    }
    
    # Common/Base class descriptions
    if ($relativePath -match "\\Common\\" -or $fileName -match "Base.*\.cs$") {
        if ($baseName -match ".*Entity") { return "Base entity class providing common functionality for SMS domain entities with identity and lifecycle management." }
        if ($baseName -match ".*ValueObject") { return "Base value object class providing structural equality and immutability for SMS domain value objects." }
        if ($baseName -match ".*Event") { return "Base domain event class providing event infrastructure for SMS domain-driven design patterns." }
        if ($baseName -match ".*Result") { return "Result pattern implementation for SMS domain operations providing explicit success/failure handling." }
        if ($baseName -match ".*Error") { return "Base error class providing structured error representation for SMS domain error handling." }
        return "Base infrastructure class providing common functionality for SMS domain components."
    }
    
    # Model descriptions
    if ($relativePath -match "\\Models\\" -or $fileName -match ".*Model.*\.cs$" -or $fileName -match ".*Statistics.*\.cs$" -or $fileName -match ".*Dashboard.*\.cs$") {
        if ($baseName -match ".*Statistics") { return "Domain model representing statistical data and metrics for SMS $($baseName.Replace('Statistics', '').ToLower()) reporting." }
        if ($baseName -match ".*Dashboard") { return "Domain model representing dashboard data structures for SMS $($baseName.Replace('Dashboard', '').ToLower()) visualization." }
        if ($baseName -match ".*Calendar") { return "Domain model representing calendar and scheduling data for SMS planning and coordination." }
        return "Domain model representing complex data structures for SMS business reporting and analytics."
    }
    
    # Default description based on location
    if ($relativePath -match "\\Entities\\") { return "SMS domain entity with business rules, invariants, and lifecycle management." }
    if ($relativePath -match "\\ValueObjects\\") { return "SMS value object with immutable properties, validation rules, and business logic." }
    if ($relativePath -match "\\Enums\\") { return "SMS domain enumeration defining valid states and business classifications." }
    if ($relativePath -match "\\Interfaces\\") { return "SMS domain contract defining operations and ensuring architectural boundaries." }
    if ($relativePath -match "\\Common\\") { return "Shared domain infrastructure providing base functionality for Domain-Driven Design." }
    if ($relativePath -match "\\Errors\\") { return "SMS domain error definitions with structured error codes and business-meaningful messages." }
    if ($relativePath -match "\\Models\\") { return "SMS domain model for complex data representation and business analytics." }
    if ($relativePath -match "\\Exceptions\\") { return "SMS domain exception for business rule violations and exceptional conditions." }
    
    return "SMS domain component implementing business logic and rules following Domain-Driven Design principles."
}

# Function to determine template type
function Get-TemplateType {
    param([string]$fileName, [string]$relativePath)
    
    # Check specific patterns first
    foreach ($pattern in $FilePatterns.Keys) {
        if ($fileName -like $pattern) {
            return $FilePatterns[$pattern]
        }
    }
    
    # Determine by folder location
    if ($relativePath -match "\\Entities\\") { return "Entity" }
    if ($relativePath -match "\\ValueObjects\\") { return "ValueObject" }
    if ($relativePath -match "\\Enums\\") { return "Enum" }
    if ($relativePath -match "\\Interfaces\\") { return "Interface" }
    if ($relativePath -match "\\Common\\") { return "Common" }
    if ($relativePath -match "\\Errors\\") { return "Error" }
    if ($relativePath -match "\\Models\\") { return "Model" }
    if ($relativePath -match "\\Exceptions\\") { return "Exception" }
    
    # Default based on naming patterns
    if ($fileName -match "^I.*\.cs$") { return "Interface" }
    if ($fileName -match ".*ID\.cs$") { return "ValueObject" }
    if ($fileName -match ".*Enum.*\.cs$") { return "Enum" }
    if ($fileName -match "Base.*\.cs$") { return "Common" }
    
    return "Entity"  # Default for domain layer
}

# Function to check if file already has a header
function Has-Header {
    param([string]$content)
    
    return $content -match "//-----------------------------------------------------------------------" -or 
           $content -match "<copyright" -or 
           $content -match "/// <summary>" -or
           $content -match "Author:" -or
           $content -match "Description:"
}

# Function to add header to file
function Add-HeaderToFile {
    param(
        [string]$filePath,
        [switch]$WhatIf
    )
    
    $fileName = [System.IO.Path]::GetFileName($filePath)
    $content = Get-Content $filePath -Raw
    $relativePath = $filePath.Replace($ProjectPath, "")
    
    if (Has-Header $content) {
        if ($Verbose) { Write-Host "Skipping $fileName - already has header" -ForegroundColor Yellow }
        return $false
    }
    
    $templateType = Get-TemplateType $fileName $relativePath
    $description = Get-FileDescription $fileName $content $relativePath
    $template = $HeaderTemplates[$templateType]
    $header = $template -f $fileName, $description
    
    if ($WhatIf) {
        Write-Host "Would add header to: $fileName" -ForegroundColor Green
        Write-Host "Template type: $templateType" -ForegroundColor Cyan
        Write-Host "Description: $description" -ForegroundColor Gray
        return $true
    }
    
    # Add header to beginning of file
    $newContent = $header + "`r`n`r`n" + $content
    Set-Content $filePath -Value $newContent -Encoding UTF8
    
    Write-Host "Added header to: $fileName" -ForegroundColor Green
    return $true
}

# Main execution
Write-Host "SMS Domain Layer Header Management Script" -ForegroundColor Magenta
Write-Host "=========================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files will be modified" -ForegroundColor Yellow
}

# Get all C# files in critical directories first (Domain priority)
$CriticalDirectories = @("Common", "Entities", "ValueObjects", "Enums", "Interfaces", "Errors")
$processedCount = 0
$skippedCount = 0

foreach ($dir in $CriticalDirectories) {
    $dirPath = Join-Path $ProjectPath $dir
    if (Test-Path $dirPath) {
        Write-Host "`nProcessing critical directory: $dir" -ForegroundColor Cyan
        $files = Get-ChildItem -Path $dirPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -notmatch "obj\\|bin\\" }
        
        foreach ($file in $files) {
            if (Add-HeaderToFile $file.FullName -WhatIf:$WhatIf) {
                $processedCount++
            } else {
                $skippedCount++
            }
        }
    }
}

# Then process remaining directories
$RemainingDirectories = @("Models", "Exceptions")
foreach ($dir in $RemainingDirectories) {
    $dirPath = Join-Path $ProjectPath $dir
    if (Test-Path $dirPath) {
        Write-Host "`nProcessing directory: $dir" -ForegroundColor Cyan
        $files = Get-ChildItem -Path $dirPath -Filter "*.cs" -Recurse | Where-Object { $_.Name -notmatch "obj\\|bin\\" }
        
        foreach ($file in $files) {
            if (Add-HeaderToFile $file.FullName -WhatIf:$WhatIf) {
                $processedCount++
            } else {
                $skippedCount++
            }
        }
    }
}

# Summary
Write-Host "`n=========================================" -ForegroundColor Magenta
Write-Host "Summary:" -ForegroundColor Magenta
Write-Host "Files processed: $processedCount" -ForegroundColor Green
Write-Host "Files skipped: $skippedCount" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "`nTo apply changes, run without -WhatIf parameter" -ForegroundColor Yellow
}