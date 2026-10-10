using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class MitigationEditorFields
{

[Parameter] public Mitigation Model { get; set; } = default!;
    [Parameter] public bool DisableInputs { get; set; }
    [Parameter] public bool EnableStatusEdit { get; set; } = true;
    [Parameter] public bool ShowGuidancePanel { get; set; }
    [Parameter] public IEnumerable<string> ControlTypeOptions { get; set; } = Enumerable.Empty<string>();
    [Parameter] public IEnumerable<MitigationStatus> StatusOptions { get; set; } = Enumerable.Empty<MitigationStatus>();
    [Parameter] public IEnumerable<string> DepartmentOptions { get; set; } = Enumerable.Empty<string>();
}


