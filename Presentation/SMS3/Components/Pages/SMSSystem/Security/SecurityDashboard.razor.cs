using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Application.Services;
using SMS3.Components.Shared;
namespace SMS3.Components.Pages.SMSSystem.Security;

public partial class SecurityDashboard
{

private SecurityStats? _securityStats;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _securityStats = SecurityMonitoring.GetSecurityStats();
            Logger.LogInformation("Security dashboard loaded - SQL: {SqlAttempts}, XSS: {XssAttempts}", 
                _securityStats.SqlInjectionAttempts24h, _securityStats.XssAttempts24h);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading security dashboard");
            _securityStats = new SecurityStats(); // Empty stats
        }
    }
}


