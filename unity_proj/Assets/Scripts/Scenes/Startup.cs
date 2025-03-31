using UnityEngine;

public class Startup : MonoBehaviour
{
    [Header("Settings")]
    public string DefaultScene = string.Empty;

    private void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(DefaultScene);
    }
}
