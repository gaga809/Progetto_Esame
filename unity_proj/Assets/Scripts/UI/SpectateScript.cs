using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpectateScript : MonoBehaviour
{

    public GameObject DeathPanel;
    public GameObject WaitForHostText;
    public GameObject BtnReturnToLobby;
    public GameObject SpectatorPanel;
    public TextMeshProUGUI SpectatorPanelUser;
    public string playerTag;
    public bool ded = false;

    private List<GameObject> playersStillAlive;

    public void GetPlayersAlive()
    {
        playersStillAlive = new List<GameObject>(GameObject.FindGameObjectsWithTag(playerTag));
    }

    public void StartSpectating()
    {
        GetPlayersAlive();
        if (playersStillAlive.Count > 0)
        {
            SpectatorPanel.SetActive(true);
            SpectatorPanelUser.text = playersStillAlive[0].name;
            Camera.main.GetComponent<CameraController>().playerT = playersStillAlive[0].transform;
        }
    }

    public bool LastPlayer()
    {
        GetPlayersAlive();
        return playersStillAlive.Count <= 0;
    }

    public void NextPlayer(int adv)
    {
        if (playersStillAlive.Count > 0)
        {
            int currentIndex = playersStillAlive.FindIndex(player => player.transform == Camera.main.GetComponent<CameraController>().playerT);
            if (currentIndex == -1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex = (currentIndex + adv) % playersStillAlive.Count;
                Camera.main.GetComponent<CameraController>().playerT = playersStillAlive[currentIndex].transform;
                SpectatorPanelUser.text = playersStillAlive[currentIndex].name;
            }
        }
    }
}
