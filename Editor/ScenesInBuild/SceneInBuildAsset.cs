using System;
using UnityEditor;
using UnityEngine;

namespace Fopoon.ScenesKit.Editor
{
    [Serializable]
    public class SceneInBuildAsset
    {
        #region Statics

        public static SceneInBuildAsset Create(EditorBuildSettingsScene editorBuildSettingsScene)
        {
            return new SceneInBuildAsset(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(editorBuildSettingsScene.path),
                editorBuildSettingsScene.enabled);
        }

        public static EditorBuildSettingsScene ConvertToEditorBuildSettingsScene(SceneInBuildAsset sceneInBuildAsset)
        {
            return new EditorBuildSettingsScene(
                AssetDatabase.GetAssetPath(sceneInBuildAsset.SceneAsset),
                sceneInBuildAsset.Enabled);
        }

        #endregion

        #region Fields

        [SerializeField] private bool enabled;
        [SerializeField] private SceneAsset sceneAsset;

        #endregion

        #region Constructor

        public SceneInBuildAsset(SceneAsset sceneAsset, bool enabled)
        {
            if (sceneAsset == null) throw new ArgumentNullException(nameof(sceneAsset));

            this.sceneAsset = sceneAsset;
            this.enabled = enabled;
        }

        #endregion

        #region Properties

        public bool Enabled => enabled;
        public SceneAsset SceneAsset => sceneAsset;

        #endregion

        #region Methods

        public EditorBuildSettingsScene ToEditorBuildSettingsScene()
        {
            return ConvertToEditorBuildSettingsScene(this);
        }

        #endregion
    }
}
