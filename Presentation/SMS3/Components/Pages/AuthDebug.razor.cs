using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Configuration;
namespace SMS3.Components.Pages;

public partial class AuthDebug
{

[Inject] private NavigationManager _navigation { get; set; } = default!;
    private string _configuredPrimaryMethod = "SessionBased";
    private string _configuredFallbackMethod = "CircuitBased";
    private bool _configuredFallbackEnabled = true;
    private List<AuthStrategySnapshot> _strategySnapshot = new();
    
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
        _configuredFallbackMethod = _configuration["Authentication:FallbackMethod"] ?? "CircuitBased";
        _configuredFallbackEnabled = _configuration.GetValue("Authentication:EnableFallbackChain", true);

        var validation = await _strategyManager.ValidateAllStrategiesAsync();
        var primaryMethod = ParseMethod(_configuredPrimaryMethod);
        var fallbackMethod = ParseMethod(_configuredFallbackMethod);

        var strategyTasks = _strategyManager.GetAllStrategies()
            .Select(async s =>
            {
                var isAuthenticated = await s.IsUserAuthenticatedAsync();
                var currentUserId = await s.GetCurrentUserIdAsync();

                return new AuthStrategySnapshot
                {
                    Method = s.Method.ToString(),
                    Name = s.StrategyName,
                    IsAvailable = s.IsAvailable,
                    HasStoredAuthData = validation.TryGetValue(s.Method, out var isValid) && isValid,
                    IsAuthenticated = isAuthenticated,
                    CurrentUserId = currentUserId,
                    ConfigurationRole = s.Method == primaryMethod
                        ? "Primary"
                        : (s.Method == fallbackMethod ? "Fallback" : "Registered")
                };
            });

        var strategyResults = await Task.WhenAll(strategyTasks);

        _strategySnapshot = strategyResults
            .OrderByDescending(x => x.ConfigurationRole == "Primary")
            .ThenByDescending(x => x.ConfigurationRole == "Fallback")
            .ThenBy(x => x.Method)
            .ToList();
    }

    private string GetConfigValue(string key) => _configuration[key] ?? "(not set)";

    private static AuthenticationMethod ParseMethod(string configuredValue)
    {
        return Enum.TryParse<AuthenticationMethod>(configuredValue, ignoreCase: true, out var parsed)
            ? parsed
            : AuthenticationMethod.SessionBased;
    }

    private sealed class AuthStrategySnapshot
    {
        public string Method { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool HasStoredAuthData { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? CurrentUserId { get; set; }
        public string ConfigurationRole { get; set; } = "Registered";
    }
}


