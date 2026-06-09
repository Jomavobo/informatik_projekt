using UnityEngine;

public class DebugZone : MonoBehaviour
{
    void Awake()
    {
        #if !UNITY_EDITOR
            gameObject.SetActive(false);
        #endif
    }
}