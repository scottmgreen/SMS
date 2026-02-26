//-----------------------------------------------------------------------
// <copyright file="SMS_DashboardCircuitHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Messaging.CircuitHandlers;

public class SMS_DashboardCircuitHandler : BaseCircuitHandler
{
    //private ILogger<SMS_CircuitHandler> _logger;
    private new readonly ILogger<SMS_CircuitHandler> _logger;
    public SMS_DashboardCircuitHandler(ILogger<SMS_CircuitHandler> logger, ILogSupport logsupport, IMediator mediator) : base(logger, logsupport, mediator)
    {
        _logger = logger;
    }

    protected override Task HandleConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation($"[CBT_DashboardCircuitHandler] Client Disconnected: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {

            _logger.LogError($"[CBT_DashboardCircuitHandler] Client Disconnected Error: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            throw;
        }
    }

    protected override Task HandleConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation($"[CBT_DashboardCircuitHandler] Client Connected: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            return Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            _logger.LogError($"[CBT_DashboardCircuitHandler] Client Connected Error: CircuitId: {circuit.Id}, ConnectionId: {ConnectionId}, IP: {IPAddress}, Host: {HostName}");
            throw;
        }
    }
}
