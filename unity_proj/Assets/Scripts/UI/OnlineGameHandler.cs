using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OnlineGameHandler : NetworkManager
{
    [Header("HUD")]
    public GameObject lobbyPanel;
    public GameObject lobbyPlayerPrefab;
    public TMP_InputField ipInput;
    public TextMeshProUGUI clientDebug;

    [Header("Lobby Settings")]
    public int maxPlayers = 4;

    [Header("User Info")]
    public string playerNamePref;

    [Header("UI MANAGER")]
    public UIManager uiManager;

    // GAME VARIABLES
    private List<GamePlayer> gamePlayers;

    public override void Start()
    {
        base.Start();
        ipInput.onValueChanged.AddListener(OnIpChanged);
    }
    private void OnIpChanged(string ip)
    {
        NetworkManager.singleton.networkAddress = ip;
    }

    /* CLIENT */

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        uiManager.GoTo(lobbyPanel);
        UpdateLobbyGraphic();
    }



    public void ConnectClient()
    {
        NetworkManager.singleton.StartClient();
    }


    /* SERVER */

    public void StartHosting()
    {
        NetworkManager.singleton.StartHost();
        gamePlayers = new List<GamePlayer>();
        gamePlayers.Add(new GamePlayer(
            PlayerPrefs.GetString(playerNamePref),
            null,
            null));
    }

    /* LOBBY */

    public void UpdateLobbyGraphic()
    {

    }

}

public class GamePlayer
{
    private string _name;
    private NetworkConnection _conn;
    private GameObject _lobbyGObj;

    public GamePlayer(string name, NetworkConnection conn, GameObject lobbyGObj)
    {
        _name = name;
        _conn = conn;
        _lobbyGObj = lobbyGObj;
    }

    public string Name { get => _name; set => _name = value; }
    public NetworkConnection Conn { get => _conn; set => _conn = value; }
    public GameObject LobbyGObj { get => _lobbyGObj; set => _lobbyGObj = value; }
}