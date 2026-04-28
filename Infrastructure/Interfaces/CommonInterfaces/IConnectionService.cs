//-----------------------------------------------------------------------
// <copyright file="IConnectionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service interface defining operations for connection functionality with dependency injection support.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;
public interface IConnectionService
{
    void SetConnectionInfo(string ipAddress, string hostName);
    (string IpAddress, string HostName) GetConnectionInfo();
}

