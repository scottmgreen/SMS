//-----------------------------------------------------------------------
// <copyright file="MessengerService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Services;

//public sealed class MessengerService : IMessenger
//{
//    private readonly Dictionary<Type, List<object>> _subscriptions = new Dictionary<Type, List<object>>();

//    public void Subscribe<TEvent>(Action<TEvent> action)
//    {
//        var eventType = typeof(TEvent);
//        if (!_subscriptions.ContainsKey(eventType))
//        {
//            _subscriptions[eventType] = new List<object>();
//        }
//        _subscriptions[eventType].Add(action);
//    }
//    public void Unsubscribe<TEvent>(Action<TEvent> action)
//    {
//        var eventType = typeof(TEvent);
//        if (_subscriptions.ContainsKey(eventType))
//        {
//            _subscriptions[eventType].Remove(action);
//        }
//    }
//    public void Publish<TEvent>(TEvent @event)
//    {
//        //Console.WriteLine($"EventAggregator Publishing Event Type {@event.GetType().ToString()}");
//        var eventType = typeof(TEvent);
//        if (_subscriptions.ContainsKey(eventType))
//        {
//            foreach (var subscriber in _subscriptions[eventType])
//            {
//                ((Action<TEvent>)subscriber)(@event);
//            }
//        }
//    }
//}


//public sealed class MessengerService : IMessenger
//{
//    private readonly object _lock = new();
//    private readonly Dictionary<Type, List<Delegate>> _subscriptions = new();

//    public void Subscribe<TEvent>(Action<TEvent> action)
//    {
//        var eventType = typeof(TEvent);

//        lock (_lock)
//        {
//            if (!_subscriptions.TryGetValue(eventType, out var handlers))
//            {
//                handlers = new List<Delegate>();
//                _subscriptions[eventType] = handlers;
//            }

//            // Prevent duplicate subscriptions
//            if (!handlers.Contains(action))
//            {
//                handlers.Add(action);
//            }
//        }
//    }

//    public void Unsubscribe<TEvent>(Action<TEvent> action)
//    {
//        var eventType = typeof(TEvent);

//        lock (_lock)
//        {
//            if (_subscriptions.TryGetValue(eventType, out var handlers))
//            {
//                handlers.RemoveAll(h => h.Equals(action));
//                if (handlers.Count == 0)
//                {
//                    _subscriptions.Remove(eventType);
//                }
//            }
//        }
//    }

//    public void Publish<TEvent>(TEvent @event)
//    {
//        var eventType = typeof(TEvent);
//        List<Delegate>? handlersCopy = null;

//        lock (_lock)
//        {
//            if (_subscriptions.TryGetValue(eventType, out var handlers))
//            {
//                handlersCopy = handlers.ToList(); // defensive copy
//            }
//        }

//        if (handlersCopy != null)
//        {
//            foreach (var handler in handlersCopy)
//            {
//                try
//                {
//                    ((Action<TEvent>)handler)(@event);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Exception in subscriber for event {eventType.Name}: {ex}");
//                    // Optional: publish an error event or log via logger
//                }
//            }
//        }
//    }
//}



public sealed class MessengerService : IMessenger
{
    private readonly Dictionary<Type, Delegate> _subscriptions = new();
    private readonly object _lock = new();

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(type, out var existing))
            {
                // If the exact same handler is already subscribed, skip
                if (existing == (Delegate)handler)
                    return;
            }

            _subscriptions[type] = handler;
        }
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(type, out var existing) && existing == (Delegate)handler)
                _subscriptions.Remove(type);
        }
    }

    public void Publish<TEvent>(TEvent @event)
    {
        var type = typeof(TEvent);
        if (_subscriptions.TryGetValue(type, out var handler))
        {
            ((Action<TEvent>)handler)(@event);
        }
    }
}

