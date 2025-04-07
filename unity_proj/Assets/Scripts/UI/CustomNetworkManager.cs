using Mirror;
using TMPro;
using UnityEngine;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    [Header("Lobby Settings")]
    public GameObject lobbyPanel;
    public GameObject lobbyPlayerPrefab;
    public Transform lobbyView;
    public TextMeshProUGUI playersNumLabel;
    public UIManager UIManager;

    public int maxPlayers = 4;

    public void StartHosting()
    {
        this.StartHost();
        UIManager.GoTo(lobbyPanel);
        UpdateLobbyGraphic();
        Debug.Log("Hosting Iniziato");
    }

    public void StopHosting()
    {
        this.StopHost();
        UIManager.BackOne();
        Debug.Log("Hosting Fermato");
    }

    public void Connect()
    {
        this.StartClient();
        Debug.Log("Connettendo a " + NetworkManager.singleton.networkAddress);

    }

    public void Disconnect()
    {
        this.StopClient();
        UIManager.BackOne();
        Debug.Log("Disconesso da " + NetworkManager.singleton.networkAddress );
    }

    public override void OnRoomClientConnect()
    {
        UIManager.GoTo(lobbyPanel);
        Debug.Log("Connesso a " + networkAddress);
    }

    public override void OnRoomClientDisconnect()
    {
        this.Disconnect();
    }


    public override void OnRoomServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnRoomServerAddPlayer(conn);
        UpdateLobbyGraphic();
        Debug.Log("Aggiungo i giocatori");
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        UpdateLobbyGraphic();
        Debug.Log("Rimuovo i giocatori");
    }

    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        //base.OnRoomServerAddPlayer(conn);
        UpdateLobbyGraphic();
        Debug.Log("Aggiungo i giocatori");
    }


    private void UpdateLobbyGraphic()
    {
        // Rimuovi tutti gli oggetti esistenti dalla lobby view
        foreach (Transform child in lobbyView)
        {
            Destroy(child.gameObject);
        }

        // Crea una card per ogni giocatore nella lobby
        foreach (var slot in roomSlots)
        {
            if (slot != null)
            {
                GameObject playerCard = Instantiate(lobbyPlayerPrefab, lobbyView);
                playerCard.GetComponentInChildren<TextMeshProUGUI>().text = slot.name;
            }
        }

        // Aggiorna il numero di giocatori nella lobby
        playersNumLabel.text = $"{roomSlots.Count}/{maxPlayers} Giocatori";
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        base.OnRoomServerSceneChanged(sceneName);

        if (sceneName == "GameScene")
        {
            // Eventuali operazioni da fare quando la scena di gioco è caricata
        }
    }
}
