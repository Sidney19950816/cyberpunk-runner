using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace PG
{
    /// <summary> To hide unnecessary fields from the inspector </summary>
    [CustomPropertyDrawer (typeof (HideInInspectorIf))]
    [CustomPropertyDrawer (typeof (ShowInInspectorIf))]
    public class ShowInInspectorIfAttributeDrawer :PropertyDrawer
    {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            //get the attribute data
            var hideAtt = (HideInInspectorIf)attribute;

            //Check if we should draw the property
            if (!hideAtt.HideInInspector == GetResult (hideAtt, property))
            {
                EditorGUI.PropertyField (position, property, label, true);
            }
        }

        private bool GetResult (HideInInspectorIf hideAtt, SerializedProperty property)
        {
            var enabled = true;
            //Look for the sourcefield within the object that the property belongs to
            var propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
            var conditionPath = propertyPath.Replace(property.name, hideAtt.PropertyToCheck); //changes the path to the conditionalsource property path
            var sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

            if (sourcePropertyValue != null)
            {
                enabled = sourcePropertyValue.boolValue;
            }
            else
            {
                Debug.LogWarning ("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: " + hideAtt.PropertyToCheck);
            }

            return enabled;
        }

        public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
        {
            var hideAtt = (HideInInspectorIf)attribute;

            //Check if we should draw the property
            if (!hideAtt.HideInInspector == GetResult (hideAtt, property))
            {
                return base.GetPropertyHeight (property, label);
            }

            return 0;
        }

    }
}
#endif