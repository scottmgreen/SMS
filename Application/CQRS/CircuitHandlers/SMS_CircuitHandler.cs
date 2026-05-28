//-----------------------------------------------------------------------
// <copyright file="SMS_CircuitHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Interfaces;

namespace SMS_Application.CircuitHandlers;

/// <summary>
/// SMS Circuit Handler - Simple Authentication State Management
/// </summary>
public class SMS_CircuitHandler : BaseCircuitHandler
{
    private new readonly ILogger<SMS_CircuitHandler> _logger;

    // Simple static storage for circuit authentication
    private static readonly ConcurrentDictionary<string, CircuitAuthState> _circuitAuth = new();

    public SMS_CircuitHandler(
        ILogger<SMS_CircuitHandler> logger,
        ILogSupport logsupport,
        IBaseMediator mediator,
        IHttpContextAccessor httpContextAccessor)
        : base(logger, logsupport, mediator, httpContextAccessor)
    {
        _logger = logger;
    }

    protected override async Task HandleConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            // Clean up circuit auth when connection drops
            _circuitAuth.TryRemove(circuit.Id, out _);
            _logger.LogApplicationInformation("[SMS_CircuitHandler] Cleaned up auth for circuit: {CircuitId}", circuit.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationError("[SMS_CircuitHandler] Connection cleanup cancelled: {CircuitId}", circuit.Id);
            throw;
        }
    }

    protected override async Task HandleConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationInformation("[SMS_CircuitHandler] Circuit connected: {CircuitId}", circuit.Id);
            // Connection up - auth will be set when user logs in
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationError("[SMS_CircuitHandler] Connection setup error: {CircuitId}", circuit.Id);
            throw;
        }
    }

    /// <summary>
    /// Set authentication for a circuit (called from login)
    /// </summary>
    public static void SetCircuitAuthentication(string circuitId, BaseUser user, SMSUserType userType)
    {
        var authState = new CircuitAuthState
        {
            UserId = user.Code,
            UserType = userType.Value,
            DisplayName = user.DisplayName,
            Email = user.UserName.Value,
            FirstName = user.FirstName.Value,
            LastName = user.LastName.Value,
            LoginTime = DateTime.UtcNow,
            IsAuthenticated = true
        };

        _circuitAuth.AddOrUpdate(circuitId, authState, (key, existing) => authState);
    }

    /// <summary>
    /// Clear authentication for a circuit (called from logout)
    /// </summary>
    public static void ClearCircuitAuthentication(string circuitId)
    {
        _circuitAuth.TryRemove(circuitId, out _);
    }

    /// <summary>
    /// Get authentication state for current circuit
    /// </summary>
    public static CircuitAuthState? GetCircuitAuthentication(string circuitId)
    {
        _circuitAuth.TryGetValue(circuitId, out var authState);
        return authState;
    }

    /// <summary>
    /// Check if circuit is authenticated
    /// </summary>
    public static bool IsCircuitAuthenticated(string circuitId)
    {
        var authState = GetCircuitAuthentication(circuitId);
        return authState?.IsAuthenticated == true;
    }
}

/// <summary>
/// Simple authentication state for a circuit
/// </summary>
public class CircuitAuthState
{
    public string UserId { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
    public bool IsAuthenticated { get; set; }
}


