//-----------------------------------------------------------------------
// <copyright file="AuthenticationConfiguration.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration for authentication strategy selection and behavior.
//                  Enables explicit control over authentication methods and fallback chains.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace SMS_Application.Configuration;

/// <summary>
/// Authentication method selection enumeration
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>Traditional HTTP Session cookies - Most secure for HTTPS</summary>
    SessionBased,
    
    /// <summary>Blazor Server circuit storage - Handles response timing issues</summary>
    CircuitBased,
    
    /// <summary>Request-scoped HttpContext.Items - Emergency fallback</summary>
    ContextBased,
    
    /// <summary>Smart fallback chain - Current blurry behavior (for migration)</summary>
    Hybrid
}

/// <summary>
/// Authentication strategy configuration
/// Provides explicit control over authentication behavior
/// </summary>
public class AuthenticationConfiguration
{
    public const string SectionName = "Authentication";

    /// <summary>
    /// Primary authentication method to use
    /// </summary>
    [Required]
    public AuthenticationMethod PreferredMethod { get; set; } = AuthenticationMethod.SessionBased;

    /// <summary>
    /// Fallback authentication method when primary fails
    /// </summary>
    public AuthenticationMethod FallbackMethod { get; set; } = AuthenticationMethod.CircuitBased;

    /// <summary>
    /// Enable automatic fallback chain when primary method fails
    /// </summary>
    public bool EnableFallbackChain { get; set; } = true;

    /// <summary>
    /// Require complete user instantiation with roles and permissions
    /// </summary>
    public bool RequireCompleteUserInstantiation { get; set; } = true;

    /// <summary>
    /// Log authentication method selection and fallback decisions
    /// </summary>
    public bool LogAuthenticationDecisions { get; set; } = true;

    /// <summary>
    /// Validate user completeness on every authentication check
    /// </summary>
    public bool ValidateUserCompletenessOnAccess { get; set; } = true;

    /// <summary>
    /// Maximum time to cache user data in circuit storage (minutes)
    /// </summary>
    [Range(1, 1440)] // 1 minute to 24 hours
    public int CircuitCacheTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Enable strict mode - fail if user is not complete rather than fallback
    /// </summary>
    public bool StrictMode { get; set; } = false;

    /// <summary>
    /// Required protocol for authentication (HTTP or HTTPS)
    /// </summary>
    [Required]
    public string RequiredProtocol { get; set; } = "HTTPS";

    /// <summary>
    /// Allow fallback to HTTP protocol when HTTPS is not available
    /// </summary>
    public bool AllowProtocolFallback { get; set; } = true;

    /// <summary>
    /// Override protocol detection for development/testing scenarios
    /// Values: "HTTP", "HTTPS", or null for auto-detection
    /// </summary>
    public string? OverrideProtocol { get; set; } = null;

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (CircuitCacheTimeoutMinutes < 1 || CircuitCacheTimeoutMinutes > 1440)
        {
            errors.Add("CircuitCacheTimeoutMinutes must be between 1 and 1440 minutes");
        }

        if (PreferredMethod == FallbackMethod && EnableFallbackChain)
        {
            errors.Add("PreferredMethod and FallbackMethod cannot be the same when fallback chain is enabled");
        }

        // Validate protocol settings
        var validProtocols = new[] { "HTTP", "HTTPS" };
        if (!string.IsNullOrEmpty(RequiredProtocol) && !validProtocols.Contains(RequiredProtocol.ToUpper()))
        {
            errors.Add("RequiredProtocol must be either 'HTTP' or 'HTTPS'");
        }

        if (!string.IsNullOrEmpty(OverrideProtocol) && !validProtocols.Contains(OverrideProtocol.ToUpper()))
        {
            errors.Add("OverrideProtocol must be either 'HTTP', 'HTTPS', or null");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Get configuration summary for logging
    /// </summary>
    public string GetConfigurationSummary()
    {
        return $"Primary: {PreferredMethod}, Fallback: {FallbackMethod}, Chain: {EnableFallbackChain}, " +
               $"Complete: {RequireCompleteUserInstantiation}, Strict: {StrictMode}, " +
               $"Protocol: {RequiredProtocol}, AllowFallback: {AllowProtocolFallback}" +
               (OverrideProtocol != null ? $", Override: {OverrideProtocol}" : "");
    }
}