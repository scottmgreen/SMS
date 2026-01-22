namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS stakeholder groups
/// </summary>
public class GetAllSMSStakeholderGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderGroup>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSStakeholderGroupsQuery class.
    /// </summary>
    public GetAllSMSStakeholderGroupsQuery()
    {
    }
}

/// <summary>
/// Query to get an SMS stakeholder group by code
/// </summary>
public class GetSMSStakeholderGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSStakeholderGroup>>
{
    /// <summary>
    /// The code of the stakeholder group to retrieve
    /// </summary>
    public string GroupCode { get; set; }

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
}

/// <summary>
/// Query to get SMS stakeholder groups by user code
/// </summary>
public class GetSMSStakeholderGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderGroup>>>
{
    /// <summary>
    /// The code of the user to get stakeholder groups for
    /// </summary>
    public string UserCode { get; set; }

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
}

/// <summary>
/// Query to get users by stakeholder group code
/// </summary>
public class GetUsersByStakeholderGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSStakeholderUser>>>
{
    /// <summary>
    /// The code of the stakeholder group to get users for
    /// </summary>
    public string GroupCode { get; set; }

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
}