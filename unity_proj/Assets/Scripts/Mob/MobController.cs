using UnityEngine;
using UnityEngine.AI;

public class MobController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    public float followDistance = 10f;
    public float stopDistance = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("Il giocatore non è stato trovato.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        Vector3 lookAtTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookAtTarget);

        if (distance < followDistance)
        {
            if (distance > stopDistance)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                Debug.Log("Il nemico è vicino al giocatore.");
                // agent.ResetPath(); 
            }
        }
    }
}
