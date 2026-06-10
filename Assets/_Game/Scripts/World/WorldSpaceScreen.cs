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

    [Header("Proximity")]
    public float viewDistance = 8f; // Distanz für Zuschauer

    // Lock – nur ein Spieler gleichzeitig
    [SyncVar(hook = nameof(OnLockedChanged))]
    private bool isLocked = false;

    [SyncVar]
    private uint lockedByNetId = 0;

    public string PromptText
    {
        get
        {
            if (isLocked && !IsLockedByMe())
                return $"{screenName} – wird benutzt";
            return $"{screenName} bedienen";
        }
    }

    public InteractType InteractType => InteractType.Focus;
    public float        HoldDuration => holdDuration;

    public bool CanInteract(GameObject player)
    {
        if (!isLocked) return true;
        return IsLockedByMe();
    }

    bool IsLockedByMe()
    {
        var localPlayer = FindLocalPlayer();
        if (localPlayer == null) return false;
        var netId = localPlayer.GetComponent<NetworkIdentity>();
        return netId != null && netId.netId == lockedByNetId;
    }

    public void OnInteract(GameObject player)
    {
        var interSys = player.GetComponent<InteractionSystem>();
        if (interSys == null || !interSys.isLocalPlayer) return;

        // Lock anfordern
        var netId = player.GetComponent<NetworkIdentity>();
        if (netId != null)
            CmdRequestLock(netId.netId);

        StartFocus(interSys);
    }

    public void OnInteractCancelled(GameObject player) { }

    [Command(requiresAuthority = false)]
    void CmdRequestLock(uint playerNetId)
    {
        if (isLocked && lockedByNetId != playerNetId) return;
        isLocked      = true;
        lockedByNetId = playerNetId;
    }

    [Command(requiresAuthority = false)]
    void CmdReleaseLock()
    {
        isLocked      = false;
        lockedByNetId = 0;
    }

    void OnLockedChanged(bool oldVal, bool newVal)
    {
        // UI updaten wenn Lock sich ändert
    }

    // ── Fokus ─────────────────────────────────────────────────────────────────

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

        // Lock freigeben
        CmdReleaseLock();

        interSys.isFocused = false;
        var movement = interSys.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = true;
            movement.LockCursor(true);
        }
    }

    GameObject FindLocalPlayer()
    {
        foreach (var pm in FindObjectsOfType<PlayerMovement>())
            if (pm.isLocalPlayer) return pm.gameObject;
        return null;
    }
}