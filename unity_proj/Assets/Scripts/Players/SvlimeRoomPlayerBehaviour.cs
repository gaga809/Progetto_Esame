using UnityEngine;
using Mirror;
using TMPro;

public class SvlimeRoomPlayerBehaviour : NetworkRoomPlayer
{
    public string playerNamePref = "playerName";
    public string playerIdPref = "playerId";

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SyncVar(hook = nameof(OnIdChanged))]
    public int playerId;

    public RoomUI roomUI;
    public GameObject readyStatusImg;

    [SerializeField] private TextMeshProUGUI nameText;

    void OnNameChanged(string _, string newName)
    {
        UpdateNameDisplay(newName);
    }

    void OnIdChanged(int _, int newId)
    {
        playerId = newId;
    }

    public void UpdateNameDisplay(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    [Command]
    public void CmdSetPlayerInfo(string name, int id)
    {
        playerName = name;
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        base.ReadyStateChanged(oldReadyState, newReadyState);

        if (roomUI == null)
        {
            Debug.LogWarning("roomUI is null in ReadyStateChanged!");
            return; 
        }

        if (isLocalPlayer)
        {
            if (newReadyState)
            {
                roomUI.btnReady.GetComponentInChildren<TextMeshProUGUI>().text = "CANCEL";
            }
            else
            {
                roomUI.btnReady.GetComponentInChildren<TextMeshProUGUI>().text = "READY!";
            }
        }
        ShowReadyStatusOnClients(newReadyState);
    }


    public override void Start()
    {
        base.Start();
        roomUI = GameObject.Find("UILobby").GetComponent<RoomUI>();

        if (isLocalPlayer)
        {
            string localPlayerName = PlayerPrefs.GetString(playerNamePref);
            int localPlayerId = PlayerPrefs.GetInt(playerIdPref, 0);
            if(localPlayerId == 0)
            {
                // Disconnect
                Debug.LogError("Player ID is not set. Disconnecting local player.");
                NetworkManager.singleton.StopClient();
            }
            CmdSetPlayerInfo(localPlayerName, localPlayerId);
            roomUI.btnReady.onClick.AddListener(HandlerReady);
        }

    }

    public void HandlerReady()
    {
        CmdChangeReadyState(!readyToBegin);
    }

    private void ShowReadyStatusOnClients(bool newReadyState)
    {
        if (readyStatusImg != null)
        {
            readyStatusImg.SetActive(newReadyState);
        }
    }
}
