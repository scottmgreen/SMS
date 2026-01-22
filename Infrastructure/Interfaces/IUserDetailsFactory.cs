using SMS_Infrastructure.Configuration;
namespace SMS_Infrastructure.Interfaces;

public interface IUserDetailsFactory
{
    UserDetails GetUserDetails();
}