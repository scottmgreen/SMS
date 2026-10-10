using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace SMS3.Components.Pages;

public partial class SecurityTest
{

[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    
    private string HeaderCheckResult = "";
    private string Protocol => Navigation.ToAbsoluteUri("/").Scheme;
    private string Host => Navigation.ToAbsoluteUri("/").Host;
    private bool IsHttps => Protocol == "https";

    private async Task CheckHeaders()
    {
        try
        {
            HeaderCheckResult = $"Checked at {DateTime.Now:HH:mm:ss}";
            StateHasChanged();
            
            // Show instructions for manual header checking
            await JSRuntime.InvokeVoidAsync("alert", 
                "To check headers:\n\n" +
                "1. Open Developer Tools (F12)\n" +
                "2. Go to Network tab\n" +
                "3. Refresh this page\n" +
                "4. Click on the page request\n" +
                "5. Check Response Headers section\n\n" +
                "You should see the security headers listed on the left.");
        }
        catch (Exception ex)
        {
            HeaderCheckResult = $"Error: {ex.Message}";
            StateHasChanged();
        }
    }
}


