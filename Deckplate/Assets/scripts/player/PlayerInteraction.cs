using Mirror;
using UnityEngine;
using TMPro;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Interaction")]
    public float interactRange = 3f;
    public LayerMask interactLayer;

    [Header("UI")]
    public TextMeshProUGUI promptText;

    private Camera playerCamera;
    private IInteractable currentTarget;

    void Start()
    {
        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }
    
        playerCamera = GetComponentInChildren<Camera>();
        
        // Canvas aus der Szene finden statt per Referenz
        GameObject promptObj = GameObject.FindWithTag("PromptText");
        if (promptObj != null)
            promptText = promptObj.GetComponent<TextMeshProUGUI>();
    
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;
        
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            CmdInteract(((MonoBehaviour)currentTarget).GetComponent<NetworkIdentity>());
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            Debug.Log("Raycast trifft: " + hit.collider.gameObject.name);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                Debug.Log("Interactable gefunden");
                currentTarget = interactable;
                ShowPrompt(interactable.GetPrompt());
                return;
            }
        }

        currentTarget = null;
        HidePrompt();
    }

    [Command]
    void CmdInteract(NetworkIdentity target)
    {
        IInteractable interactable = target.GetComponent<IInteractable>();
        interactable?.Interact(netIdentity);
    }

    void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    void HidePrompt()
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }
}