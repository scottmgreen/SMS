// -----------------------------------------------------------------------------
// <copyright file="LogSupport.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Provides log header generation utilities for logging user and timestamp information.
// </copyright>
// -----------------------------------------------------------------------------

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Configuration;

/// <summary>
/// Provides log header generation utilities for logging user and timestamp information.
/// </summary>
public class LogSupport : ILogSupport
{
    private IUserDetailsFactory _iUserDetailsFactory;
    private UserDetails _userDetails;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSupport"/> class.
    /// </summary>
    /// <param name="iUserDetailsFactory">The user details factory.</param>
    public LogSupport(IUserDetailsFactory iUserDetailsFactory)
    {
        _iUserDetailsFactory = iUserDetailsFactory;
        _userDetails = _iUserDetailsFactory.GetUserDetails();
    }

    /// <summary>
    /// Generates a log header containing the current user and IP address.
    /// </summary>
    /// <returns>A string representing the log header.</returns>
    public string GenerateLogHeader()
    {
        string currentUser = _userDetails.GetRemoteHostName();
        string currentUserIp = _userDetails.GetRemoteIpAddress();

        return $"{currentUser} {currentUserIp} =>";
    }

    /// <summary>
    /// Generates a log header containing the current user, IP address, and timestamp.
    /// </summary>
    /// <returns>A string representing the log header with timestamp.</returns>
    public string GenerateLogHeaderWithTimestamp()
    {
        string currentUser = _userDetails.GetRemoteHostName();
        string currentUserIp = _userDetails.GetRemoteIpAddress();
        string timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format for logs

        return $"{timestamp} {currentUser} {currentUserIp} =>";
    }
}
