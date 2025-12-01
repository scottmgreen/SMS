using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS stakeholder user entity for external partners, airlines, contractors, etc.
/// </summary>
public sealed class SMSStakeholderUser : BaseUser
{
    // Simple constructors
       
    public SMSStakeholderUser(SMSStakeholderUserID id) : base(id,"SYSTEM", DateTime.UtcNow) 
    {
        StakeholderUserId = id;
    }

    // Simple properties with public setters
    public SMSStakeholderUserID StakeholderUserId { get; set; }
    public string StakeholderType { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    

    /// <summary>
    /// Checks if the user is of a specific stakeholder type
    /// </summary>
    public bool IsStakeholderType(string type)
    {
        return string.Equals(StakeholderType, type, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user belongs to a specific organization
    /// </summary>
    public bool IsFromOrganization(string organization)
    {
        return string.Equals(Organization, organization, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the stakeholder's external organization display
    /// </summary>
   

    
}