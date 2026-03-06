//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS user data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS application users
/// </summary>
public class GetAllSMSApplicationUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSApplicationUsersQuery class.
    /// </summary>
    public GetAllSMSApplicationUsersQuery()
    {
    }
}

/// <summary>
/// Query to get an SMS application user by ID
/// </summary>
//public class GetSMSApplicationUserByCodeQuery : BaseQueryBundle, IRequest<Result<SMSApplicationUser>>
//{
//    /// <summary>
//    /// The ID of the application user to retrieve
//    /// </summary>
//    public string UserCode { get; set; }

//    /// <summary>
//    /// Initializes a new instance of the GetSMSApplicationUserByCodeQuery class.
//    /// </summary>
//    /// <param name="userId">The ID of the application user to retrieve</param>
//    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
//    public GetSMSApplicationUserByCodeQuery(string userId)
//    {
//        if (string.IsNullOrWhiteSpace(userId))
//            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

//        UserCode = userId;
//    }
//}

/// <summary>
/// Query to get an SMS application user by code
/// </summary>
public class GetSMSApplicationUserByCodeQuery : BaseQueryBundle, IRequest<Result<SMSApplicationUser>>
{
    /// <summary>
    /// The code of the application user to retrieve
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUserByCodeQuery class.
    /// </summary>
    /// <param name="userCode">The code of the application user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSApplicationUserByCodeQuery(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}

/// <summary>
/// Query to get SMS application user by username
/// </summary>
public class GetSMSApplicationUserByUserNameQuery : BaseQueryBundle, IRequest<Result<SMSApplicationUser>>
{
    /// <summary>
    /// The username of the application user to retrieve
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUserByUserNameQuery class.
    /// </summary>
    /// <param name="userName">The username of the application user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public GetSMSApplicationUserByUserNameQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to get all active SMS application users
/// </summary>
public class GetActiveSMSApplicationUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetActiveSMSApplicationUsersQuery class.
    /// </summary>
    public GetActiveSMSApplicationUsersQuery()
    {
    }
}

/// <summary>
/// Query to get SMS application users by application role
/// </summary>
public class GetSMSApplicationUsersByRoleQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// The application role to filter by
    /// </summary>
    public string ApplicationRole { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUsersByRoleQuery class.
    /// </summary>
    /// <param name="applicationRole">The application role to filter by</param>
    /// <exception cref="ArgumentException">Thrown when applicationRole is null or empty</exception>
    public GetSMSApplicationUsersByRoleQuery(string applicationRole)
    {
        if (string.IsNullOrWhiteSpace(applicationRole))
            throw new ArgumentException("Application role cannot be null or empty", nameof(applicationRole));

        ApplicationRole = applicationRole;
    }
}

/// <summary>
/// Query to get SMS application users by permission level
/// </summary>
public class GetSMSApplicationUsersByPermissionLevelQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// The permission level to filter by
    /// </summary>
    public string PermissionLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUsersByPermissionLevelQuery class.
    /// </summary>
    /// <param name="permissionLevel">The permission level to filter by</param>
    /// <exception cref="ArgumentException">Thrown when permissionLevel is null or empty</exception>
    public GetSMSApplicationUsersByPermissionLevelQuery(string permissionLevel)
    {
        if (string.IsNullOrWhiteSpace(permissionLevel))
            throw new ArgumentException("Permission level cannot be null or empty", nameof(permissionLevel));

        PermissionLevel = permissionLevel;
    }
}

/// <summary>
/// Query to get SMS application users with minimum permission level
/// </summary>
public class GetSMSApplicationUsersWithMinimumPermissionQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// The minimum permission level required
    /// </summary>
    public string MinimumPermissionLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUsersWithMinimumPermissionQuery class.
    /// </summary>
    /// <param name="minimumPermissionLevel">The minimum permission level required</param>
    /// <exception cref="ArgumentException">Thrown when minimumPermissionLevel is null or empty</exception>
    public GetSMSApplicationUsersWithMinimumPermissionQuery(string minimumPermissionLevel)
    {
        if (string.IsNullOrWhiteSpace(minimumPermissionLevel))
            throw new ArgumentException("Minimum permission level cannot be null or empty", nameof(minimumPermissionLevel));

        MinimumPermissionLevel = minimumPermissionLevel;
    }
}

/// <summary>
/// Query to check if an application username exists
/// </summary>
public class CheckSMSApplicationUserNameExistsQuery : BaseQueryBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username to check
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the CheckSMSApplicationUserNameExistsQuery class.
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public CheckSMSApplicationUserNameExistsQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to validate application user credentials
/// </summary>
public class ValidateSMSApplicationUserCredentialsQuery : BaseQueryBundle, IRequest<Result<bool>>
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
    /// Initializes a new instance of the ValidateSMSApplicationUserCredentialsQuery class.
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password to validate</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public ValidateSMSApplicationUserCredentialsQuery(string userName, string password)
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
/// Query to get application users requiring password change
/// </summary>
public class GetSMSApplicationUsersRequiringPasswordChangeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUsersRequiringPasswordChangeQuery class.
    /// </summary>
    public GetSMSApplicationUsersRequiringPasswordChangeQuery()
    {
    }
}

/// <summary>
/// Query to get stale application users (haven't logged in recently)
/// </summary>
public class GetStaleSMSApplicationUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// Number of days to consider a user stale
    /// </summary>
    public int StaleDays { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetStaleSMSApplicationUsersQuery class.
    /// </summary>
    /// <param name="staleDays">Number of days to consider a user stale (default: 90)</param>
    /// <exception cref="ArgumentException">Thrown when staleDays is less than 1</exception>
    public GetStaleSMSApplicationUsersQuery(int staleDays = 90)
    {
        if (staleDays < 1)
            throw new ArgumentException("Stale days must be greater than 0", nameof(staleDays));

        StaleDays = staleDays;
    }
}

/// <summary>
/// Query to get application user statistics
/// </summary>
public class GetSMSApplicationUserStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, object>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationUserStatisticsQuery class.
    /// </summary>
    public GetSMSApplicationUserStatisticsQuery()
    {
    }
}
