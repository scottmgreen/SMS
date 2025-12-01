using System.Reflection;
using SMS_Domain.Common;

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
    /// Gets all available SMS user types
    /// </summary>
    public static IEnumerable<SMSUserType> GetAllValues()
    {
        return typeof(SMSUserType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(SMSUserType))
            .Select(f => (SMSUserType)f.GetValue(null)!)
            .Where(ut => ut != null);
    }

 
}