//-----------------------------------------------------------------------
// <copyright file="SecurityHeadersOptions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration options for security headers middleware allowing customization per environment.
//                  Configuration class providing customizable security header settings
//                  for different deployment environments.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for security headers middleware
    /// </summary>
    public class SecurityHeadersOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "SecurityHeaders";

        /// <summary>
        /// Whether to enable security headers (default: true)
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// X-Frame-Options header value (default: DENY)
        /// </summary>
        public string FrameOptions { get; set; } = "DENY";

        /// <summary>
        /// Content Security Policy settings
        /// </summary>
        public ContentSecurityPolicyOptions ContentSecurityPolicy { get; set; } = new();

        /// <summary>
        /// Additional allowed domains for development/testing
        /// </summary>
        public string[] AllowedDomains { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Whether to add Strict-Transport-Security header
        /// </summary>
        public bool EnableHSTS { get; set; } = true;

        /// <summary>
        /// HSTS max-age value in seconds (default: 1 year)
        /// </summary>
        public int HSTSMaxAge { get; set; } = 31536000;

        /// <summary>
        /// Whether to log security header application (default: false for performance)
        /// </summary>
        public bool LogHeaderApplication { get; set; } = false;
    }

    /// <summary>
    /// Content Security Policy specific options
    /// </summary>
    public class ContentSecurityPolicyOptions
    {
        /// <summary>
        /// Whether to enable CSP (default: true)
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Additional script sources beyond the defaults
        /// </summary>
        public string[] AdditionalScriptSources { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Additional style sources beyond the defaults  
        /// </summary>
        public string[] AdditionalStyleSources { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Additional connect sources beyond the defaults
        /// </summary>
        public string[] AdditionalConnectSources { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Whether to use report-only mode (logs violations without blocking)
        /// </summary>
        public bool ReportOnly { get; set; } = false;

        /// <summary>
        /// CSP violation report URI (optional)
        /// </summary>
        public string? ReportUri { get; set; }
    }
}