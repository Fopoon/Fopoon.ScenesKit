using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Fopoon.ScenesKit.Editor
{
    public class ScenesInBuildLoaderWindow
        : EditorWindow
    {
        #region Constants

        private const float LINE_HEIGHT = 20f;
        private const float BUTTON_WIDTH = 20f;
        private const float PREFIX_WIDTH = 30f;
        private const string MENU_LABEL = "Scenes in Build Loader";
        private const string MENU_SHORTCUT = " %`"; // CTRL/CMD + `

        #endregion

        #region Statics

        private static readonly GUIContent EMPTY_SCENES = new GUIContent("No scenes in build.");
        private static readonly GUIContent CLEAR_SEARCH_TERM = new GUIContent("×", "Clear search term.");
        private static readonly GUIContent DISABLE_SCENE = new GUIContent(string.Empty, "Disable scene.");
        private static readonly GUIContent ENABLE_SCENE = new GUIContent(string.Empty, "Enable scene.");
        private static readonly GUIContent LOAD_ADDITIVE = new GUIContent("☩", "Load scene additively.");
        private static readonly GUIContent LOAD_SINGLE = new GUIContent("◉", "Load scene.");
        private static readonly GUIContent PING = new GUIContent("!", "Ping scene asset.");
        private static readonly GUIContent QUICK_PLAY = new GUIContent("Quick Play");
        private static readonly GUIContent OPEN_BUILD_SETTINGS = new GUIContent("Open Build Settings");

        [MenuItem(
            Constants.MENU_NAME_BASE + MENU_LABEL + MENU_SHORTCUT,
            priority = Constants.ORDER_BASE + 10)]
        private static void OpenWindow()
        {
            GetWindow<ScenesInBuildLoaderWindow>(false, MENU_LABEL);
        }

        #endregion

        #region Fields

        private bool _isInitialized;
        private string _projectDirectory;
        private string _searchTerm;
        private Vector2 _scrollPosition;
        private readonly List<EditorBuildSettingsScene> _dirtyEditorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

        #endregion

        #region Unity Events

        private void OnGUI()
        {
            if (!_isInitialized)
                Initialize();

            var emptyScenesInBuild = EditorBuildSettings.scenes.Length == 0;

            if (emptyScenesInBuild)
                GUILayout.Label(EMPTY_SCENES);
            else
            {
                DrawSearchField();

                GUILayout.Space(10f);

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                var index = 0;

                foreach (var editorBuildSettingsScene in EditorBuildSettings.scenes)
                {
                    var prefix = editorBuildSettingsScene.enabled
                        ? (index++).ToString()
                        : string.Empty;

                    DrawSceneItem(editorBuildSettingsScene, prefix);
                }

                GUILayout.EndScrollView();

                ApplyChanges();
            }

            GUILayout.FlexibleSpace();

            if (!emptyScenesInBuild &&
                GUILayout.Button(QUICK_PLAY))
                QuickPlay.Execute();

            if (GUILayout.Button(OPEN_BUILD_SETTINGS))
                GetWindow(typeof(BuildPlayerWindow));
        }

        #endregion

        #region Methods

        private void ApplyChanges()
        {
            if (_dirtyEditorBuildSettingsScenes.Count == 0)
                return;

            var newEditorBuildSettingsScenes = new List<EditorBuildSettingsScene>();

            for (var i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                var didNotChange = true;
                var editorBuildSettingsScene = EditorBuildSettings.scenes[i];

                for (var j = _dirtyEditorBuildSettingsScenes.Count - 1; j >= 0; --j)
                {
                    var dirtyEditorBuildSettingsScene = _dirtyEditorBuildSettingsScenes[j];

                    if (editorBuildSettingsScene.guid != dirtyEditorBuildSettingsScene.guid)
                        continue;

                    didNotChange = false;
                    newEditorBuildSettingsScenes.Add(dirtyEditorBuildSettingsScene);
                    _dirtyEditorBuildSettingsScenes.RemoveAt(j);
                }

                if (didNotChange)
                    newEditorBuildSettingsScenes.Add(editorBuildSettingsScene);
            }

            EditorBuildSettings.scenes = newEditorBuildSettingsScenes.ToArray();
        }

        private void DrawSceneItem(EditorBuildSettingsScene editorBuildSettingsScene, string prefix = "")
        {
            var scenePath = editorBuildSettingsScene.path;
            var fullPath = Path.Combine(_projectDirectory, scenePath);

            if (string.IsNullOrWhiteSpace(scenePath) ||
                string.IsNullOrWhiteSpace(AssetDatabase.AssetPathToGUID(scenePath)) ||
                !File.Exists(fullPath))
                return;

            var sceneName = Path.GetFileNameWithoutExtension(scenePath);

            if (!string.IsNullOrWhiteSpace(_searchTerm) &&
                !sceneName.ToLower().Contains(_searchTerm.ToLower()))
                return;

            GUILayout.BeginHorizontal();

            var enabled = GUILayout.Toggle(
                editorBuildSettingsScene.enabled,
                editorBuildSettingsScene.enabled
                    ? DISABLE_SCENE
                    : ENABLE_SCENE,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(BUTTON_WIDTH));

            if (editorBuildSettingsScene.enabled != enabled)
            {
                editorBuildSettingsScene.enabled = enabled;
                _dirtyEditorBuildSettingsScenes.Add(editorBuildSettingsScene);
            }

            var prefixGuiContent = new GUIContent(prefix, scenePath);

            GUILayout.Label(
                prefixGuiContent,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(PREFIX_WIDTH));

            var labelGuiContent = new GUIContent(sceneName, scenePath);

            GUILayout.Label(
                labelGuiContent,
                GUILayout.Height(LINE_HEIGHT));

            if (GUILayout.Button(
                PING,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(BUTTON_WIDTH)))
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (GUILayout.Button(
                LOAD_SINGLE,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(BUTTON_WIDTH)) &&
                !Application.isPlaying)
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            if (GUILayout.Button(
                LOAD_ADDITIVE,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(BUTTON_WIDTH)) &&
                !Application.isPlaying)
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            GUILayout.EndHorizontal();
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal();

            _searchTerm = GUILayout.TextField(
                _searchTerm,
                GUILayout.Height(LINE_HEIGHT));

            if (GUILayout.Button(
                CLEAR_SEARCH_TERM,
                GUILayout.Height(LINE_HEIGHT),
                GUILayout.Width(BUTTON_WIDTH)))
                _searchTerm = string.Empty;

            GUILayout.EndHorizontal();
        }

        private void Initialize()
        {
            _projectDirectory = Path.GetDirectoryName(Application.dataPath);

            _isInitialized = true;
        }

        #endregion
    }
}
