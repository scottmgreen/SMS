namespace SMS_Domain.Enums;

/// <summary>
/// SMS User types for authentication and authorization management
/// </summary>
public abstract class SMSUserType : BaseEnum<SMSUserType>
{
    protected SMSUserType(string value, string name, string description) : base(value, name)
    {
        Description = description;
        
    }

    public string Description { get; }
    
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
            "Application administrator with role-based access to SMS functions")
        {
        }
    }

    private sealed class OrganizationalType : SMSUserType
    {
        public OrganizationalType() : base("ORGANIZATIONAL", "Organizational User",
            "Internal organizational user with role-based access to SMS functions")
        {
        }
    }

    private sealed class StakeholderType : SMSUserType
    {
        public StakeholderType() : base("STAKEHOLDER", "Stakeholder User",
            "External stakeholder with role-based access to SMS functions")
        {
        }
    }

    #endregion

    

    

   
}