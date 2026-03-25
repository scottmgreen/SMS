using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

using SMS_Application.Interfaces;
using SMS_Application.Services;

using SMS_Infrastructure.Security;

using SMS3.Api.Extensions;
using SMS3.Components.Shared.UIHelpers;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace SMS3.Configuration;

/// <summary>
/// SMS Presentation Configuration - Session-Based Authentication Approach
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
        public static IServiceCollection AddSMSSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                ConfigureSwaggerDocument(options);
                ConfigureApiKeySecurity(options);
                ConfigureXmlDocumentation(options);
                ConfigureServers(options, configuration);
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
            // Check if Swagger should be enabled
            if (app.Environment.IsDevelopment() || IsSwaggerEnabledInProduction(app))
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "api-docs/{documentname}/swagger.json";
                });

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/api-docs/v1/swagger.json", "SMS External Reporting API v1");
                    options.RoutePrefix = "api-docs";
                    options.DocumentTitle = "SMS External Reporting API Documentation";

                    // Security-focused UI configuration
                    options.DefaultModelsExpandDepth(-1); // Don't expand models by default
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                    options.EnableFilter();
                    options.EnableDeepLinking();

                    // Custom CSS for branding (optional)
                    options.InjectStylesheet("/css/swagger-custom.css");
                });
            }

            return app;
        }

        #region Private Configuration Methods

        private static void ConfigureSwaggerDocument(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SMS External Reporting API",
                Version = "v1.0",
                Description = BuildApiDescription()
                //Contact = new OpenApiContact
                //{
                //    Name = "SMS Support Team",
                //    Email = "sms-support@flypdx.com",
                //    Url = new Uri("https://flypdx.com/sms-support")
                //},
                //License = new OpenApiLicense
                //{
                //    Name = "Port of Portland - Internal Use Only",
                //    Url = new Uri("https://flypdx.com/terms")
                //}
            });
        }

        private static void ConfigureApiKeySecurity(SwaggerGenOptions options)
        {
            // API Key authentication scheme
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key authentication. Provide your API key in the X-API-Key header.",
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            // Apply security requirement globally
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
        }

        private static void ConfigureXmlDocumentation(SwaggerGenOptions options)
        {
            // Include XML comments from API assemblies
            var xmlFiles = new[]
            {
                "SMS3.xml",           // Main presentation layer
                "Application.xml",    // Application layer
                "Domain.xml"          // Domain layer
            };

            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
        }

        private static void ConfigureServers(SwaggerGenOptions options, IConfiguration configuration)
        {
            // Always use servers from appsettings.json configuration
            var configuredServers = configuration.GetSection("Swagger:Servers").Get<List<ApiServer>>();
            
            if (configuredServers?.Any() == true)
            {
                // Use servers from appsettings.json
                foreach (var server in configuredServers)
                {
                    options.AddServer(new OpenApiServer
                    {
                        Url = server.Url,
                        Description = server.Description
                    });
                }
            }
            
            // ALWAYS add a relative path server as primary option
            options.AddServer(new OpenApiServer
            {
                Url = "",
                Description = "Current Host (Relative Path)"
            });
        }

        private static string BuildApiDescription()
        {
            return @"
## SMS External Reporting API

This API provides secure endpoints for external systems to submit safety reports to the Port of Portland SMS system.

### Features
- ?? **API Key Authentication** - Secure access control
- ?? **Comprehensive Validation** - Request validation and error handling
- ?? **File Attachments** - Support for document uploads
- ?? **Location Data** - Geographic coordinate support
- ?? **Reference Data** - Hazard categories and types

### Getting Started
1. Obtain an API key from the SMS administrator
2. Include the API key in the `X-API-Key` header
3. Submit reports using the `/api/pdxsms` endpoint

### Rate Limiting
- 10 requests per minute per API key
- Larger files may require additional time

### Support
For technical support or API key requests, contact the SMS team.";
        }

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

        #region Supporting Types

        /// <summary>
        /// Configuration model for additional API servers
        /// </summary>
        public class ApiServer
        {
            public string Url { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        #endregion
    //}





}