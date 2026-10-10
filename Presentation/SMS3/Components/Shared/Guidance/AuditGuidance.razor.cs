using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Shared.Components;

public partial class AuditGuidance
{

[Parameter] public EventCallback OnAddFinding { get; set; }
    [Parameter] public EventCallback OnUploadEvidence { get; set; }
    [Parameter] public EventCallback OnGenerateReport { get; set; }
    [Parameter] public EventCallback OnCompleteAudit { get; set; }

    private bool isExpanded = false;
}


