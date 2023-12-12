using UnityEngine;

public static class FloatExtensions
{
    public static int ToInt(this float floatValue)
    {
        // You can implement your own logic here, such as rounding, flooring, or ceiling
        return Mathf.RoundToInt(floatValue);
    }
}