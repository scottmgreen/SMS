using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Domain.ValueObjects;
namespace SMS3.Components.Pages.SMSSystem.EventBus;

public partial class EventDetailsDialog
{

[Parameter] public QueuedEvent Event { get; set; } = null!;
    [Inject] private DialogService DialogService { get; set; } = null!;
}


