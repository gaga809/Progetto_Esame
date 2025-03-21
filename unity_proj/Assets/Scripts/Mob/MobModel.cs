using UnityEngine;
using UnityEngine.AI;

public class MobModel : MonoBehaviour
{
    [Header("Mob Settings")]
    public float speed = 3.5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
    }
}
