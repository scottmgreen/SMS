using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class EventConsole
{

private bool showEventConsole = false;
    private string eventLog = string.Empty;

    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        eventLog += $"[{timestamp}] {message}\n";
        
        // Keep log reasonable size
        var lines = eventLog.Split('\n');
        if (lines.Length > 50)
        {
            eventLog = string.Join("\n", lines.Skip(lines.Length - 40));
        }
        
        StateHasChanged();
    }

    public void ClearEventLog()
    {
        eventLog = string.Empty;
        StateHasChanged();
    }
}


