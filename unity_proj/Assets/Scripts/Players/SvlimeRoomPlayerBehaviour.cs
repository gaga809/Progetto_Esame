using UnityEngine;
using Mirror;
using TMPro;

public class SvlimeRoomPlayerBehaviour : NetworkRoomPlayer
{
    public string playerNamePref = "playerName";
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    public RoomUI roomUI;
    public GameObject readyStatusImg;

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

    [Command]
    public void CmdSetPlayerName(string name)
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
            // TODO: Get Name from PlayerPrefs
            string localPlayerName = playerName;
            PlayerPrefs.SetString(playerNamePref, localPlayerName);
            CmdSetPlayerName(localPlayerName);
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
