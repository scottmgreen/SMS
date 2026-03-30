//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a new SMS application group
/// </summary>
public class CreateSMSApplicationGroupCommand : BaseCommandBundle, IRequest<Result<SMSApplicationGroup>>, ICreateCommand
{
    /// <summary>
    /// The SMS Application Group entity to create
    /// </summary>
    public SMSApplicationGroup ApplicationGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSApplicationGroupCommand class.
    /// </summary>
    /// <param name="applicationGroup">The application group to create</param>
    /// <exception cref="ArgumentNullException">Thrown when applicationGroup is null</exception>
    public CreateSMSApplicationGroupCommand(SMSApplicationGroup applicationGroup)
    {
        ApplicationGroup = applicationGroup ?? throw new ArgumentNullException(nameof(applicationGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        ApplicationGroup.CreatedBy = userId;
        ApplicationGroup.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Command to update an existing SMS application group
/// </summary>
public class UpdateSMSApplicationGroupCommand : BaseCommandBundle, IRequest<Result<SMSApplicationGroup>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Application Group entity to update
    /// </summary>
    public SMSApplicationGroup ApplicationGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSApplicationGroupCommand class.
    /// </summary>
    /// <param name="applicationGroup">The application group to update</param>
    /// <exception cref="ArgumentNullException">Thrown when applicationGroup is null</exception>
    public UpdateSMSApplicationGroupCommand(SMSApplicationGroup applicationGroup)
    {
        ApplicationGroup = applicationGroup ?? throw new ArgumentNullException(nameof(applicationGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ApplicationGroup.UpdatedBy = userId;
        ApplicationGroup.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to delete an SMS application group
/// </summary>
public class DeleteSMSApplicationGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The code of the SMS Application Group to delete
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSApplicationGroupCommand class.
    /// </summary>
    /// <param name="groupCode">The code of the application group to delete</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public DeleteSMSApplicationGroupCommand(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }
}

/// <summary>
/// Command to assign a user to an application group
/// </summary>
public class AssignUserToApplicationGroupCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The user code to assign to the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group code to assign the user to
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// The user who is making the assignment
    /// </summary>
    public string AssignedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the AssignUserToApplicationGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to assign</param>
    /// <param name="groupCode">The group code to assign to</param>
    /// <exception cref="ArgumentException">Thrown when userCode or groupCode is null or empty</exception>
    public AssignUserToApplicationGroupCommand(string userCode, string groupCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        UserCode = userCode;
        GroupCode = groupCode;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        AssignedBy = userId;
    }
}

/// <summary>
/// Command to remove a user from an application group
/// </summary>
public class RemoveUserFromApplicationGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to remove from the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group code to remove the user from
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the RemoveUserFromApplicationGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to remove</param>
    /// <param name="groupCode">The group code to remove from</param>
    /// <exception cref="ArgumentException">Thrown when userCode or groupCode is null or empty</exception>
    public RemoveUserFromApplicationGroupCommand(string userCode, string groupCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        UserCode = userCode;
        GroupCode = groupCode;
    }
}

/// <summary>
/// Command to clear all group memberships for a user
/// </summary>
public class ClearUserApplicationGroupsCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to clear group memberships for
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the ClearUserApplicationGroupsCommand class.
    /// </summary>
    /// <param name="userCode">The user code to clear group memberships for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public ClearUserApplicationGroupsCommand(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}
