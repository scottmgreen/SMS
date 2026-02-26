//-----------------------------------------------------------------------
// <copyright file="ConnectionInfoMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: ASP.NET Core middleware providing database connection tracking and performance monitoring.
//                  ASP.NET Core middleware component providing cross-cutting
//                  concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Configuration;
public class ConnectionInfoMiddleware
{
    private static readonly ConcurrentDictionary<string, (string connectionid, string ip, string host)> ConnectionInfoMap = new();

    private readonly RequestDelegate _next;
    public ConnectionInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {


        var ipAddress = GetRemoteIpAddress(httpContext);
        var hostName = GetRemoteHostName(httpContext);
        var connectionId = httpContext.Connection.Id;

        // Store connection info
        ConnectionInfoMap[connectionId] = (connectionId, ipAddress, hostName);
        //connectionService.SetConnectionInfo(ipAddress, hostName);

        await _next(httpContext);
    }

    public static (string connectionid, string ip, string host) GetConnectionInfo(string connectionId)
    {
        return ConnectionInfoMap.TryGetValue(connectionId, out var info) ? info : ("Unknown Connection ID", "Unknown IP", "Unknown Host");
    }

    private string GetRemoteHostName(HttpContext httpcontext)
    {
        var httpContext = httpcontext;
        if (httpContext == null)
            return "Unknown Host";

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

    private string GetRemoteIpAddress(HttpContext httpcontext)
    {
        var httpContext = httpcontext;
        if (httpContext == null)
            return "Unknown IP";

        string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
                        ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
                        ?? httpContext.Connection?.RemoteIpAddress?.ToString();

        if (ipAddress == "0.0.0.1" || ipAddress == "::1" || ipAddress == "127.0.0.1")
        {
            return ipAddress; // Localhost situation
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
