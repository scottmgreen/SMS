//-----------------------------------------------------------------------
// <copyright file="SecurityHeadersMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: ASP.NET Core middleware providing security headers for protection against common web vulnerabilities.
//                  ASP.NET Core middleware component providing cross-cutting
//                  security concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------


//-----------------------------------------------------------------------
// <copyright file="SecurityHeadersMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: ASP.NET Core middleware providing security headers for protection against common web vulnerabilities.
//                  ASP.NET Core middleware component providing cross-cutting
//                  security concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Configuration.Middleware
{
    /// <summary>
    /// Middleware that adds security headers to HTTP responses to protect against common web vulnerabilities
    /// including clickjacking, MIME sniffing, XSS attacks, and unauthorized content injection
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers before calling the next middleware
            AddSecurityHeaders(context);

            // Continue to the next middleware in the pipeline
            await _next(context);
        }

        private void AddSecurityHeaders(HttpContext context)
        {
            var response = context.Response;
            var request = context.Request;

            try
            {
                // Only add headers if they haven't been set already
                if (!response.Headers.ContainsKey("X-Frame-Options"))
                {
                    // ?? PRODUCTION FIX: Allow same-origin frames instead of DENY for IIS compatibility
                    // DENY was causing Chrome security errors in production IIS deployment
                    response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                }

                if (!response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    // Prevents MIME type sniffing attacks
                    response.Headers["X-Content-Type-Options"] = "nosniff";
                }

                if (!response.Headers.ContainsKey("X-XSS-Protection"))
                {
                    // Enables browser's built-in XSS protection (legacy browsers)
                    response.Headers["X-XSS-Protection"] = "1; mode=block";
                }

                if (!response.Headers.ContainsKey("Referrer-Policy"))
                {
                    // Controls how much referrer information is included with requests
                    response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                }

                if (!response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    // Content Security Policy - Blazor Server optimized
                    var csp = BuildContentSecurityPolicy(request);
                    response.Headers["Content-Security-Policy"] = csp;
                }

                if (!response.Headers.ContainsKey("Permissions-Policy"))
                {
                    // Permissions Policy - restricts access to browser features (updated with valid features)
                    response.Headers["Permissions-Policy"] = 
                        "camera=(), microphone=(), payment=(), usb=(), geolocation=(), gyroscope=(), magnetometer=(), midi=()";
                }

                // Add Strict-Transport-Security for HTTPS requests
                if (request.IsHttps && !response.Headers.ContainsKey("Strict-Transport-Security"))
                {
                    response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
                }

                // Cross-Origin policies for API endpoints
                if (request.Path.StartsWithSegments("/api"))
                {
                    if (!response.Headers.ContainsKey("Cross-Origin-Embedder-Policy"))
                    {
                        response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
                    }

                    if (!response.Headers.ContainsKey("Cross-Origin-Opener-Policy"))
                    {
                        response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
                    }
                }

                // Log security headers application (debug level to avoid spam)
                _logger.LogInfrastructureDebug("Security headers applied to {Method} {Path} from {RemoteIP}", 
                    request.Method, 
                    request.Path, 
                    GetClientIpAddress(context));
            }
            catch (Exception ex)
            {
                // Log error but don't break the pipeline
                _logger.LogInfrastructureWarning(ex, "Failed to add security headers for {Path}", request.Path);
            }
        }

        /// <summary>
        /// Build Content Security Policy optimized for Blazor Server applications
        /// </summary>
        private string BuildContentSecurityPolicy(HttpRequest request)
        {
            var cspBuilder = new List<string>();
            var isDevMode = IsDebugEnvironment(request);

            // Default source policy - only allow same origin
            cspBuilder.Add("default-src 'self'");

            // Script sources - Blazor Server needs inline scripts and eval for SignalR
            if (isDevMode)
            {
                // More permissive for development (includes CDNs, Browser Link, and Map services)
                cspBuilder.Add("script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com localhost:* 127.0.0.1:* *.visualstudio.com");
            }
            else
            {
                // Production - allow common CDNs and Map services but be more restrictive
                cspBuilder.Add("script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://unpkg.com");
            }

            // Style sources - Blazor and Radzen need inline styles, plus CDNs and Map services
            if (isDevMode)
            {
                cspBuilder.Add("style-src 'self' 'unsafe-inline' data: https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com https://unpkg.com");
            }
            else
            {
                cspBuilder.Add("style-src 'self' 'unsafe-inline' data: https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com https://unpkg.com");
            }

            // Font sources - support web fonts and data URIs
            cspBuilder.Add("font-src 'self' data: https://cdnjs.cloudflare.com https://fonts.gstatic.com");

            // Image sources - support data URIs and blob for dynamic images + map tiles
            cspBuilder.Add("img-src 'self' data: blob: https://tile.openstreetmap.org https://*.tile.openstreetmap.org https://server.arcgisonline.com https://*.arcgisonline.com https://cdn.portofportland.com https://*.portofportland.com https://unpkg.com");

            // Connect sources - WebSocket connections for Blazor SignalR + Map services
            if (isDevMode)
            {
                // Include Browser Link for development + Map services + Source maps
                cspBuilder.Add("connect-src 'self' ws: wss: http://localhost:* https://localhost:* *.visualstudio.com https://cdn.jsdelivr.net https://nominatim.openstreetmap.org https://server.arcgisonline.com https://tile.openstreetmap.org https://*.tile.openstreetmap.org https://unpkg.com");
            }
            else
            {
                cspBuilder.Add("connect-src 'self' ws: wss: https://cdn.jsdelivr.net https://nominatim.openstreetmap.org https://server.arcgisonline.com https://tile.openstreetmap.org https://*.tile.openstreetmap.org https://unpkg.com");
            }

            // Media sources
            cspBuilder.Add("media-src 'self'");

            // Object restrictions - more secure to block these completely
            cspBuilder.Add("object-src 'none'");

            // Base URI restriction
            cspBuilder.Add("base-uri 'self'");

            // Form action restriction
            cspBuilder.Add("form-action 'self'");

            // Frame ancestors (allow same-origin instead of none for IIS compatibility)
            cspBuilder.Add("frame-ancestors 'self'");

            // Remove invalid embed-src directive (not a standard CSP directive)
            // Use object-src 'none' instead which is more secure

            return string.Join("; ", cspBuilder);
        }

        /// <summary>
        /// Check if we're in a debug/development environment
        /// </summary>
        private bool IsDebugEnvironment(HttpRequest request)
        {
            // Check for development indicators
            return request.Host.Host.Contains("localhost") || 
                   request.Host.Host.Contains("127.0.0.1") ||
                   request.Host.Host.EndsWith(".local") ||
                   request.Headers.ContainsKey("X-Development-Mode") ||
                   Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        }

        /// <summary>
        /// Get client IP address from various possible headers
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            try
            {
                // Check for forwarded headers first (load balancer/proxy scenarios)
                string? ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                                ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                                ?? context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() // Cloudflare
                                ?? context.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

                return string.IsNullOrEmpty(ipAddress) ? "Unknown" : ipAddress;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
