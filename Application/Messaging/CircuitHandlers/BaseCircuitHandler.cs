using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Interfaces;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;
namespace SMS_Application.Messaging.CircuitHandlers;

public abstract class BaseCircuitHandler : CircuitHandler
{
    protected readonly ILogger _logger;
    protected readonly ILogSupport _logsupport;
    protected string HostName { get; set; }
    protected  string IPAddress { get; set; }
    protected string ConnectionId { get; set; }
    protected IMediator Mediator;

    private static readonly ConcurrentDictionary<string, Circuit> _activeCircuits = new();

    public BaseCircuitHandler(ILogger<SMS_CircuitHandler> logger, ILogSupport logsupport, IMediator mediator)
    {
        _logger = logger;
        _logsupport = logsupport;
        Mediator = mediator;
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        string connectionId = "Unknown", ip = "Unknown IP", host = "Unknown Host";

        // Retrieve the latest ConnectionId from Middleware
        string? latestConnectionId = CircuitUserTrackingMiddleware.GetConnectionId();

        if (!string.IsNullOrEmpty(latestConnectionId))
        {
            CircuitUserTrackingMiddleware.RegisterCircuit(circuit.Id, latestConnectionId);
        }

        // Retry loop to allow CircuitUserTrackingMiddleware time to register the circuit
        for (int i = 0; i < 10; i++) // Max wait time: 1 second (10 x 100ms)
        {
            var (connId, ipAddress, hostName) = CircuitUserTrackingMiddleware.GetUserInfoFromCircuitId(circuit.Id);

            if (connId != "Unknown")
            {
                connectionId = connId;
                ip = ipAddress;
                host = hostName;

                HostName = host;
                IPAddress = ip;
                ConnectionId = connectionId;
                break;
            }

            await Task.Delay(100); // Wait 100ms before retrying
        }

        _activeCircuits.TryAdd(circuit.Id, circuit);
        await HandleConnectionUpAsync(circuit, cancellationToken);
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        
        _activeCircuits.TryRemove(circuit.Id, out _);
        CircuitUserTrackingMiddleware.UnregisterCircuit(circuit.Id);

        return HandleConnectionDownAsync(circuit, cancellationToken);
    }

    protected abstract Task HandleConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken);
    protected abstract Task HandleConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken);
}