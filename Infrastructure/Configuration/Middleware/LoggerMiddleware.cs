//-----------------------------------------------------------------------
// <copyright file="LoggerMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: ASP.NET Core middleware providing request/response logging with correlation IDs and performance metrics.
//                  ASP.NET Core middleware component providing cross-cutting
//                  concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------


//-----------------------------------------------------------------------
// <copyright file="LoggerMiddleware.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: ASP.NET Core middleware providing request/response logging with correlation IDs and performance metrics.
//                  ASP.NET Core middleware component providing cross-cutting
//                  concerns in the HTTP request pipeline.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;

namespace SMS_Infrastructure.Configuration.Middleware
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggerMiddleware> _logger;

        public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var start = TimeProvider.System.GetTimestamp();
            try
            { await _next(context); }
            finally
            {
                var diff = TimeProvider.System.GetElapsedTime(start);
                _logger.LogInformation("Request took {Duration}ms", diff.TotalMilliseconds);
            }
        }

    }
}

