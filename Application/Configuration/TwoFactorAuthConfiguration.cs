//-----------------------------------------------------------------------
// <copyright file="TwoFactorAuthConfiguration.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration for Two-Factor Authentication with Microsoft Authenticator integration.
//                  Provides TOTP-based 2FA with configurable security settings.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace SMS_Application.Configuration;

/// <summary>
/// Configuration for Two-Factor Authentication system
/// Provides TOTP-based 2FA with Microsoft Authenticator integration
/// </summary>
public class TwoFactorAuthConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "TwoFactorAuth";

    /// <summary>
    /// Enable Two-Factor Authentication (default: true)
    /// </summary>
    public bool Enable2FA { get; set; } = true;

    /// <summary>
    /// Require 2FA for all users (default: true)
    /// </summary>
    public bool RequireFor2FAForAllUsers { get; set; } = true;

    /// <summary>
    /// Application name displayed in authenticator apps
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Application name must be between 3 and 50 characters")]
    public string ApplicationName { get; set; } = "PDX SMS Portal";

    /// <summary>
    /// Issuer name for QR codes (organization name)
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Issuer name must be between 3 and 50 characters")]
    public string IssuerName { get; set; } = "Port of Portland";

    /// <summary>
    /// TOTP time window in seconds (default: 30 seconds)
    /// </summary>
    [Range(15, 300, ErrorMessage = "Time window must be between 15 and 300 seconds")]
    public int TimeWindowSeconds { get; set; } = 30;

    /// <summary>
    /// Number of digits in TOTP codes (default: 6)
    /// </summary>
    [Range(4, 8, ErrorMessage = "TOTP digits must be between 4 and 8")]
    public int TotpDigits { get; set; } = 6;

    /// <summary>
    /// Allow previous/next time window for clock drift (default: 1)
    /// </summary>
    [Range(0, 3, ErrorMessage = "Time tolerance must be between 0 and 3 windows")]
    public int TimeTolerance { get; set; } = 1;

    /// <summary>
    /// 2FA session timeout in minutes (how long 2FA verification lasts)
    /// </summary>
    [Range(1, 60, ErrorMessage = "2FA session timeout must be between 1 and 60 minutes")]
    public int TwoFASessionTimeoutMinutes { get; set; } = 10;

    /// <summary>
    /// Maximum failed attempts before lockout
    /// </summary>
    [Range(1, 10, ErrorMessage = "Max failed attempts must be between 1 and 10")]
    public int MaxFailedAttempts { get; set; } = 3;

    /// <summary>
    /// Lockout duration in minutes after max failed attempts
    /// </summary>
    [Range(5, 1440, ErrorMessage = "Lockout duration must be between 5 minutes and 24 hours")]
    public int LockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Allow backup codes for recovery (default: true)
    /// </summary>
    public bool EnableBackupCodes { get; set; } = true;

    /// <summary>
    /// Number of backup codes to generate
    /// </summary>
    [Range(5, 20, ErrorMessage = "Backup code count must be between 5 and 20")]
    public int BackupCodeCount { get; set; } = 10;

    /// <summary>
    /// Enable email-based backup authentication
    /// </summary>
    public bool EnableEmailBackup { get; set; } = true;

    /// <summary>
    /// Enable SMS-based backup authentication  
    /// </summary>
    public bool EnableSmsBackup { get; set; } = false;

    /// <summary>
    /// Log 2FA events for security auditing
    /// </summary>
    public bool LogTwoFactorEvents { get; set; } = true;

    /// <summary>
    /// QR code size in pixels
    /// </summary>
    [Range(100, 500, ErrorMessage = "QR code size must be between 100 and 500 pixels")]
    public int QrCodeSize { get; set; } = 200;

    /// <summary>
    /// Convert timeout to TimeSpan
    /// </summary>
    public TimeSpan TwoFASessionTimeout => TimeSpan.FromMinutes(TwoFASessionTimeoutMinutes);

    /// <summary>
    /// Convert lockout duration to TimeSpan
    /// </summary>
    public TimeSpan LockoutDuration => TimeSpan.FromMinutes(LockoutDurationMinutes);

    /// <summary>
    /// Get the application identifier for TOTP URIs
    /// </summary>
    public string GetApplicationIdentifier() => $"{IssuerName}:{ApplicationName}";

    /// <summary>
    /// Validate the configuration settings
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ApplicationName))
        {
            errors.Add("ApplicationName cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(IssuerName))
        {
            errors.Add("IssuerName cannot be empty");
        }

        if (TimeWindowSeconds < 15 || TimeWindowSeconds > 300)
        {
            errors.Add("TimeWindowSeconds must be between 15 and 300");
        }

        if (TotpDigits < 4 || TotpDigits > 8)
        {
            errors.Add("TotpDigits must be between 4 and 8");
        }

        if (MaxFailedAttempts < 1 || MaxFailedAttempts > 10)
        {
            errors.Add("MaxFailedAttempts must be between 1 and 10");
        }

        return (errors.Count == 0, errors);
    }
}