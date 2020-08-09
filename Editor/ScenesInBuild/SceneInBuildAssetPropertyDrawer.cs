using UnityEditor;
using UnityEngine;

namespace Fopoon.ScenesKit.Editor
{
    [CustomPropertyDrawer(typeof(SceneInBuildAsset))]
    public class SceneInBuildAssetPropertyDrawer
        : PropertyDrawer
    {
        #region Constants

        private const float ENABLED_WIDTH = 10f;
        private const float LABEL_WIDTH = 50f;
        private const float SPACING_WIDTH = 10f;
        private const string ENABLED_PROPERTY = "enabled";
        private const string SCENE_ASSET_PROPERTY = "sceneAsset";

        #endregion

        #region Statics
        
        private static SerializedProperty GetEnabledProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative(ENABLED_PROPERTY);
        }

        private static SerializedProperty GetSceneAssetProperty(SerializedProperty property)
        {
            return property.FindPropertyRelative(SCENE_ASSET_PROPERTY);
        }

        #endregion
        
        #region Unity Events

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Change prefix label from "Element x" to "x:".
            var labelText = $"{label.text.Replace("Element", string.Empty).Trim()}:";

            // Save the original label width.
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            // Draw prefix label.
            position = EditorGUI.PrefixLabel(
                position,
                GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent(labelText));

            // Set label width to what it was.
            EditorGUIUtility.labelWidth = labelWidth;

            // Don't make child fields be indented.
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects.
            var enabledRect = new Rect(
                position.x,
                position.y,
                ENABLED_WIDTH,
                position.height);
            var sceneAssetRect = new Rect(
                position.x + ENABLED_WIDTH + SPACING_WIDTH,
                position.y,
                position.width - ENABLED_WIDTH - SPACING_WIDTH,
                position.height);

            // Get serialized properties.
            var enabledProperty = GetEnabledProperty(property);
            var sceneAssetProperty = GetSceneAssetProperty(property);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(enabledRect, enabledProperty, GUIContent.none);
            EditorGUI.PropertyField(sceneAssetRect, sceneAssetProperty, GUIContent.none);

            // Set indent back to what it was.
            EditorGUI.indentLevel = indentLevel;

            EditorGUI.EndProperty();
        }
        
        #endregion
    }
}