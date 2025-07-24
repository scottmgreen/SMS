using System;
using System.Linq;

namespace CBT3_Infrastructure.Interfaces;
public interface IConnectionService
{
    void SetConnectionInfo(string ipAddress, string hostName);
    (string IpAddress, string HostName) GetConnectionInfo();
}
