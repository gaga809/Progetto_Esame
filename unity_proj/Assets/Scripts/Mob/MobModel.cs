using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MobModel : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject player;

    [Header("Mob Settings")]
    public int health = 5;
    public int maxHealth = 5;
    public float speed = 3.5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;
    public int attackDamage = 1;
    public float attackRate = 1f;

    private NavMeshAgent agent;
    private Vector3 lastPosition = Vector3.zero;
    private Transform trsPly;
    private bool canAttack = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        trsPly = player.transform;

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
            float distance = Vector3.Distance(transform.position, trsPly.position);
            Debug.Log(distance);
            if (distance > stoppingDistance)
            {
                if (lastPosition != trsPly.position)
                {
                    lastPosition = trsPly.position;
                    agent.SetDestination(trsPly.position);
                }
            }
            else
            {
                Debug.Log("Il nemico è vicino al giocatore.");
                if (canAttack)
                {
                    StartCoroutine(CooldownAttack());
                    player.GetComponent<PlayerModel>().Hurt(attackDamage);
                }
            }

            
        }
    }

    IEnumerator CooldownAttack()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackRate);
        canAttack = true;
    }

    public void Hurt(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
