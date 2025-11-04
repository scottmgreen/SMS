using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SMS_Infrastructure.Configuration
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
