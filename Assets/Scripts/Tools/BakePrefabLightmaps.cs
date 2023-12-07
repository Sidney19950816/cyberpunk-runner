using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    [ExecuteAlways]
    [HelpURL("https://docs.google.com/document/d/1FFPvIzae2gbUTtNjSzJdJItWKz_EGdUrGY6WtVXz3V4/edit")]
    public class BakePrefabLightmaps : MonoBehaviour
    {
        [System.Serializable]
        struct RendererInfo
        {
            public Renderer renderer;
            public int lightmapIndex;
            public Vector4 lightmapOffsetScale;
        }

        [System.Serializable]
        struct LightInfo
        {
            public Light light;
            public int lightmapBaketype;
            public int mixedLightingMode;
        }

        [SerializeField] RendererInfo[] _rendererInfo;
        [SerializeField] Texture2D[] _lightmaps;
        [SerializeField] Texture2D[] _lightmapsDir;
        [SerializeField] Texture2D[] _shadowMasks;
        [SerializeField] LightInfo[] _lightInfo;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (_rendererInfo == null || _rendererInfo.Length == 0)
            {
                return;
            }

            LightmapData[] lightmaps = LightmapSettings.lightmaps;
            int[] offsetsIndexes = new int[_lightmaps.Length];
            int countTotal = lightmaps.Length;
            List<LightmapData> combinedLightmaps = new();

            for (int i = 0; i < _lightmaps.Length; i++)
            {
                bool lightmapDataExists = false;
                for (int j = 0; j < lightmaps.Length; j++)
                {
                    if (_lightmaps[i] == lightmaps[j].lightmapColor)
                    {
                        lightmapDataExists = true;
                        offsetsIndexes[i] = j;
                    }
                }
                if (!lightmapDataExists)
                {
                    offsetsIndexes[i] = countTotal;
                    var newLightmapData = new LightmapData
                    {
                        lightmapColor = _lightmaps[i],
                        lightmapDir = _lightmapsDir.Length == _lightmaps.Length ? _lightmapsDir[i] : default(Texture2D),
                        shadowMask = _shadowMasks.Length == _lightmaps.Length ? _shadowMasks[i] : default(Texture2D),
                    };

                    combinedLightmaps.Add(newLightmapData);
                    countTotal += 1;
                }
            }

            LightmapData[] combinedLightmaps2 = new LightmapData[countTotal];

            lightmaps.CopyTo(combinedLightmaps2, 0);
            combinedLightmaps.ToArray().CopyTo(combinedLightmaps2, lightmaps.Length);

            bool directional = true;

            foreach (Texture2D t in _lightmapsDir)
            {
                if (t == null)
                {
                    directional = false;
                    break;
                }
            }

            LightmapSettings.lightmapsMode = (_lightmapsDir.Length == _lightmaps.Length && directional) ? LightmapsMode.CombinedDirectional : LightmapsMode.NonDirectional;
            ApplyRendererInfo(_rendererInfo, offsetsIndexes, _lightInfo);
            LightmapSettings.lightmaps = combinedLightmaps2;
        }

        private static void ApplyRendererInfo(RendererInfo[] infos, int[] lightmapOffsetIndex, LightInfo[] lightsInfo)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                var info = infos[i];

                info.renderer.lightmapIndex = lightmapOffsetIndex[info.lightmapIndex];
                info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;

                // You have to release shaders.
                Material[] mat = info.renderer.sharedMaterials;
                for (int j = 0; j < mat.Length; j++)
                {
                    if (mat[j] != null && Shader.Find(mat[j].shader.name) != null)
                    {
                        mat[j].shader = Shader.Find(mat[j].shader.name);
                    }
                }
            }

            for (int i = 0; i < lightsInfo.Length; i++)
            {
                LightBakingOutput bakingOutput = new LightBakingOutput();
                bakingOutput.isBaked = true;
                bakingOutput.lightmapBakeType = (LightmapBakeType)lightsInfo[i].lightmapBaketype;
                bakingOutput.mixedLightingMode = (MixedLightingMode)lightsInfo[i].mixedLightingMode;

                lightsInfo[i].light.bakingOutput = bakingOutput;
            }
        }

        private void ResetLightmapData()
        {
            LightmapSettings.lightmaps = null;
        }

        public void SetLightmaps(bool state)
        {
            if (state)
            {
                Init();
            }
            else
            {
                ResetLightmapData();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Custom/Bake Prefab Lightmaps")]
        private static void GenerateLightmapInfo()
        {
            if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                return;
            }
            UnityEditor.Lightmapping.Bake();

            BakePrefabLightmaps[] prefabs = FindObjectsOfType<BakePrefabLightmaps>();

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

                var targetPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance.gameObject) as GameObject;
                if (targetPrefab != null)
                {
                    GameObject root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance.gameObject);
                    if (root != null)
                    {
                        GameObject rootPrefab = PrefabUtility.GetCorrespondingObjectFromSource(instance.gameObject);
                        string rootPath = AssetDatabase.GetAssetPath(rootPrefab);

                        PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
                        try
                        {
                            PrefabUtility.ApplyPrefabInstance(instance.gameObject, InteractionMode.AutomatedAction);
                        }
                        catch { }
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
        }

        private static void GenerateLightmapInfo(GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps, List<Texture2D> lightmapsDir, List<Texture2D> shadowMasks, List<LightInfo> lightsInfo)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.lightmapIndex != -1)
                {
                    RendererInfo info = new RendererInfo();
                    info.renderer = renderer;

                    if (renderer.lightmapScaleOffset != Vector4.zero)
                    {
                        //1ibrium's pointed out this issue : https://docs.unity3d.com/ScriptReference/Renderer-lightmapIndex.html
                        if (renderer.lightmapIndex < 0 || renderer.lightmapIndex == 0xFFFE)
                        {
                            continue;
                        }

                        info.lightmapOffsetScale = renderer.lightmapScaleOffset;

                        Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                        Texture2D lightmapDir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                        Texture2D shadowMask = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;

                        info.lightmapIndex = lightmaps.IndexOf(lightmap);
                        if (info.lightmapIndex == -1)
                        {
                            info.lightmapIndex = lightmaps.Count;
                            lightmaps.Add(lightmap);
                            lightmapsDir.Add(lightmapDir);
                            shadowMasks.Add(shadowMask);
                        }

                        rendererInfos.Add(info);
                    }
                }
            }

            var lights = root.GetComponentsInChildren<Light>(true);

            foreach (Light l in lights)
            {
                LightInfo lightInfo = new LightInfo();
                lightInfo.light = l;
                lightInfo.lightmapBaketype = (int)l.lightmapBakeType;
                lightInfo.mixedLightingMode = (int)UnityEditor.Lightmapping.lightingSettings.mixedBakeMode;
                lightsInfo.Add(lightInfo);
            }
        }
#endif
    }
}
