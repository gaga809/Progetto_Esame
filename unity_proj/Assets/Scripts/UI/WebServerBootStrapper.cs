using UnityEngine;

public class WebServerBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        if (WebServerAPI.Instance == null)
        {
            GameObject prefab = Resources.Load<GameObject>("WebServer"); // il nome del prefab
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab);
                DontDestroyOnLoad(instance);
            }
            else
            {
                Debug.LogError("WebServer prefab not found in Resources!");
            }
        }
    }
}
