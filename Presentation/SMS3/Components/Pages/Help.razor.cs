using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;
namespace SMS3.Components.Pages;

public partial class Help
{

[Inject] private NavigationManager Navigation { get; set; } = default!;
    
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;

    private void ContactSupport()
    {
            NotificationHelper.ShowInfoAsync("Support Contact: sms-support@pdx.com or call (503) 555-SMS3 (7673)", 8000);
    }
}


