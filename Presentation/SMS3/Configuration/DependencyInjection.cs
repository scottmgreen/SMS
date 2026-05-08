using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Events.UIEvents;

using SMS_Infrastructure.Security;

using SMS3.Api.Extensions;
using SMS3.Components.Shared.UIHelpers;
using SMS3.EventHandlers;

using Swashbuckle.AspNetCore.SwaggerGen;
using SMS3.Api.Endpoints;

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
4. SessionBasedCurrentUserService reads user data from session per request
5. NavMenu and authorization checks use ICurrentUserService (SessionBasedCurrentUserService)
6. Logout calls CurrentUserService.ClearAuthentication() and SessionService.ClearSMSSessionAsync()

SECURITY BENEFITS:
✅ Proper per-user session isolation - eliminates static field vulnerabilities
✅ Automatic session timeout and cleanup - enhanced security
✅ HttpContext-based authentication - follows ASP.NET Core best practices
✅ Session encryption and secure cookies - data protection
✅ Same interface and functionality as static version - zero breaking changes
✅ Uses existing SMSSessionService serialization - no new dependencies

TECHNICAL IMPLEMENTATION:
- SessionBasedCurrentUserService implements identical ICurrentUserService interface
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
        services.AddSessionBasedAuthenticationServices();

        // ===========================================================================
        // API SERVICES - External integration services
        // ===========================================================================
        services.AddPDXSMSApiServices();

        // ===========================================================================
        // SWAGGER/OpenAPI DOCUMENTATION - Centralized API documentation
        // ===========================================================================
        services.AddSMSSwaggerServices(configuration);

        // ===========================================================================
        // SECURITY SERVICES - API authentication and filtering
        // ===========================================================================
        services.AddApiSecurityServices();

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
    private static IServiceCollection AddSessionBasedAuthenticationServices(this IServiceCollection services)
    {
        // 🔐 SESSION-BASED AUTHENTICATION - Secure per-user session isolation
        services.AddScoped<ISMSSessionService, SMSSessionService>();
        
        // 🔧 TEMPORARY: Add both services for safe rollback during startup issues
        services.AddScoped<StaticCurrentUserService>();
        services.AddScoped<SessionBasedCurrentUserService>();
        
        // 🚀 PERFORMANCE: Add authentication state caching
        services.AddScoped<IAuthenticationStateCache, AuthenticationStateCache>();
        
        // 🔧 DISABLED: Strategy-based authentication is now handled in Program.cs
        // Register the session-based version as the primary implementation
        // services.AddScoped<ICurrentUserService>(provider => 
        //     provider.GetRequiredService<SessionBasedCurrentUserService>());

        // 🔐 SESSION TIMER SERVICE - For session timeout management
        services.AddScoped<SessionTimerService>();

        // 🔐 TWO-FACTOR AUTHENTICATION SERVICES - TOTP and Microsoft Authenticator integration
        services.AddScoped<TwoFactorAuthService>();

        // 🔐 REQUEST VALIDATION SERVICES - Input validation and security
        services.AddRequestValidationServices();

        // Add any additional authentication-related services here
        // services.AddScoped<IAuthorizationService, AuthorizationService>();
        // services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }

    /// <summary>
    /// Registers request validation and security services
    /// </summary>
    private static IServiceCollection AddRequestValidationServices(this IServiceCollection services)
    {
        // Request validation configuration will be registered in Program.cs
        // No additional services needed for basic validation
        
        return services;
    }

    /// <summary>
    /// Registers API security services
    /// </summary>
    private static IServiceCollection AddApiSecurityServices(this IServiceCollection services)
    {
        // API key authentication for external endpoints
        services.AddScoped<ApiKeyAuthenticationFilter>();

        // Add any additional API security services here
        // services.AddScoped<IRateLimitingService, RateLimitingService>();
        // services.AddScoped<IApiAuditService, ApiAuditService>();

        return services;
    }

    /// <summary>
    /// Registers UI Event Handlers for the EventBus (Presentation Layer)
    /// Maintains Clean Architecture by keeping UI handler registration in Presentation layer
    /// </summary>
    public static IApplicationBuilder InitializeUIEventHandlers(this IApplicationBuilder app)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IBaseEventBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBaseEventBus>>();

            logger.LogInformation("🔔 Registering UI Event Handlers (Presentation Layer)...");

            // Register UI notification handler (local to SMS3 project)
            eventBus.SubscribeUI<UINotificationEvent, UIEventHandler>();
            logger.LogInformation("✅ Registered UINotificationEventHandler for UINotificationEvent");

            // TODO: Register additional UI event handlers as they're implemented
            // eventBus.SubscribeUI<UserPreferenceChangedEvent, UserPreferenceChangedEventHandler>();
            // eventBus.SubscribeUI<ThemeChangedEvent, ThemeChangedEventHandler>();
            // eventBus.SubscribeUI<DashboardRefreshEvent, DashboardRefreshEventHandler>();

            logger.LogInformation("✅ UI event handler registration completed (Presentation Layer)");

            return app;
        }
        catch (Exception ex)
        {
            // Use a basic logger if dependency injection logger fails
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("EventBus.UI.Initialization") ?? 
                        Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            logger.LogError(ex, "❌ Failed to initialize UI EventBus subscriptions");
            throw; // Re-throw to prevent silent failures during startup
        }
    }

    /// <summary>
    /// Registers presentation-specific middleware services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddPresentationMiddleware(this IServiceCollection services)
    {
        // 🔧 NOTE: HttpContextAccessor is already registered in Infrastructure layer
        // Removed duplicate registration to follow DI best practices
        
        // Add any middleware-specific services here
        // services.AddScoped<IRequestLoggingService, RequestLoggingService>();

        return services;
    }

    /// <summary>
    /// Configures presentation layer options and settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection ConfigurePresentationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure IIS options
        services.Configure<IISServerOptions>(options =>
        {
            options.AutomaticAuthentication = false;
            options.AllowSynchronousIO = true;
        });

        // Configure Radzen components (if using specific options)
        // services.Configure<RadzenOptions>(options => { ... });

        return services;
    }


    //public static class SwaggerConfiguration
    //{
        /// <summary>
        /// Adds Swagger services with comprehensive configuration for SMS APIs
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        /// <returns>Service collection for method chaining</returns>
        // No-op. Swagger configuration is now handled by ConfigureSwaggerOptions
        public static IServiceCollection AddSMSSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SMS External Reporting API", Version = "v1" });
                options.SwaggerDoc("v2", new OpenApiInfo { Title = "SMS External Reporting API", Version = "v2" });

                // Use a predicate to control which endpoints are included in each document
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

                    var groupName = apiDesc.GroupName;
                    return groupName == docName;
                });

                // Configure API Key Security
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key authentication. Provide your API key in the X-API-Key header.",
                    In = ParameterLocation.Header,
                    Name = "X-API-Key",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKeyScheme"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            },
                            Scheme = "ApiKeyScheme",
                            Name = "X-API-Key",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Configure XML Documentation
                var xmlFiles = new[]
                {
                    "SMS3.xml",
                    "Application.xml",
                    "Domain.xml"
                };

                foreach (var xmlFile in xmlFiles)
                {
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                }
            });
            return services;
        }

        /// <summary>
        /// Configures Swagger UI middleware with feature flag support
        /// </summary>
        /// <param name="app">Web application</param>
        /// <returns>Web application for method chaining</returns>
        public static WebApplication UseSMSSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || IsSwaggerEnabledInProduction(app))
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                    options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2");
                    options.RoutePrefix = "api-docs";
                    options.DocumentTitle = "SMS External Reporting API Documentation";
                });
            }
        // Endpoint registration is now handled in Program.cs with version sets
        return app;
        }

        #region Private Configuration Methods

        private static bool IsSwaggerEnabledInProduction(WebApplication app)
        {
            // Check if Swagger is explicitly enabled via feature flag OR configuration
            var featureManager = app.Services.GetService<IFeatureManager>();
            var featureEnabled = featureManager?.IsEnabledAsync("SwaggerEnabled").GetAwaiter().GetResult() ?? false;

            // Also check the configuration setting
            var configEnabled = app.Configuration.GetValue<bool>("Swagger:EnableInProduction", false);

            return featureEnabled || configEnabled;
        }

    #endregion
    //}





}