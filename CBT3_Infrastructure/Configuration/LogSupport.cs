using CBT3_Infrastructure.Interfaces;

namespace CBT3_Infrastructure.Configuration;

public class LogSupport : ILogSupport
{
    private IUserDetailsFactory _iUserDetailsFactory;
    private UserDetails _userDetails;
    public LogSupport(IUserDetailsFactory iUserDetailsFactory)
    {
        _iUserDetailsFactory = iUserDetailsFactory;
        _userDetails = _iUserDetailsFactory.GetUserDetails();

    }
    public string GenerateLogHeader()
    {
        string currentUser = _userDetails.GetRemoteHostName();
        string currentUserIp = _userDetails.GetRemoteIpAddress();

        return $"{currentUser} {currentUserIp} =>";
    }

    public string GenerateLogHeaderWithTimestamp()
    {
        string currentUser = _userDetails.GetRemoteHostName();
        string currentUserIp = _userDetails.GetRemoteIpAddress();
        string timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601 format for logs

        return $"{timestamp} {currentUser} {currentUserIp} =>";
    }
}
