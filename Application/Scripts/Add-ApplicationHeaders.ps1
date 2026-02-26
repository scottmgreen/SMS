# PowerShell Script to Add Headers to Application Layer Files
# SMS Safety Management System - Application Layer Header Management
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

# Define file patterns and their corresponding template types
$FilePatterns = @{
    "*Service.cs" = "Service"
    "*Commands.cs" = "Command"
    "*Queries.cs" = "Query"
    "*CommandHandlers.cs" = "CommandHandler"
    "*QueryHandlers.cs" = "QueryHandler"
    "*CommandHandler.cs" = "CommandHandler"
    "*QueryHandler.cs" = "QueryHandler"
    "I*.cs" = "Interface"
    "*Pipeline.cs" = "Pipeline"
    "DependencyInjection.cs" = "Configuration"
    "ServiceCollectionExtensions.cs" = "Configuration"
    "*State.cs" = "State"
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
    
    foreach ($pattern in $FilePatterns.Keys) {
        if ($fileName -like $pattern) {
            return $FilePatterns[$pattern]
        }
    }
    
    # Default based on path
    if ($relativePath -match "\\Common\\") { return "Common" }
    if ($relativePath -match "\\Configuration\\") { return "Configuration" }
    if ($relativePath -match "\\States\\") { return "State" }
    if ($relativePath -match "\\Interfaces\\") { return "Interface" }
    
    return "Common"  # Default template
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
Write-Host "SMS Application Layer Header Management Script" -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files will be modified" -ForegroundColor Yellow
}

# Get all C# files in critical directories first
$CriticalDirectories = @("Services", "Interfaces", "Configuration", "Common")
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
$RemainingDirectories = @("Messaging", "States")
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
Write-Host "`n=============================================" -ForegroundColor Magenta
Write-Host "Summary:" -ForegroundColor Magenta
Write-Host "Files processed: $processedCount" -ForegroundColor Green
Write-Host "Files skipped: $skippedCount" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "`nTo apply changes, run without -WhatIf parameter" -ForegroundColor Yellow
}