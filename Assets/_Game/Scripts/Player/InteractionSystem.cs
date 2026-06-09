using Mirror;
using UnityEngine;

public class InteractionSystem : NetworkBehaviour
{
    [Header("Settings")]
    public float interactRange  = 3.5f;
    public LayerMask interactLayer;

    // State
    private IInteractable currentTarget;
    private IInteractable holdTarget;
    private float         holdTimer;
    private bool          isHolding;
    private Camera        cam;

    // Focus state
    [HideInInspector] public bool isFocused;

    void Start()
    {
        if (!isLocalPlayer) { enabled = false; return; }
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (isFocused) return; // Im Fokus-Modus übernimmt WorldSpaceScreen

        ScanForTarget();
        HandleInput();
    }

void ScanForTarget()
{
    Ray ray = new Ray(cam.transform.position, cam.transform.forward);
    Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

    if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
    {
        Debug.Log("Raycast trifft: " + hit.collider.gameObject.name + " Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if (interactable != currentTarget)
                {
                    currentTarget = interactable;
                    UpdatePrompt();
                }
                return;
            }
        }

        // Nichts gefunden
        if (currentTarget != null)
        {
            currentTarget = null;
            CancelHold();
            InteractionUI.Instance?.HidePrompt();
        }
    }

    void HandleInput()
{
    if (currentTarget == null) return;
    if (!currentTarget.CanInteract(gameObject)) return;

    switch (currentTarget.InteractType)
    {
        case InteractType.Tap:
            if (Input.GetKeyDown(KeyCode.E))
                TriggerInteract(currentTarget);
            break;

        case InteractType.Hold:
            if (Input.GetKey(KeyCode.E))
                UpdateHold(currentTarget);
            else if (Input.GetKeyUp(KeyCode.E))
                CancelHold();
            break;

        case InteractType.Focus:
            Debug.Log($"Focus - LMB: {Input.GetMouseButton(0)}, CursorLock: {Cursor.lockState}");
            if (Input.GetMouseButton(0))
                UpdateHold(currentTarget);
            else if (Input.GetMouseButtonUp(0))
                CancelHold();
            break;
    }
}

void OnLocalInteract(IInteractable target)
{
    // Cursor für Tap-Interaktionen die ein UI öffnen freigeben
    var movement = GetComponent<PlayerMovement>();
    if (movement != null) movement.LockCursor(false);
}

    void UpdateHold(IInteractable target)
    {
        if (holdTarget != target)
        {
            holdTarget = target;
            holdTimer  = 0f;
            isHolding  = true;
        }

        holdTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(holdTimer / target.HoldDuration);
        InteractionUI.Instance?.UpdateHoldRing(progress);

        if (holdTimer >= target.HoldDuration)
        {
            TriggerInteract(target);
            CancelHold();
        }
    }

    void CancelHold()
    {
        if (isHolding && holdTarget != null)
            holdTarget.OnInteractCancelled(gameObject);

        holdTarget = null;
        holdTimer  = 0f;
        isHolding  = false;
        InteractionUI.Instance?.UpdateHoldRing(0f);
    }

    void TriggerInteract(IInteractable target)
    {
        if (target is MonoBehaviour mb)
        {
            var netId = mb.GetComponent<NetworkIdentity>();
            if (netId != null)
                CmdInteract(netId);
            else
                target.OnInteract(gameObject); // Lokal falls kein NetworkIdentity
        }
    }

    [Command]
    void CmdInteract(NetworkIdentity targetNetId)
    {
        var interactable = targetNetId.GetComponent<IInteractable>()
                        ?? targetNetId.GetComponentInChildren<IInteractable>();
        interactable?.OnInteract(connectionToClient.identity.gameObject);
    }

    void UpdatePrompt()
    {
        if (currentTarget == null) return;

        bool canInteract = currentTarget.CanInteract(gameObject);
        string prefix = currentTarget.InteractType switch
        {
            InteractType.Tap   => "[E]",
            InteractType.Hold  => "[Halten E]",
            InteractType.Focus => "[Halten LMB]",
            _                  => "[E]"
        };

        string text = canInteract
            ? $"{prefix} {currentTarget.PromptText}"
            : $"[Besetzt] {currentTarget.PromptText}";

        InteractionUI.Instance?.ShowPrompt(text, canInteract);
    }
}
