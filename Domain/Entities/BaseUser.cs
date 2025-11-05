using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents the base user entity with common properties and behaviors
/// </summary>
public abstract class BaseUser : BaseAuditableEntity
{
    public BaseUserID UserId { get; protected set; }
    public string Code { get; protected set; }
    public FirstName FirstName { get; protected set; }
    public LastName LastName { get; protected set; }
    public UserName UserName { get; protected set; }
    public Password Password { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTime? LastLoginDate { get; protected set; }
    
    protected BaseUser() : base(new BaseUserID(Guid.NewGuid().ToString()), "System", DateTime.UtcNow)
    {
        UserId = new BaseUserID(Guid.NewGuid().ToString());
        Code = string.Empty;
        FirstName = FirstName.Create("System").Value;
        LastName = LastName.Create("User").Value;
        UserName = UserName.Create("system").Value;
        Password = Password.Create("TempPassword123!").Value;
        IsActive = true;
    }

    protected BaseUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string createdBy) : base(new BaseUserID(Guid.NewGuid().ToString()), createdBy, DateTime.UtcNow)
    {
        UserId = new BaseUserID(Guid.NewGuid().ToString());
        Code = code;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Password = password;
        IsActive = true;
        LastLoginDate = null;
    }

    /// <summary>
    /// Gets the user's full display name
    /// </summary>
    public string DisplayName => $"{FirstName.Value} {LastName.Value}";

    /// <summary>
    /// Gets the user's initials
    /// </summary>
    public string Initials => $"{FirstName.Value.FirstOrDefault()}{LastName.Value.FirstOrDefault()}".ToUpperInvariant();

    /// <summary>
    /// Authenticates the user with the provided password
    /// </summary>
    public bool Authenticate(string plainTextPassword)
    {
        if (!IsActive)
            return false;

        return Password.Verify(plainTextPassword);
    }

    /// <summary>
    /// Records a successful login
    /// </summary>
    public void RecordLogin()
    {
        if (IsActive)
        {
            LastLoginDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates the user's password
    /// </summary>
    public void UpdatePassword(Password newPassword)
    {
        Password = newPassword;
    }

    /// <summary>
    /// Updates the user's basic information
    /// </summary>
    public void UpdateBasicInfo(FirstName firstName, LastName lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Activates the user account
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the user account
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Checks if the user's password has expired
    /// </summary>
    public bool IsPasswordExpired(int maxAgeDays = 90)
    {
        return Password.IsExpired(maxAgeDays);
    }

    /// <summary>
    /// Checks if the user needs to change their password
    /// </summary>
    public bool RequiresPasswordChange => Password.RequiresChange || IsPasswordExpired();

    /// <summary>
    /// Gets the number of days since the user last logged in
    /// </summary>
    public int? DaysSinceLastLogin
    {
        get
        {
            if (LastLoginDate == null) return null;
            return (int)(DateTime.UtcNow - LastLoginDate.Value).TotalDays;
        }
    }

    /// <summary>
    /// Checks if the user account is considered stale (hasn't logged in for a long time)
    /// </summary>
    public bool IsStale(int staleDays = 90)
    {
        return DaysSinceLastLogin > staleDays;
    }

    /// <summary>
    /// Abstract method to get the user type - implemented by derived classes
    /// </summary>
    public abstract string GetUserType();

    /// <summary>
    /// Abstract method to get user-specific department/organization info
    /// </summary>
    public abstract string GetDepartmentInfo();
}