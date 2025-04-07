using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    [Header("Lobby Settings")]
    public int maxPlayers = 4;

    public TextMeshProUGUI playersNumLabel;

    private SyncList<PlayerData> gamePlayers = new SyncList<PlayerData>();
    private List<GameObject> playerCards = new List<GameObject>();

    public GameObject lobbyPlayerPrefab;
    public Transform lobbyView;
    public TMP_InputField ipInput;
    public UIManager uiManager;

    private void Start()
    {
        if (isServer)
        {
            gamePlayers.Callback += OnGamePlayersChanged;
        }

        ipInput.onValueChanged.AddListener(OnIpChanged);
    }

    private void OnGamePlayersChanged(SyncList<PlayerData>.Operation operation, int arg2, PlayerData data1, PlayerData data2)
    {
        RpcUpdateLobby();
    }

    private void OnIpChanged(string ip)
    {
        NetworkManager.singleton.networkAddress = ip;
    }

    /* CLIENT */

    [ClientRpc]
    private void RpcUpdateLobby()
    {
        if (playersNumLabel != null)
        {
            playersNumLabel.text = $"{gamePlayers.Count}/{maxPlayers}";
        }

        for (int i = 0; i < playerCards.Count; i++)
        {
            Destroy(playerCards[i]);
        }

        playerCards.Clear();

        foreach (var player in gamePlayers)
        {
            GameObject playerCard = Instantiate(lobbyPlayerPrefab, lobbyView);
            playerCard.GetComponentInChildren<TextMeshProUGUI>().text = player.playerName;
            playerCards.Add(playerCard);
        }
    }

    /* SERVER */
    public void AddPlayer(PlayerData playerData)
    {
        if (isServer)
        {
            gamePlayers.Add(playerData);
        }
    }

    public void RemovePlayer(PlayerData playerData)
    {
        if (isServer)
        {
            gamePlayers.Remove(playerData);
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int conn;

        public PlayerData()
        {
            playerName = string.Empty;
            conn = 0;
        }

        public PlayerData(string playerName, int connectionId)
        {
            this.playerName = playerName;
            this.conn = connectionId;
        }
    }
}