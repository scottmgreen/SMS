using System;
using System.Linq;

using CBT3_Infrastructure.Interfaces;


namespace CBT3_Infrastructure.Configuration;

public class UserDetailsFactory : IUserDetailsFactory
{
    private readonly IConnectionService _connectionService;

    public UserDetailsFactory(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }
    public UserDetails GetUserDetails()
    {
        //var connectionInfo = _connectionService.GetConnectionInfo();

        return new UserDetails(_connectionService);
    }
}
