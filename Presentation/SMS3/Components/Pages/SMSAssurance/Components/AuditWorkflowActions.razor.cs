using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Commands;
using SMS_Domain.Entities;
namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditWorkflowActions
{

[Parameter] public SMSAudit? Audit { get; set; }
    [Parameter] public int TotalFindings { get; set; } = 0;
    [Parameter] public int CriticalFindings { get; set; } = 0;
    [Parameter] public int TotalEvidence { get; set; } = 0;
    [Parameter] public int ProgressPercentage { get; set; } = 0;
    [Parameter] public bool IsProcessing { get; set; } = false;

    [Parameter] public EventCallback OnStartAudit { get; set; }
    [Parameter] public EventCallback OnAddFinding { get; set; }
    [Parameter] public EventCallback OnUploadEvidence { get; set; }
    [Parameter] public EventCallback OnCompleteAudit { get; set; }
    [Parameter] public EventCallback OnGenerateReport { get; set; }
}


