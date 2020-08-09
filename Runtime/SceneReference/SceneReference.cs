using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

// Author: JohannesMP
//
// A wrapper that provides the means to safely serialize Scene Asset References.
//
// Internally we serialize a SceneAsset which only exists at editor time.
// Any time the SceneAsset is serialized, we store the path provided by this SceneAsset (assuming it was valid).
//
// This means that, come build time, the string path of the scene asset is always already stored,
// which if the scene was added to the build settings means it can be loaded.
//
// It is up to the user to ensure the scene exists in the build settings so it is loadable at runtime.
//
// Known issues:
//     - When reverting back to a prefab which has the asset stored as null,
//       Unity will show the property as modified despite having just reverted.
//       This only happens the first time, and reverting again fixes it.
//       Under the hood the state is still always valid, and serialized correctly regardless.

[Serializable]
public class SceneReference
    : ISerializationCallbackReceiver
{
    #region Static

    public static implicit operator string(SceneReference sceneReference)
    {
        return sceneReference.ScenePath;
    }

    #endregion

    #region Fields

#if UNITY_EDITOR

    [HideInInspector]

#endif

    [SerializeField]
    private string scenePath;

#if UNITY_EDITOR

    [SerializeField]
    private SceneAsset sceneAsset;

#endif

    #endregion

    #region Properties

    public string ScenePath
    {
        get
        {

#if UNITY_EDITOR

            return GetScenePathFromSceneAsset();

#else

            return scenePath;

#endif

        }
        set
        {
            scenePath = value;

#if UNITY_EDITOR

            sceneAsset = GetSceneAssetFromScenePath();

#endif

        }
    }

#if UNITY_EDITOR

    private bool IsSceneAssetValid => sceneAsset != null;

    private bool IsScenePathValid => !string.IsNullOrWhiteSpace(scenePath);

#endif

    #endregion

    #region Methods

#if UNITY_EDITOR

    private SceneAsset GetSceneAssetFromScenePath()
    {
        return IsScenePathValid
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)
            : null;
    }

    private string GetScenePathFromSceneAsset()
    {
        return IsSceneAssetValid
            ? AssetDatabase.GetAssetPath(sceneAsset)
            : string.Empty;
    }

    private void HandleOnBeforeSerialize()
    {
        // When 'sceneAsset' is not valid and 'scenePath' is valid,
        // try to recover 'sceneAsset' using it.
        // Otherwise, overwrite 'scenePath' with
        // the value of 'sceneAsset'.

        if (!IsSceneAssetValid && IsScenePathValid)
        {
            TryRecoverSceneAsset();

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
        else
        {
            scenePath = GetScenePathFromSceneAsset();
        }
    }

    private void HandleOnAfterDeserialize()
    {
        // ReSharper disable DelegateSubtraction

        EditorApplication.update -= HandleOnAfterDeserialize;

        // ReSharper restore DelegateSubtraction

        if (!IsSceneAssetValid && IsScenePathValid)
        {
            TryRecoverSceneAsset();

            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }

    private void TryRecoverSceneAsset()
    {
        sceneAsset = GetSceneAssetFromScenePath();

        // When 'sceneAsset' is still null,
        // invalidate 'scenePath'.

        if (sceneAsset == null)
            scenePath = string.Empty;
    }

#endif

    #endregion

    #region ISerializationCallbackReceiver Methods

    public void OnBeforeSerialize()
    {

#if UNITY_EDITOR

        HandleOnBeforeSerialize();

#endif

    }

    public void OnAfterDeserialize()
    {

#if UNITY_EDITOR

        // We sadly cannot touch AssetDatabase during serialization, so defer by a bit.
        EditorApplication.update += HandleOnAfterDeserialize;

#endif

    }

    #endregion
}
