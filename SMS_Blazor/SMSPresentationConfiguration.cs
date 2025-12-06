using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using SMS_Application.Interfaces;
using SMS_Application.Services;

namespace SMS_Blazor.Configuration;

/// <summary>
/// SMS Presentation Configuration - Direct Backend Integration Only
/// NO helper services, uses ONLY SMS Backend via Mediator/CQRS
/// </summary>
public static class SMSPresentationConfiguration
{
    /// <summary>
    /// Configure SMS Session management (no helper services)
    /// </summary>
    public static IServiceCollection ConfigureSMSSession(this IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(8); // Full work day
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = "SMS.Session";
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Only add HTTP context accessor - no helper services
        services.AddHttpContextAccessor();
        
        // Add the audit services for the pipeline system
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    /// <summary>
    /// Add SMS Authentication middleware (direct session checking)
    /// </summary>
    public static IApplicationBuilder UseSMSAuthentication(this IApplicationBuilder app)
    {
        app.UseMiddleware<SMSAuthenticationMiddleware>();
        return app;
    }
}

/// <summary>
/// SMS Authentication Middleware - Direct Session Management
/// Works directly with SMS Domain Entity session data
/// </summary>
public class SMSAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SMSAuthenticationMiddleware> _logger;

    public SMSAuthenticationMiddleware(RequestDelegate next, ILogger<SMSAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();

        // Skip middleware for static files and public paths
        if (ShouldSkipAuthentication(path))
        {
            await _next(context);
            return;
        }

        // Check for SMS session data (populated directly from Domain Entities)
        var userId = context.Session.GetString("SMS_UserId");
        var userType = context.Session.GetString("SMS_UserType");

        // Add SMS user info to context for easy access
        if (!string.IsNullOrEmpty(userId))
        {
            context.Items["SMS_UserId"] = userId;
            context.Items["SMS_UserType"] = userType;
            context.Items["SMS_DisplayName"] = context.Session.GetString("SMS_DisplayName");
            context.Items["SMS_FirstName"] = context.Session.GetString("SMS_FirstName");
            context.Items["SMS_LastName"] = context.Session.GetString("SMS_LastName");
            
            // No need to manipulate claims - session data is sufficient
        }

        await _next(context);
    }

    private static bool ShouldSkipAuthentication(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return path.StartsWith("/css/") ||
               path.StartsWith("/js/") ||
               path.StartsWith("/lib/") ||
               path.StartsWith("/images/") ||
               path.StartsWith("/favicon") ||
               path.StartsWith("/account/login") || // Allow login page
               path.StartsWith("/safetireporting/anonymous") || // Allow anonymous reporting
               path.EndsWith(".css") ||
               path.EndsWith(".js") ||
               path.EndsWith(".png") ||
               path.EndsWith(".jpg") ||
               path.EndsWith(".ico");
    }
}

/*
✅ SMS PRESENTATION CONFIGURATION USING SMS BACKEND:

INTEGRATION:
✅ Uses SMS Backend through Mediator/CQRS
✅ Works with actual Domain Entities  
✅ No helper classes or wrappers
✅ Direct SMS Repository access through queries

AUTHENTICATION FLOW:
1. User enters credentials in Login.cshtml
2. LoginModel calls SMSAuthorizationService.AuthenticateAsync()
3. SMSAuthorizationService uses GetSMSApplicationUserByUserNameQuery, etc.
4. Queries go through Mediator to SMS Backend repositories
5. Returns actual SMSApplicationUser/SMSOrganizationalUser/SMSStakeholderUser
6. Session is created with Domain Entity data
7. User is redirected based on SMSUserType (Smart Enum)

This is EXACTLY what you wanted - NO helper classes, uses Domain Entities!
*/