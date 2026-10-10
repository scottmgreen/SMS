using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS3.Components.Layout;
using SMS3.Configuration.Extensions;
using SMS3.Security;
namespace SMS3.Components.Pages;

public partial class PDX
{

private bool IsProcessing { get; set; } = false;
    
    protected override void OnInitialized()
    {
        Logger.LogInformation("PDX disclaimer page initialized");
    }

    /// <summary>
    /// Handle terms acceptance and proceed to login
    /// </summary>
    private async Task ProceedToLogin()
    {
        try
        {
            IsProcessing = true;
            
            Logger.LogInformation("✅ User accepted PDX terms, proceeding to login");
            
            // Simulate processing delay for better UX
            await Task.Delay(500);
            
            // Navigate directly to login page - 2FA will be handled there if enabled
            Navigation.NavigateToSecure("/login");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error proceeding to login from PDX terms");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }
}


