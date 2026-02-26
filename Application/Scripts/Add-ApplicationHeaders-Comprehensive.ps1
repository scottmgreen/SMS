# PowerShell Script to Add Headers to Application Layer Files - COMPREHENSIVE VERSION
# SMS Safety Management System - Application Layer Header Management (NO SKIPPING!)
# Author: SMS Development Team
# Date: January 2025

param(
    [string]$ProjectPath = ".",
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

# Define header templates for different file types
$HeaderTemplates = @{
    "Service" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Command" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Query" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------
"@

    "CommandHandler" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------
"@

    "QueryHandler" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Interface" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Pipeline" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Configuration" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Provides dependency injection configuration and service registration
//                  for the Application layer in the Clean Architecture.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Common" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------
"@

    "State" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Implements state management for workflow and business process
//                  coordination within the Application layer.
// </copyright>
//-----------------------------------------------------------------------
"@
}

# Function to check if file has PROPER COPYRIGHT header (not just XML comments)
function Has-CopyrightHeader {
    param([string]$content)
    
    # We need ACTUAL copyright headers, not just XML documentation
    return $content -match "//-----------------------------------------------------------------------" -and 
           $content -match "<copyright" -and 
           $content -match "SMS Safety Management System"
}

# Function to determine file description based on filename and content
function Get-FileDescription {
    param([string]$fileName, [string]$content, [string]$relativePath)
    
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    
    # Service descriptions
    if ($fileName -match ".*Service\.cs$") {
        if ($baseName -match "SMS.*User.*Service") { return "SMS User management service handling user lifecycle and authentication operations." }
        if ($baseName -match ".*Audit.*Service") { return "SMS Audit management service coordinating audit planning, execution, and reporting." }
        if ($baseName -match ".*Investigation.*Service") { return "SMS Investigation workflow service managing safety investigation processes." }
        if ($baseName -match ".*Hazard.*Service") { return "SMS Hazard management service handling hazard identification and lifecycle." }
        if ($baseName -match ".*Risk.*Service") { return "SMS Risk assessment service managing risk analysis and mitigation strategies." }
        if ($baseName -match ".*Report.*Service") { return "SMS Report management service handling report creation, validation, and processing." }
        if ($baseName -match ".*Mitigation.*Service") { return "SMS Mitigation management service coordinating risk mitigation implementation." }
        return "Application service providing business logic operations for SMS domain entities."
    }
    
    # Command descriptions
    if ($fileName -match ".*Commands\.cs$") {
        if ($baseName -match ".*User.*") { return "Command definitions for SMS user management operations including create, update, delete actions." }
        if ($baseName -match ".*Audit.*") { return "Command definitions for SMS audit management operations and workflow actions." }
        if ($baseName -match ".*Investigation.*") { return "Command definitions for SMS investigation workflow and process management." }
        if ($baseName -match ".*Hazard.*") { return "Command definitions for SMS hazard management and lifecycle operations." }
        if ($baseName -match ".*Risk.*") { return "Command definitions for SMS risk assessment and analysis operations." }
        if ($baseName -match ".*Report.*") { return "Command definitions for SMS report management and processing operations." }
        return "Command definitions for write operations in the SMS CQRS architecture."
    }
    
    # Query descriptions
    if ($fileName -match ".*Queries\.cs$") {
        if ($baseName -match ".*User.*") { return "Query definitions for SMS user data retrieval and search operations." }
        if ($baseName -match ".*Audit.*") { return "Query definitions for SMS audit data retrieval and reporting operations." }
        if ($baseName -match ".*Investigation.*") { return "Query definitions for SMS investigation data retrieval and status tracking." }
        if ($baseName -match ".*Hazard.*") { return "Query definitions for SMS hazard data retrieval and analysis operations." }
        if ($baseName -match ".*Risk.*") { return "Query definitions for SMS risk assessment data retrieval and reporting." }
        if ($baseName -match ".*Report.*") { return "Query definitions for SMS report data retrieval and search operations." }
        return "Query definitions for read operations in the SMS CQRS architecture."
    }
    
    # Handler descriptions
    if ($fileName -match ".*CommandHandlers\.cs$") {
        if ($baseName -match ".*User.*") { return "Command handlers implementing SMS user management business logic and operations." }
        if ($baseName -match ".*Audit.*") { return "Command handlers implementing SMS audit management business logic and workflow." }
        if ($baseName -match ".*Investigation.*") { return "Command handlers implementing SMS investigation workflow and process logic." }
        if ($baseName -match ".*Hazard.*") { return "Command handlers implementing SMS hazard management and lifecycle logic." }
        if ($baseName -match ".*Risk.*") { return "Command handlers implementing SMS risk assessment and analysis logic." }
        if ($baseName -match ".*Report.*") { return "Command handlers implementing SMS report management and processing logic." }
        return "Command handlers implementing business logic for SMS write operations."
    }
    
    if ($fileName -match ".*QueryHandlers\.cs$") {
        if ($baseName -match ".*User.*") { return "Query handlers implementing SMS user data retrieval and search logic." }
        if ($baseName -match ".*Audit.*") { return "Query handlers implementing SMS audit data retrieval and reporting logic." }
        if ($baseName -match ".*Investigation.*") { return "Query handlers implementing SMS investigation data retrieval and tracking logic." }
        if ($baseName -match ".*Hazard.*") { return "Query handlers implementing SMS hazard data retrieval and analysis logic." }
        if ($baseName -match ".*Risk.*") { return "Query handlers implementing SMS risk assessment data retrieval logic." }
        if ($baseName -match ".*Report.*") { return "Query handlers implementing SMS report data retrieval and search logic." }
        return "Query handlers implementing data retrieval logic for SMS read operations."
    }
    
    # Interface descriptions
    if ($fileName -match "^I.*\.cs$") {
        if ($baseName -match ".*Service") { return "Service contract defining operations for SMS business logic coordination." }
        if ($baseName -match ".*Repository") { return "Repository contract defining data access operations for SMS entities." }
        if ($baseName -match ".*Mediator") { return "Mediator contract defining request/response handling in CQRS architecture." }
        if ($baseName -match ".*Pipeline") { return "Pipeline contract defining cross-cutting concern processing interface." }
        return "Interface contract defining operations and ensuring clean architecture boundaries."
    }
    
    # Default descriptions based on path
    if ($relativePath -match "\\Common\\") { return "Shared utility providing common functionality for Application layer components." }
    if ($relativePath -match "\\Configuration\\") { return "Configuration component for Application layer dependency injection and setup." }
    if ($relativePath -match "\\States\\") { return "State management component for workflow and business process coordination." }
    
    return "Application layer component providing functionality for the SMS safety management system."
}

# Function to determine template type
function Get-TemplateType {
    param([string]$fileName, [string]$relativePath)
    
    if ($fileName -like "*Service.cs") { return "Service" }
    if ($fileName -like "*Commands.cs") { return "Command" }
    if ($fileName -like "*Queries.cs") { return "Query" }
    if ($fileName -like "*CommandHandlers.cs") { return "CommandHandler" }
    if ($fileName -like "*QueryHandlers.cs") { return "QueryHandler" }
    if ($fileName -like "*CommandHandler.cs") { return "CommandHandler" }
    if ($fileName -like "*QueryHandler.cs") { return "QueryHandler" }
    if ($fileName -like "I*.cs") { return "Interface" }
    if ($fileName -like "*Pipeline.cs") { return "Pipeline" }
    if ($fileName -eq "DependencyInjection.cs") { return "Configuration" }
    if ($fileName -eq "ServiceCollectionExtensions.cs") { return "Configuration" }
    if ($fileName -like "*State.cs") { return "State" }
    
    # Default based on path
    if ($relativePath -match "\\Common\\") { return "Common" }
    if ($relativePath -match "\\Configuration\\") { return "Configuration" }
    if ($relativePath -match "\\States\\") { return "State" }
    if ($relativePath -match "\\Interfaces\\") { return "Interface" }
    
    return "Common"  # Default template
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
Write-Host "SMS Application Layer Header Management Script - COMPREHENSIVE VERSION" -ForegroundColor Magenta
Write-Host "=====================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files will be modified" -ForegroundColor Yellow
}

# Get ALL C# files, no exceptions
$processedCount = 0
$skippedCount = 0

Write-Host "`nProcessing ALL C# files in Application project..." -ForegroundColor Cyan
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
Write-Host "`n=====================================================================" -ForegroundColor Magenta
Write-Host "COMPREHENSIVE SUMMARY:" -ForegroundColor Magenta
Write-Host "Files processed: $processedCount" -ForegroundColor Green
Write-Host "Files skipped: $skippedCount" -ForegroundColor Yellow
Write-Host "Total files checked: $($allFiles.Count)" -ForegroundColor White
Write-Host "=====================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "`nTo apply changes, run without -WhatIf parameter" -ForegroundColor Yellow
}