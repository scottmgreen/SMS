using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Configuration.Extensions;
using SMS3.Security;
namespace SMS3.Components.Layout;

public partial class AnonymousMenu
{

// Environment Detection - WITH PROPER SPACING
    private string BrandCssClass => Environment.IsDevelopment() ? "development" : "";
    private string EnvironmentSuffix => Environment.IsDevelopment() ? "[Development]" : "";

    // Hide all menu items on PDX and Login pages  
    private bool IsOnRestrictedPage => Navigation.Uri.Contains("/pdx") || IsObfuscatedLogin();

    /// <summary>
    /// Handle menu clicks for SECURE navigation - all URLs automatically encrypted
    /// </summary>
    private void OnMenuClick(MenuItemEventArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value?.ToString()))
        {
            var value = args.Value.ToString();
            if (!string.IsNullOrEmpty(value) && value.StartsWith("/"))
            {
                // ?? SECURE NAVIGATION - All anonymous menu URLs automatically encrypted
                Navigation.NavigateToSecure(value);
            }
        }
    }

    // Only show login if user explicitly goes to /pdx or /admin paths, or if on encrypted login
    private bool ShouldShowLogin => Navigation.Uri.Contains("/pdx") || 
                                   Navigation.Uri.Contains("/admin") ||
                                   Navigation.Uri.Contains("/login") ||
                                   IsObfuscatedLogin();

    /// <summary>
    /// Check if current URL is an obfuscated login page
    /// </summary>
    private bool IsObfuscatedLogin()
    {
        try
        {
            var currentPath = Navigation.ToBaseRelativePath(Navigation.Uri);
            if (currentPath.StartsWith("s/"))
            {
                var encryptedPart = currentPath.Substring(2);
                var decrypted = SecureRoutingService.DecryptRouteParameter(encryptedPart);
                return decrypted.Contains("/login");
            }
        }
        catch { /* Ignore decrypt errors */ }
        
        return false;
    }
}


