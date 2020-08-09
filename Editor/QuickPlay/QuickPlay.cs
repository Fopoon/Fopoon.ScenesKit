using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fopoon.ScenesKit.Editor
{
    [InitializeOnLoad]
    public static class QuickPlay
    {
        #region Constants

        private const string IS_ACTIVE = nameof(QuickPlay) + ".IsActive";
        private const string LAST_OPENED_SCENE = nameof(QuickPlay) + ".LastOpenedScene";
        private const string MENU_LABEL = "Quick Play";
        private const string MENU_SHORTCUT = " %l"; // CTRL/CMD + L

        #endregion

        #region Statics

        static QuickPlay()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
        }

        [MenuItem(
            Constants.MENU_NAME_BASE + MENU_LABEL + MENU_SHORTCUT,
            priority = Constants.ORDER_BASE + 1)]
        public static void Execute()
        {
            // Stop playing when it is already playing.

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // Check if there is a scene in Scenes in Build.

            var firstScene = EditorBuildSettings.scenes
                .FirstOrDefault(x => x.enabled &&
                                     !string.IsNullOrWhiteSpace(x.path));

            if (firstScene == null)
            {
                EditorUtility.DisplayDialog(
                    MENU_LABEL,
                    "No playable scenes in build.",
                    "Ok");

                return;
            }

            // Save last opened scene.

            var sceneManagerSetup = EditorSceneManager.GetSceneManagerSetup();

            if (sceneManagerSetup.Length > 0)
            {
                EditorPrefs.SetBool(IS_ACTIVE, true);
                EditorPrefs.SetString(LAST_OPENED_SCENE, sceneManagerSetup[0].path);
            }

            // Prompt user to save scene.

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Open first active scene in Scenes in Build and Play.

            EditorSceneManager.OpenScene(firstScene.path);
            EditorApplication.isPlaying = true;
        }

        private static void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
                return;

            // Load last scene if available.

            var isActive = EditorPrefs.GetBool(IS_ACTIVE, false);
            var lastScene = EditorPrefs.GetString(LAST_OPENED_SCENE, string.Empty);

            if (isActive &&
                !string.IsNullOrWhiteSpace(lastScene))
                EditorSceneManager.OpenScene(lastScene);

            // Clean up.

            EditorPrefs.DeleteKey(IS_ACTIVE);
            EditorPrefs.DeleteKey(LAST_OPENED_SCENE);
        }

        #endregion
    }
}