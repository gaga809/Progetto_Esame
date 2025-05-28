using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Mirror;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using Edgegap;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

public class PlayerModel : NetworkBehaviour
{
    [Header("User Info")]
    public string playerNamePref = "playerName";
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SerializeField] private TextMeshProUGUI nameText;
    public GameObject UI;

    [Header("Player Statuses")]
    [SyncVar(hook = nameof(OnDeathStatusChanged))]
    public bool died = false;
    [SyncVar(hook = nameof(OnKillsStatusChanged))]
    public int kills = 0;

    [Header("UI Settings")]
    public GameObject healthCanvas;
    public RectTransform healthBar;
    public TextMeshProUGUI killsPanel;
    public SpectateScript spectateScript;

    [Header("Player Settings")]
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 10;
    [SyncVar(hook = nameof(OnMaxHealthChanged))]
    public int maxHealth = 10;
    public bool canJump = false;
    public float speed = 5f;
    public float deceleration = 5f;
    public float minSpeed = 0.1f;
    public float jumpForce = 5f;
    public int attackDamage = 5;
    public float attackRate = 1f;
    public float rangeRadius = 10f;
    public InputActionAsset playerActions;
    public PlayerInput playerControls;
    public Transform model;
    public GameObject projectilePrefab;
    public GameObject particlesPrefab;
    public string deadTag;
    public string playerTag;

    [Header("Fall Settings")]
    public float fallDeathY = -20f;

    [Header("Damage UI")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float damageDisplayTime = 2f;

    private float minRightOffset = 0.1f;
    private float maxRightOffset;
    private PlayerController playerController;
    private bool canAttack = true;

    private Coroutine damageCoroutine;
    private int cumulativeDamage = 0;
    private List<GameObject> playersStillAlive = new List<GameObject>();

    void OnNameChanged(string _, string newName)
    {
        UpdateNameDisplay(newName);
    }

    void OnDeathStatusChanged(bool oldValue, bool newValue)
    {
        if (newValue && !oldValue)
        {
            Die();
        }
    }

    void OnKillsStatusChanged(int oldValue, int newValue)
    {
        kills = newValue;

        if (isLocalPlayer)
        {
            killsPanel.text = "Kills: " + kills;
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    private void Start()
    {

        if (healthCanvas != null)
        {
            minRightOffset = -healthBar.offsetMax.x;
            maxRightOffset = 1 - minRightOffset;
            OnHealthChanged(maxHealth, maxHealth);
        }

        if (isLocalPlayer)
        {
            StartCoroutine(AutoAttackLoop());
            Camera.main.GetComponent<CameraController>().playerT = transform;
        }

        playerController = new PlayerController(speed, deceleration, minSpeed, jumpForce,
                            gameObject.transform, gameObject.GetComponent<Rigidbody>(), model, this);

        if (damageText != null)
            damageText.gameObject.SetActive(false);

        
    }



    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        health = newHealth;

        float healthPercentage = Mathf.Clamp01((float)health / maxHealth);

        float barRange = maxRightOffset - minRightOffset;
        float targetRightOffset = minRightOffset + (1f - healthPercentage) * barRange;

        Vector2 currentOffsetMax = healthBar.offsetMax;
        Vector2 targetOffsetMax = new Vector2(-targetRightOffset, currentOffsetMax.y);

        healthBar.DOComplete();
        DOTween.To(() => healthBar.offsetMax,
                   x => healthBar.offsetMax = x,
                   targetOffsetMax,
                   0.3f).SetEase(Ease.OutCubic);

        Debug.Log($"Health: {health}, MaxHealth: {maxHealth}, HealthPercentage: {healthPercentage}, TargetRightOffset: {targetRightOffset}");


        ChangeHealthColor(healthPercentage);
    }

    private void OnMaxHealthChanged(int oldHealth, int newHealth)
    {
        maxHealth = newHealth;

        float healthPercentage = Mathf.Clamp01((float)health / maxHealth);

        float barRange = maxRightOffset - minRightOffset;
        float targetRightOffset = minRightOffset + (1f - healthPercentage) * barRange;

        Vector2 currentOffsetMax = healthBar.offsetMax;
        Vector2 targetOffsetMax = new Vector2(-targetRightOffset, currentOffsetMax.y);

        healthBar.DOComplete();
        DOTween.To(() => healthBar.offsetMax,
                   x => healthBar.offsetMax = x,
                   targetOffsetMax,
                   0.3f).SetEase(Ease.OutCubic);

        ChangeHealthColor(healthPercentage);
    }

    private void ChangeHealthColor(float healthPerc)
    {
        Image healtBarImage = healthBar.GetComponent<Image>();

        if (healthPerc < 0.2f)
            healtBarImage.color = Color.red;
        else if (healthPerc < 0.5f)
            healtBarImage.color = Color.yellow;
        else
            healtBarImage.color = new Color(0.462f, 0.816f, 0.373f);
    }

    private void FixedUpdate()
    {
        if (!killsPanel)
        {
            killsPanel = GameObject.Find("KillsPanel").GetComponent<TextMeshProUGUI>();
            spectateScript = GameObject.Find("UI").GetComponent<SpectateScript>();
        }

        if (healthCanvas.activeSelf)
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

        if (!isLocalPlayer) return;

        if (transform.position.y < fallDeathY && !died)
        {
            if (isServer)
            {
                died = true;
                RpcDie();
            }
            else
            {
                CmdFallDeath();
            }
            return;
        }

        playerController.Speed = speed;
        playerController.Deceleration = deceleration;
        playerController.MinSpeed = minSpeed;
        playerController.JumpForce = jumpForce;

        playerController.ResolveMovements();
    }

    [Command]
    void CmdFallDeath()
    {
        if (!died)
        {
            died = true;
            RpcDie();
        }
    }

    private bool debouncePanels = false;
    IEnumerator AutoAttackLoop()
    {
        while (!died)
        {
            yield return new WaitForSeconds(attackRate);
            Vector3 mobPosition = FindClosestMob();
            if (mobPosition != Vector3.zero)
            {
                if (!debouncePanels)
                {
                    debouncePanels = true;
                }
                CmdAttack(mobPosition);
            }
        }
    }

    [Command]
    public void CmdAttack(Vector3 mobPosition)
    {
        if (died) return;

        Vector3 startingPos = transform.position;
        if (startingPos.y > 1)
            startingPos.y = 1;

        playerController.OnAttack(attackDamage, projectilePrefab, mobPosition, startingPos, particlesPrefab);
    }

    public void Hurt(int damage)
    {
        if (!isServer) return;

        health -= damage;

        RpcShowDamage(damage);

        if (health <= 0)
        {
            died = true;
            RpcDie();
        }
    }

    [ClientRpc]
    void RpcShowDamage(int damage)
    {
        if (damageText == null) return;

        cumulativeDamage += damage;

        damageText.text = "-" + cumulativeDamage.ToString();
        damageText.gameObject.SetActive(true);

        damageText.transform.DOKill();
        damageText.transform.localScale = Vector3.zero;

        damageText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        damageCoroutine = StartCoroutine(HideDamageTextAfterDelay());
    }

    IEnumerator HideDamageTextAfterDelay()
    {
        yield return new WaitForSeconds(damageDisplayTime);
        damageText.gameObject.SetActive(false);
        cumulativeDamage = 0;
    }

    [ClientRpc]
    void RpcDie()
    {
        died = true;

        model.gameObject.SetActive(false);
        gameObject.tag = deadTag;
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        UI.SetActive(false);
        healthCanvas.SetActive(false);

        if (isLocalPlayer)
        {
            playerControls.actions.Disable();
            SetupSpectatorMode();
        }

        if (spectateScript.LastPlayer())
        {
            spectateScript.SpectatorPanel.SetActive(false);
            spectateScript.DeathPanel.SetActive(true);
            if (isServer)
            {
                spectateScript.BtnReturnToLobby.SetActive(true);
            }
            else
            {
                spectateScript.WaitForHostText.SetActive(true);
            }
        }
    }

    public void Heal(int amount)
    {
        if (!isServer) return;

        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    void Die()
    {
    }

    void SetupSpectatorMode()
    {
        spectateScript.SpectatorPanel.SetActive(true);
        spectateScript.StartSpectating();
        spectateScript.ded = true;
    }

    Vector3 FindClosestMob()
    {
        GameObject[] mobs = GameObject.FindGameObjectsWithTag("Mob");
        GameObject closestMob = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject mob in mobs)
        {
            float distance = Vector3.Distance(transform.position, mob.transform.position);
            if (distance < closestDistance && distance < rangeRadius)
            {
                closestDistance = distance;
                closestMob = mob;
            }
        }

        return closestMob?.transform.position ?? Vector3.zero;
    }

    void UpdateNameDisplay(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    private void OnEnable()
    {
        playerControls = gameObject.AddComponent<PlayerInput>();
        playerControls.actions = playerActions;
        playerControls.actions.Enable();

        playerControls.actions["Move"].performed += ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled += ctx => playerController.OnFinishMove();
        if (canJump)
            playerControls.actions["Jump"].performed += ctx => playerController.OnJump();
        playerControls.actions["Interact"].performed += ctx => playerController.OnInteract();
    }

    private void OnDisable()
    {
        playerControls.actions["Move"].performed -= ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled -= ctx => playerController.OnFinishMove();
        playerControls.actions["Jump"].performed -= ctx => playerController.OnJump();
        playerControls.actions["Interact"].performed -= ctx => playerController.OnInteract();
    }
}
