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
namespace SMS3.Components.Shared.Components;

public partial class TechnicalAssessmentGuidance
{

[Parameter] public int StepNumber { get; set; } = 1;

    private bool isExpanded = false;

    private string GetGuidanceTitle()
    {
        var stage = StepNumber switch
        {
            1 => RiskAssessmentStage.DescribingSystem,
            2 => RiskAssessmentStage.IdentifyingHazards,
            3 => RiskAssessmentStage.AnalyzingRisk,
            4 => RiskAssessmentStage.AssessingRisk,
            5 => RiskAssessmentStage.MitigatingRisk,
            _ => RiskAssessmentStage.DescribingSystem
        };

        return $"{stage.Name} Guidance";
    }
}


