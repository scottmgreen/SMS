//-----------------------------------------------------------------------
// <copyright file="ProtocolDetectionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for protocol detection and authentication compatibility.
//                  Handles HTTP/HTTPS detection and configuration-based overrides.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Service for protocol detection and authentication compatibility
/// Handles HTTP/HTTPS detection and configuration-based overrides
/// </summary>
public class ProtocolDetectionService : IProtocolDetectionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationConfiguration _authConfig;
    private readonly ILogger<ProtocolDetectionService> _logger;

    public ProtocolDetectionService(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationConfiguration authConfig,
        ILogger<ProtocolDetectionService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _authConfig = authConfig;
        _logger = logger;
    }

    /// <summary>
    /// Get the current protocol being used (HTTP or HTTPS)
    /// </summary>
    public string GetCurrentProtocol()
    {
        try
        {
            // Check for override first
            if (!string.IsNullOrEmpty(_authConfig.OverrideProtocol))
            {
                _logger.LogApplicationDebug("Using protocol override: {OverrideProtocol}", _authConfig.OverrideProtocol);
                return _authConfig.OverrideProtocol.ToUpper();
            }

            // Auto-detect from HttpContext
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogApplicationWarning("HttpContext not available for protocol detection, defaulting to HTTPS");
                return "HTTPS";
            }

            var detectedProtocol = context.Request.IsHttps ? "HTTPS" : "HTTP";
            _logger.LogApplicationDebug("Detected protocol: {Protocol}", detectedProtocol);
            return detectedProtocol;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error detecting protocol, defaulting to HTTPS");
            return "HTTPS";
        }
    }

    /// <summary>
    /// Check if the current protocol matches configuration requirements
    /// </summary>
    public bool IsProtocolCompatible()
    {
        try
        {
            var currentProtocol = GetCurrentProtocol();
            var requiredProtocol = _authConfig.RequiredProtocol?.ToUpper() ?? "HTTPS";

            if (currentProtocol == requiredProtocol)
            {
                return true;
            }

            // Check if fallback is allowed
            if (_authConfig.AllowProtocolFallback)
            {
                _logger.LogApplicationInformation("Protocol fallback allowed - Current: {Current}, Required: {Required}", 
                    currentProtocol, requiredProtocol);
                return true;
            }

            _logger.LogApplicationWarning("Protocol mismatch - Current: {Current}, Required: {Required}, Fallback: {AllowFallback}", 
                currentProtocol, requiredProtocol, _authConfig.AllowProtocolFallback);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error checking protocol compatibility");
            return false;
        }
    }

    /// <summary>
    /// Get the effective protocol considering overrides and fallbacks
    /// </summary>
    public string GetEffectiveProtocol()
    {
        try
        {
            var currentProtocol = GetCurrentProtocol();
            var requiredProtocol = _authConfig.RequiredProtocol?.ToUpper() ?? "HTTPS";

            // If override is set, use it
            if (!string.IsNullOrEmpty(_authConfig.OverrideProtocol))
            {
                return _authConfig.OverrideProtocol.ToUpper();
            }

            // If current matches required, use current
            if (currentProtocol == requiredProtocol)
            {
                return currentProtocol;
            }

            // If fallback is allowed, use current
            if (_authConfig.AllowProtocolFallback)
            {
                _logger.LogApplicationInformation("Using fallback protocol: {CurrentProtocol} (required: {RequiredProtocol})", 
                    currentProtocol, requiredProtocol);
                return currentProtocol;
            }

            // Default to required protocol
            _logger.LogApplicationWarning("Forcing required protocol: {RequiredProtocol} (current: {CurrentProtocol})", 
                requiredProtocol, currentProtocol);
            return requiredProtocol;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error determining effective protocol, defaulting to HTTPS");
            return "HTTPS";
        }
    }

    /// <summary>
    /// Check if session-based authentication can work with current protocol
    /// </summary>
    public bool CanUseSessionAuth()
    {
        try
        {
            var effectiveProtocol = GetEffectiveProtocol();
            
            // Session auth works with both HTTP and HTTPS
            // But secure cookies only work with HTTPS
            var canUseSecure = CanUseSecureCookies();
            
            _logger.LogApplicationDebug("Session auth compatibility - Protocol: {Protocol}, SecureCookies: {CanUseSecure}", 
                effectiveProtocol, canUseSecure);
                
            return true; // Session auth can work with any protocol, but security varies
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error checking session auth compatibility");
            return false;
        }
    }

    /// <summary>
    /// Check if secure cookies can be used with current protocol
    /// </summary>
    public bool CanUseSecureCookies()
    {
        try
        {
            var effectiveProtocol = GetEffectiveProtocol();
            var canUse = effectiveProtocol == "HTTPS";
            
            _logger.LogApplicationDebug("Secure cookies compatibility - Protocol: {Protocol}, CanUse: {CanUse}", 
                effectiveProtocol, canUse);
                
            return canUse;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error checking secure cookies compatibility");
            return false;
        }
    }

    /// <summary>
    /// Get protocol information for logging/debugging
    /// </summary>
    public string GetProtocolInfo()
    {
        try
        {
            var current = GetCurrentProtocol();
            var effective = GetEffectiveProtocol();
            var compatible = IsProtocolCompatible();
            var sessionOk = CanUseSessionAuth();
            var secureOk = CanUseSecureCookies();

            return $"Current: {current}, Effective: {effective}, Compatible: {compatible}, " +
                   $"SessionAuth: {sessionOk}, SecureCookies: {secureOk}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}

