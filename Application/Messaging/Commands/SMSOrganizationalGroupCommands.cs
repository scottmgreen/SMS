//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Entities;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a new SMS organizational group
/// </summary>
public class CreateSMSOrganizationalGroupCommand : BaseCommandBundle, IRequest<Result<SMSOrganizationalGroup>>, ICreateCommand
{
    /// <summary>
    /// The SMS Organizational Group entity to create
    /// </summary>
    public SMSOrganizationalGroup OrganizationalGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSOrganizationalGroupCommand class.
    /// </summary>
    /// <param name="organizationalGroup">The organizational group to create</param>
    /// <exception cref="ArgumentNullException">Thrown when organizationalGroup is null</exception>
    public CreateSMSOrganizationalGroupCommand(SMSOrganizationalGroup organizationalGroup)
    {
        OrganizationalGroup = organizationalGroup ?? throw new ArgumentNullException(nameof(organizationalGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        OrganizationalGroup.CreatedBy = userId;
        OrganizationalGroup.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Command to update an existing SMS organizational group
/// </summary>
public class UpdateSMSOrganizationalGroupCommand : BaseCommandBundle, IRequest<Result<SMSOrganizationalGroup>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Organizational Group entity to update
    /// </summary>
    public SMSOrganizationalGroup OrganizationalGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSOrganizationalGroupCommand class.
    /// </summary>
    /// <param name="organizationalGroup">The organizational group to update</param>
    /// <exception cref="ArgumentNullException">Thrown when organizationalGroup is null</exception>
    public UpdateSMSOrganizationalGroupCommand(SMSOrganizationalGroup organizationalGroup)
    {
        OrganizationalGroup = organizationalGroup ?? throw new ArgumentNullException(nameof(organizationalGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        OrganizationalGroup.UpdatedBy = userId;
        OrganizationalGroup.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to delete an SMS organizational group
/// </summary>
public class DeleteSMSOrganizationalGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The SMS Organizational Group to delete
    /// </summary>
    public SMSOrganizationalGroup OrganizationalGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSOrganizationalGroupCommand class.
    /// </summary>
    /// <param name="organizationalGroup">The organizational group to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when organizationalGroup is null</exception>
    public DeleteSMSOrganizationalGroupCommand(SMSOrganizationalGroup organizationalGroup)
    {
        OrganizationalGroup = organizationalGroup ?? throw new ArgumentNullException(nameof(organizationalGroup));
    }
}

/// <summary>
/// Command to assign a user to an organizational group
/// </summary>
public class AssignUserToOrganizationalGroupCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The user code to assign to the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group ID to assign the user to
    /// </summary>
    public SMSOrganizationalGroupID GroupId { get; set; }

    /// <summary>
    /// The user who is making the assignment
    /// </summary>
    public string AssignedBy { get; set; } = "SYSTEM";

    /// <summary>
    /// Initializes a new instance of the AssignUserToOrganizationalGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to assign</param>
    /// <param name="groupId">The group ID to assign to</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when groupId is null</exception>
    public AssignUserToOrganizationalGroupCommand(string userCode, SMSOrganizationalGroupID groupId)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
        GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
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
/// Command to remove a user from an organizational group
/// </summary>
public class RemoveUserFromOrganizationalGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to remove from the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group ID to remove the user from
    /// </summary>
    public SMSOrganizationalGroupID GroupId { get; set; }

    /// <summary>
    /// Initializes a new instance of the RemoveUserFromOrganizationalGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to remove</param>
    /// <param name="groupId">The group ID to remove from</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when groupId is null</exception>
    public RemoveUserFromOrganizationalGroupCommand(string userCode, SMSOrganizationalGroupID groupId)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
        GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
    }
}

/// <summary>
/// Command to clear all group memberships for a user
/// </summary>
public class ClearUserOrganizationalGroupsCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to clear group memberships for
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the ClearUserOrganizationalGroupsCommand class.
    /// </summary>
    /// <param name="userCode">The user code to clear group memberships for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public ClearUserOrganizationalGroupsCommand(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}
