using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class RationaleViewDialog
{

[Parameter] public string HazardCode { get; set; } = string.Empty;
    [Parameter] public string MatrixCode { get; set; } = string.Empty;
    [Parameter] public string Rationale { get; set; } = string.Empty;
    [Parameter] public EventCallback OnClose { get; set; }
}


