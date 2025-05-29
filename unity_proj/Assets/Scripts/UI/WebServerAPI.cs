using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerAPI : MonoBehaviour
{
    public static WebServerAPI Instance { get; private set; }
    public string BaseUrl = "https://localhost:4433/api/v1/";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/WebServer");
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                DontDestroyOnLoad(instance);
            }
            else
            {
                Debug.LogError("WebServer prefab non trovato in Resources/Prefab");
            }
        }
    }

    public void PostRequest<T>(string url, object data, Action<int, T, ErrorResponse> callback)
    {
        StartCoroutine(PostCoroutine(url, data, callback));
    }

    IEnumerator PostCoroutine<T>(string url, object data, Action<int, T, ErrorResponse> callback)
    {
        string jsonData = JsonConvert.SerializeObject(data);

        UnityWebRequest request = new UnityWebRequest(BaseUrl + url, "POST");
        request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey(); // TO REMOVE WHEN IN PRODUCTION. ITS ONLY BECAUSE I DON'T HAVE TIME TO SET Let's Encrypt SSL CERTIFICATE
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        int statusCode = (int)request.responseCode;
        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T responseObj = JsonConvert.DeserializeObject<T>(responseText);
                callback?.Invoke(statusCode, responseObj, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Errore nel parsing JSON: " + e.Message);
                callback?.Invoke(statusCode, default, null);
            }
        }
        else
        {
            ErrorResponse responseObj = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
            Debug.LogError($"HTTP Error {statusCode}: {responseObj.message}");
            callback?.Invoke(statusCode, default, responseObj);
        }
    }

    public void PostRequestWithToken<T>(string url, object data, string tokenName, Action<int, T, ErrorResponse> callback)
    {
        StartCoroutine(PostCoroutineWithToken(url, data, tokenName, callback));
    }

    IEnumerator PostCoroutineWithToken<T>(string url, object data, string tokenName, Action<int, T, ErrorResponse> callback)
    {
        string jsonData = JsonConvert.SerializeObject(data);

        UnityWebRequest request = new UnityWebRequest(BaseUrl + url, "POST");
        request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey(); // TO REMOVE WHEN IN PRODUCTION. ITS ONLY BECAUSE I DON'T HAVE TIME TO SET Let's Encrypt SSL CERTIFICATE
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {PlayerPrefs.GetString(tokenName)}");

        yield return request.SendWebRequest();

        int statusCode = (int)request.responseCode;
        string responseText = request.downloadHandler.text;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T responseObj = JsonConvert.DeserializeObject<T>(responseText);
                callback?.Invoke(statusCode, responseObj, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Errore nel parsing JSON: " + e.Message);
                callback?.Invoke(-1, default, null);
            }
        }
        else
        {
            ErrorResponse responseObj = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
            Debug.LogError($"HTTP Error {statusCode}: {responseObj.message}");
            callback?.Invoke(statusCode, default, responseObj);
        }
    }

    [Serializable]
    public class LoginResponse
    {
        public int id;
        public string username;
        public bool isAdmin;
        public string message;
        public string access_token;
        public string refresh_token;
        public string type;
    }

    [Serializable]
    public class LoginRequest
    {
        public string username;
        public string email;
        public string password;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string message;
    }

    // TO REMOVE WHEN IN PRODUCTION. ITS ONLY BECAUSE I DON'T HAVE TIME TO SET Let's Encrypt SSL CERTIFICATE
    private class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 🔓 Accetta tutti i certificati, ignora qualsiasi errore SSL
            return true;
        }
    }

}
