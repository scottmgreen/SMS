using SMS_Infrastructure.Configuration;

using SMS_Shared;

using System;

using System.Linq;
namespace SMS_Infrastructure.Interfaces;

public interface IUserDetailsFactory
{
    UserDetails GetUserDetails();
}