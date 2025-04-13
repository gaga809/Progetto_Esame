using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class MobModel : NetworkBehaviour
{
    [Header("Player Settings")]
    [SyncVar(hook = nameof(OnClosestPlayerChanged))]
    private GameObject player;

    [Header("Mob Settings")]
    public int health = 5;
    public int maxHealth = 5;
    public float speed = 5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;
    public int attackDamage = 1;
    public float attackRate = 2f;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpForwardForce = 5f;
    public float jumpCooldown = 2f;


    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform trsPly;
    private bool canAttack = true;
    private Coroutine jumpRoutine;
    private bool isGrounded = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = stoppingDistance;

        rb.linearDamping = 2f;
        rb.angularDamping = 2f;

        if (isServer)
        {
            FindClosestPlayer();
            jumpRoutine = StartCoroutine(JumpLoop());
        }
    }

    void Update()
    {
        if (!isServer) return;

        FindClosestPlayer();

        if (player == null || trsPly == null) return;

        float distance = Vector3.Distance(transform.position, trsPly.position);

        Vector3 direction = (trsPly.position - transform.position).normalized;
        direction.y = 0f; 
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }

        if (distance > stoppingDistance)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(trsPly.position);
            }
        }
        else
        {
            if (canAttack)
            {
                StartCoroutine(AttackPlayer());
            }
        }

        CheckIfGrounded();
    }

    void CheckIfGrounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, 1.2f);
    }

    IEnumerator JumpLoop()
    {
        WaitForSeconds cooldown = new WaitForSeconds(jumpCooldown);

        while (true)
        {
            if (trsPly != null && agent.isOnNavMesh && isGrounded)
            {
                agent.enabled = false;

                yield return new WaitForFixedUpdate();

                Vector3 direction = (trsPly.position - transform.position).normalized;
                direction.y = 0f;

                Vector3 jumpVector = direction * jumpForwardForce + Vector3.up * jumpForce;
                rb.AddForce(jumpVector, ForceMode.Impulse);

                yield return cooldown;

                if (agent != null && !agent.enabled)
                    agent.enabled = true;
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator AttackPlayer()
    {
        canAttack = false;

        PlayerModel playerModel = player.GetComponent<PlayerModel>();
        if (playerModel != null)
        {
            playerModel.Hurt(attackDamage);
            Debug.Log("Player hit!");
        }

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

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = p;
            }
        }

        if (closest != player)
        {
            player = closest;
        }

        if (player != null)
        {
            trsPly = player.transform;
        }
    }

    private void OnClosestPlayerChanged(GameObject oldPlayer, GameObject newPlayer)
    {
        player = newPlayer;
        trsPly = player?.transform;
    }
}
