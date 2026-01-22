using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Interfaces;
namespace SMS_Application.Messaging.CircuitHandlers;

public abstract class BaseCircuitHandler : CircuitHandler
{
    protected readonly ILogger _logger;
    protected readonly ILogSupport _logsupport;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected string HostName { get; set; }
    protected string IPAddress { get; set; }
    protected string ConnectionId { get; set; }
    protected IMediator Mediator;

    private static readonly ConcurrentDictionary<string, Circuit> _activeCircuits = new();

    public BaseCircuitHandler(ILogger<SMS_CircuitHandler> logger, ILogSupport logsupport, IMediator mediator, IHttpContextAccessor httpContextAccessor = null)
    {
        _logger = logger;
        _logsupport = logsupport;
        Mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets current user code for audit trails - uses session data
    /// </summary>
    protected string GetCurrentUserCode()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        if (session == null) return "SYSTEM";

        return session.GetString("SMS_UserCode") ??
               session.GetString("SMS_UserId") ??
               "SYSTEM";
    }

    /// <summary>
    /// Gets current user display name
    /// </summary>
    protected string GetCurrentUserDisplayName()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        if (session == null) return "System";

        return session.GetString("SMS_DisplayName") ??
               session.GetString("SMS_Email") ??
               "System";
    }

    /// <summary>
    /// Checks if user is authenticated
    /// </summary>
    protected bool IsUserAuthenticated()
    {
        var session = _httpContextAccessor?.HttpContext?.Session;
        if (session == null) return false;

        return session.GetString("IsAuthenticated") == "true" &&
               !string.IsNullOrEmpty(session.GetString("SMS_UserId"));
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