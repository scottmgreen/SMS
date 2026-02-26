//-----------------------------------------------------------------------
// <copyright file="ILogSupport.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Logging support interface defining structured logging operations with correlation and audit trail support.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

// -----------------------------------------------------------------------------
// <copyright file="ILogSupport.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Interface for log support utilities, providing log header generation methods.
// </copyright>
// ----------------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Interface for log support utilities, providing log header generation methods.
/// </summary>
public interface ILogSupport
{
    /// <summary>
    /// Generates a log header containing the current user and IP address.
    /// </summary>
    /// <returns>A string representing the log header.</returns>
    string GenerateLogHeader();

    /// <summary>
    /// Generates a log header containing the current user, IP address, and timestamp.
    /// </summary>
    /// <returns>A string representing the log header with timestamp.</returns>
    string GenerateLogHeaderWithTimestamp();
}

