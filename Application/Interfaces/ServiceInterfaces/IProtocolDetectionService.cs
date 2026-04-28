//-----------------------------------------------------------------------
// <copyright file="IProtocolDetectionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service interface for protocol detection and authentication compatibility.
//                  Handles HTTP/HTTPS detection and configuration-based overrides.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for protocol detection and authentication compatibility
/// Handles HTTP/HTTPS detection and configuration-based overrides
/// </summary>
public interface IProtocolDetectionService
{
    /// <summary>
    /// Get the current protocol being used (HTTP or HTTPS)
    /// </summary>
    string GetCurrentProtocol();

    /// <summary>
    /// Check if the current protocol matches configuration requirements
    /// </summary>
    bool IsProtocolCompatible();

    /// <summary>
    /// Get the effective protocol considering overrides and fallbacks
    /// </summary>
    string GetEffectiveProtocol();

    /// <summary>
    /// Check if session-based authentication can work with current protocol
    /// </summary>
    bool CanUseSessionAuth();

    /// <summary>
    /// Check if secure cookies can be used with current protocol
    /// </summary>
    bool CanUseSecureCookies();

    /// <summary>
    /// Get protocol information for logging/debugging
    /// </summary>
    string GetProtocolInfo();
}