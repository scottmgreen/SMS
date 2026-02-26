# PowerShell Script to Add Headers to Infrastructure Layer Files - COMPREHENSIVE VERSION
# SMS Safety Management System - Infrastructure Layer Header Management (NO SKIPPING!)
# Author: SMS Development Team
# Date: January 2025

param(
    [string]$ProjectPath = ".",
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

# Define header templates for Infrastructure layer file types
$HeaderTemplates = @{
    "Repository" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------
"@

    "DataService" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Data service providing business-focused data operations
//                  with repository coordination and transaction management.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Configuration" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Infrastructure configuration providing dependency injection,
//                  service registration, and system setup.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Middleware" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  ASP.NET Core middleware component providing cross-cutting
//                  concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Security" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Security component providing authentication, authorization,
//                  and access control functionality.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Common" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Interface" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------
"@

    "Service" = @"
//-----------------------------------------------------------------------
// <copyright file="{0}" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: {1}
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------
"@
}

# Function to check if file has PROPER COPYRIGHT header (not just XML comments)
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
    
    # Repository descriptions
    if ($fileName -like "*Repository.cs") {
        if ($baseName -match ".*User.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities with CRUD operations and business queries." }
        if ($baseName -match ".*Audit.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities supporting compliance and audit workflows." }
        if ($baseName -match ".*Hazard.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities with safety management integration." }
        if ($baseName -match ".*Risk.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities supporting risk assessment workflows." }
        if ($baseName -match ".*Investigation.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities with investigation workflow support." }
        if ($baseName -match ".*Report.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities with reporting and validation workflows." }
        if ($baseName -match ".*Mitigation.*Repository") { return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities supporting mitigation tracking and management." }
        if ($baseName -match "BaseRepository") { return "Base repository class providing common data access functionality with connection management and stored procedure execution." }
        return "Repository implementing data access operations for SMS $($baseName.Replace('Repository', '').ToLower()) entities with stored procedure integration."
    }
    
    # Data Service descriptions  
    if ($fileName -like "*DataService.cs") {
        if ($baseName -match ".*User.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations with business logic and validation." }
        if ($baseName -match ".*Audit.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations supporting compliance and audit processes." }
        if ($baseName -match ".*Hazard.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations with safety management workflows." }
        if ($baseName -match ".*Risk.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations supporting risk assessment and analysis." }
        if ($baseName -match ".*Investigation.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations with investigation workflow management." }
        if ($baseName -match ".*Report.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations with reporting and validation processes." }
        if ($baseName -match ".*Mitigation.*DataService") { return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations supporting mitigation implementation and tracking." }
        return "Data service coordinating $($baseName.Replace('DataService', '').ToLower()) repository operations with transaction management and business validation."
    }
    
    # Configuration descriptions
    if ($fileName -like "DependencyInjection.cs" -or $fileName -like "ServiceCollectionExtensions.cs") {
        return "Infrastructure layer dependency injection configuration providing service registration, repository setup, and data service coordination."
    }
    
    # Middleware descriptions
    if ($fileName -like "*Middleware.cs") {
        if ($baseName -match ".*Logger.*") { return "ASP.NET Core middleware providing request/response logging with correlation IDs and performance metrics." }
        if ($baseName -match ".*Connection.*") { return "ASP.NET Core middleware providing database connection tracking and performance monitoring." }
        if ($baseName -match ".*User.*") { return "ASP.NET Core middleware providing user context tracking and session management for Blazor applications." }
        return "ASP.NET Core middleware providing $($baseName.Replace('Middleware', '').ToLower()) functionality in the HTTP request pipeline."
    }
    
    # Security descriptions
    if ($fileName -like "*Filter.cs" -or $fileName -like "*Authentication*.cs" -or $fileName -like "*Authorization*.cs") {
        if ($baseName -match "ApiKey") { return "API key authentication filter providing secure service-to-service communication with validation and audit logging." }
        if ($baseName -match ".*Authentication.*") { return "Authentication component providing user identity verification with session management and security logging." }
        if ($baseName -match ".*Authorization.*") { return "Authorization component providing role-based access control with permission validation and audit trails." }
        return "Security component providing $($baseName.ToLower()) functionality with authentication and authorization support."
    }
    
    # Interface descriptions
    if ($fileName -like "I*.cs") {
        if ($baseName -match ".*Repository") { return "Repository interface defining data access contracts for $($baseName.Replace('Repository', '').Replace('I', '').ToLower()) entities with CRUD and query operations." }
        if ($baseName -match ".*DataService") { return "Data service interface defining business data operations for $($baseName.Replace('DataService', '').Replace('I', '').ToLower()) coordination and transaction management." }
        if ($baseName -match ".*Service") { return "Service interface defining operations for $($baseName.Replace('Service', '').Replace('I', '').ToLower()) functionality with dependency injection support." }
        if ($baseName -match ".*Connection") { return "Connection service interface defining database connectivity and connection management operations." }
        if ($baseName -match ".*LogSupport") { return "Logging support interface defining structured logging operations with correlation and audit trail support." }
        if ($baseName -match ".*Factory") { return "Factory interface defining object creation and initialization operations with dependency injection integration." }
        return "Infrastructure service interface defining $($baseName.Replace('I', '').ToLower()) operations and contracts."
    }
    
    # Service descriptions
    if ($fileName -like "*Service.cs" -and $fileName -notlike "*DataService.cs") {
        if ($baseName -match "Connection") { return "Connection service managing database connectivity, connection pooling, and performance monitoring." }
        if ($baseName -match "File") { return "File service providing document management, storage operations, and file system integration." }
        if ($baseName -match "Messenger") { return "Messenger service providing internal communication, notifications, and event coordination." }
        return "Infrastructure service providing $($baseName.Replace('Service', '').ToLower()) functionality with external system integration."
    }
    
    # Common/Utility descriptions
    if ($relativePath -match "\\Common\\" -or $fileName -like "Base*.cs" -or $fileName -like "*Extensions.cs") {
        if ($baseName -match "BaseRepository") { return "Base repository class providing common data access patterns with connection management and stored procedure execution." }
        if ($baseName -match ".*Extensions") { return "Extension methods providing enhanced functionality for $($baseName.Replace('Extensions', '').ToLower()) operations and data manipulation." }
        if ($baseName -match "DataAccess") { return "Data access utility providing database operation helpers, connection management, and query execution support." }
        if ($baseName -match "Mappers") { return "Entity mapping utilities providing conversion between domain entities and database models with validation." }
        if ($baseName -match "FieldNames") { return "Field name constants providing consistent database column references and query parameter naming." }
        if ($baseName -match "ParameterNames") { return "Parameter name constants providing standardized stored procedure parameter naming and validation." }
        if ($baseName -match "StoredProcs") { return "Stored procedure name constants providing centralized database procedure reference management with lazy initialization." }
        if ($baseName -match ".*EventIds") { return "Event ID constants providing standardized logging event identification for infrastructure operations." }
        if ($baseName -match ".*LogMessages") { return "Logging message utilities providing structured log formatting with high-performance LoggerMessage delegates." }
        return "Infrastructure utility providing $($baseName.ToLower()) functionality for data access and system operations."
    }
    
    # Default based on folder location
    if ($relativePath -match "\\Persistence\\") { return "Data persistence component providing database operations with stored procedure integration and entity mapping." }
    if ($relativePath -match "\\Services\\") { return "Infrastructure service providing technical functionality and external system integration support." }
    if ($relativePath -match "\\Configuration\\") { return "Infrastructure configuration component providing system setup, service registration, and dependency management." }
    if ($relativePath -match "\\Security\\") { return "Security component providing authentication, authorization, and access control functionality." }
    
    return "Infrastructure component providing data access and external system integration functionality."
}

# Function to determine template type
function Get-TemplateType {
    param([string]$fileName, [string]$relativePath)
    
    # Determine by folder location first
    if ($relativePath -match "\\Persistence\\") { return "Repository" }
    if ($relativePath -match "\\Configuration\\") { return "Configuration" }
    if ($relativePath -match "\\Security\\") { return "Security" }
    if ($relativePath -match "\\Common\\") { return "Common" }
    if ($relativePath -match "\\Interfaces\\") { return "Interface" }
    if ($relativePath -match "\\Services\\") { return "Service" }
    
    # Check specific patterns
    if ($fileName -like "I*.cs") { return "Interface" }
    if ($fileName -like "*Repository.cs") { return "Repository" }
    if ($fileName -like "*DataService.cs") { return "DataService" }
    if ($fileName -like "*Service.cs") { return "Service" }
    if ($fileName -like "*Middleware.cs") { return "Middleware" }
    if ($fileName -like "*Filter.cs") { return "Security" }
    if ($fileName -like "*Authentication*.cs") { return "Security" }
    if ($fileName -like "*Authorization*.cs") { return "Security" }
    if ($fileName -like "Base*.cs") { return "Common" }
    if ($fileName -like "*Extensions.cs") { return "Common" }
    if ($fileName -like "DependencyInjection.cs") { return "Configuration" }
    if ($fileName -like "ServiceCollectionExtensions.cs") { return "Configuration" }
    
    return "Common"  # Default for infrastructure layer
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
Write-Host "SMS Infrastructure Layer Header Management Script - COMPREHENSIVE VERSION" -ForegroundColor Magenta
Write-Host "=======================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files will be modified" -ForegroundColor Yellow
}

# Get ALL C# files, no exceptions
$processedCount = 0
$skippedCount = 0

Write-Host "`nProcessing ALL C# files in Infrastructure project..." -ForegroundColor Cyan
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
Write-Host "`n=======================================================================" -ForegroundColor Magenta
Write-Host "COMPREHENSIVE SUMMARY:" -ForegroundColor Magenta
Write-Host "Files processed: $processedCount" -ForegroundColor Green
Write-Host "Files skipped: $skippedCount" -ForegroundColor Yellow
Write-Host "Total files checked: $($allFiles.Count)" -ForegroundColor White
Write-Host "=======================================================================" -ForegroundColor Magenta

if ($WhatIf) {
    Write-Host "`nTo apply changes, run without -WhatIf parameter" -ForegroundColor Yellow
}