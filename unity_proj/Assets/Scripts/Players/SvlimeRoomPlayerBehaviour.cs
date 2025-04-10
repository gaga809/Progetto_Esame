using UnityEngine;
using Mirror;
using TMPro;

public class SvlimeRoomPlayerBehaviour : NetworkRoomPlayer
{
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

        if (isLocalPlayer)
        {
            if (newReadyState)
            {
                roomUI.btnReady.GetComponentInChildren<TextMeshProUGUI>().text = "Annulla";
            }
            else
                roomUI.btnReady.GetComponentInChildren<TextMeshProUGUI>().text = "Pronto!";


        }
        ShowReadyStatusOnClients(newReadyState);
    }

    public override void Start()
    {
        base.Start();
        roomUI = GameObject.Find("UILobby").GetComponent<RoomUI>();

        if (isLocalPlayer)
        {
            string localPlayerName = "Player " + GetPlayerIndex().ToString();
            CmdSetPlayerName(localPlayerName);
        }

        roomUI.btnReady.onClick.AddListener(HandlerReady);
    }

    public void HandlerReady()
    {
        CmdChangeReadyState(!readyToBegin);
    }

    public int GetPlayerIndex()
    {
        if (NetworkManager.singleton is NetworkRoomManager roomManager)
        {
            return roomManager.roomSlots.Count;
        }
        return -1;
    }

    private void ShowReadyStatusOnClients(bool newReadyState)
    {
        if (readyStatusImg != null)
        {
            readyStatusImg.SetActive(newReadyState);
        }
    }
}
