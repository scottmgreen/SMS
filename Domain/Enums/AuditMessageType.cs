//-----------------------------------------------------------------------
// <copyright file="AuditMessageType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain Smart Enum for audit message types providing type safety and consistency
//                  Domain concept defining audit categorization across the entire CQRS system
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Domain Smart Enum for audit message types providing type safety and consistency
/// Replaces magic strings with strongly-typed values for better maintainability
/// </summary>
public sealed class AuditMessageType : IEquatable<AuditMessageType>
{
    #region Static Instances

    /// <summary>
    /// CRUD Read operations (Queries) - Data access and retrieval
    /// </summary>
    public static readonly AuditMessageType CrudRead = new("CRUD_READ", "Data read/query operations");

    /// <summary>
    /// CRUD Create operations (Commands) - Creating new entities
    /// </summary>
    public static readonly AuditMessageType CrudCreate = new("CRUD_CREATE", "Entity creation operations");

    /// <summary>
    /// CRUD Update operations (Commands) - Modifying existing entities
    /// </summary>
    public static readonly AuditMessageType CrudUpdate = new("CRUD_UPDATE", "Entity modification operations");

    /// <summary>
    /// CRUD Delete operations (Commands) - Removing entities
    /// </summary>
    public static readonly AuditMessageType CrudDelete = new("CRUD_DELETE", "Entity deletion operations");

    /// <summary>
    /// Complex business actions that don't fit standard CRUD patterns
    /// </summary>
    public static readonly AuditMessageType CrudAction = new("CRUD_ACTION", "Complex business actions");

    /// <summary>
    /// Authentication related operations (login, logout, validation)
    /// </summary>
    public static readonly AuditMessageType Authentication = new("AUTHENTICATION", "User authentication operations");

    /// <summary>
    /// Authorization and permission related operations
    /// </summary>
    public static readonly AuditMessageType Authorization = new("AUTHORIZATION", "User authorization operations");

    /// <summary>
    /// System configuration and administrative operations
    /// </summary>
    public static readonly AuditMessageType SystemAdmin = new("SYSTEM_ADMIN", "System administration operations");

    /// <summary>
    /// Data validation and business rule enforcement operations  
    /// </summary>
    public static readonly AuditMessageType Validation = new("VALIDATION", "Data validation operations");

    #endregion

    #region Properties

    /// <summary>
    /// The string value used in the database and logs
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Human-readable description of the message type
    /// </summary>
    public string Description { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Private constructor to prevent external instantiation
    /// </summary>
    private AuditMessageType(string value, string description)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Get all available message types
    /// </summary>
    public static IEnumerable<AuditMessageType> GetAll()
    {
        yield return CrudRead;
        yield return CrudCreate;
        yield return CrudUpdate;
        yield return CrudDelete;
        yield return CrudAction;
        yield return Authentication;
        yield return Authorization;
        yield return SystemAdmin;
        yield return Validation;
    }

    /// <summary>
    /// Parse string value back to AuditMessageType
    /// </summary>
    public static AuditMessageType FromValue(string value)
    {
        return GetAll().FirstOrDefault(x => x.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"Unknown audit message type: {value}", nameof(value));
    }

    /// <summary>
    /// Try to parse string value back to AuditMessageType
    /// </summary>
    public static bool TryFromValue(string value, out AuditMessageType messageType)
    {
        messageType = GetAll().FirstOrDefault(x => x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return messageType != null;
    }

    /// <summary>
    /// Automatically determine message type from command type for consistent categorization
    /// </summary>
    public static AuditMessageType DetermineFromCommandType(string commandType)
    {
        var withoutCommand = commandType.Replace("Command", "");
        
        // Authentication operations
        if (withoutCommand.Contains("Authentication", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.Contains("Logout", StringComparison.OrdinalIgnoreCase))
        {
            return Authentication;
        }

        // Authorization operations
        if (withoutCommand.Contains("Authorization", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.Contains("Permission", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.Contains("Role", StringComparison.OrdinalIgnoreCase))
        {
            return Authorization;
        }

        // Validation operations
        if (withoutCommand.StartsWith("Validate", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.Contains("Validation", StringComparison.OrdinalIgnoreCase))
        {
            return Validation;
        }

        // Create operations
        if (withoutCommand.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Add", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Record", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Register", StringComparison.OrdinalIgnoreCase))
        {
            return CrudCreate;
        }

        // Update operations
        if (withoutCommand.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Modify", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Reset", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Assign", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Change", StringComparison.OrdinalIgnoreCase))
        {
            return CrudUpdate;
        }

        // Delete operations
        if (withoutCommand.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Remove", StringComparison.OrdinalIgnoreCase) ||
            withoutCommand.StartsWith("Deactivate", StringComparison.OrdinalIgnoreCase))
        {
            return CrudDelete;
        }

        // Fallback for complex operations
        return CrudAction;
    }

    #endregion

    #region Equality and Operators

    public bool Equals(AuditMessageType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is AuditMessageType other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(AuditMessageType left, AuditMessageType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AuditMessageType left, AuditMessageType right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Implicit conversion to string for database storage
    /// </summary>
    public static implicit operator string(AuditMessageType messageType)
    {
        return messageType?.Value ?? string.Empty;
    }

    #endregion

    #region String Representation

    public override string ToString() => Value;

    /// <summary>
    /// Returns a detailed string representation including description
    /// </summary>
    public string ToDetailedString() => $"{Value} - {Description}";

    #endregion
}