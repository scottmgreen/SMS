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
public sealed record CreateSMSStakeholderUserCommand(
    string Code,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string StakeholderType,
    string Organization,
    string AccessLevel,
    string CreatedBy
) : IRequest<Result<SMSStakeholderUser>>;

#endregion

#region Update Commands

/// <summary>
/// Command to update SMS Stakeholder User basic information
/// </summary>
public sealed record UpdateSMSStakeholderUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Stakeholder User stakeholder information
/// </summary>
public sealed record UpdateSMSStakeholderUserInfoCommand(
    string UserId,
    string StakeholderType,
    string Organization,
    string AccessLevel,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Stakeholder User password
/// </summary>
public sealed record UpdateSMSStakeholderUserPasswordCommand(
    string UserId,
    string NewPassword,
    bool RequiresChange,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to activate SMS Stakeholder User
/// </summary>
public sealed record ActivateSMSStakeholderUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to deactivate SMS Stakeholder User
/// </summary>
public sealed record DeactivateSMSStakeholderUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

#endregion

#region Authentication Commands

/// <summary>
/// Command to authenticate SMS Stakeholder User
/// </summary>
public sealed record AuthenticateSMSStakeholderUserCommand(
    string UserName,
    string Password
) : IRequest<Result<SMSStakeholderUser>>;

/// <summary>
/// Command to record SMS Stakeholder User login
/// </summary>
public sealed record RecordSMSStakeholderUserLoginCommand(
    string UserId,
    DateTime LoginDate
) : IRequest<Result<bool>>;

#endregion

#region Delete Commands

/// <summary>
/// Command to delete SMS Stakeholder User (soft delete)
/// </summary>
public sealed record DeleteSMSStakeholderUserCommand(
    string UserId,
    string DeletedBy
) : IRequest<Result<bool>>;

#endregion