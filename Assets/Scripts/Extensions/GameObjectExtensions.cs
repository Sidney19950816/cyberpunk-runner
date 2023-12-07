using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class GameObjectExtensions
    {
        public static float GetHeight(this GameObject go, LayerMask ground) =>
            Physics.Raycast(new Ray(go.transform.position, Vector3.down), out var hit, Mathf.Infinity, ground)
                ? hit.distance
                : Mathf.Infinity;
    }
}