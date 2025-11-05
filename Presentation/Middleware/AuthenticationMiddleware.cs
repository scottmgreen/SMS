using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PDXSMS_Presentation.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    
    // Pages that don't require authentication
    private readonly string[] _publicPaths = {
        "/account/login",
        "/account/logout", 
        "/account/testcredentials",
        "/account/guestsubmissionsuccess",
        "/css/",
        "/js/",
        "/lib/",
        "/favicon.ico"
    };

    // The ONLY page guests are allowed to access
    private const string GUEST_ALLOWED_PATH = "/safetyriskmanagement/confidentialreporting";

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        
        // Check if this is a public path that doesn't require authentication
        var isPublicPath = _publicPaths.Any(p => path?.StartsWith(p) == true);
        
        // Check if user is authenticated (regular user or guest)
        var isAuthenticated = context.Session.GetString("UserId") != null;
        var isGuest = context.Session.GetString("IsGuest") == "true";
        var userId = context.Session.GetString("UserId");
        
        _logger.LogDebug("Authentication check: Path={Path}, IsPublicPath={IsPublicPath}, IsAuthenticated={IsAuthenticated}, IsGuest={IsGuest}, UserId={UserId}", 
            path, isPublicPath, isAuthenticated, isGuest, userId);
        
        // CRITICAL SECURITY: If this is a guest session
        if (isGuest)
        {
            _logger.LogInformation("Guest session detected. Path: {Path}", path);

            // Check if guest session is still valid (30-minute limit)
            var guestStartTimeStr = context.Session.GetString("GuestStartTime");
            if (DateTime.TryParse(guestStartTimeStr, out var guestStartTime))
            {
                var sessionAge = DateTime.UtcNow - guestStartTime;
                if (sessionAge.TotalMinutes > 30)
                {
                    _logger.LogWarning("Guest session expired after {Minutes} minutes, clearing session and redirecting", sessionAge.TotalMinutes);
                    context.Session.Clear();
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }
            else
            {
                _logger.LogWarning("Invalid guest session detected, clearing session and redirecting");
                context.Session.Clear();
                context.Response.Redirect("/Account/Login");
                return;
            }

            // SECURITY ENFORCEMENT: Guests can ONLY access the confidential reporting page
            // If they try to access ANY other page (including dashboard), clear session and redirect
            if (path != GUEST_ALLOWED_PATH && !isPublicPath)
            {
                _logger.LogWarning("SECURITY VIOLATION: Guest user attempting to access unauthorized path: {Path}. Clearing session.", path);
                context.Session.Clear();
                context.Response.Redirect("/Account/Login");
                return;
            }

            // If guest is trying to access home/dashboard, immediately redirect
            if (path == "/" || path == "/index" || string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("SECURITY VIOLATION: Guest user attempting to access dashboard. Clearing session.");
                context.Session.Clear();
                context.Response.Redirect("/Account/Login");
                return;
            }
        }
        
        // For regular (non-guest) authentication
        if (!isGuest)
        {
            // Allow access if:
            // 1. It's a public path, OR
            // 2. User is authenticated, OR 
            // 3. It's the home page
            var hasAccess = isPublicPath || isAuthenticated || path == "/";
            
            // If no access and trying to access protected content, redirect to login
            if (!hasAccess)
            {
                _logger.LogInformation("Redirecting unauthorized user from {Path} to login", path);
                
                // Store the original URL they were trying to access
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }
            
            // If authenticated (non-guest) but trying to access login page, redirect to dashboard
            if (isAuthenticated && path == "/account/login")
            {
                _logger.LogInformation("Redirecting authenticated user from login to dashboard");
                context.Response.Redirect("/");
                return;
            }
        }

        await _next(context);
    }
}