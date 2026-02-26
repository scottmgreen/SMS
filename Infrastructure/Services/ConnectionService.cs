//-----------------------------------------------------------------------
// <copyright file="ConnectionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Connection service managing database connectivity, connection pooling, and performance monitoring.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using System.Net;
using System.Net.Sockets;

using Microsoft.AspNetCore.Http;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;
public class ConnectionService : IConnectionService
{
    private string _ipAddress = "Unknown IP";
    private string _hostName = "Unknown Host";
    private readonly IHttpContextAccessor _httpContextAccessor;


    public ConnectionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        //var httpContext = _httpContextAccessor.HttpContext;
        //_ipAddress = GetRemoteIpAddress(httpContext);
        //_hostName = GetRemoteHostName(httpContext);
    }
    // Set connection information (called by middleware)
    public void SetConnectionInfo(string ipAddress, string hostName)
    {

        _ipAddress = ipAddress;
        _hostName = hostName;
    }

    // Get connection information
    public (string IpAddress, string HostName) GetConnectionInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            _ipAddress = GetRemoteIpAddress(httpContext);
            _hostName = GetRemoteHostName(httpContext);
        }

        return (_ipAddress, _hostName);
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
