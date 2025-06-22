using UnityEngine;
using UnityEngine.AI;

public class MenuMovingMob : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject slimePrefab;
    public int slimeCount = 30;
    public Vector3 spawnAreaSize = new Vector3(50f, 0f, 50f);
    public Vector3 spawnCenter = Vector3.zero;

    void Start()
    {
        for (int i = 0; i < slimeCount; i++)
        {
            Vector3 randomPosition = spawnCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );

            GameObject slime = Instantiate(slimePrefab, randomPosition, Quaternion.identity);
            slime.AddComponent<RandomWalker>();
        }
    }
}

public class RandomWalker : MonoBehaviour
{
    private NavMeshAgent agent;
    public float wanderRadius = 20f;
    public float wanderTimer = 5f;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = Random.Range(0f, wanderTimer);
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
            agent.SetDestination(newPos);
            timer = 0f;
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMesh.SamplePosition(randDirection, out NavMeshHit navHit, dist, NavMesh.AllAreas);

        return navHit.position;
    }
}
