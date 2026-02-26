//-----------------------------------------------------------------------
// <copyright file="CurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// Implementation of ICurrentUserService using SMS Session data
/// Works with the existing SMS authentication system
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string UserId => GetUserId();

    public string DisplayName => GetSessionValue("SMS_DisplayName") ?? GetSessionValue("SMS_UserId") ?? "SYSTEM";

    public bool IsAuthenticated => !string.IsNullOrEmpty(GetSessionValue("SMS_UserId"));

    private string GetUserId()
    {
        var userId = GetSessionValue("SMS_UserId");

        // ?? VERBOSE LOGGING for debugging
        _logger.LogInformation("?? CurrentUserService.GetUserId() called");
        _logger.LogInformation("?? Session SMS_UserId: '{UserId}'", userId ?? "NULL");
        _logger.LogInformation("?? Session SMS_DisplayName: '{DisplayName}'", GetSessionValue("SMS_DisplayName") ?? "NULL");
        _logger.LogInformation("?? Session IsAuthenticated: '{IsAuth}'", GetSessionValue("IsAuthenticated") ?? "NULL");

        // Check all session keys for debugging
        if (_httpContextAccessor.HttpContext?.Session != null)
        {
            _logger.LogInformation("?? Available session keys: {Keys}",
                string.Join(", ", _httpContextAccessor.HttpContext.Session.Keys));
        }

        return userId ?? "SYSTEM";
    }

    private string? GetSessionValue(string key)
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(key) ??
               _httpContextAccessor.HttpContext?.Items[key]?.ToString();
    }
}
