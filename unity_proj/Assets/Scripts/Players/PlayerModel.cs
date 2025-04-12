using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Mirror;
using TMPro;

public class PlayerModel : NetworkBehaviour
{
    [Header("User Info")]
    public string playerNamePref = "playerName";
    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;
    [SerializeField] private TextMeshProUGUI nameText;
    public GameObject UI;
    public GameObject deathPanel;

    [Header("Player Statuses")]
    [SyncVar(hook = nameof(OnDeathStatusChanged))]
    public bool died = false;

    [Header("Player Settings")]
    [SyncVar]
    public int health = 10;
    [SyncVar]
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

    private PlayerController playerController;
    private bool canAttack = true;

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

    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            StartCoroutine(AutoAttackLoop());
            Camera.main.GetComponent<CameraController>().playerT = transform;
        }

        playerController = new PlayerController(speed, deceleration, minSpeed, jumpForce,
                            gameObject.transform, gameObject.GetComponent<Rigidbody>(), model);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        playerController.Speed = speed;
        playerController.Deceleration = deceleration;
        playerController.MinSpeed = minSpeed;
        playerController.JumpForce = jumpForce;

        playerController.ResolveMovements();
    }

    IEnumerator AutoAttackLoop()
    {
        while (!died)
        {
            yield return new WaitForSeconds(attackRate);
            Vector3 mobPosition = FindClosestMob();
            if (mobPosition != Vector3.zero)
            {
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
        Debug.Log("\"" + playerName + "\" took " + damage + " damage. Current health: " + health);

        if (health <= 0)
        {
            died = true;
            RpcDie();
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        died = true;
        Debug.Log("\"" + playerName + "\" has died.");

        model.gameObject.SetActive(false);
        gameObject.tag = deadTag;
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        UI.SetActive(false);

        if (isLocalPlayer)
        {
            deathPanel = GameObject.Find("DeathPanel");
            if(deathPanel != null)
                deathPanel.SetActive(true);
            playerControls.actions.Disable();
            SetupSpectatorMode();
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
        Debug.Log("\"" + playerName + "\" recovered " + amount + " health points. Current health: " + health);
    }

    void Die()
    {
        // Funzione vuota mantenuta per compatibilità
    }

    //void CheckIfAllPlayersDead()
    //{
    //    if (!isServer) return;
        
    //    GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
    //    if (players.Length == 0)
    //    {
            
    //    }
    //}

    //IEnumerator ChangeSceneWithDelay()
    //{
    //    yield return new WaitForSeconds(1f);
    //    NetworkManager.singleton.ServerChangeScene("GameRoom");
    //}


    void SetupSpectatorMode()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        if (players.Length > 0)
        {
            GameObject randomPlayer = players[Random.Range(0, players.Length)];
            Camera.main.GetComponent<CameraController>().playerT = randomPlayer.transform;
        }
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
        if(canJump)
            playerControls.actions["Jump"].performed += ctx => playerController.OnJump();
        playerControls.actions["Interact"].performed += ctx => playerController.OnInteract();
    }

    private void OnDisable()
    {
        playerControls.actions["Move"].performed -= ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled -= ctx => playerController.OnFinishMove();
        if(canJump)
            playerControls.actions["Jump"].performed -= ctx => playerController.OnJump();
        playerControls.actions["Interact"].performed -= ctx => playerController.OnInteract();
    }
}