using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Queries for SMS Application User operations
/// </summary>

#region Get Queries

/// <summary>
/// Query to get all SMS Application Users
/// </summary>
public sealed record GetAllSMSApplicationUsersQuery() : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

/// <summary>
/// Query to get SMS Application User by ID
/// </summary>
public sealed record GetSMSApplicationUserByIdQuery(string UserId) : IRequest<Result<SMSApplicationUser>>;

/// <summary>
/// Query to get SMS Application User by username
/// </summary>
public sealed record GetSMSApplicationUserByUserNameQuery(string UserName) : IRequest<Result<SMSApplicationUser>>;

/// <summary>
/// Query to get all active SMS Application Users
/// </summary>
public sealed record GetActiveSMSApplicationUsersQuery() : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

/// <summary>
/// Query to get SMS Application Users by application role
/// </summary>
public sealed record GetSMSApplicationUsersByRoleQuery(string ApplicationRole) : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

/// <summary>
/// Query to get SMS Application Users by permission level
/// </summary>
public sealed record GetSMSApplicationUsersByPermissionLevelQuery(string PermissionLevel) : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

/// <summary>
/// Query to get SMS Application Users with minimum permission level
/// </summary>
public sealed record GetSMSApplicationUsersWithMinimumPermissionQuery(string MinimumPermissionLevel) : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

#endregion

#region Validation Queries

/// <summary>
/// Query to check if username exists
/// </summary>
public sealed record CheckSMSApplicationUserNameExistsQuery(string UserName) : IRequest<Result<bool>>;

/// <summary>
/// Query to validate user credentials
/// </summary>
public sealed record ValidateSMSApplicationUserCredentialsQuery(string UserName, string Password) : IRequest<Result<bool>>;

#endregion

#region Statistics Queries

/// <summary>
/// Query to get SMS Application User statistics
/// </summary>
public sealed record GetSMSApplicationUserStatisticsQuery() : IRequest<Result<UserStatistics>>;

/// <summary>
/// Query to get users requiring password change
/// </summary>
public sealed record GetSMSApplicationUsersRequiringPasswordChangeQuery() : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

/// <summary>
/// Query to get stale users (haven't logged in recently)
/// </summary>
public sealed record GetStaleSMSApplicationUsersQuery(int StaleDays = 90) : IRequest<Result<IEnumerable<SMSApplicationUser>>>;

#endregion