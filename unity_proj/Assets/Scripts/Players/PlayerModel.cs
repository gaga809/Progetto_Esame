using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Mirror;
using TMPro;

public class PlayerModel : NetworkBehaviour
{
    [Header("User Info")]
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Player Statuses")]
    [SyncVar(hook = nameof(OnPlayerDied))]
    public bool died = false;

    [Header("Player Settings")]
    [SyncVar]
    public int health = 10;
    [SyncVar]
    public int maxHealth = 10;
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

    private PlayerController playerController;
    private bool canAttack = true;

    void OnNameChanged(string _, string newName)
    {
        UpdateNameDisplay(newName);
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    public int GetPlayerIndex()
    {
        if (NetworkManager.singleton is NetworkRoomManager roomManager)
        {
            return roomManager.roomSlots.Count;
        }
        return -1;
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            Camera.main.GetComponent<CameraController>().playerT = transform;
            string localPlayerName = "Player " + GetPlayerIndex().ToString();
            CmdSetPlayerName(localPlayerName);
            
        }

        playerController = new PlayerController(speed, deceleration, minSpeed, jumpForce, gameObject.transform, gameObject.GetComponent<Rigidbody>(), model);
        
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) { return; }

        playerController.Speed = speed;
        playerController.Deceleration = deceleration;
        playerController.MinSpeed = minSpeed;
        playerController.JumpForce = jumpForce;

        playerController.ResolveMovements();

    }

    private void Update()
    {
        if (!isLocalPlayer) { return; }

        if (canAttack && !died)
        {
            CmdAttack();
        }
    }

    [Command]
    public void CmdAttack()
    {
        Vector3 pos = FindClosestMob();

        if (pos != Vector3.zero)
        {
            StartCoroutine(AttackCooldown());
            Vector3 startingPos = transform.position;
            if (startingPos.y > 1)
                startingPos.y = 1;

            playerController.OnAttack(attackDamage, projectilePrefab, pos, startingPos, particlesPrefab);
        }
    }

    private void OnEnable()
    {
        playerControls = gameObject.AddComponent<PlayerInput>();
        playerControls.actions = playerActions;
        playerControls.actions.Enable();

        playerControls.actions["Move"].performed += ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled += ctx => playerController.OnFinishMove();

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

    public void Hurt(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }

        Debug.Log("Il Giocatore ha subito " + damage + " danni. Saldo: " + health);
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        Debug.Log("Il Giocatore ha recuperato " + amount + " punti vita. Saldo: " + health);
    }

    public void Die()
    {
        Debug.Log("Il Giocatore è schiattato.");
        playerControls.actions.Disable();
        died = true;

        // Spectate someone else
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackRate);
        canAttack = true;
    }

    public Vector3 FindClosestMob()
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

        if (closestMob == null)
        {
            return Vector3.zero;
        }
        return closestMob.transform.position;
    }

    public void UpdateNameDisplay(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    void OnPlayerDied(bool oldValue, bool newValue)
    {
        Die();
        model.gameObject.SetActive(!newValue);
        gameObject.tag = newValue ? deadTag : playerTag;
        nameText.enabled = !newValue;
    }
}
