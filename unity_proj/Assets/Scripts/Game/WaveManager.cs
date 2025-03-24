using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    public string waveJSONPath = "Waves.json";
    public float waveDelay = 5f;
    public float waveDifficultyMultiplier = 1.2f;

    [Header("UI")]
    public TextMeshProUGUI waveCounter;

    [Header("Enemies")]
    public string enemyPrefabPath = "Prefabs/Mob";

    [Header("Spawn Settings")]
    public float playerCircleRadius = 5f;
    public LayerMask platformLayer;

    private int currentWave = 0;
    private bool isSpawning = false;
    private Transform player;

    private Dictionary<string, GameObject> enemyPrefabs = new Dictionary<string, GameObject>();
    public Wave[] waves;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        LoadWaveData();
        LoadAllPrefabs();
    }

    void LoadWaveData()
    {
        string jsonText = File.ReadAllText("Assets/" + waveJSONPath);

        try
        {
            WaveData waveData = JsonUtility.FromJson<WaveData>(jsonText);

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

    // Update is called once per frame
    void Update()
    {
        waveCounter.text = "Wave: " + (currentWave + 1);

        if (!isSpawning)
        {
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

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;
        Debug.Log("Inizia l'ondata: " + wave.WaveName);

        for (int i = 0; i < wave.MobsCount; i++)
        {
            // Get spanw positon
            Vector3 SpawnPosition = GetValidSpawnPoint();

            // Get spawn direction
            GameObject enemyPrefab;

            if (wave.MobsPrefab.Length == 0)
            {
                enemyPrefab = enemyPrefabs.ElementAt(Random.Range(0, enemyPrefabs.Count - 1)).Value;
            }
            else
            {
                enemyPrefab = enemyPrefabs[wave.MobsPrefab[Random.Range(0, wave.MobsPrefab.Length)]];
            }
            Vector3 direction = player.position - enemyPrefab.transform.position;
            direction.x = 0;
            direction.z = 0;

            GameObject enemy = Instantiate(enemyPrefab, SpawnPosition, Quaternion.LookRotation(direction));


            // Apply wave difficulty multiplier (TODO)
            yield return new WaitForSeconds(wave.SpawnRate);
        }

        isSpawning = false;
        currentWave++;
    }

    Wave CreateDynamicWave(int waveIndex)
    {
        Wave newWave = null;
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>(enemyPrefabPath);
        string[] prefabs = new string[0];

        if (waves == null || waves.Length == 0)
        {
            newWave = new Wave("Ondata Predefinita", prefabs, 10, 1f, 1f, 1f);
        }
        else
        {
            newWave = new Wave
            (
                "Ondata " + (waveIndex + 1),
                prefabs,
                waves[waves.Length - 1].MobsCount + (waveIndex * 3),
                Mathf.Max(0.5f, waves[0].SpawnRate - 0.05f * waveIndex),
                1f + (waveIndex * 0.1f),
                1f + (waveIndex * 0.05f)
            );
        }

        Debug.Log("Ondata dinamica creata: " + newWave.WaveName);
        return newWave;
    }

    Vector3 GetValidSpawnPoint()
    {
        const int maxAttempts = 20; // Number of tentativies to try and spawn the enemy

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomAngle = Random.Range(0, 360);

            // Random point on circle around the player
            Vector3 randomPoint = new Vector3(
                player.position.x + playerCircleRadius * Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                1,
                player.position.z + playerCircleRadius * Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );

            // Raycast tto check for the platform
            bool hit = Physics.Raycast(randomPoint, Vector3.down, 5f, platformLayer);

            if (hit)
            {
                return randomPoint;
            }
        }

        Debug.LogWarning("Nessun punto di spawn valido trovato!");
        return Vector3.zero; // No valid Point found
    }
}

public class Wave
{
    private string _waveName;
    private string[] _mobsPrefab;
    private int _mobsCount;
    private float _spawnRate;
    private float _spawnRateMultiplier = 1f;
    private float _healthMultiplier = 1f;

    public Wave(string waveName, string[] mobsPrefab, int mobsCount, float spawnRate, float spawnRateMultiplier, float healtMultiplier)
    {
        _waveName = waveName;
        _mobsPrefab = mobsPrefab;
        _mobsCount = mobsCount;
        _spawnRate = spawnRate;
        _spawnRateMultiplier = spawnRateMultiplier;
        _healthMultiplier = healtMultiplier;
    }

    public string WaveName { get => _waveName; set => _waveName = value; }
    public string[] MobsPrefab { get => _mobsPrefab; set => _mobsPrefab = value; }
    public int MobsCount { get => _mobsCount; set => _mobsCount = value; }
    public float SpawnRate { get => _spawnRate; set => _spawnRate = value; }
    public float SpawnRateMultiplier { get => _spawnRateMultiplier; set => _spawnRateMultiplier = value; }
    public float HealthMultiplier { get => _healthMultiplier; set => _healthMultiplier = value; }
}

public class WaveData
{
    public Wave[] waves;
}
