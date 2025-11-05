using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Queries for SMS Organizational User operations
/// </summary>

#region Get Queries

/// <summary>
/// Query to get all SMS Organizational Users
/// </summary>
public sealed record GetAllSMSOrganizationalUsersQuery() : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get SMS Organizational User by ID
/// </summary>
public sealed record GetSMSOrganizationalUserByIdQuery(string UserId) : IRequest<Result<SMSOrganizationalUser>>;

/// <summary>
/// Query to get SMS Organizational User by username
/// </summary>
public sealed record GetSMSOrganizationalUserByUserNameQuery(string UserName) : IRequest<Result<SMSOrganizationalUser>>;

/// <summary>
/// Query to get all active SMS Organizational Users
/// </summary>
public sealed record GetActiveSMSOrganizationalUsersQuery() : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get SMS Organizational Users by department
/// </summary>
public sealed record GetSMSOrganizationalUsersByDepartmentQuery(string Department) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get SMS Organizational Users by position
/// </summary>
public sealed record GetSMSOrganizationalUsersByPositionQuery(string Position) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get SMS Organizational Users by organization level
/// </summary>
public sealed record GetSMSOrganizationalUsersByLevelQuery(string OrganizationLevel) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get SMS Organizational Users at or above a specific level
/// </summary>
public sealed record GetSMSOrganizationalUsersAtOrAboveLevelQuery(string MinimumLevel) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get department supervisors
/// </summary>
public sealed record GetDepartmentSupervisorsQuery(string Department) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

#endregion

#region Validation Queries

/// <summary>
/// Query to check if username exists
/// </summary>
public sealed record CheckSMSOrganizationalUserNameExistsQuery(string UserName) : IRequest<Result<bool>>;

/// <summary>
/// Query to validate user credentials
/// </summary>
public sealed record ValidateSMSOrganizationalUserCredentialsQuery(string UserName, string Password) : IRequest<Result<bool>>;

#endregion

#region Statistics Queries

/// <summary>
/// Query to get SMS Organizational User statistics
/// </summary>
public sealed record GetSMSOrganizationalUserStatisticsQuery() : IRequest<Result<UserStatistics>>;

/// <summary>
/// Query to get department statistics
/// </summary>
public sealed record GetDepartmentStatisticsQuery() : IRequest<Result<Dictionary<string, int>>>;

/// <summary>
/// Query to get users requiring password change
/// </summary>
public sealed record GetSMSOrganizationalUsersRequiringPasswordChangeQuery() : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

/// <summary>
/// Query to get stale users (haven't logged in recently)
/// </summary>
public sealed record GetStaleSMSOrganizationalUsersQuery(int StaleDays = 90) : IRequest<Result<IEnumerable<SMSOrganizationalUser>>>;

#endregion