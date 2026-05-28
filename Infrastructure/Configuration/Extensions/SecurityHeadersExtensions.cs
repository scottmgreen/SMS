//-----------------------------------------------------------------------
// <copyright file="SecurityHeadersExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Extension methods for configuring security headers middleware in ASP.NET Core applications.
//                  Provides convenient configuration methods for security middleware
//                  following ASP.NET Core conventions.
// </copyright>
//-----------------------------------------------------------------------


//-----------------------------------------------------------------------
// <copyright file="SecurityHeadersExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Extension methods for configuring security headers middleware in ASP.NET Core applications.
//                  Provides convenient configuration methods for security middleware
//                  following ASP.NET Core conventions.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Infrastructure.Configuration.Middleware;

using Microsoft.AspNetCore.Builder;

namespace SMS_Infrastructure.Configuration.Extensions
{
    /// <summary>
    /// Extension methods for configuring security headers middleware
    /// </summary>
    public static class SecurityHeadersExtensions
    {
        /// <summary>
        /// Adds security headers middleware to the application pipeline
        /// This middleware adds essential security headers to protect against common web vulnerabilities
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        /// <summary>
        /// Adds security headers middleware with logging information
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="logSecurityHeaders">Whether to log when security headers are applied (default: false for performance)</param>
        /// <returns>The application builder for method chaining</returns>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, bool logSecurityHeaders = false)
        {
            if (logSecurityHeaders)
            {
                var logger = app.ApplicationServices.GetService<ILogger<SecurityHeadersMiddleware>>();
                logger?.LogInfrastructureInformation("Security headers middleware enabled with logging");
            }

            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
