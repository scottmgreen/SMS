using Microsoft.AspNetCore.SignalR;
using Radzen;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS3.Components.Shared;
using SMS3.Hubs.Contracts;

namespace SMS3.Hubs;

public class LiveUpdatesPublisher : ILiveUpdatesPublisher
{
    private readonly IHubContext<LiveUpdatesHub> _hubContext;
    private readonly IActiveUserSessionRegistry _activeUserSessionRegistry;

    public LiveUpdatesPublisher(
        IHubContext<LiveUpdatesHub> hubContext,
        IActiveUserSessionRegistry activeUserSessionRegistry)
    {
        _hubContext = hubContext;
        _activeUserSessionRegistry = activeUserSessionRegistry;
    }

    public async Task PublishToApplicationUsersAsync(LiveUpdateEnvelope envelope)
    {
        await _hubContext
            .Clients
            .Group(LiveUpdateHubConstants.ApplicationUsersGroup)
            .SendAsync("LiveUpdate", envelope);

        var applicationUsers = _activeUserSessionRegistry
            .GetActiveSessions()
            .Where(s => string.Equals(s.UserType, SMSUserType.Application.Value, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.UserCode)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var userCode in applicationUsers)
        {
            var defaultMessage = envelope.EventName == LiveUpdateEventNames.ReportReturnedToValidation
                ? "Report returned to validation and ready for processing"
                : "New report submitted";

            await EventBusDispatcher.DispatchNotificationAsync(
                NotificationSeverity.Info,
                envelope.EventName,
                envelope.Message ?? defaultMessage,
                5000,
                targetUserCode: userCode);
        }
    }

    public async Task PublishToApplicationUserAsync(string userCode, LiveUpdateEnvelope envelope)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return;
        }

        await _hubContext
            .Clients
            .Group(LiveUpdateHubConstants.GetUserGroup(userCode))
            .SendAsync("LiveUpdate", envelope);

        var fallbackMessage = envelope.EventName == LiveUpdateEventNames.ReportReturnedToValidation
            ? "Report returned to validation and ready for processing"
            : "New report submitted";

        await EventBusDispatcher.DispatchNotificationAsync(
            NotificationSeverity.Info,
            envelope.EventName,
            envelope.Message ?? fallbackMessage,
            5000,
            targetUserCode: userCode);
    }
}
