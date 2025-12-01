using SMS_Domain.Entities;

using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a new SMS stakeholder group
/// </summary>
public class CreateSMSApplicationGroupCommand : BaseCommandBundle, IRequest<Result<SMSApplicationGroup>>
{
    /// <summary>
    /// The SMS Application Group entity to create
    /// </summary>
    public SMSApplicationGroup ApplicationGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSApplicationGroupCommand class.
    /// </summary>
    /// <param name="stakeholderGroup">The stakeholder group to create</param>
    /// <exception cref="ArgumentNullException">Thrown when stakeholderGroup is null</exception>
    public CreateSMSApplicationGroupCommand(SMSApplicationGroup stakeholderGroup)
    {
        ApplicationGroup = stakeholderGroup ?? throw new ArgumentNullException(nameof(stakeholderGroup));
    }
}

/// <summary>
/// Command to update an existing SMS stakeholder group
/// </summary>
public class UpdateSMSApplicationGroupCommand : BaseCommandBundle, IRequest<Result<SMSApplicationGroup>>
{
    /// <summary>
    /// The SMS Application Group entity to update
    /// </summary>
    public SMSApplicationGroup ApplicationGroup { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSApplicationGroupCommand class.
    /// </summary>
    /// <param name="applicationGroup">The stakeholder group to update</param>
    /// <exception cref="ArgumentNullException">Thrown when applicationGroup is null</exception>
    public UpdateSMSApplicationGroupCommand(SMSApplicationGroup applicationGroup)
    {
        ApplicationGroup = applicationGroup ?? throw new ArgumentNullException(nameof(applicationGroup));
    }
}

/// <summary>
/// Command to delete an SMS stakeholder group
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
    /// <param name="groupCode">The code of the stakeholder group to delete</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public DeleteSMSApplicationGroupCommand(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }
}

/// <summary>
/// Command to assign a user to a stakeholder group
/// </summary>
public class AssignUserToApplicationGroupCommand : BaseCommandBundle, IRequest<Result<bool>>
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
    /// <param name="assignedBy">The user making the assignment</param>
    /// <exception cref="ArgumentException">Thrown when userCode or groupCode is null or empty</exception>
    public AssignUserToApplicationGroupCommand(string userCode, string groupCode, string assignedBy = "SYSTEM")
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        UserCode = userCode;
        GroupCode = groupCode;
        AssignedBy = assignedBy ?? "SYSTEM";
    }
}

/// <summary>
/// Command to remove a user from a stakeholder group
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