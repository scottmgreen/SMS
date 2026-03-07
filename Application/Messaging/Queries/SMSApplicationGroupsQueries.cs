//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupsQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS application groups - WITH AUDIT TRACKING
/// </summary>
public class GetAllSMSApplicationGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationGroup>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetAllSMSApplicationGroupsQuery class.
    /// </summary>
    public GetAllSMSApplicationGroupsQuery()
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
        return "SMSApplicationGroup:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

/// <summary>
/// Query to get an SMS application group by code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSApplicationGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSApplicationGroup>>, IReadQuery
{
    public string GroupCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSApplicationGroupByCodeQuery(string groupCode)
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
        return $"SMSApplicationGroup:Code:{GroupCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}

/// <summary>
/// Query to get SMS application groups by user code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSApplicationGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationGroup>>>, IReadQuery
{
    public string UserCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSApplicationGroupsByUserCodeQuery(string userCode)
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
        return $"SMSApplicationGroup:ByUser:{UserCode}";
    }

    public string GetAccessType()
    {
        return "GetByUser";
    }
}

/// <summary>
/// Query to get users by application group code - WITH AUDIT TRACKING
/// </summary>
public class GetUsersByApplicationGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>, IReadQuery
{
    public string GroupCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetUsersByApplicationGroupCodeQuery(string groupCode)
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
        return $"SMSApplicationUser:ByGroup:{GroupCode}";
    }

    public string GetAccessType()
    {
        return "GetUsersByGroup";
    }
}
