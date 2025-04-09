using UnityEngine;
using Mirror;
using TMPro;

public class SvlimeRoomPlayerBehaviour : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    [SerializeField] private TextMeshProUGUI nameText;

    void OnNameChanged(string _, string newName)
    {
        UpdateNameDisplay(newName);
    }

    public void UpdateNameDisplay(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    // Metodo chiamato dal client per inviare il nome al server
    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    public override void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            // Da mettere quando implementiamo il login
            //string localPlayerName = PlayerPrefs.GetString("playerName");

            string localPlayerName = "Player " + GetPlayerIndex().ToString();
            CmdSetPlayerName(localPlayerName);
        }
    }

    public int GetPlayerIndex()
    {
        if (NetworkManager.singleton is NetworkRoomManager roomManager)
        {
            return roomManager.roomSlots.Count;
        }
        return -1; // errore
    }
}
