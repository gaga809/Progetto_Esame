using Mirror;
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

    public override void Start()
    {
        base.Start();
        minPlayers = 0;

        connections = new NetworkConnectionToClient[maxPlayers];
        for (int i = 0; i < maxPlayers; i++)
            connections[i] = null;
    }

    /* ROOM METHODS*/

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        int playerIndex = FindNextIndex();

        if (playerIndex >= maxPlayers)
        {
            Debug.LogWarning("Tutti gli slot sono pieni! Connessione rifiutata.");
            return null;
        }

        Vector3 spawnPos = playerRoomPositions != null ? playerRoomPositions[playerIndex] : Vector3.zero;
        Quaternion spawnRot = playerRoomRotations != null ? playerRoomRotations[playerIndex] : Quaternion.identity;

        GameObject roomPlayer = Instantiate(roomPlayerPrefab.gameObject, spawnPos, spawnRot);

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

    private int FindNextIndex()
    {
        int i = 0;
        while(i<maxPlayers && connections[i] != null) i++;

        return i;
    }

    private void RemoveConnection(NetworkConnectionToClient conn)
    {
        int i = 0;
        while (connections[i] != conn) i++;

        connections[i] = null;
    }

    public override void OnRoomServerPlayersReady() {
        if (NetworkServer.active)
        {
            //base.OnRoomServerPlayersReady();
            ServerChangeScene(GameplayScene);
        }
    }

    /* END ROOM METHODS*/
    /* GAME METHODS */

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);

        var room = roomPlayer.GetComponent<SvlimeRoomPlayerBehaviour>();
        var game = gamePlayer.GetComponent<PlayerModel>();

        game.playerName = room.playerName;

        return true;
    }

    /* END GAME METHODS*/
}
