using System;
using UnityEngine;

namespace Assets.Scripts.PrefabLightmaps
{
    [Serializable]
    public struct RendererInfo
    {
        public Renderer Renderer;
        public int LightmapIndex;
        public Vector4 LightmapOffsetScale;
    }
}