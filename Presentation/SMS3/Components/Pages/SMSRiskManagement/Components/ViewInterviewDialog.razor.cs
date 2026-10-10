
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class ViewInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] public DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Interview Interview { get; set; } = default!;
    #endregion

    #region UI Helpers
    private BadgeStyle GetStatusBadge()
    {
        return Interview?.Status.Value switch
        {
            "PLANNED" => BadgeStyle.Secondary,
            "SCHEDULED" => BadgeStyle.Info,
            "IN_PROGRESS" => BadgeStyle.Warning,
            "COMPLETED" => BadgeStyle.Success,
            "CANCELLED" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }
    #endregion

// [Parameter] public Interview Interview { get; set; } = default!;
    // [Inject] private DialogService DialogService { get; set; } = default!;

    private BadgeStyle GetStatusBadgeStyle()
    {
        return Interview?.Status?.Value switch
        {
            "SCHEDULED" => BadgeStyle.Info,
            "IN_PROGRESS" => BadgeStyle.Warning,
            "COMPLETED" => BadgeStyle.Success,
            "CANCELLED" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }
}
