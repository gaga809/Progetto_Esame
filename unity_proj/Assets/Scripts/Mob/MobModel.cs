using UnityEngine;
using UnityEngine.AI;

public class MobModel : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;

    [Header("Mob Settings")]
    public float speed = 3.5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;

    private NavMeshAgent agent;
    private Vector3 lastPosition = Vector3.zero;   

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;

        if (player == null)
        {
            Debug.LogError("Giocatore non impostato");
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            Debug.Log(distance);
            if (distance > stoppingDistance)
            {
                if (lastPosition != player.position)
                {
                    lastPosition = player.position;
                    agent.SetDestination(player.position);
                }
            }
            else
            {
                //agent.ResetPath();
                Debug.Log("Il nemico è vicino al giocatore.");
            }

            
        }
    }
}
