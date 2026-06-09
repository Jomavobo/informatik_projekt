using Mirror;
using UnityEngine;

public class WorldSpaceScreen : NetworkBehaviour, IInteractable
{
    [Header("Screen Settings")]
    public string screenName   = "Terminal";
    public float  holdDuration = 0.6f;
    public float  focusSpeed   = 4f;

    [Header("Camera Position")]
    public Transform cameraFocusPoint;

    public string       PromptText   => $"{screenName} bedienen";
    public InteractType InteractType => InteractType.Focus;
    public float        HoldDuration => holdDuration;

    public bool CanInteract(GameObject player) => true;

    public void OnInteract(GameObject player)
    {
        var interSys = player.GetComponent<InteractionSystem>();
        if (interSys == null || !interSys.isLocalPlayer) return;
        StartFocus(interSys);
    }

    public void OnInteractCancelled(GameObject player) { }

    void StartFocus(InteractionSystem interSys)
    {
        interSys.isFocused = true;
        InteractionUI.Instance?.HidePrompt();

        var movement = interSys.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        movement?.LockCursor(false);

        var cam = interSys.GetComponentInChildren<Camera>();
        if (cam != null)
            StartCoroutine(MoveCameraTo(cam, interSys));
    }

    System.Collections.IEnumerator MoveCameraTo(Camera cam, InteractionSystem interSys)
    {
        Transform target = cameraFocusPoint != null ? cameraFocusPoint : transform;

        Vector3    startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        Vector3    endPos   = target.position;
        Quaternion endRot   = target.rotation;

        float t = 0f;
        while (t < 1f)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(MoveCameraBack(cam, interSys, startPos, startRot));
                yield break;
            }

            t += Time.deltaTime * focusSpeed;
            cam.transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            cam.transform.rotation = Quaternion.Slerp(startRot, endRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        StartCoroutine(WaitForExit(cam, interSys, startPos, startRot));
    }

    System.Collections.IEnumerator WaitForExit(
        Camera cam, InteractionSystem interSys,
        Vector3 returnPos, Quaternion returnRot)
    {
        while (true)
        {
            // Nur ESC beendet den Focus – kein LMB
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(MoveCameraBack(cam, interSys, returnPos, returnRot));
                yield break;
            }
            yield return null;
        }
    }

    System.Collections.IEnumerator MoveCameraBack(
        Camera cam, InteractionSystem interSys,
        Vector3 returnPos, Quaternion returnRot)
    {
        Vector3    startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * focusSpeed;
            cam.transform.position = Vector3.Lerp(startPos, returnPos, Mathf.SmoothStep(0f, 1f, t));
            cam.transform.rotation = Quaternion.Slerp(startRot, returnRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        interSys.isFocused = false;
        var movement = interSys.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
            movement.LockCursor(true);
        }
    }
}