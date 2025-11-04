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
