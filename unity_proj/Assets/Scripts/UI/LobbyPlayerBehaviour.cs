using Mirror;
using TMPro;
using UnityEngine;

public class LobbyPlayerBehaviour : NetworkBehaviour
{
    [Header("Settings")]
    public string playerName;
    public TextMeshProUGUI txtPlayerName;

    private void Update()
    {
        if(txtPlayerName.text != playerName)
            txtPlayerName.text = playerName;   
    }
}
