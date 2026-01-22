namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS stakeholder groups
/// </summary>
public class GetAllSMSApplicationGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationGroup>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSApplicationGroupsQuery class.
    /// </summary>
    public GetAllSMSApplicationGroupsQuery()
    {
    }
}

/// <summary>
/// Query to get an SMS stakeholder group by code
/// </summary>
public class GetSMSApplicationGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSApplicationGroup>>
{
    /// <summary>
    /// The code of the stakeholder group to retrieve
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationGroupByCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The code of the stakeholder group to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetSMSApplicationGroupByCodeQuery(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }
}

/// <summary>
/// Query to get SMS stakeholder groups by user code
/// </summary>
public class GetSMSApplicationGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationGroup>>>
{
    /// <summary>
    /// The code of the user to get stakeholder groups for
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSApplicationGroupsByUserCodeQuery class.
    /// </summary>
    /// <param name="userCode">The code of the user to get stakeholder groups for</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSApplicationGroupsByUserCodeQuery(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}

/// <summary>
/// Query to get users by stakeholder group code
/// </summary>
public class GetUsersByApplicationGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSApplicationUser>>>
{
    /// <summary>
    /// The code of the stakeholder group to get users for
    /// </summary>
    public string GroupCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetUsersByApplicationGroupCodeQuery class.
    /// </summary>
    /// <param name="groupCode">The code of the stakeholder group to get users for</param>
    /// <exception cref="ArgumentException">Thrown when groupCode is null or empty</exception>
    public GetUsersByApplicationGroupCodeQuery(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
            throw new ArgumentException("Group code cannot be null or empty", nameof(groupCode));

        GroupCode = groupCode;
    }
}