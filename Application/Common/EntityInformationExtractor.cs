//-----------------------------------------------------------------------
// <copyright file="EntityInformationExtractor.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Shared utility for extracting entity information from commands and queries.
//                  Eliminates code duplication across multiple pipelines.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Common;

/// <summary>
/// Shared utility for extracting entity information from CQRS commands and queries
/// Eliminates code duplication across audit pipelines and provides consistent entity identification
/// </summary>
public static class EntityInformationExtractor
{
    /// <summary>
    /// Extract resource identifier from command or query for audit logging
    /// Uses enhanced audit interfaces when available, otherwise falls back to reflection
    /// </summary>
    public static string GetResourceIdentifier<TRequest>(TRequest request) where TRequest : IRequest
    {
        try
        {
            // Check for enhanced audit command interface first (highest priority)
            if (request is IEnhancedAuditCommand enhancedCommand)
            {
                return enhancedCommand.GetResourceIdentifier();
            }

            // Check for read query interface
            if (request is IReadQuery readQuery)
            {
                return readQuery.GetResourceIdentifier();
            }

            // Fallback to reflection-based extraction
            return ExtractResourceIdentifierFromProperties(request);
        }
        catch (Exception)
        {
            return request.GetType().Name;
        }
    }

    /// <summary>
    /// Get action type from command or query for audit logging
    /// </summary>
    public static string GetActionType<TRequest>(TRequest request) where TRequest : IRequest
    {
        try
        {
            // Check for enhanced audit command interface first
            if (request is IEnhancedAuditCommand enhancedCommand)
            {
                return enhancedCommand.GetActionType();
            }

            // Check for read query interface
            if (request is IReadQuery readQuery)
            {
                return readQuery.GetAccessType();
            }

            // Determine action type based on command interfaces
            return request switch
            {
                ICreateCommand => "CREATE",
                IUpdateCommand => "UPDATE",
                IDeleteCommand => "DELETE",
                _ => ExtractActionFromTypeName(request.GetType().Name)
            };
        }
        catch (Exception)
        {
            return "UNKNOWN";
        }
    }

    /// <summary>
    /// Extract entity information for logging context (limited properties to avoid log bloat)
    /// </summary>
    public static string GetEntityInfo<TRequest>(TRequest request) where TRequest : IRequest
    {
        try
        {
            var properties = request.GetType().GetProperties();
            var keyProps = properties.Where(p => 
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || 
                p.Name.EndsWith("Code", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (keyProps.Any())
            {
                var values = keyProps.Take(3) // Limit to first 3 to avoid log bloat
                    .Select(p => $"{p.Name}={p.GetValue(request)}")
                    .Where(v => !string.IsNullOrEmpty(v));
                return string.Join(", ", values);
            }

            return request.GetType().Name.Replace("Command", "").Replace("Query", "");
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Determine if request is an auditable command
    /// </summary>
    public static bool IsAuditableCommand<TRequest>(TRequest request) where TRequest : IRequest
    {
        return request is ICreateCommand or IUpdateCommand or IDeleteCommand or IAuditableCommand;
    }

    /// <summary>
    /// Determine if request is a read query
    /// </summary>
    public static bool IsReadQuery<TRequest>(TRequest request) where TRequest : IRequest
    {
        return request is IReadQuery;
    }

    /// <summary>
    /// Determine if command is business critical for enhanced logging
    /// </summary>
    public static bool IsBusinessCriticalCommand(string commandType)
    {
        var criticalPatterns = new[]
        {
            "Delete", "Create", "UpdateStatus", "Validate", "Authenticate", 
            "Deactivate", "Reset", "Assign", "Report", "Assessment"
        };

        return criticalPatterns.Any(pattern => 
            commandType.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    #region Private Helper Methods

    /// <summary>
    /// Extract resource identifier using reflection on command properties
    /// </summary>
    private static string ExtractResourceIdentifierFromProperties<TRequest>(TRequest request) where TRequest : IRequest
    {
        try
        {
            var requestType = request.GetType().Name;
            var properties = request.GetType().GetProperties();
            
            // Try to get entity information from command properties
            foreach (var prop in properties)
            {
                var value = prop.GetValue(request);
                if (value == null) continue;

                // Handle entity objects with Code property
                var codeProperty = value.GetType().GetProperty("Code");
                if (codeProperty?.GetValue(value) is string code && !string.IsNullOrEmpty(code))
                {
                    var entityType = value.GetType().Name;
                    return $"{entityType}:Code:{code}";
                }

                // Handle ID properties
                if (prop.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && value is not null)
                {
                    var idValue = value.ToString();
                    if (!string.IsNullOrEmpty(idValue))
                    {
                        var entityType = prop.Name.Replace("Id", "");
                        return $"{entityType}:Id:{idValue}";
                    }
                }

                // Handle direct Code properties
                if (prop.Name.Equals("Code", StringComparison.OrdinalIgnoreCase) && value is string directCode)
                {
                    return $"Entity:Code:{directCode}";
                }

                // Handle UserName for authentication commands
                if (prop.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase) && value is string userName)
                {
                    return $"User:UserName:{userName}";
                }
            }

            // Fallback: extract from command type name
            return ExtractEntityNameFromCommandType(requestType);
        }
        catch
        {
            return "Unknown:Resource";
        }
    }

    /// <summary>
    /// Extract action type from command type name
    /// </summary>
    private static string ExtractActionFromTypeName(string typeName)
    {
        // Remove "Command" or "Query" suffix
        var cleanName = typeName.Replace("Command", "").Replace("Query", "");
        
        // Handle common patterns
        if (cleanName.StartsWith("Create")) return "CREATE";
        if (cleanName.StartsWith("Update")) return "UPDATE";
        if (cleanName.StartsWith("Delete")) return "DELETE";
        if (cleanName.StartsWith("Get") || cleanName.StartsWith("Find") || cleanName.StartsWith("Search")) return "READ";
        
        return cleanName.ToUpperInvariant();
    }

    /// <summary>
    /// Extract entity name from command type for fallback resource identification
    /// </summary>
    private static string ExtractEntityNameFromCommandType(string commandType)
    {
        var withoutCommand = commandType.Replace("Command", "").Replace("Query", "");
        
        if (withoutCommand.StartsWith("Create"))
            return withoutCommand.Substring(6);
        else if (withoutCommand.StartsWith("Update"))
            return withoutCommand.Substring(6);
        else if (withoutCommand.StartsWith("Delete"))
            return withoutCommand.Substring(6);
        else if (withoutCommand.StartsWith("Get"))
            return withoutCommand.Substring(3);
        
        return withoutCommand;
    }

    #endregion
}