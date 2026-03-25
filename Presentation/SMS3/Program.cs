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
using SMS_Application.Interfaces;

namespace SMS3;
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 🔧 ENSURE LOGS DIRECTORY EXISTS - Simple directory creation
        try
        {
            var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            Directory.CreateDirectory(logsDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create logs directory: {ex.Message}");
        }

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

        // **🎯 EXPLICIT PROTOCOL CONFIGURATION - NO SQUISHY AUTO-DETECTION**
        var masterProtocolConfig = builder.Configuration.GetSection("MasterProtocol");
        var explicitProtocol = masterProtocolConfig.GetValue<string>("Protocol", "HTTP");
        var forceEverywhere = masterProtocolConfig.GetValue<bool>("ForceProtocolEverywhere", true);
        var allowMixed = masterProtocolConfig.GetValue<bool>("AllowMixedMode", false);

        // Determine explicit settings based on protocol
        var isHttps = explicitProtocol.Equals("HTTPS", StringComparison.OrdinalIgnoreCase);
        var secureCookies = isHttps && forceEverywhere;
        var strictSameSite = isHttps ? SameSiteMode.Strict : SameSiteMode.Lax;

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
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__SMS_RequestVerificationToken";
            options.Cookie.HttpOnly = true;
            
            // 🎯 EXPLICIT: Use protocol-based settings
            options.Cookie.SecurePolicy = secureCookies ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
            options.Cookie.SameSite = strictSameSite;
            options.SuppressXFrameOptionsHeader = false;
        });
        
        // **🔐 EXPLICIT SESSION CONFIGURATION - Uses protocol settings directly**
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = sessionConfig.IdleTimeout;
            options.Cookie.HttpOnly = sessionConfig.HttpOnly;
            
            // 🎯 EXPLICIT: Use protocol-based settings
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

        // 🎯 NEW: AUTHENTICATION STRATEGY CONFIGURATION - Load from appsettings.json
        var authConfig = new SMS_Application.Configuration.AuthenticationConfiguration();
        builder.Configuration.GetSection(SMS_Application.Configuration.AuthenticationConfiguration.SectionName).Bind(authConfig);
        
        // Validate authentication configuration
        var (isAuthConfigValid, authConfigErrors) = authConfig.Validate();
        if (!isAuthConfigValid)
        {
            throw new InvalidOperationException($"Invalid authentication configuration: {string.Join(", ", authConfigErrors)}");
        }

        // Register authentication configuration
        builder.Services.Configure<SMS_Application.Configuration.AuthenticationConfiguration>(
            builder.Configuration.GetSection(SMS_Application.Configuration.AuthenticationConfiguration.SectionName));
        builder.Services.AddSingleton(authConfig);

        // 🎯 NEW: AUTHENTICATION STRATEGY SERVICES - Clean Architecture Implementation
        
        // Protocol detection and compatibility services
        builder.Services.AddScoped<SMS_Application.Interfaces.IProtocolDetectionService, SMS_Application.Services.ProtocolDetectionService>();
        
        // User validation and instantiation services (CQRS-based)
        builder.Services.AddScoped<SMS_Application.Interfaces.IUserCompletenessValidator, SMS_Application.Services.UserCompletenessValidator>();
        builder.Services.AddScoped<SMS_Application.Interfaces.IUserInstantiationService, SMS_Application.Services.UserInstantiationService>();
        
        // Authentication strategy implementations
        builder.Services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, SMS_Application.Services.Authentication.SessionBasedAuthenticationStrategy>();
        builder.Services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, SMS_Application.Services.Authentication.CircuitBasedAuthenticationStrategy>();
        builder.Services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategy, SMS_Application.Services.Authentication.ContextBasedAuthenticationStrategy>();
        
        // Authentication strategy manager (orchestrator)
        builder.Services.AddScoped<SMS_Application.Interfaces.IAuthenticationStrategyManager, SMS_Application.Services.AuthenticationStrategyManager>();

        // 🔧 CRITICAL FIX: Register Strategy-Based Current User Service for NavMenu
        builder.Services.AddScoped<SMS_Application.Interfaces.ICurrentUserService, SMS_Application.Services.StrategyBasedCurrentUserService>();

        // **🔐 LOG SECURITY CONFIGURATION** (only in development for security)
        if (builder.Environment.IsDevelopment())
        {
            // Security configuration logging disabled to reduce console noise
            // Enable these logs in appsettings.Development.json if needed for debugging
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
        // 🎯 CONDITIONAL: Only apply security headers if enabled in configuration
        if (isHttps || app.Configuration.GetValue<bool>("FeatureManagement:EnableSecurityHeaders", false))
        {
            app.UseSecurityHeaders();
        }

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

        // 🎯 EXPLICIT HTTPS redirection based on protocol configuration
        if (isHttps && forceEverywhere)
        {
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        
        // **🔐 CRITICAL FIX: SESSION MUST BE BEFORE ROUTING AND AUTHENTICATION**
        app.UseSession();
        
        app.UseRouting();

        // **🔐 AUTHENTICATION & AUTHORIZATION MIDDLEWARE**
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapPDXSMSApiEndpoints();
        app.MapAuthenticationStatusEndpoints(); // 🎯 NEW: Development diagnostics endpoints
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}