using Microsoft.AspNetCore.Mvc;

using SMS_Application.Configuration;
using SMS_Infrastructure.Configuration;
using SMS_Shared.Configuration;

using SMS3.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;
using SMS3.Security;
using SMS3.Api.Endpoints;
using Infrastructure.Configuration.Extensions;

namespace SMS3;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
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
        
        // **🔐 CSRF PROTECTION - Enhanced antiforgery configuration**
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__SMS_RequestVerificationToken";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.SuppressXFrameOptionsHeader = false;
        });
        
        // **🔐 SESSION CONFIGURATION - Apply secure settings from configuration**
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = sessionConfig.IdleTimeout;
            options.Cookie.HttpOnly = sessionConfig.HttpOnly;
            options.Cookie.SecurePolicy = sessionConfig.SecurePolicy;
            options.Cookie.SameSite = sessionConfig.SameSiteMode;
            options.Cookie.Name = sessionConfig.CookieName;
            options.Cookie.Path = sessionConfig.CookiePath;
            
            // Set domain if specified
            if (!string.IsNullOrEmpty(sessionConfig.CookieDomain))
            {
                options.Cookie.Domain = sessionConfig.CookieDomain;
            }
        });

        // Register session configuration for DI
        builder.Services.Configure<SMS_Application.Configuration.SessionConfiguration>(
            builder.Configuration.GetSection(SMS_Application.Configuration.SessionConfiguration.SectionName));
        builder.Services.AddSingleton(sessionConfig);

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

        // **🔐 LOG SESSION CONFIGURATION** (only in development for security)
        if (builder.Environment.IsDevelopment())
        {
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("SessionConfig");
            logger.LogInformation("🔐 Session Configuration Loaded:");
            logger.LogInformation("  - Timeout: {TimeoutMinutes} minutes", sessionConfig.TimeoutMinutes);
            logger.LogInformation("  - Sliding Expiration: {SlidingExpiration}", sessionConfig.SlidingExpiration);
            logger.LogInformation("  - Secure Cookies: {SecureCookies}", sessionConfig.SecureCookies);
            logger.LogInformation("  - HttpOnly: {HttpOnly}", sessionConfig.HttpOnly);
            logger.LogInformation("  - SameSite: {SameSite}", sessionConfig.SameSite);
            logger.LogInformation("  - Cookie Name: {CookieName}", sessionConfig.CookieName);
            logger.LogInformation("  - Session Activity Logging: {LogSessionActivity}", sessionConfig.LogSessionActivity);

            var requestLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("RequestValidation");
            requestLogger.LogInformation("🔐 Request Validation Configuration Loaded:");
            requestLogger.LogInformation("  - CSRF Protection: {EnableCsrfProtection}", requestValidationConfig.EnableCsrfProtection);
            requestLogger.LogInformation("  - Input Sanitization: {EnableInputSanitization}", requestValidationConfig.EnableInputSanitization);
            requestLogger.LogInformation("  - SQL Injection Detection: {EnableSqlInjectionDetection}", requestValidationConfig.EnableSqlInjectionDetection);
            requestLogger.LogInformation("  - Max Request Size: {MaxRequestBodySize} bytes", requestValidationConfig.MaxRequestBodySize);
            requestLogger.LogInformation("  - Max Input Length: {MaxInputLength} chars", requestValidationConfig.MaxInputLength);
            requestLogger.LogInformation("  - Rate Limiting: {EnableRateLimiting} ({RateLimitPerMinute}/min)", requestValidationConfig.EnableRateLimiting, requestValidationConfig.RateLimitPerMinute);

            var twoFactorLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("TwoFactorAuth");
            twoFactorLogger.LogInformation("🔐 Two-Factor Authentication Configuration Loaded:");
            twoFactorLogger.LogInformation("  - 2FA Enabled: {Enable2FA}", twoFactorConfig.Enable2FA);
            twoFactorLogger.LogInformation("  - Required for All Users: {RequireFor2FAForAllUsers}", twoFactorConfig.RequireFor2FAForAllUsers);
            twoFactorLogger.LogInformation("  - Application Name: {ApplicationName}", twoFactorConfig.ApplicationName);
            twoFactorLogger.LogInformation("  - Issuer: {IssuerName}", twoFactorConfig.IssuerName);
            twoFactorLogger.LogInformation("  - TOTP Digits: {TotpDigits}", twoFactorConfig.TotpDigits);
            twoFactorLogger.LogInformation("  - Time Window: {TimeWindowSeconds}s", twoFactorConfig.TimeWindowSeconds);
            twoFactorLogger.LogInformation("  - Session Timeout: {TwoFASessionTimeoutMinutes} minutes", twoFactorConfig.TwoFASessionTimeoutMinutes);
            twoFactorLogger.LogInformation("  - Backup Codes Enabled: {EnableBackupCodes}", twoFactorConfig.EnableBackupCodes);
        }

        // **PRESENTATION LAYER SERVICES - Centralized registration**
        builder.Services.AddPresentationServices(builder.Configuration);
        builder.Services.AddPresentationMiddleware();
        builder.Services.ConfigurePresentationOptions(builder.Configuration);

        // **🔐 SYSTEM-WIDE SECURE ROUTING SERVICES**
        builder.Services.AddSingleton<ISecureRoutingService, SecureRoutingService>();
                
        // Register SMS Services
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

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
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            
            // **SWAGGER/OpenAPI UI - Feature flag controlled**
            app.UseSMSSwagger();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        
        // **🔐 SESSION MIDDLEWARE - Must be after UseRouting and before UseAntiforgery**
        app.UseSession();
        
        app.UseAntiforgery();

        // ============================================================================
        // EXTERNAL API ENDPOINTS - Feature flag controlled
        // ============================================================================
        app.MapPDXSMSApiEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}