using Mirror;
using TMPro;
using UnityEngine;

public class RoomUI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject lastWarningPanel;
    public TextMeshProUGUI warnText;
    
    public void ShowLastWarning()
    {
        if (NetworkServer.active)
            warnText.text = "Sei sicuro di voler chiudere la stanza?";
        else
            warnText.text = "Sei sicuro di voler uscire dalla stanza?";

        lastWarningPanel.SetActive(true);
    }

    public void HideLastWarning()
    {
        lastWarningPanel.SetActive(false);
    }

    public void Exit()
    {

        if (NetworkServer.active)
            CustomNetworkRoomManager.singleton.StopHost();
        else
            CustomNetworkRoomManager.singleton.StopClient();
    }
}
