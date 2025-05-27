using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject titleScreenPanel;
    public GameObject loginPanel;
    public GameObject mainMenuStartingPanel;
    public TMP_InputField hostIpInput;

    [Header("Account Info")]
    //public TMP_InputField nameInput;
    //public TMP_InputField passwordInput;
    public string jwtTokenPlayerPrefName = "jwt_token";
    public string playerNamePrefName = "playerName";


    private Stack<GameObject> heirachyList = new Stack<GameObject>();

    private bool isLoggedIn = false;


    void Start()
    {
        StartCoroutine(GetPastTitleScreen());
        heirachyList.Push(titleScreenPanel);
        titleScreenPanel.SetActive(true);
    }

    IEnumerator GetPastTitleScreen()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        //Login();
        titleScreenPanel.SetActive(false);
        GoTo(mainMenuStartingPanel);
    }


    /* MAIN MENU NAVIGATION */
    public void Login()
    {
        string jwtToken = PlayerPrefs.GetString(jwtTokenPlayerPrefName);
        // Login Procedure with JWT TOKENS and Web Server
        bool isJwtValid = true;
        if (isJwtValid)
        {
            isLoggedIn = true;
            OverwriteCurrentPanel(mainMenuStartingPanel);
        }
        else
            OverwriteCurrentPanel(loginPanel);

    }

    public void BackOne()
    {
        if(heirachyList.Count > 1)
        {
            GameObject gObj = heirachyList.Pop();
            gObj.SetActive(false);
            heirachyList.Peek().SetActive(true);
        }
    }

    public void GoTo(GameObject panel)
    {
        GameObject obj = heirachyList.Peek() as GameObject;
        if (obj != null) {
            obj.SetActive(false);
        }
        panel.SetActive(true);
        heirachyList.Push(panel);
        
    }

    public void OverwriteCurrentPanel(GameObject panel)
    {
        heirachyList.Pop().SetActive(false);
        heirachyList.Push(panel);
        panel.SetActive(true);
    }

    public void JoinRoom()
    {
        var manager = CustomNetworkRoomManager.singleton;

        manager.networkAddress = hostIpInput.text;
        manager.StartClient();
    }

    public void CreateRoom()
    {
        var manager = CustomNetworkRoomManager.singleton;
        PlayerPrefs.SetInt("numPlayers", 4);
        manager.StartHost();
    }

    public void CreateSinglePlayerRoom()
    {
        var manager = CustomNetworkRoomManager.singleton;
        PlayerPrefs.SetInt("numPlayers", 1);
        manager.StartHost();
    }


}
