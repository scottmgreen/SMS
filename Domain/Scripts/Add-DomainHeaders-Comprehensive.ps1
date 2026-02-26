# PowerShell Script to Add Headers to Domain Layer Files - COMPREHENSIVE VERSION
# SMS Safety Management System - Domain Layer Header Management (NO SKIPPING!)
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

# Function to check if file has PROPER COPYRIGHT header (not just XML comments or namespace)
function Has-CopyrightHeader {
    param([string]$content)
    
    # We need ACTUAL copyright headers with SMS company name, not just any comment
    return $content -match "//-----------------------------------------------------------------------" -and 
           $content -match "<copyright" -and 
           $content -match "SMS Safety Management System"
}

# Function to determine file description based on filename and content
function Get-FileDescription {
    param([string]$fileName, [string]$content, [string]$relativePath)
    
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    
    # Entity descriptions (in Entities folder)
    if ($relativePath -match "\\Entities\\" -and $fileName -like "*.cs" -and $fileName -notlike "*ID.cs") {
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
    
    # Value Object descriptions (in ValueObjects folder or ID files)
    if ($relativePath -match "\\ValueObjects\\" -or $fileName -like "*ID.cs") {
        if ($baseName -match ".*Name") { return "Value object representing $($baseName.ToLower()) with validation rules and business constraints." }
        if ($baseName -match ".*Password") { return "Value object representing secure password with hashing, validation, and policy enforcement." }
        if ($baseName -match ".*Permission") { return "Value object representing $($baseName.ToLower()) with access control and authorization logic." }
        if ($baseName -match ".*ID$") { return "Strongly-typed identifier for $($baseName.Replace('ID', '').ToLower()) entities ensuring type safety." }
        if ($baseName -match ".*Assignment") { return "Value object representing $($baseName.ToLower()) with workflow and validation rules." }
        return "Value object representing $($baseName.ToLower()) with immutable properties and business validation."
    }
    
    # Enum descriptions
    if ($relativePath -match "\\Enums\\" -or $fileName -like "*Enum*.cs") {
        if ($baseName -match ".*Status") { return "Enumeration defining valid status values for SMS $($baseName.Replace('Status', '').ToLower()) workflows." }
        if ($baseName -match ".*Type") { return "Enumeration defining classification types for SMS $($baseName.Replace('Type', '').ToLower()) entities." }
        if ($baseName -match ".*Level") { return "Enumeration defining level classifications for SMS $($baseName.Replace('Level', '').ToLower()) assessment." }
        if ($baseName -match ".*Category") { return "Enumeration defining category classifications for SMS $($baseName.Replace('Category', '').ToLower()) management." }
        if ($baseName -match "SPI.*") { return "Enumeration defining Safety Performance Indicator classifications and measurement types." }
        return "Enumeration defining valid values and classifications for SMS $($baseName.ToLower()) domain concepts."
    }
    
    # Error descriptions
    if ($relativePath -match "\\Errors\\" -or $fileName -like "*Error*.cs") {
        return "Comprehensive error catalog defining structured error handling for SMS domain operations with business-meaningful error codes and messages."
    }
    
    # Exception descriptions
    if ($relativePath -match "\\Exceptions\\" -or $fileName -like "*Exception*.cs") {
        return "Domain-specific exception for SMS business rule violations and exceptional conditions requiring special handling."
    }
    
    # Interface descriptions
    if ($fileName -like "I*.cs") {
        if ($baseName -match ".*Repository") { return "Repository contract defining data access operations for SMS $($baseName.Replace('Repository', '').Replace('I', '').ToLower()) entities." }
        if ($baseName -match ".*Service") { return "Domain service contract defining complex business operations for SMS $($baseName.Replace('Service', '').Replace('I', '').ToLower()) coordination." }
        if ($baseName -match ".*Event") { return "Domain event contract defining event structure for SMS $($baseName.Replace('Event', '').Replace('I', '').ToLower()) notifications." }
        return "Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic."
    }
    
    # Common/Base class descriptions
    if ($relativePath -match "\\Common\\" -or $fileName -like "Base*.cs") {
        if ($baseName -match ".*Entity") { return "Base entity class providing common functionality for SMS domain entities with identity and lifecycle management." }
        if ($baseName -match ".*ValueObject") { return "Base value object class providing structural equality and immutability for SMS domain value objects." }
        if ($baseName -match ".*Event") { return "Base domain event class providing event infrastructure for SMS domain-driven design patterns." }
        if ($baseName -match ".*Result") { return "Result pattern implementation for SMS domain operations providing explicit success/failure handling." }
        if ($baseName -match ".*Error") { return "Base error class providing structured error representation for SMS domain error handling." }
        return "Base infrastructure class providing common functionality for SMS domain components."
    }
    
    # Model descriptions
    if ($relativePath -match "\\Models\\" -or $fileName -like "*Model*.cs" -or $fileName -like "*Statistics*.cs" -or $fileName -like "*Dashboard*.cs") {
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
    
    # Determine by folder location first
    if ($relativePath -match "\\Entities\\") { return "Entity" }
    if ($relativePath -match "\\ValueObjects\\") { return "ValueObject" }
    if ($relativePath -match "\\Enums\\") { return "Enum" }
    if ($relativePath -match "\\Interfaces\\") { return "Interface" }
    if ($relativePath -match "\\Common\\") { return "Common" }
    if ($relativePath -match "\\Errors\\") { return "Error" }
    if ($relativePath -match "\\Models\\") { return "Model" }
    if ($relativePath -match "\\Exceptions\\") { return "Exception" }
    
    # Check specific patterns
    if ($fileName -like "I*.cs") { return "Interface" }
    if ($fileName -like "*ID.cs") { return "ValueObject" }
    if ($fileName -like "*Enum*.cs") { return "Enum" }
    if ($fileName -like "Base*.cs") { return "Common" }
    if ($fileName -like "*Error*.cs") { return "Error" }
    if ($fileName -like "*Exception*.cs") { return "Exception" }
    if ($fileName -like "*Statistics.cs") { return "Model" }
    if ($fileName -like "*Dashboard.cs") { return "Model" }
    if ($fileName -like "*Data.cs") { return "Model" }
    
    return "Entity"  # Default for domain layer
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
    
    if (Has-CopyrightHeader $content) {
        if ($Verbose) { Write-Host "Skipping $fileName - already has proper copyright header" -ForegroundColor Yellow }
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
Write-Host "SMS Domain Layer Header Management Script - COMPREHENSIVE VERSION" -ForegroundColor Magenta
Write-Host "=================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files will be modified" -ForegroundColor Yellow
}

# Get ALL C# files, no exceptions
$processedCount = 0
$skippedCount = 0

Write-Host "`nProcessing ALL C# files in Domain project..." -ForegroundColor Cyan
$allFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse | Where-Object { 
    $_.FullName -notmatch "obj\\|bin\\" -and 
    $_.FullName -notmatch "AssemblyInfo\.cs$" -and
    $_.FullName -notmatch "GlobalUsings\.g\.cs$" -and
    $_.FullName -notmatch "\.NETCoreApp"
}

Write-Host "Found $($allFiles.Count) C# files to process" -ForegroundColor White

foreach ($file in $allFiles) {
    $relativeFilePath = $file.FullName.Replace($ProjectPath, "").TrimStart('\')
    Write-Host "  Processing: $relativeFilePath" -ForegroundColor Gray
    
    if (Add-HeaderToFile $file.FullName -WhatIf:$WhatIf) {
        $processedCount++
    } else {
        $skippedCount++
    }
}

# Summary
Write-Host "`n=================================================================" -ForegroundColor Magenta
Write-Host "COMPREHENSIVE SUMMARY:" -ForegroundColor Magenta
Write-Host "Files processed: $processedCount" -ForegroundColor Green
Write-Host "Files skipped: $skippedCount" -ForegroundColor Yellow
Write-Host "Total files checked: $($allFiles.Count)" -ForegroundColor White
Write-Host "=================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "`nTo apply changes, run without -WhatIf parameter" -ForegroundColor Yellow
}