using Mirror;
using UnityEngine;

public class SimpleButton : NetworkBehaviour, IInteractable
{
    [SyncVar(hook = nameof(OnStateChanged))]
    private bool isOn = false;

    private Renderer buttonRenderer;

    void Start()
    {
        buttonRenderer = GetComponent<Renderer>();
    }

    public string GetPrompt()
    {
        return isOn ? "[E] Ausschalten" : "[E] Einschalten";
    }

    public void Interact(NetworkIdentity player)
    {
        // Läuft auf dem Server
        isOn = !isOn;
    }

    void OnStateChanged(bool oldVal, bool newVal)
    {
        // Läuft auf allen Clients wenn SyncVar sich ändert
        buttonRenderer.material.color = newVal ? Color.green : Color.red;
    }
}