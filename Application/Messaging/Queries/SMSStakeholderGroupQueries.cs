//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture - WITH AUDIT TRACKING
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS stakeholder groups - WITH AUDIT TRACKING
/// </summary>
public class GetAllSMSStakeholderGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderGroup>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetAllSMSStakeholderGroupsQuery class.
    /// </summary>
    public GetAllSMSStakeholderGroupsQuery()
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
        return "SMSStakeholderGroup:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllSMSStakeholderGroups
    }
}

/// <summary>
/// Query to get an SMS stakeholder group by code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSStakeholderGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSStakeholderGroup>>, IReadQuery
{
    /// <summary>
    /// The code of the stakeholder group to retrieve
    /// </summary>
    public string GroupCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderGroupByCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The code of the stakeholder group to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetSMSStakeholderGroupByCodeQuery(string groupCode)
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
        return $"SMSStakeholderGroup:Code:{GroupCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSMSStakeholderGroupByCode
    }
}

/// <summary>
/// Query to get SMS stakeholder groups by user code - WITH AUDIT TRACKING
/// </summary>
public class GetSMSStakeholderGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderGroup>>>, IReadQuery
{
    /// <summary>
    /// The code of the user to get stakeholder groups for
    /// </summary>
    public string UserCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSStakeholderGroupsByUserCodeQuery class.
    /// </summary>
    /// <param name="userCode">The code of the user to get stakeholder groups for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSStakeholderGroupsByUserCodeQuery(string userCode)
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
        return $"SMSStakeholderGroup:ByUser:{UserCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSMSStakeholderGroupsByUserCode
    }
}

/// <summary>
/// Query to get users by stakeholder group code - WITH AUDIT TRACKING
/// </summary>
public class GetUsersByStakeholderGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>, IReadQuery
{
    /// <summary>
    /// The code of the stakeholder group to get users for
    /// </summary>
    public string GroupCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    /// <summary>
    /// Initializes a new instance of the GetUsersByStakeholderGroupCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The code of the stakeholder group to get users for</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetUsersByStakeholderGroupCodeQuery(string groupCode)
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
        return $"SMSStakeholderUser:ByGroup:{GroupCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetUsersByStakeholderGroupCode
    }
}
