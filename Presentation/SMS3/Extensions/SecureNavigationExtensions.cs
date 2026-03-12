using SMS3.Security;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Extensions;

/// <summary>
/// Simple URL encryption extensions - encrypt any URL, decrypt it back
/// </summary>
public static class SecureNavigationExtensions
{
    /// <summary>
    /// Navigate to any URL with automatic encryption
    /// </summary>
    public static void NavigateToSecure(this NavigationManager navigationManager, 
        string url, 
        bool forceLoad = false)
    {
        var secureRoutingService = ServiceLocator.Current?.GetService<ISecureRoutingService>();
        if (secureRoutingService != null)
        {
            var encryptedUrl = secureRoutingService.EncryptUrl(url);
            navigationManager.NavigateTo(encryptedUrl, forceLoad);
        }
        else
        {
            // Fallback to regular navigation
            navigationManager.NavigateTo(url, forceLoad);
        }
    }
    
    /// <summary>
    /// Navigate to URL with parameters
    /// </summary>
    public static void NavigateToSecure(this NavigationManager navigationManager,
        string path,
        string parameterName,
        string parameterValue,
        bool forceLoad = false)
    {
        var url = $"{path}?{parameterName}={Uri.EscapeDataString(parameterValue)}";
        navigationManager.NavigateToSecure(url, forceLoad);
    }
    
    /// <summary>
    /// Navigate to URL with multiple parameters
    /// </summary>
    public static void NavigateToSecure(this NavigationManager navigationManager,
        string path,
        Dictionary<string, string> parameters,
        bool forceLoad = false)
    {
        var url = path;
        if (parameters?.Any() == true)
        {
            var queryString = string.Join("&", 
                parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            url += "?" + queryString;
        }
        
        navigationManager.NavigateToSecure(url, forceLoad);
    }
    
    /// <summary>
    /// Generate secure URL without navigating
    /// </summary>
    public static string GenerateSecureUrl(this NavigationManager navigationManager,
        string url)
    {
        var secureRoutingService = ServiceLocator.Current?.GetService<ISecureRoutingService>();
        if (secureRoutingService != null)
        {
            return secureRoutingService.EncryptUrl(url);
        }
        
        return url; // Fallback to original URL
    }
    
    /// <summary>
    /// Generate secure URL with parameters
    /// </summary>
    public static string GenerateSecureUrl(this NavigationManager navigationManager,
        string path,
        Dictionary<string, string> parameters)
    {
        var url = path;
        if (parameters?.Any() == true)
        {
            var queryString = string.Join("&", 
                parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            url += "?" + queryString;
        }
        
        return navigationManager.GenerateSecureUrl(url);
    }
    
    /// <summary>
    /// Generate secure URL with single parameter
    /// </summary>
    public static string GenerateSecureUrl(this NavigationManager navigationManager,
        string path,
        string parameterName,
        string parameterValue)
    {
        var url = $"{path}?{parameterName}={Uri.EscapeDataString(parameterValue)}";
        return navigationManager.GenerateSecureUrl(url);
    }
}