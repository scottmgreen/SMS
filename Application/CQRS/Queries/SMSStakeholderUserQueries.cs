//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS user data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS stakeholder users
/// </summary>
public class GetAllSMSStakeholderUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSStakeholderUsersQuery class.
    /// </summary>
    public GetAllSMSStakeholderUsersQuery()
    {
    }
}


/// <summary>
/// Query to get an SMS stakeholder user by code
/// </summary>
public class GetSMSStakeholderUserByCodeQuery : BaseQueryBundle, IRequest<Result<SMSStakeholderUser>>
{
    /// <summary>
    /// The code of the stakeholder user to retrieve
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUserByCodeQuery class.
    /// </summary>
    /// <param name="userCode">The code of the stakeholder user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSStakeholderUserByCodeQuery(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}

/// <summary>
/// Query to get SMS stakeholder user by username
/// </summary>
public class GetSMSStakeholderUserByUserNameQuery : BaseQueryBundle, IRequest<Result<SMSStakeholderUser>>
{
    /// <summary>
    /// The username of the stakeholder user to retrieve
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUserByUserNameQuery class.
    /// </summary>
    /// <param name="userName">The username of the stakeholder user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public GetSMSStakeholderUserByUserNameQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to get all active SMS stakeholder users
/// </summary>
public class GetActiveSMSStakeholderUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetActiveSMSStakeholderUsersQuery class.
    /// </summary>
    public GetActiveSMSStakeholderUsersQuery()
    {
    }
}

/// <summary>
/// Query to get SMS stakeholder users by stakeholder type
/// </summary>
public class GetSMSStakeholderUsersByTypeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// The stakeholder type to filter by
    /// </summary>
    public string StakeholderType { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUsersByTypeQuery class.
    /// </summary>
    /// <param name="stakeholderType">The stakeholder type to filter by</param>
    /// <exception cref="ArgumentException">Thrown when stakeholderType is null or empty</exception>
    public GetSMSStakeholderUsersByTypeQuery(string stakeholderType)
    {
        if (string.IsNullOrWhiteSpace(stakeholderType))
            throw new ArgumentException("Stakeholder type cannot be null or empty", nameof(stakeholderType));

        StakeholderType = stakeholderType;
    }
}

/// <summary>
/// Query to get SMS stakeholder users by organization
/// </summary>
public class GetSMSStakeholderUsersByOrganizationQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// The organization to filter by
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUsersByOrganizationQuery class.
    /// </summary>
    /// <param name="organization">The organization to filter by</param>
    /// <exception cref="ArgumentException">Thrown when organization is null or empty</exception>
    public GetSMSStakeholderUsersByOrganizationQuery(string organization)
    {
        if (string.IsNullOrWhiteSpace(organization))
            throw new ArgumentException("Organization cannot be null or empty", nameof(organization));

        Organization = organization;
    }
}

/// <summary>
/// Query to get SMS stakeholder users by access level
/// </summary>
public class GetSMSStakeholderUsersByAccessLevelQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// The access level to filter by
    /// </summary>
    public string AccessLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUsersByAccessLevelQuery class.
    /// </summary>
    /// <param name="accessLevel">The access level to filter by</param>
    /// <exception cref="ArgumentException">Thrown when accessLevel is null or empty</exception>
    public GetSMSStakeholderUsersByAccessLevelQuery(string accessLevel)
    {
        if (string.IsNullOrWhiteSpace(accessLevel))
            throw new ArgumentException("Access level cannot be null or empty", nameof(accessLevel));

        AccessLevel = accessLevel;
    }
}

/// <summary>
/// Query to get airline stakeholder users
/// </summary>
public class GetAirlineStakeholdersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAirlineStakeholdersQuery class.
    /// </summary>
    public GetAirlineStakeholdersQuery()
    {
    }
}

/// <summary>
/// Query to get ground handler stakeholder users
/// </summary>
public class GetGroundHandlerStakeholdersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetGroundHandlerStakeholdersQuery class.
    /// </summary>
    public GetGroundHandlerStakeholdersQuery()
    {
    }
}

/// <summary>
/// Query to get contractor stakeholder users
/// </summary>
public class GetContractorStakeholdersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetContractorStakeholdersQuery class.
    /// </summary>
    public GetContractorStakeholdersQuery()
    {
    }
}

/// <summary>
/// Query to get SMS stakeholder users by group code
/// </summary>
public class GetSMSStakeholderUsersByGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// The group code to filter by
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUsersByGroupCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The group code to filter by</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetSMSStakeholderUsersByGroupCodeQuery(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }
}

/// <summary>
/// Query to get stakeholders requiring AOA access
/// </summary>
public class GetStakeholdersRequiringAOAAccessQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetStakeholdersRequiringAOAAccessQuery class.
    /// </summary>
    public GetStakeholdersRequiringAOAAccessQuery()
    {
    }
}

/// <summary>
/// Query to check if a stakeholder username exists
/// </summary>
public class CheckSMSStakeholderUserNameExistsQuery : BaseQueryBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username to check
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the CheckSMSStakeholderUserNameExistsQuery class.
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public CheckSMSStakeholderUserNameExistsQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to validate stakeholder user credentials
/// </summary>
public class ValidateSMSStakeholderUserCredentialsQuery : BaseQueryBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username to validate
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// The password to validate
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Initializes a new instance of the ValidateSMSStakeholderUserCredentialsQuery class.
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password to validate</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public ValidateSMSStakeholderUserCredentialsQuery(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

/// <summary>
/// Query to get stakeholder users requiring password change
/// </summary>
public class GetSMSStakeholderUsersRequiringPasswordChangeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUsersRequiringPasswordChangeQuery class.
    /// </summary>
    public GetSMSStakeholderUsersRequiringPasswordChangeQuery()
    {
    }
}

/// <summary>
/// Query to get stale stakeholder users (haven't logged in recently)
/// </summary>
public class GetStaleSMSStakeholderUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// Number of days to consider a user stale
    /// </summary>
    public int StaleDays { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetStaleSMSStakeholderUsersQuery class.
    /// </summary>
    /// <param name="staleDays">Number of days to consider a user stale (default: 90)</param>
    /// <exception cref="ArgumentException">Thrown when staleDays is less than 1</exception>
    public GetStaleSMSStakeholderUsersQuery(int staleDays = 90)
    {
        if (staleDays < 1)
            throw new ArgumentException("Stale days must be greater than 0", nameof(staleDays));

        StaleDays = staleDays;
    }
}

/// <summary>
/// Query to get stakeholder user statistics
/// </summary>
public class GetSMSStakeholderUserStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, object>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderUserStatisticsQuery class.
    /// </summary>
    public GetSMSStakeholderUserStatisticsQuery()
    {
    }
}

/// <summary>
/// Query to get stakeholder type statistics
/// </summary>
public class GetStakeholderTypeStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, int>>>
{
    /// <summary>
    /// Initializes a new instance of the GetStakeholderTypeStatisticsQuery class.
    /// </summary>
    public GetStakeholderTypeStatisticsQuery()
    {
    }
}

/// <summary>
/// Query to get organization statistics
/// </summary>
public class GetStakeholderOrganizationStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, int>>>
{
    /// <summary>
    /// Initializes a new instance of the GetStakeholderOrganizationStatisticsQuery class.
    /// </summary>
    public GetStakeholderOrganizationStatisticsQuery()
    {
    }
}
