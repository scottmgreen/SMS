namespace SMS3.Hubs.Contracts;

public interface ILiveUpdatesPublisher
{
    Task PublishToApplicationUsersAsync(LiveUpdateEnvelope envelope);
    Task PublishToApplicationUserAsync(string userCode, LiveUpdateEnvelope envelope);
}
