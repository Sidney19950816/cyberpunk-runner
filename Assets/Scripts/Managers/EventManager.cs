using System;
using System.Collections.Generic;

public class EventManager
{
    private static Dictionary<string, List<Action<object>>> _eventListeners = new Dictionary<string, List<Action<object>>>();

    public static void TriggerEvent(string eventName, object eventData = null)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            foreach (var listener in _eventListeners[eventName])
            {
                listener(eventData);
            }
        }
    }

    public static void StartListening(string eventName, Action<object> listener)
    {
        if (!_eventListeners.ContainsKey(eventName))
        {
            _eventListeners.Add(eventName, new List<Action<object>>());
        }
        _eventListeners[eventName].Add(listener);
    }

    public static void StopListening(string eventName, Action<object> listener)
    {
        if (_eventListeners.ContainsKey(eventName))
        {
            _eventListeners[eventName].Remove(listener);
        }
    }
}