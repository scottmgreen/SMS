//-----------------------------------------------------------------------
// <copyright file="ApplicationEventIds.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Defines standardized event IDs for application layer logging operations.
//                  Provides consistent event identification for structured logging across
//                  all application services and handlers.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Common;

/// <summary>
/// Application Layer Event IDs for Structured Logging
/// Provides standardized event identifiers for logging operations across all application services.
/// 
/// Methods/Constants:
/// - Critical: High-severity events requiring immediate attention
/// - Debug: Development and troubleshooting information
/// - Error: Error conditions and exceptions
/// - Information: General informational messages
/// - None: Default/no specific event type
/// - Trace: Detailed execution flow information
/// - Warning: Warning conditions that don't halt execution
/// </summary>
public static class ApplicationEventIds
{
    public const int Critical = 1000;
    public const int Debug = 1001;
    public const int Error = 1002;
    public const int Information = 1003;
    public const int None = 1004;
    public const int Trace = 1005;
    public const int Warning = 1006;
}
