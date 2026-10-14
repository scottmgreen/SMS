using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS3.Hubs.Contracts;

namespace SMS3.Hubs;

[Authorize]
public class LiveUpdatesHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IActiveUserSessionRegistry _activeUserSessionRegistry;

    public LiveUpdatesHub(
        ICurrentUserService currentUserService,
        IActiveUserSessionRegistry activeUserSessionRegistry)
    {
        _currentUserService = currentUserService;
        _activeUserSessionRegistry = activeUserSessionRegistry;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUserService.IsAuthenticated)
        {
            var userCode = _currentUserService.UserCode ?? string.Empty;
            var userType = _currentUserService.UserType ?? string.Empty;
            var sessionKey = BuildConnectionSessionKey();

            _activeUserSessionRegistry.UpsertSession(new ActiveUserSessionInfo
            {
                SessionKey = sessionKey,
                UserCode = userCode,
                DisplayName = _currentUserService.UserDisplayName,
                UserType = userType,
                LoginTimeUtc = DateTime.Now,
                LastSeenUtc = DateTime.Now
            });

            if (!string.IsNullOrWhiteSpace(userCode))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, LiveUpdateHubConstants.GetUserGroup(userCode));
            }

            if (string.Equals(userType, SMSUserType.Application.Value, StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, LiveUpdateHubConstants.ApplicationUsersGroup);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _activeUserSessionRegistry.RemoveSession(BuildConnectionSessionKey());
        await base.OnDisconnectedAsync(exception);
    }

    private string BuildConnectionSessionKey()
    {
        return $"signalr:{Context.ConnectionId}";
    }
}
