using Microsoft.AspNetCore.Http;

namespace SMS.Presentation.Extensions;

/// <summary>
/// Simple extension methods for consistent user access across all PageModels
/// Leverages existing proven session-based authentication from Login.cshtml.cs
/// NO HELPER CLASSES - just consistent access patterns
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets current user code for audit trails (CreatedBy, UpdatedBy, etc.)
    /// Returns "SYSTEM" if no authenticated user
    /// </summary>
    public static string GetCurrentUserCode(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return "SYSTEM";
        
        return httpContext.Session.GetString("SMS_UserCode") ?? 
               httpContext.Session.GetString("SMS_UserId") ?? 
               "SYSTEM";
    }

    /// <summary>
    /// Gets current user display name for UI purposes
    /// </summary>
    public static string GetCurrentUserDisplayName(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return "System";
        
        return httpContext.Session.GetString("SMS_DisplayName") ?? 
               httpContext.Session.GetString("SMS_Email") ?? 
               "System";
    }

    /// <summary>
    /// Gets current user email
    /// </summary>
    public static string GetCurrentUserEmail(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return "";
        
        return httpContext.Session.GetString("SMS_Email") ?? "";
    }

    /// <summary>
    /// Checks if user is authenticated using session data
    /// </summary>
    public static bool IsUserAuthenticated(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return false;
        
        return httpContext.Session.GetString("IsAuthenticated") == "true" &&
               !string.IsNullOrEmpty(httpContext.Session.GetString("SMS_UserId"));
    }

    /// <summary>
    /// Gets current user type (Application, Organizational, Stakeholder)
    /// </summary>
    public static string GetCurrentUserType(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return "SYSTEM";
        
        return httpContext.Session.GetString("SMS_UserType") ?? "SYSTEM";
    }

    /// <summary>
    /// Gets current user department (for organizational users)
    /// </summary>
    public static string GetCurrentUserDepartment(this HttpContext httpContext)
    {
        if (httpContext?.Session == null) return "";
        
        return httpContext.Session.GetString("SMS_Department") ?? "";
    }
}