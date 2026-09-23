namespace SMS3.Components.Shared.UIHelpers;

public interface INotificationBellRefreshService
{
    event Action? RefreshRequested;
    void RequestRefresh();
}

public sealed class NotificationBellRefreshService : INotificationBellRefreshService
{
    public event Action? RefreshRequested;

    public void RequestRefresh()
    {
        RefreshRequested?.Invoke();
    }
}
