using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.PrefabLightmaps
{
    [ExecuteAlways]
    public sealed class PrefabLightmapData : MonoBehaviour
    {
        [SerializeField] private RendererInfo[] _rendererInfo;
        [SerializeField] private Texture2D[] _lightmaps;
        [SerializeField] private Texture2D[] _lightmapsDir;
        [SerializeField] private Texture2D[] _shadowMasks;
        [SerializeField] private LightInfo[] _lightInfo;

        [UsedImplicitly]
        private void Awake()
        {
            Init();
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [UsedImplicitly]
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Init();
        }

        private void Init()
        {
            if (_rendererInfo == null || _rendererInfo.Length == 0)
                return;

            var lightmaps = LightmapSettings.lightmaps;
            var offsetIndexes = new int[_lightmaps.Length];
            var countTotal = lightmaps.Length;
            var combinedLightmaps = new List<LightmapData>();

            for (var i = 0; i < _lightmaps.Length; i++)
            {
                var exists = false;
                for (var j = 0; j < lightmaps.Length; j++)
                {

                    if (_lightmaps[i] == lightmaps[j].lightmapColor)
                    {
                        exists = true;
                        offsetIndexes[i] = j;
                    }

                }

                if (exists) 
                    continue;

                offsetIndexes[i] = countTotal;
                var newLightmapData = new LightmapData
                {
                    lightmapColor = _lightmaps[i],
                    lightmapDir = _lightmapsDir.Length == _lightmaps.Length ? _lightmapsDir[i] : default,
                    shadowMask = _shadowMasks.Length == _lightmaps.Length ? _shadowMasks[i] : default,
                };

                combinedLightmaps.Add(newLightmapData);

                countTotal += 1;

            }

            var combinedLightmaps2 = new LightmapData[countTotal];

            lightmaps.CopyTo(combinedLightmaps2, 0);
            combinedLightmaps.ToArray().CopyTo(combinedLightmaps2, lightmaps.Length);

            var directional = _lightmapsDir.All(t => t != null);

            LightmapSettings.lightmapsMode = (_lightmapsDir.Length == _lightmaps.Length && directional) ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;
            ApplyRendererInfo(offsetIndexes, _rendererInfo, _lightInfo);
            LightmapSettings.lightmaps = combinedLightmaps2;
        }
        
        private static void ApplyRendererInfo(
            IReadOnlyList<int> lightmapOffsetIndex,
            IEnumerable<RendererInfo> infos, 
            IList<LightInfo> lightsInfo)
        {
            foreach (var info in infos)
            {
                info.Renderer.lightmapIndex = lightmapOffsetIndex[info.LightmapIndex];
                info.Renderer.lightmapScaleOffset = info.LightmapOffsetScale;

                // You have to release shaders.
                var mats = info.Renderer.sharedMaterials;
                foreach (var mat in mats)
                {
                    if (mat != null && Shader.Find(mat.shader.name) != null)
                        mat.shader = Shader.Find(mat.shader.name);
                }
            }

            for (var i = 0; i < lightsInfo.Count; i++)
            {
                var bakingOutput = new LightBakingOutput
                {
                    isBaked = true,
                    lightmapBakeType = (LightmapBakeType)lightsInfo[i].LightmapBakeType,
                    mixedLightingMode = (MixedLightingMode)lightsInfo[i].MixedLightingMode
                };

                lightsInfo[i].Light.bakingOutput = bakingOutput;
            }
        }

#if UNITY_EDITOR
        [UsedImplicitly]
        [MenuItem("Assets/Bake Prefab Lightmaps")]
        private static void GenerateLightmapInfo()
        {
            if (Lightmapping.giWorkflowMode != Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                return;
            }

            Lightmapping.Bake();

            var prefabs = FindObjectsOfType<PrefabLightmapData>();

            foreach (var instance in prefabs)
            {
                var gameObject = instance.gameObject;
                var rendererInfos = new List<RendererInfo>();
                var lightmaps = new List<Texture2D>();
                var lightmapsDir = new List<Texture2D>();
                var shadowMasks = new List<Texture2D>();
                var lightsInfos = new List<LightInfo>();

                GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, lightmapsDir, shadowMasks, lightsInfos);

                instance._rendererInfo = rendererInfos.ToArray();
                instance._lightmaps = lightmaps.ToArray();
                instance._lightmapsDir = lightmapsDir.ToArray();
                instance._lightInfo = lightsInfos.ToArray();
                instance._shadowMasks = shadowMasks.ToArray();

                var targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject);

                if (targetPrefab == null) 
                    continue;

                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance.gameObject);

                // IsPartOfPrefabInstance
                if (root != null)
                {
                    var rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
                    var rootPath = AssetDatabase.GetAssetPath(rootPrefab);

                    PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);

                    try
                    {
                        PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                    }
                    finally
                    {
                        PrefabUtility.SaveAsPrefabAssetAndConnect(root, rootPath, InteractionMode.AutomatedAction);
                    }
                }
                else
                {
                    PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                }
            }
        }

        private static void GenerateLightmapInfo(
            GameObject root, 
            ICollection<RendererInfo> rendererInfos, 
            IList<Texture2D> lightmaps, 
            ICollection<Texture2D> lightmapsDir, 
            ICollection<Texture2D> shadowMasks, 
            List<LightInfo> lightsInfo)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in renderers)
            {
                if (renderer.lightmapIndex == -1) 
                    continue;

                if (renderer.lightmapScaleOffset == Vector4.zero) 
                    continue;

                //1ibrium's pointed out this issue : https://docs.unity3d.com/ScriptReference/Renderer-lightmapIndex.html
                if (renderer.lightmapIndex is < 0 or 0xFFFE) 
                    continue;

                var lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                var lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                var shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                var info = new RendererInfo
                {
                    Renderer = renderer,
                    LightmapOffsetScale = renderer.lightmapScaleOffset,
                    LightmapIndex = lightmaps.IndexOf(lightmap)
                };

                if (info.LightmapIndex == -1)
                {
                    info.LightmapIndex = lightmaps.Count;
                    lightmaps.Add(lightmap);
                    lightmapsDir.Add(lightmapDir);
                    shadowMasks.Add(shadowMask);
                }

                rendererInfos.Add(info);
            }

            lightsInfo.AddRange(
                root.GetComponentsInChildren<Light>(true)
                    .Select(l => new LightInfo
                    {
                        Light = l,
                        LightmapBakeType = (int)l.lightmapBakeType,
                        MixedLightingMode = (int)Lightmapping.lightingSettings.mixedBakeMode
                    }));
        }
#endif
    }
}