using UnityEngine;

public enum InteractType
{
    Tap,    // Kurz E drücken
    Hold,   // E oder LMB halten
    Focus   // LMB halten -> Kamera bewegt sich zum Screen
}

public interface IInteractable
{
    // Text der im Prompt angezeigt wird
    string PromptText { get; }

    // Wie wird interagiert
    InteractType InteractType { get; }

    // Wie lange muss gehalten werden (nur für Hold/Focus)
    float HoldDuration { get; }

    // Kann gerade interagiert werden? (z.B. Terminal besetzt)
    bool CanInteract(GameObject player);

    // Wird aufgerufen wenn Interaktion abgeschlossen
    void OnInteract(GameObject player);

    // Wird aufgerufen wenn Spieler aufhört zu interagieren (Hold abgebrochen)
    void OnInteractCancelled(GameObject player) { }
}
