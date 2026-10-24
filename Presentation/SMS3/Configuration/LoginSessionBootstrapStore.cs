using System.Collections.Concurrent;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Configuration;

public interface ILoginSessionBootstrapStore
{
    string CreateTicket(BaseUser user, SMSUserType userType, TimeSpan? ttl = null);
    bool TryConsumeTicket(string ticket, out (BaseUser User, SMSUserType UserType) payload);
}

public sealed class LoginSessionBootstrapStore : ILoginSessionBootstrapStore
{
    private readonly ConcurrentDictionary<string, (BaseUser User, SMSUserType UserType, DateTime ExpiresUtc)> _tickets = new();

    public string CreateTicket(BaseUser user, SMSUserType userType, TimeSpan? ttl = null)
    {
        var ticket = Guid.NewGuid().ToString("N");
        var expiresUtc = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(2));
        _tickets[ticket] = (user, userType, expiresUtc);
        return ticket;
    }

    public bool TryConsumeTicket(string ticket, out (BaseUser User, SMSUserType UserType) payload)
    {
        payload = default;

        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        if (!_tickets.TryRemove(ticket, out var stored))
        {
            return false;
        }

        if (stored.ExpiresUtc < DateTime.UtcNow)
        {
            return false;
        }

        payload = (stored.User, stored.UserType);
        return true;
    }
}
