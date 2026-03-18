//-----------------------------------------------------------------------
// <copyright file="RequestValidationMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Middleware for comprehensive request validation, input sanitization, and security checks.
//                  Provides protection against XSS, SQL injection, and other common web attacks.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace SMS3.Middleware;

/// <summary>
/// Middleware for comprehensive request validation and security checks
/// Provides protection against common web attacks and validates input data
/// </summary>
public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;
    private readonly RequestValidationConfiguration _config;

    // SQL injection detection patterns
    private static readonly Regex[] SqlInjectionPatterns = new[]
    {
        new Regex(@"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT|SELECT|UNION|UPDATE)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(\b(OR|AND)\s+[\d\w]+=[\d\w]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"([';](\s)?(OR|AND))", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"(--|\#|/\*|\*/)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    // XSS detection patterns
    private static readonly Regex[] XssPatterns = new[]
    {
        new Regex(@"<\s*script[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<\s*iframe[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<\s*object[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<\s*embed[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    public RequestValidationMiddleware(
        RequestDelegate next, 
        ILogger<RequestValidationMiddleware> logger,
        RequestValidationConfiguration config)
    {
        _next = next;
        _logger = logger;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Skip validation for static files and certain paths
            if (ShouldSkipValidation(context))
            {
                await _next(context);
                return;
            }

            // 1. Check request size
            if (!ValidateRequestSize(context))
            {
                await ReturnBadRequest(context, "Request size exceeds maximum allowed limit");
                return;
            }

            // 2. Validate query parameters
            if (_config.EnableInputSanitization && !ValidateQueryParameters(context))
            {
                await ReturnBadRequest(context, "Invalid characters detected in request parameters");
                return;
            }

            // 3. Validate form data for POST requests
            if (context.Request.Method == "POST" && context.Request.HasFormContentType)
            {
                if (!await ValidateFormDataAsync(context))
                {
                    await ReturnBadRequest(context, "Invalid data detected in form submission");
                    return;
                }
            }

            // 4. Log request if enabled
            if (_config.EnableRequestLogging)
            {
                LogRequest(context);
            }

            // Continue to next middleware
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in request validation middleware");
            await ReturnServerError(context, "An error occurred processing your request");
        }
    }

    private bool ShouldSkipValidation(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        
        // Skip validation for static files
        var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".gif", ".ico", ".woff", ".woff2", ".ttf", ".eot", ".svg" };
        if (staticExtensions.Any(ext => path.EndsWith(ext)))
            return true;

        // Skip validation for health checks and API documentation
        if (path.StartsWith("/_") || path.StartsWith("/health") || path.StartsWith("/api-docs"))
            return true;

        return false;
    }

    private bool ValidateRequestSize(HttpContext context)
    {
        if (context.Request.ContentLength.HasValue)
        {
            if (context.Request.ContentLength.Value > _config.MaxRequestBodySize)
            {
                _logger.LogWarning("Request size {RequestSize} exceeds maximum {MaxSize} from IP {RemoteIP}", 
                    context.Request.ContentLength.Value, _config.MaxRequestBodySize, GetClientIpAddress(context));
                return false;
            }
        }

        return true;
    }

    private bool ValidateQueryParameters(HttpContext context)
    {
        foreach (var param in context.Request.Query)
        {
            if (!ValidateInputValue(param.Key) || !ValidateInputValue(param.Value))
            {
                _logger.LogWarning("Suspicious query parameter detected from IP {RemoteIP}: {ParameterName}", 
                    GetClientIpAddress(context), param.Key);
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ValidateFormDataAsync(HttpContext context)
    {
        try
        {
            var form = await context.Request.ReadFormAsync();
            
            foreach (var field in form)
            {
                if (!ValidateInputValue(field.Key) || !ValidateInputValue(field.Value))
                {
                    _logger.LogWarning("Suspicious form data detected from IP {RemoteIP}: {FieldName}", 
                        GetClientIpAddress(context), field.Key);
                    return false;
                }

                // Check input length
                if (field.Value.ToString().Length > _config.MaxInputLength)
                {
                    _logger.LogWarning("Form field exceeds maximum length from IP {RemoteIP}: {FieldName} ({Length} chars)", 
                        GetClientIpAddress(context), field.Key, field.Value.ToString().Length);
                    return false;
                }
            }

            // Validate uploaded files
            foreach (var file in form.Files)
            {
                if (!ValidateUploadedFile(file))
                {
                    _logger.LogWarning("Invalid file upload detected from IP {RemoteIP}: {FileName}", 
                        GetClientIpAddress(context), file.FileName);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating form data");
            return false;
        }
    }

    private bool ValidateInputValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return true;

        // Check for SQL injection patterns
        if (_config.EnableSqlInjectionDetection)
        {
            foreach (var pattern in SqlInjectionPatterns)
            {
                if (pattern.IsMatch(value))
                {
                    return false;
                }
            }
        }

        // Check for XSS patterns
        if (_config.EnableInputSanitization)
        {
            foreach (var pattern in XssPatterns)
            {
                if (pattern.IsMatch(value))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool ValidateUploadedFile(IFormFile file)
    {
        if (file.Length > _config.MaxFileUploadSize)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // Check against allowed extensions first (if specified)
        if (_config.AllowedFileExtensions.Any())
        {
            return _config.AllowedFileExtensions.Contains(extension);
        }

        // Otherwise, check against blocked extensions
        return !_config.BlockedFileExtensions.Contains(extension);
    }

    private void LogRequest(HttpContext context)
    {
        var logSuspiciousOnly = _config.LogOnlySuspiciousRequests;
        var isSuspicious = IsSuspiciousRequest(context);

        if (!logSuspiciousOnly || isSuspicious)
        {
            var logLevel = isSuspicious ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel, "Request: {Method} {Path} from {RemoteIP} - UserAgent: {UserAgent}", 
                context.Request.Method,
                context.Request.Path,
                GetClientIpAddress(context),
                context.Request.Headers.UserAgent.ToString());
        }
    }

    private bool IsSuspiciousRequest(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
        var suspiciousUserAgents = new[] { "scanner", "bot", "crawler", "spider", "hack", "injection", "exploit" };
        
        return suspiciousUserAgents.Any(agent => userAgent.Contains(agent));
    }

    private string GetClientIpAddress(HttpContext context)
    {
        try
        {
            return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                   ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString()
                   ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task ReturnBadRequest(HttpContext context, string message)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        
        var response = new { error = "Bad Request", message = message };
        var json = System.Text.Json.JsonSerializer.Serialize(response);
        
        await context.Response.WriteAsync(json);
    }

    private async Task ReturnServerError(HttpContext context, string message)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var response = new { error = "Internal Server Error", message = message };
        var json = System.Text.Json.JsonSerializer.Serialize(response);
        
        await context.Response.WriteAsync(json);
    }
}