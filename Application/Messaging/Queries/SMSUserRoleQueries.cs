//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS user data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Models;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Queries for SMS User Role operations
/// </summary>

#region Get All Queries

/// <summary>
/// Query to get all SMS User Role assignments
/// </summary>
public class GetAllSMSUserRolesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSUserRolesQuery class.
    /// </summary>
    public GetAllSMSUserRolesQuery()
    {
    }
}

/// <summary>
/// Query to get all active SMS User Role assignments
/// </summary>
public class GetAllActiveSMSUserRolesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllActiveSMSUserRolesQuery class.
    /// </summary>
    public GetAllActiveSMSUserRolesQuery()
    {
    }
}

#endregion

#region Get By ID Queries

/// <summary>
/// Query to get SMS User Role assignment by ID
/// </summary>
public class GetSMSUserRoleByIdQuery : BaseQueryBundle, IRequest<Result<SMSUserRole>>
{
    /// <summary>
    /// The ID of the SMS User Role to retrieve
    /// </summary>
    public SMSUserRoleID UserRoleId { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSUserRoleByIdQuery class.
    /// </summary>
    /// <param name="userRoleId">The SMS user role ID</param>
    /// <exception cref="ArgumentNullException">Thrown when userRoleId is null</exception>
    public GetSMSUserRoleByIdQuery(SMSUserRoleID userRoleId)
    {
        UserRoleId = userRoleId ?? throw new ArgumentNullException(nameof(userRoleId));
    }

    /// <summary>
    /// Convenience constructor with string ID
    /// </summary>
    /// <param name="userRoleId">The SMS user role ID as string</param>
    public GetSMSUserRoleByIdQuery(string userRoleId)
    {
        if (string.IsNullOrWhiteSpace(userRoleId))
            throw new ArgumentException("User role ID cannot be null or empty", nameof(userRoleId));

        UserRoleId = new SMSUserRoleID(userRoleId);
    }
}

#endregion

#region Get By User Queries

/// <summary>
/// Query to get SMS User Role assignments by User ID
/// </summary>
public class GetSMSUserRolesByUserIdQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// The User ID to get roles for
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSUserRolesByUserIdQuery class.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
    public GetSMSUserRolesByUserIdQuery(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        UserId = userId;
    }
}

/// <summary>
/// Query to get active SMS User Role assignments by User ID
/// </summary>
//public class GetActiveSMSUserRolesByUserIdQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
//{
//    /// <summary>
//    /// The User ID to get active roles for
//    /// </summary>
//    public string UserId { get; set; }

//    /// <summary>
//    /// Initializes a new instance of the GetActiveSMSUserRolesByUserIdQuery class.
//    /// </summary>
//    /// <param name="userId">The user ID</param>
//    /// <exception cref="ArgumentException">Thrown when userId is null or empty</exception>
//    public GetActiveSMSUserRolesByUserIdQuery(string userId)
//    {
//        if (string.IsNullOrWhiteSpace(userId))
//            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

//        UserId = userId;
//    }
//}

#endregion

#region Get By Role Queries

/// <summary>
/// Query to get SMS User Role assignments by role value
/// </summary>
public class GetSMSUserRolesByRoleValueQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// The role value to filter by
    /// </summary>
    public string RoleValue { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSUserRolesByRoleValueQuery class.
    /// </summary>
    /// <param name="roleValue">The role value</param>
    /// <exception cref="ArgumentException">Thrown when roleValue is null or empty</exception>
    public GetSMSUserRolesByRoleValueQuery(string roleValue)
    {
        if (string.IsNullOrWhiteSpace(roleValue))
            throw new ArgumentException("Role value cannot be null or empty", nameof(roleValue));

        RoleValue = roleValue;
    }
}

#endregion

#region Get By Department/UserType Queries

/// <summary>
/// Query to get SMS User Role assignments by department
/// </summary>
public class GetSMSUserRolesByDepartmentQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// The department to filter by
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSUserRolesByDepartmentQuery class.
    /// </summary>
    /// <param name="department">The department</param>
    /// <exception cref="ArgumentException">Thrown when department is null or empty</exception>
    public GetSMSUserRolesByDepartmentQuery(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be null or empty", nameof(department));

        Department = department;
    }
}

/// <summary>
/// Query to get SMS User Role assignments by user type
/// </summary>
public class GetSMSUserRolesByUserTypeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// The user type to filter by
    /// </summary>
    public string UserType { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSUserRolesByUserTypeQuery class.
    /// </summary>
    /// <param name="userType">The user type</param>
    /// <exception cref="ArgumentException">Thrown when userType is null or empty</exception>
    public GetSMSUserRolesByUserTypeQuery(string userType)
    {
        if (string.IsNullOrWhiteSpace(userType))
            throw new ArgumentException("User type cannot be null or empty", nameof(userType));

        UserType = userType;
    }
}

#endregion

#region Expiring/Statistics Queries

/// <summary>
/// Query to get expiring SMS User Role assignments
/// </summary>
public class GetExpiringSMSUserRolesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSUserRole>>>
{
    /// <summary>
    /// The cutoff date for expiration
    /// </summary>
    public DateTime CutoffDate { get; set; }

    /// <summary>
    /// Optional number of days from now to check for expiration
    /// </summary>
    public int? DaysFromNow { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetExpiringSMSUserRolesQuery class.
    /// </summary>
    /// <param name="cutoffDate">The cutoff date</param>
    public GetExpiringSMSUserRolesQuery(DateTime cutoffDate)
    {
        CutoffDate = cutoffDate;
    }

    /// <summary>
    /// Convenience constructor with days from now
    /// </summary>
    /// <param name="daysFromNow">Number of days from now</param>
    public GetExpiringSMSUserRolesQuery(int daysFromNow)
    {
        DaysFromNow = daysFromNow;
        CutoffDate = DateTime.UtcNow.AddDays(daysFromNow);
    }

    /// <summary>
    /// Default constructor for roles expiring in 30 days
    /// </summary>
    public GetExpiringSMSUserRolesQuery() : this(30)
    {
    }
}

/// <summary>
/// Query to get SMS User Role statistics
/// </summary>
public class GetSMSUserRoleStatisticsQuery : BaseQueryBundle, IRequest<Result<UserRoleStatistics>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSUserRoleStatisticsQuery class.
    /// </summary>
    public GetSMSUserRoleStatisticsQuery()
    {
    }
}

#endregion

#region Validation Queries

/// <summary>
/// Query to validate if a user has a specific role
/// </summary>
//public class ValidateUserHasRoleQuery : BaseQueryBundle, IRequest<Result<bool>>
//{
//    /// <summary>
//    /// The user ID to check
//    /// </summary>
//    public string UserId { get; set; }

//    /// <summary>
//    /// The role value to check for
//    /// </summary>
//    public string RoleValue { get; set; }

//    /// <summary>
//    /// Whether to check only active roles
//    /// </summary>
//    public bool ActiveOnly { get; set; }

//    /// <summary>
//    /// Initializes a new instance of the ValidateUserHasRoleQuery class.
//    /// </summary>
//    /// <param name="userId">The user ID</param>
//    /// <param name="roleValue">The role value</param>
//    /// <param name="activeOnly">Whether to check only active roles</param>
//    /// <exception cref="ArgumentException">Thrown when userId or roleValue is null or empty</exception>
//    public ValidateUserHasRoleQuery(string userId, string roleValue, bool activeOnly = true)
//    {
//        if (string.IsNullOrWhiteSpace(userId))
//            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
//        if (string.IsNullOrWhiteSpace(roleValue))
//            throw new ArgumentException("Role value cannot be null or empty", nameof(roleValue));

//        UserId = userId;
//        RoleValue = roleValue;
//        ActiveOnly = activeOnly;
//    }
//}

#endregion

