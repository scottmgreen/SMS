using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS Organizational User management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS Organizational User
/// </summary>
public sealed record CreateSMSOrganizationalUserCommand(
    string Code,
    string FirstName,
    string LastName,
    string UserName,
    string Password,
    string Department,
    string Position,
    string OrganizationLevel,
    string CreatedBy
) : IRequest<Result<SMSOrganizationalUser>>;

#endregion

#region Update Commands

/// <summary>
/// Command to update SMS Organizational User basic information
/// </summary>
public sealed record UpdateSMSOrganizationalUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Organizational User organizational information
/// </summary>
public sealed record UpdateSMSOrganizationalUserInfoCommand(
    string UserId,
    string Department,
    string Position,
    string OrganizationLevel,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to update SMS Organizational User password
/// </summary>
public sealed record UpdateSMSOrganizationalUserPasswordCommand(
    string UserId,
    string NewPassword,
    bool RequiresChange,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to activate SMS Organizational User
/// </summary>
public sealed record ActivateSMSOrganizationalUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

/// <summary>
/// Command to deactivate SMS Organizational User
/// </summary>
public sealed record DeactivateSMSOrganizationalUserCommand(
    string UserId,
    string UpdatedBy
) : IRequest<Result<bool>>;

#endregion

#region Authentication Commands

/// <summary>
/// Command to authenticate SMS Organizational User
/// </summary>
public sealed record AuthenticateSMSOrganizationalUserCommand(
    string UserName,
    string Password
) : IRequest<Result<SMSOrganizationalUser>>;

/// <summary>
/// Command to record SMS Organizational User login
/// </summary>
public sealed record RecordSMSOrganizationalUserLoginCommand(
    string UserId,
    DateTime LoginDate
) : IRequest<Result<bool>>;

#endregion

#region Delete Commands

/// <summary>
/// Command to delete SMS Organizational User (soft delete)
/// </summary>
public sealed record DeleteSMSOrganizationalUserCommand(
    string UserId,
    string DeletedBy
) : IRequest<Result<bool>>;

#endregion