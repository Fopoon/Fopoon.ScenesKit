using UnityEditor;
using UnityEngine;

namespace Fopoon.ScenesKit.Editor
{
    [CustomEditor(typeof(ScenesInBuildPreset))]
    public class ScenesInBuildPresetEditor
        : UnityEditor.Editor
    {
        #region Constants

        private const float SPACING_HEIGHT = 10f;
        private const string APPLY_LABEL = "Apply to Scenes in Build";
        private const string LOAD_LABEL = "Load from Scenes in Build";

        #endregion

        #region Statics

        private static readonly GUIContent APPLY_CONTENT = new GUIContent(APPLY_LABEL);
        private static readonly GUIContent LOAD_SCENES_IN_BUILD_CONTENT = new GUIContent(LOAD_LABEL);

        #endregion

        #region Unity Events

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(SPACING_HEIGHT);

            var scenesInBuildPreset = target as ScenesInBuildPreset;

            if (scenesInBuildPreset == null) return;

            if (GUILayout.Button(LOAD_SCENES_IN_BUILD_CONTENT))
            {
                var willOverwrite = scenesInBuildPreset.Scenes != null &&
                                    scenesInBuildPreset.Scenes.Length > 0;

                if (willOverwrite)
                {
                    var confirm = EditorUtility.DisplayDialog(
                        LOAD_LABEL,
                        "Are you sure? This will overwrite existing values.",
                        "Yes",
                        "No");

                    if (!confirm) return;
                }

                scenesInBuildPreset.Load();
            }

            if (GUILayout.Button(APPLY_CONTENT))
            {
                var confirm = EditorUtility.DisplayDialog(
                    APPLY_LABEL,
                    "Are you sure?",
                    "Yes",
                    "No");

                if (!confirm) return;

                scenesInBuildPreset.Apply();
            }
        }

        #endregion
    }
}