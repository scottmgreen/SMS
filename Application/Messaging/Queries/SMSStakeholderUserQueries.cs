using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Queries for SMS Stakeholder User operations
/// </summary>

#region Get Queries

/// <summary>
/// Query to get all SMS Stakeholder Users
/// </summary>
public sealed record GetAllSMSStakeholderUsersQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get SMS Stakeholder User by ID
/// </summary>
public sealed record GetSMSStakeholderUserByIdQuery(string UserId) : IRequest<Result<SMSStakeholderUser>>;

/// <summary>
/// Query to get SMS Stakeholder User by username
/// </summary>
public sealed record GetSMSStakeholderUserByUserNameQuery(string UserName) : IRequest<Result<SMSStakeholderUser>>;

/// <summary>
/// Query to get all active SMS Stakeholder Users
/// </summary>
public sealed record GetActiveSMSStakeholderUsersQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get SMS Stakeholder Users by stakeholder type
/// </summary>
public sealed record GetSMSStakeholderUsersByTypeQuery(string StakeholderType) : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get SMS Stakeholder Users by organization
/// </summary>
public sealed record GetSMSStakeholderUsersByOrganizationQuery(string Organization) : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get SMS Stakeholder Users by access level
/// </summary>
public sealed record GetSMSStakeholderUsersByAccessLevelQuery(string AccessLevel) : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get SMS Stakeholder Users with minimum access level
/// </summary>
public sealed record GetSMSStakeholderUsersWithMinimumAccessQuery(string MinimumAccessLevel) : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get airline stakeholders
/// </summary>
public sealed record GetAirlineStakeholdersQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get ground handler stakeholders
/// </summary>
public sealed record GetGroundHandlerStakeholdersQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get contractor stakeholders
/// </summary>
public sealed record GetContractorStakeholdersQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get stakeholders requiring AOA access
/// </summary>
public sealed record GetStakeholdersRequiringAOAAccessQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

#endregion

#region Validation Queries

/// <summary>
/// Query to check if username exists
/// </summary>
public sealed record CheckSMSStakeholderUserNameExistsQuery(string UserName) : IRequest<Result<bool>>;

/// <summary>
/// Query to validate user credentials
/// </summary>
public sealed record ValidateSMSStakeholderUserCredentialsQuery(string UserName, string Password) : IRequest<Result<bool>>;

#endregion

#region Statistics Queries

/// <summary>
/// Query to get SMS Stakeholder User statistics
/// </summary>
public sealed record GetSMSStakeholderUserStatisticsQuery() : IRequest<Result<UserStatistics>>;

/// <summary>
/// Query to get stakeholder type statistics
/// </summary>
public sealed record GetStakeholderTypeStatisticsQuery() : IRequest<Result<Dictionary<string, int>>>;

/// <summary>
/// Query to get organization statistics
/// </summary>
public sealed record GetStakeholderOrganizationStatisticsQuery() : IRequest<Result<Dictionary<string, int>>>;

/// <summary>
/// Query to get users requiring password change
/// </summary>
public sealed record GetSMSStakeholderUsersRequiringPasswordChangeQuery() : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

/// <summary>
/// Query to get stale users (haven't logged in recently)
/// </summary>
public sealed record GetStaleSMSStakeholderUsersQuery(int StaleDays = 90) : IRequest<Result<IEnumerable<SMSStakeholderUser>>>;

#endregion