using UnityEngine;

public class Startup : MonoBehaviour
{
    [Header("Settings")]
    public string DefaultScene = string.Empty;
    public GameObject[] gameobjectToSave;

    private void Awake()
    {
        foreach (GameObject go in gameobjectToSave)
        {
            DontDestroyOnLoad(go);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(DefaultScene);
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(0);
    }
}
