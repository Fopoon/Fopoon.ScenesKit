using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Fopoon.ScenesKit.Editor
{
    [CreateAssetMenu(
        menuName = Constants.MENU_NAME_BASE + nameof(ScenesInBuildPreset),
        fileName = nameof(ScenesInBuildPreset),
        order = Constants.ORDER_BASE + 1)]
    public sealed class ScenesInBuildPreset
        : ScriptableObject
    {
        #region Fields

        [SerializeField] private SceneInBuildAsset[] scenes;

        #endregion

        #region Properties

        public SceneInBuildAsset[] Scenes => scenes;

        #endregion

        #region Methods

        public void Apply()
        {
            EditorBuildSettings.scenes = scenes
                .Select(SceneInBuildAsset.ConvertToEditorBuildSettingsScene)
                .ToArray();
        }

        public void Load()
        {
            scenes = EditorBuildSettings.scenes
                .Select(SceneInBuildAsset.Create)
                .ToArray();
        }

        #endregion
    }
}
