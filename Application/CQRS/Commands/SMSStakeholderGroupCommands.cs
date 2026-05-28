//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Commands;

/// <summary>
/// Command to create a new SMS stakeholder group
/// </summary>
public class CreateSMSStakeholderGroupCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderGroup>>, ICreateCommand
{
    /// <summary>
    /// The SMS Stakeholder Group entity to create
    /// </summary>
    public SMSStakeholderGroup StakeholderGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSStakeholderGroupCommand class.
    /// </summary>
    /// <param name="stakeholderGroup">The stakeholder group to create</param>
    /// <exception cref="ArgumentNullException">Thrown when stakeholderGroup is null</exception>
    public CreateSMSStakeholderGroupCommand(SMSStakeholderGroup stakeholderGroup)
    {
        StakeholderGroup = stakeholderGroup ?? throw new ArgumentNullException(nameof(stakeholderGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        StakeholderGroup.CreatedBy = userId;
        StakeholderGroup.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Command to update an existing SMS stakeholder group
/// </summary>
public class UpdateSMSStakeholderGroupCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderGroup>>, IUpdateCommand
{
    /// <summary>
    /// The SMS Stakeholder Group entity to update
    /// </summary>
    public SMSStakeholderGroup StakeholderGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSStakeholderGroupCommand class.
    /// </summary>
    /// <param name="stakeholderGroup">The stakeholder group to update</param>
    /// <exception cref="ArgumentNullException">Thrown when stakeholderGroup is null</exception>
    public UpdateSMSStakeholderGroupCommand(SMSStakeholderGroup stakeholderGroup)
    {
        StakeholderGroup = stakeholderGroup ?? throw new ArgumentNullException(nameof(stakeholderGroup));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        StakeholderGroup.UpdatedBy = userId;
        StakeholderGroup.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to delete an SMS stakeholder group
/// </summary>
public class DeleteSMSStakeholderGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The code of the SMS Stakeholder Group to delete
    /// </summary>
    public SMSStakeholderGroup StakeholderGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSStakeholderGroupCommand class.
    /// </summary>
    /// <param name="group">The stakeholder group to delete</param>
    /// <exception cref="ArgumentException">Thrown when group is null or has empty code</exception>
    public DeleteSMSStakeholderGroupCommand(SMSStakeholderGroup group)
    {
        if (string.IsNullOrWhiteSpace(group?.Code))
            throw new ArgumentException("Group code cannot be null or empty", nameof(group));

        StakeholderGroup = group;
    }
}

/// <summary>
/// Command to assign a user to a stakeholder group
/// </summary>
public class AssignUserToStakeholderGroupCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The user code to assign to the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group code to assign the user to
    /// </summary>
    public SMSStakeholderGroupID StakeholderGroupID { get; set; }

    /// <summary>
    /// The user who is making the assignment
    /// </summary>
    public string AssignedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the AssignUserToStakeholderGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to assign</param>
    /// <param name="groupID">The group ID to assign to</param>
    /// <exception cref="ArgumentException">Thrown when userCode or groupID is invalid</exception>
    public AssignUserToStakeholderGroupCommand(string userCode, SMSStakeholderGroupID groupID)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));
        if (string.IsNullOrWhiteSpace(groupID?.Value))
            throw new ArgumentException("Group ID cannot be null or empty", nameof(groupID));

        UserCode = userCode;
        StakeholderGroupID = groupID;
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
/// Command to remove a user from a stakeholder group
/// </summary>
public class RemoveUserFromStakeholderGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to remove from the group
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// The group code to remove the user from
    /// </summary>
    public SMSStakeholderGroupID StakeholderGroupID { get; set; }

    /// <summary>
    /// Initializes a new instance of the RemoveUserFromStakeholderGroupCommand class.
    /// </summary>
    /// <param name="userCode">The user code to remove</param>
    /// <param name="groupID">The group ID to remove from</param>
    /// <exception cref="ArgumentException">Thrown when userCode or groupID is invalid</exception>
    public RemoveUserFromStakeholderGroupCommand(string userCode, SMSStakeholderGroupID groupID)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));
        if (string.IsNullOrWhiteSpace(groupID?.Value))
            throw new ArgumentException("Group ID cannot be null or empty", nameof(groupID));

        UserCode = userCode;
        StakeholderGroupID = groupID;
    }
}

/// <summary>
/// Command to clear all group memberships for a user
/// </summary>
public class ClearUserStakeholderGroupsCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user code to clear group memberships for
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the ClearUserStakeholderGroupsCommand class.
    /// </summary>
    /// <param name="userCode">The user code to clear group memberships for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public ClearUserStakeholderGroupsCommand(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}
