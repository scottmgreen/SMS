//-----------------------------------------------------------------------
// <copyright file="BaseUser.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS user entity representing baseuser with authentication and authorization capabilities.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents the base user entity with common properties and behaviors
/// </summary>
public abstract class BaseUser : BaseAuditableEntity
{
    protected BaseUser(BaseID<string> id, string createdBy, DateTime createdDate) : base(id, createdBy, createdDate)
    {
    }

    public BaseUserID UserId { get; set; }
    public string Code { get; set; }
    public FirstName FirstName { get; set; }
    public LastName LastName { get; set; }
    public UserName UserName { get; set; }
    public Password Password { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public SMSUserType SMSUserType { get; set; }

    public SMSUserRole UserRole { get; set; }

    // 🔐 Two-Factor Authentication Properties
    public string? TwoFactorSecretKey { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
    public string? BackupCodes { get; set; } // JSON array of backup codes
    public DateTime? TwoFactorSetupDate { get; set; }
    public int FailedTwoFactorAttempts { get; set; } = 0;
    public DateTime? TwoFactorLockedUntil { get; set; }

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

}





