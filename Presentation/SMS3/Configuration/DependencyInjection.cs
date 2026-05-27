using Microsoft.FeatureManagement;
using Microsoft.OpenApi;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Events;

using SMS_Infrastructure.Security;

using SMS3.Api.Extensions;
using SMS3.Components.Shared.UIHelpers;
using SMS3.EventHandlers;

using SMS3.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;

namespace SMS3.Configuration;

/// <summary>
/// SMS Presentation Configuration 
/// Clean architecture with secure session management and proper user isolation
/// </summary>

/*
✅ SMS PRESENTATION CONFIGURATION - SESSION-BASED AUTHENTICATION APPROACH:

The SMS application now uses secure session-based authentication instead of static storage:

SESSION-BASED AUTHENTICATION FLOW:
1. User enters credentials in Login.razor
2. AuthenticationService validates credentials using CQRS queries
3. SMSSessionService.CreateSMSSessionAsync() stores user data in secure session
4. SessionCurrentUserService reads user data from session per request
5. NavMenu and authorization checks use ICurrentUserService (SessionCurrentUserService)
6. Logout calls CurrentUserService.ClearAuthentication() and SessionService.ClearSMSSessionAsync()

SECURITY BENEFITS:
✅ Proper per-user session isolation - eliminates static field vulnerabilities
✅ Automatic session timeout and cleanup - enhanced security
✅ HttpContext-based authentication - follows ASP.NET Core best practices
✅ Session encryption and secure cookies - data protection
✅ Same interface and functionality as static version - zero breaking changes
✅ Uses existing SMSSessionService serialization - no new dependencies

TECHNICAL IMPLEMENTATION:
- SessionCurrentUserService implements identical ICurrentUserService interface
- Uses existing SMSSessionService for session creation (already working)
- Permission reconstruction uses proven logic from AuthorizationService
- Maintains all existing business logic and permission checking
- Graceful fallback handling for session unavailability

This approach provides enterprise-grade security while maintaining 
full compatibility with existing authentication and authorization functionality.
*/

/// <summary>
/// Extension methods for configuring SMS Presentation layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all presentation layer services including UI helpers, authentication, APIs, and documentation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration for feature-dependent services</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ===========================================================================
        // UI HELPER SERVICES - Blazor component support services
        // ===========================================================================
        services.AddUIHelperServices();

        // ===========================================================================
        // AUTHENTICATION SERVICES - Session-based authentication approach
        // ===========================================================================
        services.AddSessionAuthenticationServices();

        //// ===========================================================================
        //// API SERVICES - External integration services
        //// ===========================================================================
        //services.AddSMSApiServices();

        //// ===========================================================================
        //// SWAGGER/OpenAPI DOCUMENTATION - Centralized API documentation
        //// ===========================================================================
        //services.AddSMSSwaggerServices(configuration);

        
        return services;
    }

    /// <summary>
    /// Registers UI helper services for Blazor components
    /// </summary>
    private static IServiceCollection AddUIHelperServices(this IServiceCollection services)
    {
        // Notification system with feature management integration
        services.AddScoped<INotificationHelper, NotificationHelper>();

        // EventBus UI Event Handlers
        services.AddScoped<UIEventHandler>();

        // Add any additional UI helper services here
        // services.AddScoped<IDialogService, DialogService>();
        // services.AddScoped<IToastService, ToastService>();

        return services;
    }

    /// <summary>
    /// Registers session-based authentication services (secure session-based authentication)
    /// REPLACES static authentication approach with proper session isolation
    /// INCLUDES fallback mechanism for startup scenarios
    /// </summary>
    private static IServiceCollection AddSessionAuthenticationServices(this IServiceCollection services)
    {
        // 🔐 SESSION-BASED AUTHENTICATION - Secure per-user session isolation
        services.AddScoped<ISMSSessionService, SMSSessionService>();
        
        // 🔧 TEMPORARY: Add both services for safe rollback during startup issues
        services.AddScoped<StaticCurrentUserService>();
        services.AddScoped<SessionCurrentUserService>();
        
        // 🚀 PERFORMANCE: Add authentication state caching
        services.AddScoped<IAuthenticationStateCache, AuthenticationStateCache>();
        
        // 🔧 DISABLED: Strategy-based authentication is now handled in Program.cs
        // Register the session-based version as the primary implementation
        // services.AddScoped<ICurrentUserService>(provider => 
        //     provider.GetRequiredService<SessionCurrentUserService>());

        // 🔐 SESSION TIMER SERVICE - For session timeout management
        services.AddScoped<SessionTimerService>();

        // 🔐 TWO-FACTOR AUTHENTICATION SERVICES - TOTP and Microsoft Authenticator integration
        services.AddScoped<TwoFactorAuthService>();

        // Add any additional authentication-related services here
        // services.AddScoped<IAuthorizationService, AuthorizationService>();
        // services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    
    /// <summary>
    /// Registers API security services
    /// </summary>
    

    /// <summary>
    /// Registers UI Event Handlers for the EventBus (Presentation Layer)
    /// Maintains Clean Architecture by keeping UI handler registration in Presentation layer
    /// </summary>
    //public static IApplicationBuilder InitializeUIEventHandlers(this IApplicationBuilder app)
    //{
    //    try
    //    {
    //        using var scope = app.ApplicationServices.CreateScope();
    //        var eventBus = scope.ServiceProvider.GetRequiredService<IBaseEventBus>();
    //        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBaseEventBus>>();

    //        logger.LogInformation("Registering UI Event Handlers (Presentation Layer)...");

    //        // Register UI notification handler (local to SMS3 project)
    //        eventBus.SubscribeUI<UINotificationEvent, UIEventHandler>();
    //        logger.LogInformation("Registered UINotificationEventHandler for UINotificationEvent");

    //        // TODO: Register additional UI event handlers as they're implemented
    //        // eventBus.SubscribeUI<UserPreferenceChangedEvent, UserPreferenceChangedEventHandler>();
    //        // eventBus.SubscribeUI<ThemeChangedEvent, ThemeChangedEventHandler>();
    //        // eventBus.SubscribeUI<DashboardRefreshEvent, DashboardRefreshEventHandler>();

    //        logger.LogInformation("UI event handler registration completed (Presentation Layer)");

    //        return app;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Use a basic logger if dependency injection logger fails
    //        var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
    //        var logger = loggerFactory?.CreateLogger("EventBus.UI.Initialization") ?? 
    //                    Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    //        logger.LogError(ex, "❌ Failed to initialize UI EventBus subscriptions");
    //        throw; // Re-throw to prevent silent failures during startup
    //    }
    //}

    /// <summary>
    /// Automatically subscribes all IBaseEventHandler<T> implementations to the event bus at startup.
    /// </summary>
    /// <param name="app">The application builder</param>
    


}