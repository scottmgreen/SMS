//-----------------------------------------------------------------------
// <copyright file="SessionConfiguration.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration options for secure session management with validation and defaults.
//                  Provides strongly-typed configuration binding for session security settings.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SMS_Application.Configuration;

/// <summary>
/// Configuration options for secure session management
/// Provides strongly-typed configuration binding with validation and security defaults
/// </summary>
public class SessionConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "SessionConfiguration";

    /// <summary>
    /// Session timeout in minutes (default: 30 minutes)
    /// </summary>
    [Range(5, 1440, ErrorMessage = "Session timeout must be between 5 minutes and 24 hours")]
    public int TimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Enable sliding expiration (extends session on activity)
    /// </summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Require HTTPS for session cookies (default: true for security)
    /// </summary>
    public bool SecureCookies { get; set; } = true;

    /// <summary>
    /// Prevent client-side JavaScript access to session cookies (default: true)
    /// </summary>
    public bool HttpOnly { get; set; } = true;

    /// <summary>
    /// SameSite cookie policy for CSRF protection
    /// Valid values: "None", "Lax", "Strict"
    /// </summary>
    [RegularExpression("^(None|Lax|Strict)$", ErrorMessage = "SameSite must be 'None', 'Lax', or 'Strict'")]
    public string SameSite { get; set; } = "Strict";

    /// <summary>
    /// Custom session cookie name (default: "SMS_Session")
    /// </summary>
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Cookie name must be between 3 and 50 characters")]
    public string CookieName { get; set; } = "SMS_Session";

    /// <summary>
    /// Cookie domain (optional - uses request domain if not specified)
    /// </summary>
    public string? CookieDomain { get; set; }

    /// <summary>
    /// Cookie path (default: "/")
    /// </summary>
    public string CookiePath { get; set; } = "/";

    /// <summary>
    /// Whether to log session creation and destruction (default: false for privacy)
    /// </summary>
    public bool LogSessionActivity { get; set; } = false;

    /// <summary>
    /// Convert TimeoutMinutes to TimeSpan for ASP.NET Core session configuration
    /// </summary>
    public TimeSpan IdleTimeout => TimeSpan.FromMinutes(TimeoutMinutes);

    /// <summary>
    /// Convert SameSite string to enum for ASP.NET Core session configuration
    /// </summary>
    public SameSiteMode SameSiteMode => SameSite switch
    {
        "None" => Microsoft.AspNetCore.Http.SameSiteMode.None,
        "Lax" => Microsoft.AspNetCore.Http.SameSiteMode.Lax,
        "Strict" => Microsoft.AspNetCore.Http.SameSiteMode.Strict,
        _ => Microsoft.AspNetCore.Http.SameSiteMode.Strict // Default to most secure
    };

    /// <summary>
    /// Get CookieSecurePolicy based on SecureCookies setting
    /// </summary>
    public CookieSecurePolicy SecurePolicy => SecureCookies 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;

    /// <summary>
    /// Validate the configuration settings
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (TimeoutMinutes < 5 || TimeoutMinutes > 1440)
        {
            errors.Add("TimeoutMinutes must be between 5 and 1440 (24 hours)");
        }

        if (string.IsNullOrWhiteSpace(CookieName))
        {
            errors.Add("CookieName cannot be empty");
        }

        if (CookieName?.Length > 50)
        {
            errors.Add("CookieName cannot exceed 50 characters");
        }

        var validSameSiteValues = new[] { "None", "Lax", "Strict" };
        if (!validSameSiteValues.Contains(SameSite))
        {
            errors.Add("SameSite must be 'None', 'Lax', or 'Strict'");
        }

        return (errors.Count == 0, errors);
    }
}