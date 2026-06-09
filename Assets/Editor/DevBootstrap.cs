using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class DevBootstrap
{
    static DevBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Immer Bootstrap als Startszene setzen wenn Play gedrückt wird
            EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Scenes/Bootstrap.unity");
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Reset damit man Szenen normal editieren kann
            EditorSceneManager.playModeStartScene = null;
        }
    }
}