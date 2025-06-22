using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Mirror.BouncyCastle.Bcpg.OpenPgp;
using Newtonsoft.Json;
using Unity.Android.Gradle.Manifest;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Networking;

public class WebServerAPI : MonoBehaviour
{
    public static WebServerAPI Instance { get; private set; }
    public string BaseUrl = "https://localhost:4433/api/v1/";

    [Header("Preferences")]
    public string jwtTokenPref;
    public string jwtRefreshPref;
    public string playerIdPref;
    public string playerNamePref;

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
        //request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey(); // TO REMOVE WHEN IN PRODUCTION. ITS ONLY BECAUSE I DON'T HAVE TIME TO SET Let's Encrypt SSL CERTIFICATE
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
        string token = PlayerPrefs.GetString(tokenName, null);

        if (string.IsNullOrEmpty(token))
        {
            callback?.Invoke(401, default, new ErrorResponse { message = "Access token is missing (client)" });
            yield break;
        }

        UnityWebRequest request = new UnityWebRequest(BaseUrl + url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        int statusCode = (int)request.responseCode;
        string responseText = request.downloadHandler.text;
        string contentType = request.GetResponseHeader("Content-Type");

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T responseObj = JsonConvert.DeserializeObject<T>(responseText);
                callback?.Invoke(statusCode, responseObj, null);
            }
            catch
            {
                callback?.Invoke(-1, default, null);
            }
        }
        else
        {
            ErrorResponse responseObj = null;

            try
            {
                if (!string.IsNullOrEmpty(responseText) && responseText.TrimStart().StartsWith("{"))
                {
                    responseObj = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
                }
            }
            catch { }

            callback?.Invoke(statusCode, default, responseObj ?? new ErrorResponse { message = "Unknown error or non-JSON response" });
        }
    }

    public void GetRequestWithToken<T>(string url, string tokenName, Action<int, T, ErrorResponse> callback)
    {
        StartCoroutine(GetCoroutineWithToken(url, tokenName, callback));
    }

    IEnumerator GetCoroutineWithToken<T>(string url, string tokenName, Action<int, T, ErrorResponse> callback)
    {
        string token = PlayerPrefs.GetString(tokenName, null);

        if (string.IsNullOrEmpty(token))
        {
            callback?.Invoke(401, default, new ErrorResponse { message = "Access token is missing (client)" });
            yield break;
        }

        UnityWebRequest request = UnityWebRequest.Get(BaseUrl + url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        int statusCode = (int)request.responseCode;
        string responseText = request.downloadHandler.text;
        string contentType = request.GetResponseHeader("Content-Type");

        Debug.Log($"Request returned: " + responseText);

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T responseObj = JsonConvert.DeserializeObject<T>(responseText);
                callback?.Invoke(statusCode, responseObj, null);
            }
            catch
            {
                callback?.Invoke(-1, default, null);
            }
        }
        else
        {
            ErrorResponse responseObj = null;

            try
            {
                if (!string.IsNullOrEmpty(responseText) && responseText.TrimStart().StartsWith("{"))
                {
                    responseObj = JsonConvert.DeserializeObject<ErrorResponse>(responseText);
                }
            }
            catch { }

            callback?.Invoke(statusCode, default, responseObj ?? new ErrorResponse { message = "Unknown error or non-JSON response" });
        }
    }



    public void CheckSession(Action<bool> onComplete)
    {
        StartCoroutine(CheckSessionCoroutine(onComplete));
    }

    private IEnumerator CheckSessionCoroutine(Action<bool> onComplete)
    {
        yield return GetCoroutineWithToken("users/me", jwtTokenPref, (int statusCode, UserMeResponse response, ErrorResponse errorObj) =>
        {
            if (statusCode == 201)
            {
                Debug.Log("Sessione valida. Utente autenticato con successo.");
                PlayerPrefs.SetString(playerNamePref, response.user.username);
                PlayerPrefs.SetInt(playerIdPref, response.user.id);
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError("Errore durante la convalida della sessione: " + (errorObj?.message ?? "Unknown error"));
                Debug.LogError("Sessione scaduta o non valida. Tentativo di prendere un nuovo access token.");
                PlayerPrefs.DeleteKey(jwtTokenPref);

                StartCoroutine(GetCoroutineWithToken("auth/refresh", jwtRefreshPref, (int refreshStatusCode, LoginResponse refreshResponse, ErrorResponse refreshError) =>
                {
                    if (refreshStatusCode == 201 && refreshResponse != null)
                    {
                        Debug.Log("Nuovo access token ottenuto con successo.");
                        PlayerPrefs.SetString(jwtTokenPref, refreshResponse.access_token);

                        StartCoroutine(GetCoroutineWithToken("users/me", jwtTokenPref, (int statusCode2, UserMeResponse response2, ErrorResponse errorObj2) =>
                        {
                            if (statusCode2 == 201)
                            {
                                Debug.Log("Nuova sessione valida. Utente autenticato con successo.");
                                PlayerPrefs.SetString(playerNamePref, response2.user.username);
                                PlayerPrefs.SetInt(playerIdPref, response2.user.id);
                                onComplete?.Invoke(true);
                            }
                            else
                            {
                                Debug.LogError($"Errore durante la convalida del token nuovo: {errorObj2?.message ?? "Unknown error"}");
                                PlayerPrefs.DeleteKey(jwtRefreshPref);
                                onComplete?.Invoke(false);
                            }
                        }));
                    }
                    else
                    {
                        Debug.LogError($"Errore durante il refresh del token: {refreshError?.message ?? "Unknown error"}");
                        PlayerPrefs.DeleteKey(jwtRefreshPref);
                        onComplete?.Invoke(false);
                    }
                }));
            }
        });
    }


    [Serializable]
    public class RefreshResponse
    {
        public string message;
        public string access_token;
        public string type;
    }

    [Serializable]
    public class UserMeResponse
    {
        public string message;
        public User user;
    }

    [Serializable]
    public class User
    {
        public int id;
        public string username;
        public string email;
        public bool isAdmin;
        public DateTime creation_date;
        public DateTime last_login;
        public string pfp;
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
