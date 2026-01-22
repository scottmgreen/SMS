namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Query to get all SMS organizational groups
/// </summary>
public class GetAllSMSOrganizationalGroupsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalGroup>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSOrganizationalGroupsQuery class.
    /// </summary>
    public GetAllSMSOrganizationalGroupsQuery()
    {
    }
}

/// <summary>
/// Query to get an SMS organizational group by code
/// </summary>
public class GetSMSOrganizationalGroupByCodeQuery : BaseQueryBundle, IRequest<Result<SMSOrganizationalGroup>>
{
    /// <summary>
    /// The group code to retrieve
    /// </summary>
    public string GroupCode { get; set; }

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
}

/// <summary>
/// Query to get SMS organizational groups by user code
/// </summary>
public class GetSMSOrganizationalGroupsByUserCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalGroup>>>
{
    /// <summary>
    /// The user code to get groups for
    /// </summary>
    public string UserCode { get; set; }

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
}

/// <summary>
/// Query to get users by organizational group code
/// </summary>
public class GetUsersByOrganizationalGroupCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The group code to get users for
    /// </summary>
    public string GroupCode { get; set; }

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
}