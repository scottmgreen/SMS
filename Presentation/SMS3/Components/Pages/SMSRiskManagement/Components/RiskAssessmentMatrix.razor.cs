using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Shared;
using SMS_Domain.Enums;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class RiskAssessmentMatrix
{

[Parameter] public string Title { get; set; } = "Risk Assessment Matrix";
    [Parameter] public string CardStyle { get; set; } = string.Empty;
    [Parameter] public bool ShowRiskLevelsLegend { get; set; }
    [Parameter] public int MaxBadgesPerCell { get; set; } = 3;
    [Parameter] public Func<int, string>? SeverityLabelProvider { get; set; }
    [Parameter] public Func<int, int, List<string>>? HazardsAtMatrixCodeProvider { get; set; }

    private string GetSeverityLabel(int severity)
    {
        return SeverityLabelProvider?.Invoke(severity) ?? HazardSeverity.GetDisplayName(severity);
    }

    private List<string> GetHazardsAtMatrixCode(int severity, int likelihood)
    {
        return HazardsAtMatrixCodeProvider?.Invoke(severity, likelihood) ?? new List<string>();
    }
}


