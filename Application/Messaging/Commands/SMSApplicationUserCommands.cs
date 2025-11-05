using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Application User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Application User
/// </summary>
public sealed record CreateSMSApplicationUserCommand(
    string Code,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string ApplicationRole,
    string PermissionLevel,
    string CreatedBy
) : IRequest<Result<SMSApplicationUser>>;

#endregion

#region Update Commands

/// <summary>
/// Command to update SMS Application User basic information
/// </summary>
public sealed record UpdateSMSApplicationUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Application User application-specific information
/// </summary>
public sealed record UpdateSMSApplicationUserInfoCommand(
    string UserId,
    string ApplicationRole,
    string PermissionLevel,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Application User password
/// </summary>
public sealed record UpdateSMSApplicationUserPasswordCommand(
    string UserId,
    string NewPassword,
    bool RequiresChange,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to activate SMS Application User
/// </summary>
public sealed record ActivateSMSApplicationUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to deactivate SMS Application User
/// </summary>
public sealed record DeactivateSMSApplicationUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

#endregion

#region Authentication Commands

/// <summary>
/// Command to authenticate SMS Application User
/// </summary>
public sealed record AuthenticateSMSApplicationUserCommand(
    string UserName,
    string Password
) : IRequest<Result<SMSApplicationUser>>;

/// <summary>
/// Command to record SMS Application User login
/// </summary>
public sealed record RecordSMSApplicationUserLoginCommand(
    string UserId,
    DateTime LoginDate
) : IRequest<Result<bool>>;

#endregion

#region Delete Commands

/// <summary>
/// Command to delete SMS Application User (soft delete)
/// </summary>
public sealed record DeleteSMSApplicationUserCommand(
    string UserId,
    string DeletedBy
) : IRequest<Result<bool>>;

#endregion