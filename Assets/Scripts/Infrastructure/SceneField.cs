using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Assets.Scripts.Infrastructure
{
    [System.Serializable]
    public class SceneField
    {
        [SerializeField] private Object _sceneAsset;
        [SerializeField] private string _sceneName = "";
        [SerializeField] private string _scenePath = "";

        public string SceneName => _sceneName;
        public string ScenePath => _scenePath;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            var sceneAsset = property.FindPropertyRelative("_sceneAsset");
            var sceneName = property.FindPropertyRelative("_sceneName");
            var scenePath = property.FindPropertyRelative("_scenePath");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null)
            {
                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
                if (EditorGUI.EndChangeCheck())
                {
                    sceneAsset.objectReferenceValue = value;
                    if (sceneAsset.objectReferenceValue != null)
                    {
                        var path = AssetDatabase.GetAssetPath(sceneAsset.objectReferenceValue);
                        var assetsIndex = path.IndexOf("Assets", StringComparison.Ordinal) + 7;
                        var extensionIndex = path.LastIndexOf(".unity", StringComparison.Ordinal);

                        path = path.Substring(assetsIndex, extensionIndex - assetsIndex);

                        scenePath.stringValue = path;
                        sceneName.stringValue = path.Split('/').Last();
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}