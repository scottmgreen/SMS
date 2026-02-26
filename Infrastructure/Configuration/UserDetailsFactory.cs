//-----------------------------------------------------------------------
// <copyright file="UserDetailsFactory.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure configuration component providing system setup, service registration, and dependency management.
//                  Infrastructure configuration providing dependency injection,
//                  service registration, and system setup.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Infrastructure.Interfaces;


namespace SMS_Infrastructure.Configuration;

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

