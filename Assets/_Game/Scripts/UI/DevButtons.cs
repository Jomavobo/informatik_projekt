using UnityEngine;

public class DevButtons : MonoBehaviour
{
    public void RepairReactor()
    {
        if (ReactorSystem.Instance != null)
            ReactorSystem.Instance.CmdRepair(10f);
        else
            Debug.LogWarning("ReactorSystem nicht gefunden");
    }

    public void TriggerGameOver()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerGameOver("DEV: Manuell ausgelöst");
        else
            Debug.LogWarning("GameManager nicht gefunden");
    }
}