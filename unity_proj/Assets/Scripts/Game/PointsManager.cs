using Mirror;
using UnityEngine;
using TMPro;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    [SyncVar(hook = nameof(OnScoreChanged))]
    private int score;

    [SerializeField] private TMP_Text scoreText; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreText();
    }

    [Server]
    public void AddScore(int value)
    {
        score += value;
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"KILL: {score}";
        }
    }
}
