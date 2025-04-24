using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class MobModel : NetworkBehaviour
{
    [Header("Player Settings")]
    [SyncVar(hook = nameof(OnClosestPlayerChanged))]
    protected GameObject player;

    [Header("Mob Settings")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 5;
    public int maxHealth = 5;
    public float speed = 5f;
    public float acceleration = 8f;
    public float stoppingDistance = 2f;
    public int attackDamage = 1;
    public float attackRate = 2f;

    [Header("UI Settings")]
    public GameObject healthCanvas;
    public RectTransform healthBar;

    [Header("Jump Settings")]
    public float jumpForce = 8f;
    public float jumpForwardForce = 5f;
    public float jumpCooldown = 2f;

    [Header("Visual Rotation")]
    public Transform visualTransform; // <-- parte visiva da ruotare (es: mesh del mob)

    private float minRightOffset = 0.1f;
    private float maxRightOffset;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Transform trsPly;
    protected bool canAttack = true;
    private Coroutine jumpRoutine;
    protected bool isGrounded = true;

    protected virtual void Start()
    {
        // Make the ui look at the camera
        if (healthCanvas != null)
        {
            minRightOffset = -healthBar.offsetMax.x;
            maxRightOffset = 1 - minRightOffset;
            healthCanvas.SetActive(false);
        }

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

            if (jumpForce != 0f || jumpForwardForce != 0f || jumpCooldown != 0f)
            {
                jumpRoutine = StartCoroutine(JumpLoop());
            }
        }
    }

    protected virtual void Update()
    {
        if(healthCanvas.activeSelf)
        {
            Transform cam = Camera.main.transform;

            Vector3 toCameraFlat = cam.forward;
            toCameraFlat.y = 0f;
            toCameraFlat.Normalize();

            Vector3 basePosition = transform.position;
            healthCanvas.transform.position = basePosition + toCameraFlat * -1f;

            healthCanvas.transform.LookAt(cam);
            healthCanvas.transform.rotation = Quaternion.Euler(0f, healthCanvas.transform.rotation.eulerAngles.y + 180f, 0f);
        }



        if (!isServer)
        {
            return;
        }

        FindClosestPlayer();

        if (player == null || trsPly == null) return;

        float distance = Vector3.Distance(transform.position, trsPly.position);

        Vector3 direction = (trsPly.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            if (visualTransform != null)
            {
                visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, toRotation, Time.deltaTime * 10f);
            }
            else
            {
                // fallback: ruota tutto il mob se visualTransform non è assegnato
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
            }
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
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (!healthCanvas.activeSelf)
            healthCanvas.SetActive(true);

        float healthPercentage = Mathf.Clamp01((float)newHealth / maxHealth);

        // Calcolo corretto: da piena (-minRightOffset) a vuota (-maxRightOffset)
        float barRange = maxRightOffset - minRightOffset;
        float targetRightOffset = minRightOffset + (1f - healthPercentage) * barRange;

        Vector2 currentOffsetMax = healthBar.offsetMax;
        Vector2 targetOffsetMax = new Vector2(-targetRightOffset, currentOffsetMax.y);

        healthBar.DOComplete();
        DOTween.To(() => healthBar.offsetMax,
                   x => healthBar.offsetMax = x,
                   targetOffsetMax,
                   0.3f).SetEase(Ease.OutCubic);
    }


    protected void CheckIfGrounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, 1.2f);
    }

    protected IEnumerator JumpLoop()
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

    protected IEnumerator AttackPlayer()
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

    protected void FindClosestPlayer()
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
