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
    public float attackRate = 2f;

    private NavMeshAgent agent;
    private Transform trsPly;
    private bool canAttack = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Giocatore non impostato!");
            return;
        }

        trsPly = player.transform;
        agent = GetComponent<NavMeshAgent>();

        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, trsPly.position);

            if (distance > stoppingDistance)
            {
                agent.SetDestination(trsPly.position);
            }
            else
            {
                agent.ResetPath();
                if (canAttack)
                {
                    StartCoroutine(AttackPlayer());
                }
            }
        }
    }

    IEnumerator AttackPlayer()
    {
        canAttack = false;

        PlayerModel playerModel = player.GetComponent<PlayerModel>();
        if (playerModel != null)
        {
            Debug.Log("Attacco eseguito! Danno: " + attackDamage);
            playerModel.Hurt(attackDamage);
        }
        else
        {
            Debug.LogError("PlayerModel non trovato!");
        }

        yield return new WaitForSeconds(attackRate);
        canAttack = true;
    }

    public void Hurt(int damage)
    {
        Debug.Log("Il nemico ha subito " + damage + " danni.");
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
