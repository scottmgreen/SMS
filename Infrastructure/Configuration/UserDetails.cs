//using System;
//using System.Net.Sockets;
//using System.Security.Principal;

//using Microsoft.AspNetCore.Http;

//namespace SMS_Shared;

//public class UserDetails
//{
//    private readonly IHttpContextAccessor _contextaccessor;

//    public UserDetails(IHttpContextAccessor contextaccessor)
//    {
//        _contextaccessor = contextaccessor;


//    }

//    public string GetRemoteHostName()
//    {
//        var httpContext = _contextaccessor?.HttpContext;

//        // Check for proxy headers
//        string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
//                        ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
//                        ?? httpContext.Connection?.RemoteIpAddress?.MapToIPv4().ToString();

//        IPHostEntry hostentry = null;

//        if (ipAddress == "0.0.0.1" || ipAddress == "::1" || ipAddress == "127.0.0.1")
//        {
//            hostentry = Dns.GetHostEntry(Dns.GetHostName());
//        }
//        else
//        {
//            hostentry = System.Net.Dns.GetHostEntry(ipAddress);
//        }

//        string[] parts = hostentry.HostName.Split('.');
//        return parts[0] ?? "Unknown Host";
//    }

//    public string GetRemoteIpAddress()
//    {
//        var httpContext = _contextaccessor?.HttpContext;

//        if (httpContext == null)
//            return "Unknown IP";

//           // Check for proxy headers
//            string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
//                            ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
//                            ?? httpContext.Connection?.RemoteIpAddress?.MapToIPv4().ToString();



//        if (ipAddress == "0.0.0.1" || ipAddress == "::1" || ipAddress == "127.0.0.1")
//        {
//            return ipAddress; //Local Host situtation//
//        }
//        else
//        {
//            var host = System.Net.Dns.GetHostEntry(ipAddress);
//            string ipaddress = string.Empty;
//            foreach (var ip in host.AddressList)
//            {
//                if (ip.AddressFamily == AddressFamily.InterNetwork)
//                {
//                    ipaddress =  ip.ToString();
//                }
//            }
//            return ipaddress;
//        }
//    }

//}
//-----------------REFACTORED-----
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using SMS_Infrastructure.Interfaces;

using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Configuration
{
    //public class UserDetails
    //{
    //    private readonly IHttpContextAccessor _contextAccessor;

    //    public UserDetails(IHttpContextAccessor contextAccessor)
    //    {
    //        _contextAccessor = contextAccessor;
    //    }

    //    public string GetRemoteHostName()
    //    {
    //        var httpContext = _contextAccessor?.HttpContext;
    //        if (httpContext == null)
    //            return "Unknown Host";

    //        string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
    //                        ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
    //                        ?? httpContext.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString();

    //        try
    //        {
    //            IPHostEntry hostEntry = ipAddress switch
    //            {
    //                "0.0.0.1" or "::1" or "127.0.0.1" => Dns.GetHostEntry(Dns.GetHostName()),
    //                _ => Dns.GetHostEntry(ipAddress)
    //            };

    //            return hostEntry.HostName.Split('.').FirstOrDefault() ?? "Unknown Host";
    //        }
    //        catch
    //        {
    //            return "Unknown Host";
    //        }
    //    }

    //    public string GetRemoteIpAddress()
    //    {
    //        var httpContext = _contextAccessor?.HttpContext;
    //        if (httpContext == null)
    //            return "Unknown IP";

    //        string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()
    //                        ?? httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
    //                        ?? httpContext.Connection?.RemoteIpAddress?.ToString();

    //        if (ipAddress == "0.0.0.1" || ipAddress == "::1" || ipAddress == "127.0.0.1")
    //        {
    //            return ipAddress; // Localhost situation
    //        }

    //        try
    //        {
    //            var hostEntry = Dns.GetHostEntry(ipAddress);
    //            return hostEntry.AddressList
    //                            .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
    //                            ?.ToString() ?? "Unknown IP";
    //        }
    //        catch
    //        {
    //            return "Unknown IP";
    //        }
    //    }
    //}

    public class UserDetails
    {
        private readonly IConnectionService _connectionService;
        // Constructor accepts IP address and HostName as parameters
        public UserDetails(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public string GetRemoteHostName()
        {
            try
            {
                var connectionInfo = _connectionService.GetConnectionInfo();
                return connectionInfo.HostName;
            }
            catch
            {
                return "Unknown Host";
            }
        }

        public string GetRemoteIpAddress()
        {
            try
            {
                var connectionInfo = _connectionService.GetConnectionInfo();
                return connectionInfo.IpAddress;
            }
            catch
            {
                return "Unknown IP";
            }
        }
    }

}
