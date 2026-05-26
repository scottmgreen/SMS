//-----------------------------------------------------------------------
// <copyright file="ApiKeyAuthenticationFilter.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: API key authentication filter providing secure service-to-service communication with validation and audit logging.
//                  Security component providing authentication, authorization,
//                  and access control functionality.
// </copyright>
//-----------------------------------------------------------------------

// Create a new file: Infrastructure/Security/ApiKeyAuthenticationFilter.cs
using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Security;

/// <summary>
/// Endpoint filter for API key authentication
/// Validates API keys from headers or query parameters
/// </summary>
public class ApiKeyAuthenticationFilter : IEndpointFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthenticationFilter> _logger;

    // API key header names to check
    private static readonly string[] ApiKeyHeaders = { "X-API-Key", "X-Api-Key", "ApiKey", "Authorization" };

    public ApiKeyAuthenticationFilter(IConfiguration configuration, ILogger<ApiKeyAuthenticationFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // Extract API key from request
        var apiKey = ExtractApiKey(httpContext.Request);

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("API request rejected: No API key provided. Path: {Path}", httpContext.Request.Path);
            return Results.Unauthorized();
        }

        // Validate API key
        if (!await ValidateApiKeyAsync(apiKey, httpContext))
        {
            _logger.LogWarning("API request rejected: Invalid API key. Path: {Path}, Key: {Key}",
                httpContext.Request.Path, MaskApiKey(apiKey));
            return Results.Unauthorized();
        }

        _logger.LogInformation("API request authenticated successfully. Path: {Path}, System: {System}",
            httpContext.Request.Path, GetSourceSystemFromContext(httpContext));

        // Continue to the actual endpoint
        return await next(context);
    }

    /// <summary>
    /// Extract API key from headers or query parameters
    /// </summary>
    private string? ExtractApiKey(HttpRequest request)
    {
        // 1. Check headers first
        foreach (var headerName in ApiKeyHeaders)
        {
            if (request.Headers.TryGetValue(headerName, out var headerValues))
            {
                var value = headerValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    // Handle "Bearer {token}" format
                    if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return value["Bearer ".Length..].Trim();
                    }
                    return value;
                }
            }
        }

        // 2. Check query parameters as fallback
        if (request.Query.TryGetValue("apikey", out var queryValues) ||
            request.Query.TryGetValue("api_key", out queryValues))
        {
            return queryValues.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Diagnostic helper to identify whether the request included any API key header.
    /// </summary>
    private string GetObservedApiKeyHeader(HttpRequest request)
    {
        foreach (var headerName in ApiKeyHeaders)
        {
            if (request.Headers.TryGetValue(headerName, out var headerValues))
            {
                var value = headerValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    return headerName;
                }
            }
        }

        if (request.Query.ContainsKey("apikey")) return "query:apikey";
        if (request.Query.ContainsKey("api_key")) return "query:api_key";

        return "none";
    }

    /// <summary>
    /// Validate the provided API key
    /// </summary>
    private Task<bool> ValidateApiKeyAsync(string apiKey, HttpContext context)
    {
        try
        {
            // Method 1: Simple configuration-based validation
            var validApiKeys = GetValidApiKeysFromConfiguration();
            if (validApiKeys.Contains(apiKey))
            {
                // Store source system info for logging
                var sourceSystem = GetSourceSystemForApiKey(apiKey);
                context.Items["SourceSystem"] = sourceSystem;
                return Task.FromResult(true);
            }

            // Method 2: Database validation (if you want to store API keys in database)
            // return await ValidateApiKeyFromDatabaseAsync(apiKey, context);

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Get valid API keys from configuration
    /// </summary>
    private HashSet<string> GetValidApiKeysFromConfiguration()
    {
        var apiKeys = new HashSet<string>();

        // Read from appsettings.json
        var configKeys = _configuration.GetSection("ApiAuthentication:ValidApiKeys").Get<string[]>();
        if (configKeys != null)
        {
            foreach (var key in configKeys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    apiKeys.Add(key);
                }
            }
        }

        // Read individual keys with system names
        var namedKeys = _configuration.GetSection("ApiAuthentication:Systems").GetChildren();
        foreach (var system in namedKeys)
        {
            var key = system["ApiKey"];
            if (!string.IsNullOrEmpty(key))
            {
                apiKeys.Add(key);
            }
        }

        return apiKeys;
    }

    /// <summary>
    /// Get source system name for an API key
    /// </summary>
    private string GetSourceSystemForApiKey(string apiKey)
    {
        var systems = _configuration.GetSection("ApiAuthentication:Systems").GetChildren();
        foreach (var system in systems)
        {
            if (system["ApiKey"] == apiKey)
            {
                return system["Name"] ?? system.Key;
            }
        }
        return "Unknown";
    }

    /// <summary>
    /// Mask API key for logging
    /// </summary>
    private string MaskApiKey(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 8)
            return "***";

        return apiKey[..4] + "***" + apiKey[^4..];
    }

    /// <summary>
    /// Get source system from HTTP context
    /// </summary>
    private string GetSourceSystemFromContext(HttpContext context)
    {
        return context.Items["SourceSystem"]?.ToString() ?? "Unknown";
    }
}
