using CBT3_Infrastructure.Configuration;

using CBT3_Shared;

using System;

using System.Linq;
namespace CBT3_Infrastructure.Interfaces;

public interface IUserDetailsFactory
{
    UserDetails GetUserDetails();
}