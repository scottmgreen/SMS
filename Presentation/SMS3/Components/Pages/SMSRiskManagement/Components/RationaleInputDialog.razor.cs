using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class RationaleInputDialog
{

[Parameter] public string MemberName { get; set; } = string.Empty;
    [Parameter] public string HazardCode { get; set; } = string.Empty;
    [Parameter] public string MatrixCode { get; set; } = string.Empty;
    [Parameter] public string RationaleInput { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> RationaleInputChanged { get; set; }
    [Parameter] public EventCallback<bool> OnResult { get; set; } // Pass result back to parent

    private string CurrentRationale = string.Empty;

    protected override void OnInitialized()
    {
        CurrentRationale = RationaleInput ?? string.Empty;
    }

    private async Task OnRationaleChanged(string value)
    {
        CurrentRationale = value ?? string.Empty;
        
        // Notify parent of the change
        if (RationaleInputChanged.HasDelegate)
        {
            await RationaleInputChanged.InvokeAsync(CurrentRationale);
        }
    }

    private async Task OnSubmitClick()
    {
        // Ensure final value is passed to parent
        if (RationaleInputChanged.HasDelegate)
        {
            await RationaleInputChanged.InvokeAsync(CurrentRationale);
        }

        // Close with success result
        if (OnResult.HasDelegate)
        {
            await OnResult.InvokeAsync(!string.IsNullOrWhiteSpace(CurrentRationale));
        }
    }

    private async Task OnCancelClick()
    {
        // Close with cancel result
        if (OnResult.HasDelegate)
        {
            await OnResult.InvokeAsync(false);
        }
    }
}


