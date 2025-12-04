namespace SMS_Application.Interfaces;

/// <summary>
/// Marker interface for commands that contain entities with audit fields
/// </summary>
public interface IAuditableCommand
{
}

/// <summary>
/// Interface for commands that can have audit fields automatically set
/// </summary>
public interface IHasAuditFields : IAuditableCommand
{
    /// <summary>
    /// Set audit fields for creation
    /// </summary>
    /// <param name="userId">The user performing the action</param>
    /// <param name="timestamp">The timestamp of the action</param>
    void SetCreatedBy(string userId, DateTime timestamp);
    
    /// <summary>
    /// Set audit fields for updates
    /// </summary>
    /// <param name="userId">The user performing the action</param>
    /// <param name="timestamp">The timestamp of the action</param>
    void SetUpdatedBy(string userId, DateTime timestamp);
}

/// <summary>
/// Interface for commands that create new entities
/// </summary>
public interface ICreateCommand : IHasAuditFields
{
}

/// <summary>
/// Interface for commands that update existing entities
/// </summary>
public interface IUpdateCommand : IHasAuditFields
{
}