namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS application user entity for system administrators and application-level users
/// </summary>
public sealed class SMSApplicationUser : BaseUser
{
    // Simple constructors

    public SMSApplicationUser(SMSApplicationUserID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        ApplicationUserId = id;
    }

    // Simple properties with public setters
    public SMSApplicationUserID ApplicationUserId { get; set; }
    //public string ApplicationRole { get; set; } = string.Empty;
    //public string PermissionLevel { get; set; } = string.Empty;




}