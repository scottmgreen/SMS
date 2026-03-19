using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SMS_Application.Configuration;
using SMS_Infrastructure.Configuration;
using SMS_Shared.Configuration;
using SMS3.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;
using SMS3.Security;
using SMS3.Api.Endpoints;
using Infrastructure.Configuration.Extensions;
using SMS_Application.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SMS3;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
        // **🚀 FEATURE MANAGEMENT - Must be registered early**
        builder.Services.AddSharedServices(builder.Configuration);
        
        // **🔐 SESSION CONFIGURATION - Load from appsettings.json**
        var sessionConfig = new SMS_Application.Configuration.SessionConfiguration();
        builder.Configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName).Bind(sessionConfig);
        
        // Validate session configuration
        var (isValid, errors) = sessionConfig.Validate();
        if (!isValid)
        {
            throw new InvalidOperationException($"Invalid session configuration: {string.Join(", ", errors)}");
        }

        // **🔐 DISTRIBUTED CACHE - Required for Session support**
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        
        // Register session configuration for DI
        builder.Services.Configure<SMS_Application.Configuration.SessionConfiguration>(
            builder.Configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName));
        builder.Services.AddSingleton(sessionConfig);

        // **🚀 REGISTER APPLICATION SERVICES EARLY - Need SecurityFeatureService**
        builder.Services.AddApplicationServices();

        // **🔧 BLAZOR SERVER CIRCUIT AUTHENTICATION STORAGE**
        builder.Services.AddSingleton<SMS_Application.Services.IBlazorCircuitAuthStorage, SMS_Application.Services.BlazorCircuitAuthStorage>();

        // **🔐 SIMPLE COOKIE AUTHENTICATION - Replace problematic session management**
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

        // **🔐 AUTHORIZATION**
        builder.Services.AddAuthorizationBuilder();
        
        // **🔐 FEATURE-DRIVEN CSRF PROTECTION - Enhanced antiforgery configuration**
        builder.Services.AddAntiforgery(async options =>
        {
            // Create a temporary service provider to resolve the security feature service
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var securityFeatureService = serviceProvider.GetRequiredService<ISecurityFeatureService>();
            
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__SMS_RequestVerificationToken";
            options.Cookie.HttpOnly = true;
            
            // 🚀 Feature-driven secure policy
            options.Cookie.SecurePolicy = await securityFeatureService.GetCookieSecurePolicyAsync(SecurityFeatures.AllowHttpCookies);
            
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SuppressXFrameOptionsHeader = false;
            
            // Log the configuration decision
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("🔐 Antiforgery cookie policy set to: {Policy}", options.Cookie.SecurePolicy);
        });
        
        // **🔐 FEATURE-DRIVEN SESSION CONFIGURATION - Apply secure settings from configuration**
        builder.Services.AddSession(async options =>
        {
            // Create a temporary service provider to resolve the security feature service
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var securityFeatureService = serviceProvider.GetRequiredService<ISecurityFeatureService>();
            
            options.IdleTimeout = sessionConfig.IdleTimeout;
            options.Cookie.HttpOnly = sessionConfig.HttpOnly;
            
            // 🚀 Feature-driven secure policy
            options.Cookie.SecurePolicy = await securityFeatureService.GetCookieSecurePolicyAsync(SecurityFeatures.AllowHttpCookies);
            
            options.Cookie.SameSite = sessionConfig.SameSiteMode;
            options.Cookie.Name = sessionConfig.CookieName;
            options.Cookie.Path = sessionConfig.CookiePath;
            
            // Set domain if specified
            if (!string.IsNullOrEmpty(sessionConfig.CookieDomain))
            {
                options.Cookie.Domain = sessionConfig.CookieDomain;
            }
            
            // Log the configuration decision
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("🔐 Session cookie policy set to: {Policy}", options.Cookie.SecurePolicy);
        });

        // **🔐 REQUEST VALIDATION CONFIGURATION - Load from appsettings.json**
        var requestValidationConfig = new SMS_Application.Configuration.RequestValidationConfiguration();
        builder.Configuration.GetSection(SMS_Application.Configuration.RequestValidationConfiguration.SectionName).Bind(requestValidationConfig);
        
        // Validate request validation configuration
        var (isRequestValidationValid, requestValidationErrors) = requestValidationConfig.Validate();
        if (!isRequestValidationValid)
        {
            throw new InvalidOperationException($"Invalid request validation configuration: {string.Join(", ", requestValidationErrors)}");
        }

        // Register request validation configuration
        builder.Services.Configure<SMS_Application.Configuration.RequestValidationConfiguration>(
            builder.Configuration.GetSection(SMS_Application.Configuration.RequestValidationConfiguration.SectionName));
        builder.Services.AddSingleton(requestValidationConfig);

        // **🔐 TWO-FACTOR AUTHENTICATION CONFIGURATION - Load from appsettings.json**
        var twoFactorConfig = new SMS_Application.Configuration.TwoFactorAuthConfiguration();
        builder.Configuration.GetSection(SMS_Application.Configuration.TwoFactorAuthConfiguration.SectionName).Bind(twoFactorConfig);
        
        // Validate 2FA configuration
        var (is2FAValid, twoFactorErrors) = twoFactorConfig.Validate();
        if (!is2FAValid)
        {
            throw new InvalidOperationException($"Invalid Two-Factor Authentication configuration: {string.Join(", ", twoFactorErrors)}");
        }

        // Register 2FA configuration
        builder.Services.Configure<SMS_Application.Configuration.TwoFactorAuthConfiguration>(
            builder.Configuration.GetSection(SMS_Application.Configuration.TwoFactorAuthConfiguration.SectionName));
        builder.Services.AddSingleton(twoFactorConfig);

        // **🔐 LOG SECURITY CONFIGURATION** (only in development for security)
        if (builder.Environment.IsDevelopment())
        {
            // Create temporary service provider for logging
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var securityFeatureService = serviceProvider.GetRequiredService<ISecurityFeatureService>();
            var securityContext = await securityFeatureService.GetSecurityContextAsync(builder.Environment);
            
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("SecurityConfig");
            logger.LogInformation("🚀 Security Feature Context:");
            logger.LogInformation("  - Environment: {Environment}", securityContext.Environment);
            logger.LogInformation("  - Allow HTTP Cookies: {AllowHttpCookies}", securityContext.AllowHttpCookies);
            logger.LogInformation("  - Allow HTTP in Development: {AllowHttpInDevelopment}", securityContext.AllowHttpInDevelopment);
            logger.LogInformation("  - Cookie Secure Policy: {CookieSecurePolicy}", securityContext.CookieSecurePolicy);
            
            var sessionLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("SessionConfig");
            sessionLogger.LogInformation("🔐 Session Configuration Loaded:");
            sessionLogger.LogInformation("  - Timeout: {TimeoutMinutes} minutes", sessionConfig.TimeoutMinutes);
            sessionLogger.LogInformation("  - Sliding Expiration: {SlidingExpiration}", sessionConfig.SlidingExpiration);
            sessionLogger.LogInformation("  - Secure Cookies: {SecureCookies}", sessionConfig.SecureCookies);
            sessionLogger.LogInformation("  - HttpOnly: {HttpOnly}", sessionConfig.HttpOnly);
            sessionLogger.LogInformation("  - SameSite: {SameSite}", sessionConfig.SameSite);
            sessionLogger.LogInformation("  - Cookie Name: {CookieName}", sessionConfig.CookieName);
            sessionLogger.LogInformation("  - Development Mode: {EnableDevelopmentMode}", sessionConfig.EnableDevelopmentMode);
        }

        // **PRESENTATION LAYER SERVICES - Centralized registration**
        // 🔧 NOTE: AddPresentationServices includes AddHttpContextAccessor() registration
        builder.Services.AddPresentationServices(builder.Configuration);
        builder.Services.AddPresentationMiddleware();
        builder.Services.ConfigurePresentationOptions(builder.Configuration);

        // **🔐 SYSTEM-WIDE SECURE ROUTING SERVICES**
        builder.Services.AddSingleton<ISecureRoutingService, SecureRoutingService>();
                
        // Register SMS Services
        // 🔧 NOTE: AddInfrastructureServices also includes AddHttpContextAccessor() registration
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        var app = builder.Build();
        
        // **🔐 SET UP SERVICE LOCATOR FOR SECURE NAVIGATION - Using existing ServiceLocator**
        SMS3.Components.Shared.UIHelpers.ServiceLocator.Current = app.Services;

        // ⚡ SECURITY MIDDLEWARE - Must be first to add headers to all responses
        app.UseSecurityHeaders();

        // **🔐 REQUEST VALIDATION MIDDLEWARE - Validate and sanitize all requests**
        app.UseMiddleware<SMS3.Middleware.RequestValidationMiddleware>();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            app.UseSMSSwagger();
        }

        // 🚀 Feature-driven HTTPS redirection
        var featureManager = app.Services.GetRequiredService<IFeatureManager>();
        if (await featureManager.IsEnabledAsync(SecurityFeatures.EnforceHttpsRedirection))
        {
            app.UseHttpsRedirection();
        }
        else
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("🚨 HTTPS redirection is DISABLED via feature flag");
        }

        app.UseStaticFiles();
        app.UseRouting();

        // **🔐 AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSession();
        app.UseAntiforgery();

        app.MapPDXSMSApiEndpoints();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}