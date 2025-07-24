

namespace CBT3_Application.Interfaces;

public interface IMessenger
{
    void Subscribe<TEvent>(Action<TEvent> action);
    void Unsubscribe<TEvent>(Action<TEvent> action);
    void Publish<TEvent>(TEvent @event);
}
