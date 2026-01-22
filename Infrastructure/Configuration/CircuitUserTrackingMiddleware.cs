using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Configuration
{
    public class CircuitUserTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly ConcurrentDictionary<string, (string ip, string host)> ConnectionInfoMap = new();
        private static readonly ConcurrentDictionary<string, string> CircuitToConnectionMap = new();
        private static readonly ConcurrentBag<string> ConnectionIds = new();

        public CircuitUserTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/_blazor"))
            {
                var ipAddress = GetRemoteIpAddress(context);
                var hostName = GetRemoteHostName(context);
                var connectionId = context.Connection.Id;

                Console.WriteLine($"[DEBUG] Middleware: ConnectionId: {connectionId}, IP: {ipAddress}, Host: {hostName}");

                // Store connection details if not already stored
                ConnectionInfoMap.TryAdd(connectionId, (ipAddress, hostName));

                // Store connectionId separately for retrieval in CircuitHandler
                if (!ConnectionIds.Contains(connectionId))
                {
                    ConnectionIds.Add(connectionId);
                }
            }

            await _next(context);
        }

        // ✅ New method to retrieve a ConnectionId when CircuitHandler fires
        public static string? GetConnectionId()
        {
            return ConnectionIds.FirstOrDefault();
        }

        public static void RegisterCircuit(string circuitId, string connectionId)
        {
            if (!CircuitToConnectionMap.ContainsKey(circuitId))
            {
                CircuitToConnectionMap[circuitId] = connectionId;
                Console.WriteLine($"[DEBUG] Registered Circuit ID {circuitId} to Connection ID {connectionId}");
            }
        }
        public static void UnregisterCircuit(string circuitId)
        {
            if (CircuitToConnectionMap.TryRemove(circuitId, out var connectionId))
            {
                Console.WriteLine($"[DEBUG] Unregistered Circuit ID {circuitId} from Connection ID {connectionId}");
                bool isConnectionStillUsed = CircuitToConnectionMap.Values.Any(id => id == connectionId);
                // Also remove the connection if no other circuits are using it
                if (!isConnectionStillUsed)
                {
                    ConnectionInfoMap.TryRemove(connectionId, out _);
                    ConnectionIds.TryTake(out _);
                }
            }
        }







        public static (string connectionid, string ip, string host) GetUserInfoFromCircuitId(string circuitId)
        {
            if (CircuitToConnectionMap.TryGetValue(circuitId, out var connectionId))
            {
                var (ip, host) = ConnectionInfoMap.TryGetValue(connectionId, out var connInfo)
                    ? connInfo
                    : ("Unknown IP", "Unknown Host");
                return (connectionId, ip, host);
            }

            return ("Unknown", "Unknown IP", "Unknown Host");
        }
        public static (string circuitid, string connectionid, string ip, string host) GetUserInfoFromConnectionId(string connectionid)
        {
            if (ConnectionInfoMap.TryGetValue(connectionid, out _))
            {
                var (ip, host) = ConnectionInfoMap.TryGetValue(connectionid, out var connInfo)
                    ? connInfo
                    : ("Unknown IP", "Unknown Host");
                var circuitId = CircuitToConnectionMap.Where(x => x.Value == connectionid).FirstOrDefault().Key;

                return (circuitId, connectionid, ip, host);
            }

            return ("Unknown", "Unknown", "Unknown IP", "Unknown Host");
        }

        public static (string circuitId, string connectionId, string ip, string host) GetUserInfoFromHostName(string hostName)
        {
            // Find the first matching connection entry with the given hostname
            var connectionEntry = ConnectionInfoMap.FirstOrDefault(kvp => kvp.Value.host.Equals(hostName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(connectionEntry.Key))
            {
                string connectionId = connectionEntry.Key;
                var (ip, host) = connectionEntry.Value;

                // Find the corresponding circuit ID, if any
                string circuitId = CircuitToConnectionMap.FirstOrDefault(x => x.Value == connectionId).Key ?? "Unknown";

                return (circuitId, connectionId, ip, host);
            }

            return ("Unknown", "Unknown", "Unknown IP", "Unknown Host");
        }


        private string GetRemoteHostName(HttpContext httpContext)
        {
            string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                            ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                            ?? httpContext.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString();

            try
            {
                IPHostEntry hostEntry = ipAddress switch
                {
                    "0.0.0.1" or "::1" or "127.0.0.1" => Dns.GetHostEntry(Dns.GetHostName()),
                    _ => Dns.GetHostEntry(ipAddress)
                };

                return hostEntry.HostName.Split('.').FirstOrDefault() ?? "Unknown Host";
            }
            catch
            {
                return "Unknown Host";
            }
        }

        private string GetRemoteIpAddress(HttpContext httpContext)
        {
            string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                            ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                            ?? httpContext.Connection?.RemoteIpAddress?.ToString();

            if (ipAddress == "0.0.0.1" || ipAddress == "::1" || ipAddress == "127.0.0.1")
            {
                return ipAddress; // Localhost case
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(ipAddress);
                return hostEntry.AddressList
                                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                ?.ToString() ?? "Unknown IP";
            }
            catch
            {
                return "Unknown IP";
            }
        }
    }
}
