//-----------------------------------------------------------------------
// <copyright file="ApiServicesExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Dependency injection extensions for API services with clean service registration.
//                  Provides centralized service registration for API-related services
//                  following dependency injection best practices.
// </copyright>
//-----------------------------------------------------------------------

using SMS3.Api.Services;

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
        public static IServiceCollection AddPDXSMSApiServices(this IServiceCollection services)
        {
            // Register API business logic services
            services.AddScoped<IPDXSMSApiService, PDXSMSApiService>();
            
            // Add any additional API-related services here
            // services.AddScoped<IApiValidationService, ApiValidationService>();
            // services.AddScoped<IApiSecurityService, ApiSecurityService>();
            
            return services;
        }
    }
}