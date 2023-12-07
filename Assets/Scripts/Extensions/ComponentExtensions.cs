using UnityEngine;

public static class ComponentExtensions
{
    /// <summary>
    /// Activates the GameObject associated with the Component.
    /// </summary>
    /// <param name="component">The Component whose GameObject will be activated.</param>
    public static void Activate(this Component component) 
        => component.gameObject.SetActive(true);

    /// <summary>
    /// Deactivates the GameObject associated with the Component.
    /// </summary>
    /// <param name="component">The Component whose GameObject will be deactivated.</param>
    public static void Deactivate(this Component component) 
        => component.gameObject.SetActive(false);
}