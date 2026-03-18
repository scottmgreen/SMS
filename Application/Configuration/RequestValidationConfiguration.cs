//-----------------------------------------------------------------------
// <copyright file="RequestValidationConfiguration.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration for request validation, input sanitization, and CSRF protection.
//                  Provides comprehensive request security settings for SMS applications.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace SMS_Application.Configuration;

/// <summary>
/// Configuration for request validation and security measures
/// Provides settings for input validation, CSRF protection, and request sanitization
/// </summary>
public class RequestValidationConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "RequestValidation";

    /// <summary>
    /// Enable CSRF protection for forms (default: true)
    /// </summary>
    public bool EnableCsrfProtection { get; set; } = true;

    /// <summary>
    /// Maximum request body size in bytes (default: 10MB)
    /// </summary>
    [Range(1024, 100 * 1024 * 1024, ErrorMessage = "Max request size must be between 1KB and 100MB")]
    public long MaxRequestBodySize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Enable input sanitization for XSS prevention (default: true)
    /// </summary>
    public bool EnableInputSanitization { get; set; } = true;

    /// <summary>
    /// Enable SQL injection detection and prevention (default: true)
    /// </summary>
    public bool EnableSqlInjectionDetection { get; set; } = true;

    /// <summary>
    /// Maximum length for text inputs (default: 10000 characters)
    /// </summary>
    [Range(100, 100000, ErrorMessage = "Max input length must be between 100 and 100,000 characters")]
    public int MaxInputLength { get; set; } = 10000;

    /// <summary>
    /// Blocked file extensions for uploads
    /// </summary>
    public string[] BlockedFileExtensions { get; set; } = 
    {
        ".exe", ".bat", ".cmd", ".com", ".scr", ".pif", ".vbs", ".js", ".jar", ".asp", ".aspx", ".php"
    };

    /// <summary>
    /// Allowed file extensions for uploads (if empty, uses blocked list)
    /// </summary>
    public string[] AllowedFileExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum file upload size in bytes (default: 5MB)
    /// </summary>
    [Range(1024, 50 * 1024 * 1024, ErrorMessage = "Max file size must be between 1KB and 50MB")]
    public long MaxFileUploadSize { get; set; } = 5 * 1024 * 1024; // 5MB

    /// <summary>
    /// Enable rate limiting per IP address (default: true)
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Maximum requests per minute per IP (default: 60)
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Rate limit must be between 1 and 1000 requests per minute")]
    public int RateLimitPerMinute { get; set; } = 60;

    /// <summary>
    /// Enable request logging for security auditing (default: true)
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Log only suspicious requests (reduces log volume)
    /// </summary>
    public bool LogOnlySuspiciousRequests { get; set; } = false;

    /// <summary>
    /// Validate the configuration settings
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        if (MaxRequestBodySize < 1024)
        {
            errors.Add("MaxRequestBodySize must be at least 1KB");
        }

        if (MaxInputLength < 100)
        {
            errors.Add("MaxInputLength must be at least 100 characters");
        }

        if (MaxFileUploadSize < 1024)
        {
            errors.Add("MaxFileUploadSize must be at least 1KB");
        }

        if (RateLimitPerMinute < 1)
        {
            errors.Add("RateLimitPerMinute must be at least 1");
        }

        return (errors.Count == 0, errors);
    }
}