using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using static WebServerAPI;

public class UIManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject titleScreenPanel;
    public GameObject loginPanel;
    public GameObject mainMenuStartingPanel;
    public TMP_InputField hostIpInput;

    [Header("Account Info")]
    public string ip;
    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public string jwtTokenPlayerPrefName = "jwt_token";
    public string jwtRefreshPlayerPrefName = "jwt_refresh";
    public string playerIdPrefName = "playerId";
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

        Login();
        titleScreenPanel.SetActive(false);
    }


    /* MAIN MENU NAVIGATION */
    public void Login()
    {
        PlayerPrefs.DeleteKey(jwtTokenPlayerPrefName); // To remove when in production
        string jwtToken = PlayerPrefs.GetString(jwtTokenPlayerPrefName, "");
        Debug.Log("JWT Token: '" + jwtToken + "'");
        if (jwtToken.Length > 0)
        {
            // TODO: Try users/me
            isLoggedIn = true;
            OverwriteCurrentPanel(mainMenuStartingPanel);
        }
        else
        {
            //Prompt the login
            OverwriteCurrentPanel(loginPanel);
        }

    }

    public void SendLogin()
    {
        WebServerAPI.EnsureInstance();

        string name = nameInput.text;
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Name or Password cannot be empty.");
            return;
        }

        var data = new 
        {
            username = !System.Text.RegularExpressions.Regex.IsMatch(name, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? name : null,
            email = System.Text.RegularExpressions.Regex.IsMatch(name, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? name : null,
            password = password
        };

        WebServerAPI.Instance.PostRequest<WebServerAPI.LoginResponse>($"auth/login", data, (statusCode, response, error) =>
        {
            Debug.Log($"Login response status code: {statusCode}");
            if (statusCode != 201 || response == null)
            {
                Debug.LogError("Login failed or response is null.");
                return;
            }
            if (statusCode == 201 && response != null)
            {
                Debug.Log(response.message);
                PlayerPrefs.SetString(jwtTokenPlayerPrefName, response.access_token);
                PlayerPrefs.SetString(jwtRefreshPlayerPrefName, response.refresh_token);
                PlayerPrefs.SetInt(playerIdPrefName, response.id);
                PlayerPrefs.SetString(playerNamePrefName, response.username);

                isLoggedIn = true;
                OverwriteCurrentPanel(mainMenuStartingPanel);
            }
        });

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
