//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture - WITH AUDIT TRACKING
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

/// <summary>
/// Query to get all SMS organizational groups - WITH AUDIT TRACKING
/// </summary>
public class GetAllSMSOrganizationalGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalGroup>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetAllSMSOrganizationalGroupsQuery class.
    /// </summary>
    public GetAllSMSOrganizationalGroupsQuery()
    {
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SMSOrganizationalGroup:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllSMSOrganizationalGroups
    }
}

/// <summary>
/// Query to get an SMS organizational group by code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSOrganizationalGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSOrganizationalGroup>>, IReadQuery
{
    /// <summary>
    /// The group code to retrieve
    /// </summary>
    public string GroupCode { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalGroupByCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The group code to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetSMSOrganizationalGroupByCodeQuery(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSOrganizationalGroup:Code:{GroupCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSMSOrganizationalGroupByCode
    }
}

/// <summary>
/// Query to get SMS organizational groups by user code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSOrganizationalGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalGroup>>>, IReadQuery
{
    /// <summary>
    /// The user code to get groups for
    /// </summary>
    public string UserCode { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalGroupsByUserCodeQuery class.
    /// </summary>
    /// <param name="userCode">The user code to get groups for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSOrganizationalGroupsByUserCodeQuery(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSOrganizationalGroup:ByUser:{UserCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSMSOrganizationalGroupsByUserCode
    }
}

/// <summary>
/// Query to get users by organizational group code - WITH AUDIT TRACKING
/// </summary>
public class GetUsersByOrganizationalGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>, IReadQuery
{
    /// <summary>
    /// The group code to get users for
    /// </summary>
    public string GroupCode { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetUsersByOrganizationalGroupCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The group code to get users for</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetUsersByOrganizationalGroupCodeQuery(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSOrganizationalUser:ByGroup:{GroupCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetUsersByOrganizationalGroupCode
    }
}
