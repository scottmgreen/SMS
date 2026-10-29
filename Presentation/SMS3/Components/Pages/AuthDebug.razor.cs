using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Application.Configuration;
using SMS_Application.Services;

namespace SMS3.Components.Pages;

public partial class AuthDebug
{
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    private string _configuredPrimaryMethod = "SessionBased";
    private bool _configuredFallbackEnabled;
    private SessionCurrentUserService.AuthStateDiagnostics? _authStateDiagnostics;
    
    private async Task RefreshData()
    {
        Logger.LogInformation("Refreshing authentication debug data");
        await LoadAuthDiagnostics();
        StateHasChanged();
    }
    
    private void NavigateToHome()
    {
        _navigation.NavigateTo("/");
    }
    
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("AuthDebug page loaded");
        await LoadAuthDiagnostics();
    }

    private async Task LoadAuthDiagnostics()
    {
        _configuredPrimaryMethod = _configuration["Authentication:PreferredMethod"] ?? "SessionBased";
        _configuredFallbackEnabled = _configuration.GetValue("Authentication:EnableFallbackChain", true);

        if (_currentUserService is SessionCurrentUserService sessionCurrentUserService)
        {
            _authStateDiagnostics = sessionCurrentUserService.GetAuthStateDiagnostics();
        }
        else
        {
            _authStateDiagnostics = null;
        }

        await Task.CompletedTask;
    }

    private string GetConfigValue(string key) => _configuration[key] ?? "(not set)";
}


