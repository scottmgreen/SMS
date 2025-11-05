using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Membership types within committees for role-based access and participation
/// </summary>
public abstract class MembershipType : BaseEnum<MembershipType>
{
    protected MembershipType(string value, string name, string description, bool hasVotingRights, int accessLevel) : base(value, name)
    {
        Description = description;
        HasVotingRights = hasVotingRights;
        AccessLevel = accessLevel;
    }

    public string Description { get; }
    public bool HasVotingRights { get; }
    public int AccessLevel { get; }

    #region Membership Types

    /// <summary>Core voting member with full participation rights</summary>
    public static readonly MembershipType Core = new CoreType();

    /// <summary>Subject Matter Expert providing specialized knowledge</summary>
    public static readonly MembershipType SME = new SMEType();

    /// <summary>External stakeholder representative</summary>
    public static readonly MembershipType External = new ExternalType();

    /// <summary>Advisory member without voting rights</summary>
    public static readonly MembershipType Advisory = new AdvisoryType();

    /// <summary>Ex-officio member (by virtue of position)</summary>
    public static readonly MembershipType ExOfficio = new ExOfficioType();

    /// <summary>Temporary member for specific topics</summary>
    public static readonly MembershipType Temporary = new TemporaryType();

    #endregion

    #region Implementations

    private sealed class CoreType : MembershipType
    {
        public CoreType() : base("CORE", "Core Member",
            "Core voting member with full participation rights and decision-making authority", true, 10)
        {
        }
    }

    private sealed class SMEType : MembershipType
    {
        public SMEType() : base("SME", "Subject Matter Expert",
            "Subject Matter Expert providing specialized knowledge and technical expertise", true, 8)
        {
        }
    }

    private sealed class ExternalType : MembershipType
    {
        public ExternalType() : base("EXTERNAL", "External Representative",
            "External stakeholder representative providing outside perspective", true, 6)
        {
        }
    }

    private sealed class AdvisoryType : MembershipType
    {
        public AdvisoryType() : base("ADVISORY", "Advisory Member",
            "Advisory member providing input and guidance without voting rights", false, 5)
        {
        }
    }

    private sealed class ExOfficioType : MembershipType
    {
        public ExOfficioType() : base("EX_OFFICIO", "Ex-Officio Member",
            "Ex-officio member participating by virtue of their position or role", true, 7)
        {
        }
    }

    private sealed class TemporaryType : MembershipType
    {
        public TemporaryType() : base("TEMPORARY", "Temporary Member",
            "Temporary member assigned for specific topics or time-limited participation", false, 4)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available membership types
    /// </summary>
    public static IEnumerable<MembershipType> GetAllValues()
    {
        return typeof(MembershipType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(MembershipType))
            .Select(f => (MembershipType)f.GetValue(null)!)
            .Where(mt => mt != null);
    }

    /// <summary>
    /// Gets membership types with voting rights
    /// </summary>
    public static IEnumerable<MembershipType> GetVotingMembershipTypes()
    {
        return GetAllValues().Where(mt => mt.HasVotingRights);
    }

    /// <summary>
    /// Gets membership types by minimum access level
    /// </summary>
    public static IEnumerable<MembershipType> GetMembershipTypesByAccessLevel(int minAccessLevel)
    {
        return GetAllValues().Where(mt => mt.AccessLevel >= minAccessLevel);
    }

    /// <summary>
    /// Checks if this is a core membership type
    /// </summary>
    public bool IsCore => this == Core;

    /// <summary>
    /// Checks if this is a permanent membership type
    /// </summary>
    public bool IsPermanent => this != Temporary;

    /// <summary>
    /// Checks if this membership type has high access level
    /// </summary>
    public bool IsHighAccess => AccessLevel >= 8;
}