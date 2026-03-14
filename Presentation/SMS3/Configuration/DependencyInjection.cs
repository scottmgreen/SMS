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
/// SMS Presentation Configuration - Static Authentication Approach
/// Clean architecture with no session or HttpContext dependencies
/// </summary>

/*
✅ SMS PRESENTATION CONFIGURATION - STATIC AUTHENTICATION APPROACH:

The SMS application now uses static authentication storage instead of sessions:

STATIC AUTHENTICATION FLOW:
1. User enters credentials in Login.razor
2. AuthenticationService validates credentials using CQRS queries
3. StaticCurrentUserService.SetAuthenticationState() stores user data in static fields
4. NavMenu and authorization checks use ICurrentUserService (StaticCurrentUserService)
5. Logout calls StaticCurrentUserService.ClearAuthenticationState() and navigates to home

BENEFITS:
✅ No HttpContext dependencies - works in any deployment environment
✅ No session configuration required - bypasses IIS session issues
✅ No cookie encryption problems - no cookies needed
✅ Direct integration with SMS Backend via Mediator/CQRS
✅ Uses actual Domain Entities without wrapper classes
✅ Clean separation between business logic and infrastructure concerns

NOTIFICATION SETTINGS:
NotificationSettings is configured in SMS_Shared.Configuration.DependencyInjection
and used throughout the presentation layer for UI notifications.

This approach eliminates deployment environment issues while maintaining 
full authentication and authorization functionality.
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
        // AUTHENTICATION SERVICES - Static authentication approach
        // ===========================================================================
        services.AddStaticAuthenticationServices();

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
    /// Registers static authentication services (no HttpContext/Session dependencies)
    /// </summary>
    private static IServiceCollection AddStaticAuthenticationServices(this IServiceCollection services)
    {
        // Static authentication approach - eliminates deployment environment issues
        services.AddScoped<ISMSSessionService, SMSSessionService>();
        services.AddScoped<ICurrentUserService, StaticCurrentUserService>();

        // Add any additional authentication-related services here
        // services.AddScoped<IAuthorizationService, AuthorizationService>();
        // services.AddScoped<IPermissionService, PermissionService>();

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
        // HTTP context accessor for static services
        services.AddHttpContextAccessor();

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
            // Only enable Swagger in development or if explicitly enabled
            if (app.Environment.IsDevelopment() || IsSwaggerEnabledInProduction(app))
            {
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "api-docs/{documentname}/swagger.json";
                });

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/api-docs/v1/swagger.json", "SMS Confidential Reporting API v1");
                    options.RoutePrefix = "api-docs";
                    options.DocumentTitle = "SMS Safety Management API Documentation";

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
                Title = "SMS Confidential Reporting API",
                Version = "v1.0",
                Description = BuildApiDescription(),
                Contact = new OpenApiContact
                {
                    Name = "SMS Support Team",
                    Email = "sms-support@flypdx.com",
                    Url = new Uri("https://flypdx.com/sms-support")
                },
                License = new OpenApiLicense
                {
                    Name = "Port of Portland - Internal Use Only",
                    Url = new Uri("https://flypdx.com/terms")
                }
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
            // Add server URLs for different environments
            options.AddServer(new OpenApiServer
            {
                Url = "https://localhost:7178",
                Description = "Development Server"
            });

            // Add additional servers from configuration
            var servers = configuration.GetSection("Swagger:Servers").Get<List<ApiServer>>();
            if (servers?.Any() == true)
            {
                foreach (var server in servers)
                {
                    options.AddServer(new OpenApiServer
                    {
                        Url = server.Url,
                        Description = server.Description
                    });
                }
            }
        }

        private static string BuildApiDescription()
        {
            return @"
## SMS Confidential Reporting API

This API provides secure endpoints for external systems to submit confidential safety reports to the Port of Portland SMS system.

### Features
- ?? **API Key Authentication** - Secure access control
- ?? **Comprehensive Validation** - Request validation and error handling
- ?? **File Attachments** - Support for document uploads
- ??? **Location Data** - Geographic coordinate support
- ?? **Reference Data** - Hazard categories and types

### Getting Started
1. Obtain an API key from the SMS administrator
2. Include the API key in the `X-API-Key` header
3. Submit reports using the `/api/pdxsms` endpoint

### Rate Limiting
- 10 requests per minute per API key
- Larger files may require additional time

### Support
For technical support or API key requests, contact the SMS team.
            ";
        }

        private static bool IsSwaggerEnabledInProduction(WebApplication app)
        {
            // Check if Swagger is explicitly enabled in production via feature flag
            var featureManager = app.Services.GetService<IFeatureManager>();
            return featureManager?.IsEnabledAsync("SwaggerInProduction").GetAwaiter().GetResult() ?? false;
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