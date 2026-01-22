namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Queries for SMS Organizational User operations
/// </summary>

#region Get Queries

/// <summary>
/// Query to get all SMS Organizational Users
/// </summary>
public class GetAllSMSOrganizationalUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetAllSMSOrganizationalUsersQuery class.
    /// </summary>
    public GetAllSMSOrganizationalUsersQuery()
    {
    }
}


/// <summary>
/// Query to get SMS Organizational User by Code
/// </summary>
public class GetSMSOrganizationalUserByCodeQuery : BaseQueryBundle, IRequest<Result<SMSOrganizationalUser>>
{
    /// <summary>
    /// The Code of the organizational user to retrieve
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUserByCodeQuery class.
    /// </summary>
    /// <param name="userCode">The code of the organizational user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userCode is null or empty</exception>
    public GetSMSOrganizationalUserByCodeQuery(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode))
            throw new ArgumentException("User code cannot be null or empty", nameof(userCode));

        UserCode = userCode;
    }
}

/// <summary>
/// Query to get SMS Organizational User by username
/// </summary>
public class GetSMSOrganizationalUserByUserNameQuery : BaseQueryBundle, IRequest<Result<SMSOrganizationalUser>>
{
    /// <summary>
    /// The username of the organizational user to retrieve
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUserByUserNameQuery class.
    /// </summary>
    /// <param name="userName">The username of the organizational user to retrieve</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public GetSMSOrganizationalUserByUserNameQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to get all active SMS Organizational Users
/// </summary>
public class GetActiveSMSOrganizationalUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetActiveSMSOrganizationalUsersQuery class.
    /// </summary>
    public GetActiveSMSOrganizationalUsersQuery()
    {
    }
}

/// <summary>
/// Query to get SMS Organizational Users by department
/// </summary>
public class GetSMSOrganizationalUsersByDepartmentQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The department to filter by
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUsersByDepartmentQuery class.
    /// </summary>
    /// <param name="department">The department to filter by</param>
    /// <exception cref="ArgumentException">Thrown when department is null or empty</exception>
    public GetSMSOrganizationalUsersByDepartmentQuery(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be null or empty", nameof(department));

        Department = department;
    }
}

/// <summary>
/// Query to get SMS Organizational Users by position
/// </summary>
public class GetSMSOrganizationalUsersByPositionQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The position to filter by
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUsersByPositionQuery class.
    /// </summary>
    /// <param name="position">The position to filter by</param>
    /// <exception cref="ArgumentException">Thrown when position is null or empty</exception>
    public GetSMSOrganizationalUsersByPositionQuery(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position cannot be null or empty", nameof(position));

        Position = position;
    }
}

/// <summary>
/// Query to get SMS Organizational Users by organization level
/// </summary>
public class GetSMSOrganizationalUsersByOrganizationLevelQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The organization level to filter by
    /// </summary>
    public string OrganizationLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUsersByOrganizationLevelQuery class.
    /// </summary>
    /// <param name="organizationLevel">The organization level to filter by</param>
    /// <exception cref="ArgumentException">Thrown when organizationLevel is null or empty</exception>
    public GetSMSOrganizationalUsersByOrganizationLevelQuery(string organizationLevel)
    {
        if (string.IsNullOrWhiteSpace(organizationLevel))
            throw new ArgumentException("Organization level cannot be null or empty", nameof(organizationLevel));

        OrganizationLevel = organizationLevel;
    }
}

/// <summary>
/// Query to get SMS Organizational Users at or above a specific level
/// </summary>
public class GetSMSOrganizationalUsersAtOrAboveLevelQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The minimum organization level required
    /// </summary>
    public string MinimumLevel { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUsersAtOrAboveLevelQuery class.
    /// </summary>
    /// <param name="minimumLevel">The minimum organization level required</param>
    /// <exception cref="ArgumentException">Thrown when minimumLevel is null or empty</exception>
    public GetSMSOrganizationalUsersAtOrAboveLevelQuery(string minimumLevel)
    {
        if (string.IsNullOrWhiteSpace(minimumLevel))
            throw new ArgumentException("Minimum level cannot be null or empty", nameof(minimumLevel));

        MinimumLevel = minimumLevel;
    }
}

/// <summary>
/// Query to get department supervisors
/// </summary>
public class GetDepartmentSupervisorsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// The department to get supervisors for
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetDepartmentSupervisorsQuery class.
    /// </summary>
    /// <param name="department">The department to get supervisors for</param>
    /// <exception cref="ArgumentException">Thrown when department is null or empty</exception>
    public GetDepartmentSupervisorsQuery(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            throw new ArgumentException("Department cannot be null or empty", nameof(department));

        Department = department;
    }
}

#endregion

#region Validation Queries

/// <summary>
/// Query to check if username exists
/// </summary>
public class CheckSMSOrganizationalUserNameExistsQuery : BaseQueryBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The username to check
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Initializes a new instance of the CheckSMSOrganizationalUserNameExistsQuery class.
    /// </summary>
    /// <param name="userName">The username to check</param>
    /// <exception cref="ArgumentException">Thrown when userName is null or empty</exception>
    public CheckSMSOrganizationalUserNameExistsQuery(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        UserName = userName;
    }
}

/// <summary>
/// Query to validate user credentials
/// </summary>
public class ValidateSMSOrganizationalUserCredentialsQuery : BaseQueryBundle, IRequest<Result<bool>>
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
    /// Initializes a new instance of the ValidateSMSOrganizationalUserCredentialsQuery class.
    /// </summary>
    /// <param name="userName">The username to validate</param>
    /// <param name="password">The password to validate</param>
    /// <exception cref="ArgumentException">Thrown when userName or password is null or empty</exception>
    public ValidateSMSOrganizationalUserCredentialsQuery(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("Username cannot be null or empty", nameof(userName));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        UserName = userName;
        Password = password;
    }
}

#endregion

#region Statistics Queries

/// <summary>
/// Query to get SMS Organizational User statistics
/// </summary>
public class GetSMSOrganizationalUserStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, object>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUserStatisticsQuery class.
    /// </summary>
    public GetSMSOrganizationalUserStatisticsQuery()
    {
    }
}

/// <summary>
/// Query to get department statistics
/// </summary>
public class GetDepartmentStatisticsQuery : BaseQueryBundle, IRequest<Result<Dictionary<string, int>>>
{
    /// <summary>
    /// Initializes a new instance of the GetDepartmentStatisticsQuery class.
    /// </summary>
    public GetDepartmentStatisticsQuery()
    {
    }
}

/// <summary>
/// Query to get users requiring password change
/// </summary>
public class GetSMSOrganizationalUsersRequiringPasswordChangeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// Initializes a new instance of the GetSMSOrganizationalUsersRequiringPasswordChangeQuery class.
    /// </summary>
    public GetSMSOrganizationalUsersRequiringPasswordChangeQuery()
    {
    }
}

/// <summary>
/// Query to get stale users (haven't logged in recently)
/// </summary>
public class GetStaleSMSOrganizationalUsersQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganizationalUser>>>
{
    /// <summary>
    /// Number of days to consider a user stale
    /// </summary>
    public int StaleDays { get; set; }

    /// <summary>
    /// Initializes a new instance of the GetStaleSMSOrganizationalUsersQuery class.
    /// </summary>
    /// <param name="staleDays">Number of days to consider a user stale (default: 90)</param>
    /// <exception cref="ArgumentException">Thrown when staleDays is less than 1</exception>
    public GetStaleSMSOrganizationalUsersQuery(int staleDays = 90)
    {
        if (staleDays < 1)
            throw new ArgumentException("Stale days must be greater than 0", nameof(staleDays));

        StaleDays = staleDays;
    }
}

#endregion