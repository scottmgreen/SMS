//-----------------------------------------------------------------------
// <copyright file="ApiServicesExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Dependency injection extensions for API services with clean service registration.
//                  Provides centralized service registration for API-related services
//                  following dependency injection best practices.
// </copyright>
//-----------------------------------------------------------------------

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

using SMS_Infrastructure.Security;

using SMS3.Api.Endpoints;
using SMS3.Api.Services;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace SMS3.Api.Extensions
{
    /// <summary>
    /// Extension methods for registering API services
    /// </summary>
    public static class ApiServicesExtensions
    {
        /// <summary>
        /// Registers PDXSMS API services with the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddSMSApiServices(this IServiceCollection services)
        {
            // Register API business logic services
            services.AddScoped<IPDXSMSApiService, PDXSMSApiService>();
            services.AddScoped<ApiKeyAuthenticationFilter>();

            return services;
        }

        public static IServiceCollection AddSMSApiVersioning(this IServiceCollection services)
        {
            // Add API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0); // Default to v1.0
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // Adds API version headers to responses
            }
            ).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // e.g., v1, v2
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
        public static IServiceCollection AddSMSApiSecurityServices(this IServiceCollection services)
        {
            // API key authentication for external endpoints
            services.AddScoped<ApiKeyAuthenticationFilter>();

            // Add any additional API security services here
            // services.AddScoped<IRateLimitingService, RateLimitingService>();
            // services.AddScoped<IApiAuditService, ApiAuditService>();

            return services;
        }
        public static bool IsSwaggerEnabled(WebApplication app)
        {
            // Check if Swagger is explicitly enabled via feature flag OR configuration
            var featureManager = app.Services.GetService<IFeatureManager>();
            var api_featureEnabled = featureManager?.IsEnabledAsync("ExternalApiEnabled").GetAwaiter().GetResult() ?? false;
            var swagger_featureEnabled = featureManager?.IsEnabledAsync("SwaggerEnabled").GetAwaiter().GetResult() ?? false;

            // Also check the configuration setting
            var configEnabled = app.Configuration.GetValue<bool>("Swagger:EnableInProduction", false);

            return api_featureEnabled && swagger_featureEnabled; // || configEnabled;
        }
        public static IServiceCollection AddSMSSwaggerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SMS External Reporting API", Version = "v1" });
                options.SwaggerDoc("v2", new OpenApiInfo { Title = "SMS External Reporting API", Version = "v2" });

                // Use a predicate to control which endpoints are included in each document
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    //if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

                    //var groupName = apiDesc.GroupName;
                    //return groupName == docName;
                    if (!string.IsNullOrEmpty(apiDesc.GroupName))
                    {
                        return apiDesc.GroupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
                    }

                    // Fallback if no group metadata is detected
                    return docName.Equals("v1", StringComparison.OrdinalIgnoreCase);
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

        public static WebApplication UseSMSSwagger(this WebApplication app)
        {
            var v1ApiVersionSet = app.NewApiVersionSet()
                 .HasApiVersion(new ApiVersion(1, 0))
                 .ReportApiVersions()
                 .Build();

            var v2ApiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            // Register endpoints for each version
            app.MapPDXSMSApiEndpointsV1(v1ApiVersionSet);
            app.MapPDXSMSApiEndpointsV2(v2ApiVersionSet);

            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                //options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
                //options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2");
                options.SwaggerEndpoint("../swagger/v1/swagger.json", "V1");
                options.SwaggerEndpoint("../swagger/v2/swagger.json", "V2");
                options.RoutePrefix = "api-docs";
                options.DocumentTitle = "SMS External Reporting API Documentation";
                
            });
            //}
            // Endpoint registration is now handled in Program.cs with version sets
            return app;
        }

    }
}