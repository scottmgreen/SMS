using Application.Services.Strategies;

using Microsoft.AspNetCore.Authentication.Cookies;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS3.Security;

namespace SMS3.Configuration;

/// <summary>
/// Extension methods for configuring SMS Presentation Authentication services
/// Maintains exact same loading sequence as Program.cs but organized into logical groups
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// ?? MAIN AUTHENTICATION SERVICES REGISTRATION
    /// Wraps all authentication-related IServices from Program.cs into organized extension method
    /// Maintains EXACT same loading sequence to prevent breaking changes
    /// </summary>
    public static IServiceCollection AddPresentationAuthenticationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // ?? AUTHENTICATION CONFIGURATIONS - Load and validate all auth-related configurations
        services.AddAuthenticationConfigurations(configuration);

        // ?? DISTRIBUTED CACHE & SESSION - Required for session support  
        services.AddDistributedAuthenticationSupport(configuration);

        // ?? BLAZOR CIRCUIT AUTHENTICATION STORAGE
        services.AddBlazorCircuitAuthentication();

        // ?? COOKIE AUTHENTICATION & AUTHORIZATION
        services.AddCookieAuthenticationServices(configuration);

        // ?? AUTHENTICATION STRATEGY SERVICES - Clean Architecture Implementation
        services.AddAuthenticationStrategies();

        // ?? SECURITY SERVICES
        services.AddSecurityServices();

        return services;
    }

    /// <summary>
    /// ?? AUTHENTICATION CONFIGURATIONS - Load from appsettings.json with validation
    /// Exact copy from Program.cs lines 45-169
    /// </summary>
    private static IServiceCollection AddAuthenticationConfigurations(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // **?? SESSION CONFIGURATION - Load from appsettings.json**
        var sessionConfig = new SMS_Application.Configuration.SessionConfiguration();
        configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName).Bind(sessionConfig);
        
        // Validate session configuration
        var (isValid, errors) = sessionConfig.Validate();
        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid session configuration: {string.Join(", ", errors)}");
        }

        // Register session configuration for DI
        services.Configure<SMS_Application.Configuration.SessionConfiguration>(
            configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName));
        services.AddSingleton(sessionConfig);

        // **?? REQUEST VALIDATION CONFIGURATION - Load from appsettings.json**
        var requestValidationConfig = new SMS_Application.Configuration.RequestValidationConfiguration();
        configuration.GetSection(SMS_Application.Configuration.RequestValidationConfiguration.SectionName).Bind(requestValidationConfig);
        
        // Validate request validation configuration
        var (isRequestValidationValid, requestValidationErrors) = requestValidationConfig.Validate();
        if (!isRequestValidationValid)
        {
            throw new InvalidOperationException($"Invalid request validation configuration: {string.Join(", ", requestValidationErrors)}");
        }

        // Register request validation configuration
        services.Configure<SMS_Application.Configuration.RequestValidationConfiguration>(
            configuration.GetSection(SMS_Application.Configuration.RequestValidationConfiguration.SectionName));
        services.AddSingleton(requestValidationConfig);

        // **?? TWO-FACTOR AUTHENTICATION CONFIGURATION - Load from appsettings.json**
        var twoFactorConfig = new SMS_Application.Configuration.TwoFactorAuthConfiguration();
        configuration.GetSection(SMS_Application.Configuration.TwoFactorAuthConfiguration.SectionName).Bind(twoFactorConfig);
        
        // Validate 2FA configuration
        var (is2FAValid, twoFactorErrors) = twoFactorConfig.Validate();
        if (!is2FAValid)
        {
            throw new InvalidOperationException($"Invalid Two-Factor Authentication configuration: {string.Join(", ", twoFactorErrors)}");
        }

        // Register 2FA configuration
        services.Configure<SMS_Application.Configuration.TwoFactorAuthConfiguration>(
            configuration.GetSection(SMS_Application.Configuration.TwoFactorAuthConfiguration.SectionName));
        services.AddSingleton(twoFactorConfig);

        // ?? NEW: AUTHENTICATION STRATEGY CONFIGURATION - Load from appsettings.json
        var authConfig = new SMS_Application.Configuration.AuthenticationConfiguration();
        configuration.GetSection(SMS_Application.Configuration.AuthenticationConfiguration.SectionName).Bind(authConfig);
        
        // Validate authentication configuration
        var (isAuthConfigValid, authConfigErrors) = authConfig.Validate();
        if (!isAuthConfigValid)
        {
            throw new InvalidOperationException($"Invalid authentication configuration: {string.Join(", ", authConfigErrors)}");
        }

        // Register authentication configuration
        services.Configure<SMS_Application.Configuration.AuthenticationConfiguration>(
            configuration.GetSection(SMS_Application.Configuration.AuthenticationConfiguration.SectionName));
        services.AddSingleton(authConfig);

        return services;
    }

    /// <summary>
    /// ?? DISTRIBUTED CACHE & SESSION - Required for Session support
    /// Exact copy from Program.cs lines 54-125 plus protocol configuration
    /// </summary>
    private static IServiceCollection AddDistributedAuthenticationSupport(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // **?? DISTRIBUTED CACHE - Required for Session support**
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();

        // **?? EXPLICIT PROTOCOL CONFIGURATION - NO SQUISHY AUTO-DETECTION**
        var masterProtocolConfig = configuration.GetSection("MasterProtocol");
        var explicitProtocol = masterProtocolConfig.GetValue<string>("Protocol", "HTTP") ?? "HTTP";
        var forceEverywhere = masterProtocolConfig.GetValue<bool>("ForceProtocolEverywhere", true);
        var allowMixed = masterProtocolConfig.GetValue<bool>("AllowMixedMode", false);

        // Determine explicit settings based on protocol
        var isHttps = explicitProtocol.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);
        var secureCookies = isHttps && forceEverywhere;
        var strictSameSite = isHttps ? SameSiteMode.Strict : SameSiteMode.Lax;

        // Get session config (already registered above)
        var sessionConfig = new SMS_Application.Configuration.SessionConfiguration();
        configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName).Bind(sessionConfig);

        // **?? EXPLICIT SESSION CONFIGURATION - Uses protocol settings directly**
        services.AddSession(options =>
        {
            options.IdleTimeout = sessionConfig.IdleTimeout;
            options.Cookie.HttpOnly = sessionConfig.HttpOnly;
            
            // ?? EXPLICIT: Use protocol-based settings
            options.Cookie.SecurePolicy = secureCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
            options.Cookie.SameSite = strictSameSite;
            
            options.Cookie.Name = sessionConfig.CookieName;
            options.Cookie.Path = sessionConfig.CookiePath;
            
            // Set domain if specified
            if (!string.IsNullOrEmpty(sessionConfig.CookieDomain))
            {
                options.Cookie.Domain = sessionConfig.CookieDomain;
            }
        });

        // **?? FEATURE-DRIVEN CSRF PROTECTION - Enhanced antiforgery configuration**
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__SMS_RequestVerificationToken";
            options.Cookie.HttpOnly = true;
            
            // ?? EXPLICIT: Use protocol-based settings
            options.Cookie.SecurePolicy = secureCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
            options.Cookie.SameSite = strictSameSite;
            options.SuppressXFrameOptionsHeader = false;
        });

        return services;
    }

    /// <summary>
    /// ?? BLAZOR CIRCUIT AUTHENTICATION STORAGE
    /// Exact copy from Program.cs line 83
    /// </summary>
    private static IServiceCollection AddBlazorCircuitAuthentication(this IServiceCollection services)
    {
        // **?? BLAZOR SERVER CIRCUIT AUTHENTICATION STORAGE**
        services.AddSingleton<SMS_Application.Services.IBlazorCircuitAuthStorage, SMS_Application.Services.BlazorCircuitAuthStorage>();

        return services;
    }

    /// <summary>
    /// ?? COOKIE AUTHENTICATION & AUTHORIZATION 
    /// Exact copy from Program.cs lines 85-98
    /// </summary>
    private static IServiceCollection AddCookieAuthenticationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Get session config for authentication settings
        var sessionConfig = new SMS_Application.Configuration.SessionConfiguration();
        configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName).Bind(sessionConfig);

        // **?? SIMPLE COOKIE AUTHENTICATION - Replace problematic session management**
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = sessionConfig.IdleTimeout;
                options.SlidingExpiration = sessionConfig.SlidingExpiration;
                options.Cookie.Name = "SMS_Auth";
                options.Cookie.HttpOnly = sessionConfig.HttpOnly;
                options.Cookie.SameSite = sessionConfig.SameSiteMode;
            });

        // **?? AUTHORIZATION**
        services.AddAuthorizationBuilder();

        return services;
    }

    /// <summary>
    /// ?? AUTHENTICATION STRATEGY SERVICES - Clean Architecture Implementation  
    /// Exact copy from Program.cs lines 171-185
    /// </summary>
    private static IServiceCollection AddAuthenticationStrategies(this IServiceCollection services)
    {
        // ?? NEW: AUTHENTICATION STRATEGY SERVICES - Clean Architecture Implementation
        
        // Protocol detection and compatibility services
        services.AddScoped<SMS_Application.Interfaces.IProtocolDetectionService, SMS_Application.Services.ProtocolDetectionService>();
        
        // User validation and instantiation services (CQRS-based)
        services.AddScoped<SMS_Application.Interfaces.IUserCompletenessValidator, SMS_Application.Services.UserCompletenessValidator>();
        services.AddScoped<SMS_Application.Interfaces.IUserInstantiationService, SMS_Application.Services.UserInstantiationService>();
        
        // Authentication strategy implementations
        services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, SessionBasedAuthenticationStrategy>();
        services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, CircuitBasedAuthenticationStrategy>();
        services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, ContextBasedAuthenticationStrategy>();
        
        // Authentication strategy manager (orchestrator)
        services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategyManager, SMS_Application.Services.AuthenticationStrategyManager>();

        // ?? CRITICAL FIX: Register Strategy-Based Current User Service for NavMenu
        services.AddScoped<SMS_Application.Interfaces.ICurrentUserService, SMS_Application.Services.StrategyBasedCurrentUserService>();

        return services;
    }

    /// <summary>
    /// ?? SECURITY SERVICES
    /// Exact copy from Program.cs line 194
    /// </summary>
    private static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // **?? SYSTEM-WIDE SECURE ROUTING SERVICES**
        services.AddSingleton<ISecureRoutingService, SecureRoutingService>();

        return services;
    }
}