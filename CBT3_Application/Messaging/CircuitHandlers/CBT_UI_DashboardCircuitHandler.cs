using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBT3_Infrastructure.Configuration;
using CBT3_Infrastructure.Interfaces;

using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CBT3_Application.Messaging.CircuitHandlers;

public class CBT_UI_DashboardCircuitHandler : BaseCircuitHandler
{
    private ILogger<CBT_UI_DashboardCircuitHandler> _logger;
    
    public CBT_UI_DashboardCircuitHandler(ILogger<CBT_UI_DashboardCircuitHandler> logger, ILogSupport logsupport, IMediator mediator) : base(logger, logsupport, mediator)
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