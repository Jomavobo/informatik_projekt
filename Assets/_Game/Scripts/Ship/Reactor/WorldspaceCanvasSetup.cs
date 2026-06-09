using UnityEngine;
using System.Collections;

public class WorldSpaceCanvasSetup : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    IEnumerator WaitForPlayer()
    {
        Camera cam = null;

        while (cam == null)
        {
            foreach (var pm in FindObjectsOfType<PlayerMovement>())
            {
                if (pm.isLocalPlayer)
                {
                    cam = pm.GetComponentInChildren<Camera>();
                    break;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }

        GetComponent<Canvas>().worldCamera = cam;
        Debug.Log("World Space Canvas: Kamera zugewiesen");
    }
}