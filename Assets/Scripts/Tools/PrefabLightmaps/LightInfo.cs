using System;
using UnityEngine;

namespace Assets.Scripts.PrefabLightmaps
{
    [Serializable]
    public struct LightInfo
    {
        public Light Light;
        public int LightmapBakeType;
        public int MixedLightingMode;
    }
}