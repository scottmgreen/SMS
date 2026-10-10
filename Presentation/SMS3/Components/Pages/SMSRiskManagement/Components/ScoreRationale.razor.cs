using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class ScoreRationale
{

[Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public string MemberName { get; set; } = string.Empty;
    [Parameter] public string HazardCode { get; set; } = string.Empty;
    [Parameter] public string MatrixCode { get; set; } = string.Empty;
    [Parameter] public string RationaleInput { get; set; } = string.Empty;
    [Parameter] public string Rationale { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> RationaleInputChanged { get; set; }
    [Parameter] public EventCallback<bool> OnResult { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private string CurrentRationale { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        CurrentRationale = IsReadOnly
            ? (Rationale ?? string.Empty)
            : (RationaleInput ?? string.Empty);
    }

    private string GetHeaderIcon() => IsReadOnly ? "fas fa-user-check" : "fas fa-comment-dots";

    private string GetHeaderText()
    {
        return IsReadOnly
            ? $"Rationale for {HazardCode} risk assessment {MatrixCode} score"
            : $"Please provide {MemberName} rationale for scoring {HazardCode} as {MatrixCode}";
    }

    private string GetTextAreaStyle()
    {
        return IsReadOnly
            ? "width: 100%; background-color: #f8f9fa; border: 2px solid #e9ecef;"
            : "width: 100%;";
    }

    private async Task OnRationaleChanged(string value)
    {
        if (IsReadOnly)
        {
            return;
        }

        CurrentRationale = value ?? string.Empty;

        if (RationaleInputChanged.HasDelegate)
        {
            await RationaleInputChanged.InvokeAsync(CurrentRationale);
        }
    }

    private async Task HandleSubmit()
    {
        if (RationaleInputChanged.HasDelegate)
        {
            await RationaleInputChanged.InvokeAsync(CurrentRationale);
        }

        if (OnResult.HasDelegate)
        {
            await OnResult.InvokeAsync(!string.IsNullOrWhiteSpace(CurrentRationale));
        }
    }

    private async Task HandleCancel()
    {
        if (OnResult.HasDelegate)
        {
            await OnResult.InvokeAsync(false);
        }
    }

    private async Task HandleClose()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }
}


