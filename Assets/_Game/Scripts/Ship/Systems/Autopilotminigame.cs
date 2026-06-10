using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AutopilotMinigame : MonoBehaviour
{
    public static AutopilotMinigame Instance;

    [Header("References")]
    public GameObject minigamePanel;
    public GameObject showPanel;        // Panel das die Zahlen anzeigt
    public GameObject inputPanel;       // Panel mit den Eingabefeldern
    public TMP_InputField[] inputFields; // 4 Eingabefelder
    public TextMeshProUGUI[] displayDigits; // 4 große Ziffern
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Button submitButton;

    [Header("Settings")]
    public float showDuration  = 3f;
    public float inputDuration = 15f;

    private int[] code = new int[4];
    private float timer;
    private bool inputActive;

    void Awake() => Instance = this;

    void Update()
    {
        if (!inputActive) return;

        timer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text  = $"{timer:0.0}s";
            timerText.color = timer < 5f ? Color.red : timer < 8f ? Color.yellow : Color.white;
        }

        if (timer <= 0f)
        {
            inputActive = false;
            OnFailed();
        }
    }

    // ── Minispiel starten ─────────────────────────────────────────────────────

    public void StartMinigame()
    {
        if (minigamePanel != null) minigamePanel.SetActive(true);

        GenerateCode();
        StartCoroutine(MinigameRoutine());

        // Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    IEnumerator MinigameRoutine()
    {
        // Phase 1: Code anzeigen
        if (showPanel  != null) showPanel.SetActive(true);
        if (inputPanel != null) inputPanel.SetActive(false);

        for (int i = 0; i < 4; i++)
            if (displayDigits[i] != null)
                displayDigits[i].text = code[i].ToString();

        if (statusText != null)
        {
            statusText.text  = "Merke dir den Code!";
            statusText.color = Color.yellow;
        }

        yield return new WaitForSeconds(showDuration);

        // Phase 2: Eingabe
        if (showPanel  != null) showPanel.SetActive(false);
        if (inputPanel != null) inputPanel.SetActive(true);

        ClearInputs();
        SetupInputFields();

        timer       = inputDuration;
        inputActive = true;

        if (statusText != null)
        {
            statusText.text  = "Gib den Code ein!";
            statusText.color = Color.white;
        }

        // Erstes Feld fokussieren
        if (inputFields.Length > 0)
            inputFields[0].Select();
    }

    void GenerateCode()
    {
        for (int i = 0; i < 4; i++)
            code[i] = Random.Range(0, 10);
    }

    void ClearInputs()
    {
        foreach (var field in inputFields)
        {
            field.text         = "";
            field.interactable = true;
        }
    }

    void SetupInputFields()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            int index = i;
            inputFields[i].onValueChanged.RemoveAllListeners();
            inputFields[i].onValueChanged.AddListener(val => OnFieldChanged(index, val));

            // Nur Zahlen erlauben, max 1 Zeichen
            inputFields[i].contentType      = TMP_InputField.ContentType.IntegerNumber;
            inputFields[i].characterLimit   = 1;
            inputFields[i].interactable     = true;
        }
    }

    void OnFieldChanged(int index, string val)
    {
        if (string.IsNullOrEmpty(val)) return;

        // Nur letztes Zeichen behalten
        if (val.Length > 1)
        {
            inputFields[index].text = val[val.Length - 1].ToString();
            return;
        }

        // Zum nächsten Feld springen
        if (index < inputFields.Length - 1)
        {
            inputFields[index + 1].Select();
            inputFields[index + 1].text = "";
        }
        else
        {
            // Letztes Feld – automatisch submitten
            inputFields[index].DeactivateInputField();
            CheckCode();
        }
    }

    // ── Submit ────────────────────────────────────────────────────────────────

    public void Submit()
    {
        if (!inputActive) return;
        CheckCode();
    }

    void CheckCode()
    {
        if (!inputActive) return;
        inputActive = false;

        bool correct = true;
        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(inputFields[i].text, out int val) || val != code[i])
            {
                correct = false;
                break;
            }
        }

        if (correct) OnSuccess();
        else         OnFailed();
    }

    // ── Ergebnis ──────────────────────────────────────────────────────────────

    void OnSuccess()
    {
        if (statusText != null)
        {
            statusText.text  = "AUTOPILOT WIEDERHERGESTELLT";
            statusText.color = Color.green;
        }

        // Event auflösen
        EventSystem.Instance?.CmdResolveEvent();

        StartCoroutine(CloseAfterDelay(2f));
    }

    void OnFailed()
    {
        if (statusText != null)
        {
            statusText.text  = "FEHLER – SCHADEN AM STEUERWERK";
            statusText.color = Color.red;
        }

        // Schaden am Schiff (Hülle)
        // HullSystem.Instance?.TakeDamage(10f); // kommt später

        StartCoroutine(CloseAfterDelay(2f));
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseMinigame();
    }

    public void CloseMinigame()
    {
        inputActive = false;
        if (minigamePanel != null) minigamePanel.SetActive(false);

        // Cursor wieder sperren
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }
}