using UnityEngine.Events;
using UnityEngine.UI;

public static class ButtonExtensions
{
    /// <summary>
    /// Adds a Unity action to the Button's onClick event.
    /// </summary>
    /// <param name="button">The Button to add the action to.</param>
    /// <param name="action">The Unity action to be triggered when the Button is clicked.</param>
    public static void Add(this Button button, UnityAction action)
        => button.onClick.AddListener(action);

    /// <summary>
    /// Removes a Unity action from the Button's onClick event.
    /// </summary>
    /// <param name="button">The Button to remove the action from.</param>
    /// <param name="action">The Unity action to be removed from the Button's onClick event.</param>
    public static void Remove(this Button button, UnityAction action)
        => button.onClick.RemoveListener(action);

    /// <summary>
    /// Removes all Unity actions from the Button's onClick event.
    /// </summary>
    /// <param name="button">The Button from which all actions will be removed.</param>
    public static void RemoveAll(this Button button)
        => button.onClick.RemoveAllListeners();
}