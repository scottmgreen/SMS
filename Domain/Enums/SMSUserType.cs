namespace SMS_Domain.Enums;

/// <summary>
/// SMS User types for authentication and authorization management
/// </summary>
public abstract class SMSUserType : BaseEnum<SMSUserType>
{
    protected SMSUserType(string value, string name, string description, int authorizationLevel) : base(value, name)
    {
        Description = description;
        AuthorizationLevel = authorizationLevel;
    }

    public string Description { get; }
    public int AuthorizationLevel { get; }

    #region SMS User Types

    /// <summary>Application administrator user</summary>
    public static readonly SMSUserType Application = new ApplicationType();

    /// <summary>Organizational user (internal staff)</summary>
    public static readonly SMSUserType Organizational = new OrganizationalType();

    /// <summary>External stakeholder user</summary>
    public static readonly SMSUserType Stakeholder = new StakeholderType();

    #endregion

    #region Implementations

    private sealed class ApplicationType : SMSUserType
    {
        public ApplicationType() : base("APPLICATION", "Application User",
            "Application administrator with role-based access to SMS functions", 10)
        {
        }
    }

    private sealed class OrganizationalType : SMSUserType
    {
        public OrganizationalType() : base("ORGANIZATIONAL", "Organizational User",
            "Internal organizational user with role-based access to SMS functions", 7)
        {
        }
    }

    private sealed class StakeholderType : SMSUserType
    {
        public StakeholderType() : base("STAKEHOLDER", "Stakeholder User",
            "External stakeholder with role-based access to SMS functions", 3)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets user types by minimum authorization level
    /// </summary>
    public static IEnumerable<SMSUserType> GetUserTypesByAuthLevel(int minLevel)
    {
        return GetAllValues().Where(ut => ut.AuthorizationLevel >= minLevel);
    }

    /// <summary>
    /// Gets the highest authorization level user type
    /// </summary>
    public static SMSUserType GetHighestAuthType()
    {
        return GetAllValues().OrderByDescending(ut => ut.AuthorizationLevel).First();
    }

    /// <summary>
    /// Checks if this user type has administrative privileges
    /// </summary>
    public bool IsAdministrator => AuthorizationLevel >= 8;

    /// <summary>
    /// Checks if this user type is internal to the organization
    /// </summary>
    public bool IsInternal => this == Application || this == Organizational;

    /// <summary>
    /// Checks if this user type is external to the organization
    /// </summary>
    public bool IsExternal => this == Stakeholder;
}