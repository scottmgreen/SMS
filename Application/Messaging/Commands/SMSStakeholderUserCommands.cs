using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Stakeholder User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Stakeholder User
/// </summary>
public class CreateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderUser>>
{
    /// <summary>
    /// The SMS Stakeholder User entity to create
    /// </summary>
    public SMSStakeholderUser SMSStakeholderUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUser">The SMS stakeholder user to create</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUser is null</exception>
    public CreateSMSStakeholderUserCommand(SMSStakeholderUser smsStakeholderUser)
    {
        SMSStakeholderUser = smsStakeholderUser ?? throw new ArgumentNullException(nameof(smsStakeholderUser));
    }
}

#endregion

#region Update Commands

/// <summary>
/// Command to update an existing SMS Stakeholder User
/// </summary>
public class UpdateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<SMSStakeholderUser>>
{
    /// <summary>
    /// The SMS Stakeholder User entity to update
    /// </summary>
    public SMSStakeholderUser SMSStakeholderUser { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUser">The SMS stakeholder user to update</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUser is null</exception>
    public UpdateSMSStakeholderUserCommand(SMSStakeholderUser smsStakeholderUser)
    {
        SMSStakeholderUser = smsStakeholderUser ?? throw new ArgumentNullException(nameof(smsStakeholderUser));
    }
}

/// <summary>
/// Command to update SMS Stakeholder User password
/// </summary>
public class UpdateSMSStakeholderUserPasswordCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The new password
    /// </summary>
    public string NewPassword { get; set; }

    /// <summary>
    /// User who is updating the password
    /// </summary>
    public string UpdatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSStakeholderUserPasswordCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="newPassword">The new password</param>
    /// <param name="updatedBy">User updating the password</param>
    /// <exception cref="ArgumentException">Thrown when userId, newPassword, or updatedBy is null or empty</exception>
    public UpdateSMSStakeholderUserPasswordCommand(string userId, string newPassword, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));
        if (string.IsNullOrWhiteSpace(updatedBy)) throw new ArgumentException("Updated by cannot be null or empty", nameof(updatedBy));

        UserId = userId;
        NewPassword = newPassword;
        UpdatedBy = updatedBy;
    }
}

#endregion

#region Authentication Commands

/// <summary>
/// Command to authenticate SMS Stakeholder User
/// </summary>
public class AuthenticateSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// The password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Initializes a new instance of the AuthenticateSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="password">The password</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public AuthenticateSMSStakeholderUserCommand(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("Username cannot be null or empty", nameof(userName));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

/// <summary>
/// Command to record SMS Stakeholder User login
/// </summary>
public class RecordSMSStakeholderUserLoginCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The user ID
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The login date
    /// </summary>
    public DateTime LoginDate { get; set; }

    /// <summary>
    /// Initializes a new instance of the RecordSMSStakeholderUserLoginCommand class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="loginDate">The login date</param>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
    public RecordSMSStakeholderUserLoginCommand(string userId, DateTime loginDate)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        UserId = userId;
        LoginDate = loginDate;
    }
}

#endregion

#region Delete Commands

/// <summary>
/// Command to delete an SMS Stakeholder User by ID
/// </summary>
public class DeleteSMSStakeholderUserCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the SMS Stakeholder User to delete
    /// </summary>
    public SMSStakeholderUserID SMSStakeholderUserId { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSStakeholderUserCommand class.
    /// </summary>
    /// <param name="smsStakeholderUserId">The ID of the SMS stakeholder user to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when smsStakeholderUserId is null</exception>
    public DeleteSMSStakeholderUserCommand(SMSStakeholderUserID smsStakeholderUserId)
    {
        SMSStakeholderUserId = smsStakeholderUserId ?? throw new ArgumentNullException(nameof(smsStakeholderUserId));
    }
}

#endregion