using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class MobModel : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnClosestPlayerChanged))]
    protected GameObject player;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 5;
    public int maxHealth = 5;
    public float speed = 5f;
    public float acceleration = 8f;
    public float stoppingDistance = 1f;
    public float attackRange = 2.5f;
    public float detectionRange = 30f;
    public int attackDamage = 1;
    public float attackRate = 2f;

    public GameObject healthCanvas;
    public RectTransform healthBar;

    public Transform visualTransform;
    public Transform colorTransform;
    public Transform modelTransform;
    public float visualJumpHeight = 1.5f;
    public float visualJumpDuration = 0.5f;
    public float jumpCooldown = 1;

    private float minRightOffset = 0.1f;
    private float maxRightOffset;
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Transform trsPly;
    protected bool canAttack = true;
    private Coroutine jumpRoutine;
    protected bool isGrounded = true;

    // Nuove variabili per il colore
    private Material originalMaterial;
    private Renderer modelRenderer;
    [SyncVar] private Color originalColor;

    protected virtual void Start()
    {
        if (healthCanvas != null && healthBar != null)
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

        ApplyRandomColor();

        // Setup renderer e materiale originale
        if (colorTransform != null)
        {
            Transform sphereTransform = colorTransform.Find("Sphere");
            if (sphereTransform != null)
            {
                modelRenderer = sphereTransform.GetComponent<Renderer>();
                if (modelRenderer != null)
                {
                    originalMaterial = modelRenderer.material;
                }
            }
        }

        if (isServer)
        {
            FindClosestPlayer();

            if (visualJumpHeight != 0f && visualJumpDuration != 0)
            {
                jumpRoutine = StartCoroutine(JumpLoop());
            }
        }
    }

    protected virtual void Update()
    {
        if (healthCanvas != null && healthCanvas.activeSelf)
        {
            Transform cam = Camera.main.transform;
            Vector3 toCameraFlat = cam.forward;
            toCameraFlat.y = 0f;
            toCameraFlat.Normalize();
            Vector3 basePosition = transform.position;
            healthCanvas.transform.position = basePosition + toCameraFlat * -1f + Vector3.up * 2f;
            healthCanvas.transform.LookAt(cam);
            healthCanvas.transform.rotation = Quaternion.Euler(0f, healthCanvas.transform.rotation.eulerAngles.y + 180f, 0f);
        }

        if (!isServer) return;

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
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
            }
        }

        if (distance <= attackRange)
        {
            if (canAttack)
            {
                StartCoroutine(AttackPlayer());
            }

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }
        else if (distance <= detectionRange)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(trsPly.position);
            }
        }

        CheckIfGrounded();
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (healthCanvas != null && !healthCanvas.activeSelf)
            healthCanvas.SetActive(true);

        if (healthBar == null) return;

        float healthPercentage = Mathf.Clamp01((float)newHealth / maxHealth);
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
            if (trsPly != null && isGrounded)
            {
                VisualJump();
                yield return cooldown;
            }
            else
            {
                yield return null;
            }
        }
    }

    private void VisualJump()
    {
        if (modelTransform == null) return;

        float halfDuration = visualJumpDuration / 2f;

        Sequence jumpSeq = DOTween.Sequence();

        jumpSeq.Append(modelTransform.DOLocalMoveY(visualJumpHeight, halfDuration)
            .SetEase(Ease.OutQuad));

        jumpSeq.Join(modelTransform.DOScaleY(0.7f, halfDuration).SetEase(Ease.OutQuad));
        jumpSeq.Join(modelTransform.DOScaleX(1.2f, halfDuration).SetEase(Ease.OutQuad));
        jumpSeq.Join(modelTransform.DOScaleZ(1.2f, halfDuration).SetEase(Ease.OutQuad));

        jumpSeq.Append(modelTransform.DOLocalMoveY(0f, halfDuration)
            .SetEase(Ease.InQuad));

        jumpSeq.Join(modelTransform.DOScaleY(1f, halfDuration).SetEase(Ease.InQuad));
        jumpSeq.Join(modelTransform.DOScaleX(1f, halfDuration).SetEase(Ease.InQuad));
        jumpSeq.Join(modelTransform.DOScaleZ(1f, halfDuration).SetEase(Ease.InQuad));
    }

    protected IEnumerator AttackPlayer()
    {
        canAttack = false;

        PlayerModel playerModel = player.GetComponent<PlayerModel>();
        if (playerModel != null)
        {
            playerModel.Hurt(attackDamage);
        }

        yield return new WaitForSeconds(attackRate);
        canAttack = true;
    }

    public void Hurt(int damage, PlayerModel pm)
    {
        health -= damage;

        if (modelRenderer != null)
        {
            if(isServer)
                StartCoroutine(HitFlash());
        }

        if (health <= 0)
        {
            if (isServer)
            {
                pm.kills++;
            }

            if (healthBar != null)
            {
                healthBar.DOComplete();
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator HitFlash()
    {
        Material redMat = Resources.Load<Material>("Materials/Red");
        if (redMat != null && modelRenderer != null)
        {
            RpcSetColor(redMat.color);
            yield return new WaitForSeconds(0.2f);
            RpcSetColor(originalColor);

        }
    }

    protected void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float closestDistance = detectionRange;

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

    private void ApplyRandomColor()
    {
        string[] materialNames = { "Grass", "Stone", "Green","LightBlue","Orange", "Blue", "Yellow", "Purple" };
        string selectedMaterialName = materialNames[UnityEngine.Random.Range(0, materialNames.Length)];
        Material randomMat = Resources.Load<Material>("Materials/" + selectedMaterialName);

        if (randomMat == null) return;

        if (isServer)
        {
            RpcSetColor(randomMat.color);
            originalColor = randomMat.color;
        }
    }

    [ClientRpc]
    public void RpcSetColor(Color color)
    {
        Transform sphereTransform = colorTransform != null ? colorTransform.Find("Sphere") : null;
        if (sphereTransform == null) return;

        Renderer renderer = sphereTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }
}
