using Mirror;
using TMPro;
using UnityEngine;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    public Vector3[] playerRoomPositions;
    public Quaternion[] playerRoomRotations;
    private int playerIndex = 0;

    public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnectionToClient conn)
    {
        Vector3 spawnPos = playerRoomPositions != null ? playerRoomPositions[playerIndex] : Vector3.zero;
        Quaternion spawnRot = playerRoomRotations != null ? playerRoomRotations[playerIndex] : Quaternion.identity;

        GameObject roomPlayer = Instantiate(roomPlayerPrefab.gameObject, spawnPos, spawnRot);

        playerIndex++;

        return roomPlayer;
    }

    public override void OnRoomServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnRoomServerDisconnect(conn);
        playerIndex--;
    }
}
