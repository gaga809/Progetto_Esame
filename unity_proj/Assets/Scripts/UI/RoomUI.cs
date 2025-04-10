using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject lastWarningPanel;
    public TextMeshProUGUI warnText;

    [Header("HUD")]
    public GameObject[] readyStatuses;
    public Button btnReady;
    
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
