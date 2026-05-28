//-----------------------------------------------------------------------
// <copyright file="SecurityFeatureService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for managing security-related feature flags and policies.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SMS_Application.Configuration;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing security-related feature flags and policies
/// </summary>
public interface ISecurityFeatureService
{
    Task<CookieSecurePolicy> GetCookieSecurePolicyAsync(string featureName = SecurityFeatures.AllowHttpCookies);
    Task<bool> IsHttpAllowedInDevelopmentAsync();
    Task<bool> IsSecurityFeatureEnabledAsync(string featureName);
    Task<SecurityFeatureContext> GetSecurityContextAsync(IHostEnvironment environment);
}

public class SecurityFeatureService : ISecurityFeatureService
{
    private readonly IFeatureManager _featureManager;
    private readonly IHostEnvironment _environment;
    private readonly SessionConfiguration _sessionConfig;
    private readonly ILogger<SecurityFeatureService> _logger;

    public SecurityFeatureService(
        IFeatureManager featureManager,
        IHostEnvironment environment,
        SessionConfiguration sessionConfig,
        ILogger<SecurityFeatureService> logger)
    {
        _featureManager = featureManager;
        _environment = environment;
        _sessionConfig = sessionConfig;
        _logger = logger;
    }

    public async Task<CookieSecurePolicy> GetCookieSecurePolicyAsync(string featureName = SecurityFeatures.AllowHttpCookies)
    {
        try
        {
            // Check if HTTP cookies are explicitly allowed
            bool allowHttp = await _featureManager.IsEnabledAsync(featureName);
            
            // Check if we're in development mode
            bool isDevelopment = _environment.IsDevelopment();
            
            // Check if development HTTP is allowed
            bool allowHttpInDev = await _featureManager.IsEnabledAsync(SecurityFeatures.AllowHttpInDevelopment);

            // Log the decision-making process
            _logger.LogApplicationInformation("Security Policy Decision: Feature={Feature}, AllowHttp={AllowHttp}, IsDevelopment={IsDevelopment}, AllowHttpInDev={AllowHttpInDev}",
                ApplicationEventIds.Information,
                featureName, allowHttp, isDevelopment, allowHttpInDev);

            // Decision matrix:
            // 1. If explicitly allowing HTTP cookies -> SameAsRequest
            // 2. If development AND allowing HTTP in development -> SameAsRequest
            // 3. If session config allows development mode AND we're in dev -> SameAsRequest
            // 4. Otherwise -> Always (secure)

            if (allowHttp)
            {
                _logger.LogApplicationWarning("HTTP cookies explicitly enabled via feature flag: {Feature}",
                    ApplicationEventIds.Warning,
                    featureName);
                return CookieSecurePolicy.SameAsRequest;
            }

            if (isDevelopment && allowHttpInDev)
            {
                _logger.LogApplicationInformation("HTTP cookies enabled for development environment", ApplicationEventIds.Information);
                return CookieSecurePolicy.SameAsRequest;
            }

            if (_sessionConfig.EnableDevelopmentMode && isDevelopment)
            {
                _logger.LogApplicationInformation("HTTP cookies enabled via session configuration development mode", ApplicationEventIds.Information);
                return CookieSecurePolicy.SameAsRequest;
            }

            // Default to secure
            _logger.LogApplicationInformation("Using secure cookie policy (HTTPS required)", ApplicationEventIds.Information);
            return CookieSecurePolicy.Always;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error determining cookie security policy, defaulting to secure", ApplicationEventIds.Error, ex);
            return CookieSecurePolicy.Always;
        }
    }

    public async Task<bool> IsHttpAllowedInDevelopmentAsync()
    {
        return _environment.IsDevelopment() && 
               await _featureManager.IsEnabledAsync(SecurityFeatures.AllowHttpInDevelopment);
    }

    public async Task<bool> IsSecurityFeatureEnabledAsync(string featureName)
    {
        return await _featureManager.IsEnabledAsync(featureName);
    }

    public async Task<SecurityFeatureContext> GetSecurityContextAsync(IHostEnvironment environment)
    {
        return new SecurityFeatureContext
        {
            Environment = environment.EnvironmentName,
            IsDevelopment = environment.IsDevelopment(),
            AllowHttpCookies = await _featureManager.IsEnabledAsync(SecurityFeatures.AllowHttpCookies),
            AllowHttpInDevelopment = await _featureManager.IsEnabledAsync(SecurityFeatures.AllowHttpInDevelopment),
            EnforceHttpsRedirection = await _featureManager.IsEnabledAsync(SecurityFeatures.EnforceHttpsRedirection),
            EnableSecurityHeaders = await _featureManager.IsEnabledAsync(SecurityFeatures.EnableSecurityHeaders),
            CookieSecurePolicy = await GetCookieSecurePolicyAsync()
        };
    }
}

/// <summary>
/// Security feature flag constants
/// </summary>
public static class SecurityFeatures
{
    public const string AllowHttpCookies = "AllowHttpCookies";
    public const string AllowHttpInDevelopment = "AllowHttpInDevelopment";
    public const string EnforceHttpsRedirection = "EnforceHttpsRedirection";
    public const string EnableSecurityHeaders = "EnableSecurityHeaders";
    public const string EnableCsrfProtection = "EnableCsrfProtection";
}

/// <summary>
/// Security feature context for decision making
/// </summary>
public class SecurityFeatureContext
{
    public string Environment { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; }
    public bool AllowHttpCookies { get; set; }
    public bool AllowHttpInDevelopment { get; set; }
    public bool EnforceHttpsRedirection { get; set; }
    public bool EnableSecurityHeaders { get; set; }
    public CookieSecurePolicy CookieSecurePolicy { get; set; }
}