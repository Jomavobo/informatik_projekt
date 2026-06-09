using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance;

    [Header("Scenes")]
    public string shipScene    = "Ship";
    public string galaxyScene  = "Galaxy";

void Awake()
{
    Debug.Log("Bootstrap Awake");
    if (Instance != null)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}

IEnumerator Start()
{
    Debug.Log("Bootstrap Start - lade Ship Szene");
    AsyncOperation op = SceneManager.LoadSceneAsync(shipScene, LoadSceneMode.Additive);
    yield return op;
    Debug.Log("Ship Szene geladen");
    SceneManager.SetActiveScene(SceneManager.GetSceneByName(shipScene));

    if (Application.isBatchMode)
    {
        Debug.Log($"Batch Mode - NetworkManager: {NetworkManager.singleton}");
        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.StartServer();
            Debug.Log("Server gestartet");
        }
        else
        {
            Debug.LogError("NetworkManager.singleton ist null!");
        }
    }
}

    public void LoadGalaxy()
    {
        SceneManager.LoadSceneAsync(galaxyScene, LoadSceneMode.Additive);
    }

    public void UnloadGalaxy()
    {
        SceneManager.UnloadSceneAsync(galaxyScene);
    }
}