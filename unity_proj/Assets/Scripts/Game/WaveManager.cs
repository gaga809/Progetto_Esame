using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [Header("Waves")]
    public string waveJSONPath = "Waves";
    public float waveDelay = 5f;
    public float waveDifficultyMultiplier = 1.2f;
    public float startWaittime = 2f;

    [Header("UI")]
    public TextMeshProUGUI waveCounter;

    [Header("Enemies")]
    public string enemyPrefabPath = "Prefabs/Mob";

    [Header("Spawn Settings")]
    public float playerCircleRadius = 5f;
    public LayerMask platformLayer;

    [SyncVar(hook = nameof(OnNextWave))]
    private int currentWave = -1;
    private bool isSpawning = false;
    private List<Transform> players;

    private Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();
    public Wave[] waves;
    public bool startSpawn = false;

    void OnEnable()
    {
        StartCoroutine(startingWait());
        LoadWaveData();
        LoadAllPrefabs();
    }

    IEnumerator startingWait()
    {
        yield return new WaitForSeconds(startWaittime);
        startSpawn = true;
    }

    void LoadWaveData()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(waveJSONPath);

        try
        {
            WaveData waveData = JsonUtility.FromJson<WaveData>(jsonText.text);

            if (waveData != null && waveData.waves != null)
            {
                waves = waveData.waves;
                Debug.Log("Dati delle ondate caricati con successo!");
            }
            else
            {
                Debug.LogError("Formato JSON non valido o waves è vuoto!");
                waves = new Wave[0];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Errore nel parsing del JSON: " + e.Message);
            waves = new Wave[0];
        }
    }

    void LoadAllPrefabs()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>(enemyPrefabPath);

        foreach (GameObject prefab in loadedPrefabs)
        {
            enemyPrefabs[prefab.name] = prefab;
            Debug.Log($"Prefab caricato: {prefab.name}");
        }

        if (enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("Nessun prefab trovato nella cartella: " + enemyPrefabPath);
        }
    }

    public GameObject GetPrefabByName(string name)
    {
        if (enemyPrefabs.ContainsKey(name))
        {
            return enemyPrefabs[name];
        }

        Debug.LogWarning("Prefab non trovato: " + name);
        return null;
    }

    void Update()
    {
        if (!isServer) return;

        if (startSpawn)
        {
            GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
            if (playerObjs.Length == 0)
            {
                Debug.Log("No players left - Changing scene");
                StartCoroutine(ChangeSceneWithDelay());
            }
            else if (!isSpawning)
            {
                currentWave++;
                if (currentWave < waves.Length)
                {
                    StartCoroutine(SpawnWave(waves[currentWave]));
                }
                else
                {
                    Debug.Log("Tutte le ondate finite! Looping o aumento infinito...");
                    StartCoroutine(SpawnWave(CreateDynamicWave(currentWave)));
                }
            }
        }
    }

    IEnumerator ChangeSceneWithDelay()
    {
        yield return new WaitForSeconds(1f);
        NetworkManager.singleton.ServerChangeScene("GameRoom");
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;
        Debug.Log("Inizia l'ondata: " + wave.WaveName);

        players = new List<Transform>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(obj.transform);
        }

        int playerCount = players.Count;
        int scaledMobCount = Mathf.RoundToInt(wave.MobsCount * Mathf.Max(playerCount, 1));
        float scaledHealthMultiplier = wave.HealthMultiplier * playerCount;

        for (int i = 0; i < scaledMobCount; i++)
        {
            if (players.Count == 0) break;

            Transform player = players[Random.Range(0, players.Count)];
            Vector3 SpawnPosition = GetValidSpawnPoint(player);

            GameObject enemyPrefab;

            if (wave.MobsPrefab.Length == 0)
            {
                enemyPrefab = enemyPrefabs.ElementAt(Random.Range(0, enemyPrefabs.Count)).Value;
            }
            else
            {
                enemyPrefab = enemyPrefabs[wave.MobsPrefab[Random.Range(0, wave.MobsPrefab.Length)]];
            }

            Vector3 direction = player.position - SpawnPosition;
            direction.y = 0;

            GameObject enemy = Instantiate(enemyPrefab, SpawnPosition, Quaternion.LookRotation(direction));
            NetworkServer.Spawn(enemy);

            if (enemy.TryGetComponent<MobModel>(out MobModel mobModel))
            {
                mobModel.maxHealth = Mathf.RoundToInt(mobModel.maxHealth * scaledHealthMultiplier);
                mobModel.health = mobModel.maxHealth;
            }

            yield return new WaitForSeconds(wave.SpawnRate);
        }

        isSpawning = false;
    }

    Wave CreateDynamicWave(int waveIndex)
    {
        string[] prefabs = new string[0];

        Wave newWave = new Wave
        (
            "Ondata " + (waveIndex + 1),
            prefabs,
            waves[waves.Length - 1].MobsCount + (waveIndex * 3),
            Mathf.Max(0.5f, waves[0].SpawnRate - 0.05f * waveIndex),
            1f + (waveIndex * 0.1f),
            1f + (waveIndex * 0.05f)
        );

        Debug.Log("Ondata dinamica creata: " + newWave.WaveName);
        return newWave;
    }

    Vector3 GetValidSpawnPoint(Transform player)
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomAngle = Random.Range(0, 360);

            Vector3 randomPoint = new Vector3(
                player.position.x + playerCircleRadius * Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                1,
                player.position.z + playerCircleRadius * Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );

            bool hit = Physics.Raycast(randomPoint, Vector3.down, 5f, platformLayer);

            if (hit)
            {
                return randomPoint;
            }
        }

        Debug.LogWarning("Nessun punto di spawn valido trovato!");
        return Vector3.zero;
    }

    private void OnNextWave(int oldValue, int newValue)
    {
        waveCounter.text = "WAVE: " + (newValue + 1);
    }
}

[System.Serializable]
public class Wave
{
    public string WaveName;
    public string[] MobsPrefab;
    public int MobsCount;
    public float SpawnRate;
    public float SpawnRateMultiplier;
    public float HealthMultiplier;

    public Wave(string waveName, string[] mobsPrefab, int mobsCount, float spawnRate, float spawnRateMultiplier, float healthMultiplier)
    {
        WaveName = waveName;
        MobsPrefab = mobsPrefab;
        MobsCount = mobsCount;
        SpawnRate = spawnRate;
        SpawnRateMultiplier = spawnRateMultiplier;
        HealthMultiplier = healthMultiplier;
    }
}

[System.Serializable]
public class WaveData
{
    public Wave[] waves;
}
