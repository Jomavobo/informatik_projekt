using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    public GameObject panel;
    public TextMeshProUGUI reasonText;
    public TextMeshProUGUI titleText;

    void Awake()
    {
        Instance = this;
        panel?.SetActive(false);
    }

    public void Show(string reason)
    {
        if (panel != null)
            panel.SetActive(true);

        if (reasonText != null)
            reasonText.text = reason;

        if (titleText != null)
            titleText.text = "SCHIFF VERLOREN";
    }

    public void Hide()
    {
        panel?.SetActive(false);
    }
}