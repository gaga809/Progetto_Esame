using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    [Header("Game Settings")]
    public int maxPlayers = 4;
    public int currentPlayersNum;

    [Header("Room Settings")]
    public Vector3[] playerRoomPositions;
    public Quaternion[] playerRoomRotations;
    private NetworkConnectionToClient[] connections;

    private bool anotherLobby = true;

    public override void Start()
    {
        base.Start();
        minPlayers = 0;

        connections = new NetworkConnectionToClient[maxPlayers];
        for (int i = 0; i < maxPlayers; i++)
            connections[i] = null;
    }

    /* ROOM METHODS*/

    public override void OnRoomStartHost()
    {
        maxPlayers = PlayerPrefs.GetInt("numPlayers");
        base.OnRoomStopHost();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        if (currentPlayersNum >= maxPlayers)
        {
            Debug.LogWarning($"Connection refused: Max players for this session is {maxPlayers}.");
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        if (anotherLobby)
        {
            foreach (var c in NetworkServer.connections)
            {
                if (c.Value.identity != null)
                {
                    var rp = c.Value.identity.GetComponent<NetworkRoomPlayer>();
                    if (rp != null)
                    {
                        NetworkServer.Destroy(rp.gameObject);
                    }
                }
            }

            connections = new NetworkConnectionToClient[maxPlayers];
            currentPlayersNum = 0;
            minPlayers = 0;

            anotherLobby = false;
        }

        int playerIndex = FindNextIndex();


        Vector3 spawnPos = playerRoomPositions != null ? playerRoomPositions[playerIndex] : Vector3.zero;
        Quaternion spawnRot = playerRoomRotations != null ? playerRoomRotations[playerIndex] : Quaternion.identity;

        GameObject roomPlayer = Instantiate(roomPlayerPrefab.gameObject, spawnPos, spawnRot);
        roomPlayer.GetComponent<SvlimeRoomPlayerBehaviour>().playerName = "Player " + (playerIndex + 1);

        connections[playerIndex] = conn;

        minPlayers++;
        currentPlayersNum++;
        Debug.Log($"Room player creato per connessione {conn.connectionId} allo slot {playerIndex}");
        return roomPlayer;
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        RemoveConnection(conn);
        minPlayers--;
        currentPlayersNum--;
    }

    public int FindNextIndex()
    {
        int i = 0;
        while (i < maxPlayers && connections[i] != null) i++;

        return i;
    }

    private void RemoveConnection(NetworkConnectionToClient conn)
    {
        int i = 0;
        while (connections[i] != conn) i++;

        connections[i] = null;
    }

    public override void OnRoomServerPlayersReady()
    {
        if (NetworkServer.active)
        {
            //base.OnRoomServerPlayersReady()
            anotherLobby = true;

            ServerChangeScene(GameplayScene);

        }
    }

    /* END ROOM METHODS*/
    /* GAME METHODS */

    private int playerModelsCreated = 0;

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);

        var room = roomPlayer.GetComponent<SvlimeRoomPlayerBehaviour>();
        var game = gamePlayer.GetComponent<PlayerModel>();

        game.playerName = room.playerName;

        playerModelsCreated++;

        Debug.Log($"Game player creato per {conn.connectionId}. Totale creati: {playerModelsCreated}/{numPlayers}");

        // Se tutti i game players sono stati creati
        if (playerModelsCreated >= numPlayers)
        {
            Debug.Log("Tutti i PlayerModel sono stati creati!");
            OnAllPlayerModelsSpawned();
        }

        return true;
    }

    private void OnAllPlayerModelsSpawned()
    {
        GameObject waveManager = GameObject.Find("GameHandler");
        waveManager.SetActive(true);
    }

    public override void OnRoomServerSceneChanged(string sceneName)
    {
        Debug.Log($"Scene changed to: {sceneName}");

        base.OnRoomServerSceneChanged(sceneName);
    }

    /* END GAME METHODS*/
}
