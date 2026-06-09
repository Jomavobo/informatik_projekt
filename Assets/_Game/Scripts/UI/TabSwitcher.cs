using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabSwitcher : MonoBehaviour
{
    public List<Button>     tabButtons;
    public List<GameObject> contentPanels;
    public Color            activeColor;
    public Color            inactiveColor;

    private int currentTab = 0;

    public void Init()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
        SwitchTab(0);
    }

    void Start()
    {
        if (tabButtons != null && tabButtons.Count > 0)
        {
            for (int i = 0; i < tabButtons.Count; i++)
            {
                tabButtons[i].onClick.RemoveAllListeners();
                int index = i;
                tabButtons[i].onClick.AddListener(() => SwitchTab(index));
            }
            SwitchTab(0);
        }
    }

    public void SwitchTab(int index)
    {
        Debug.Log($"SwitchTab aufgerufen: {index}");
        currentTab = index;

        for (int i = 0; i < contentPanels.Count; i++)
            if (contentPanels[i] != null)
                contentPanels[i].SetActive(i == index);

        for (int i = 0; i < tabButtons.Count; i++)
        {
            if (tabButtons[i] == null) continue;
            var img = tabButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = i == index ? activeColor : inactiveColor;
        }
    }
}