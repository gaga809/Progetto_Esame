using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerModel : MonoBehaviour
{
    [Header("Player Settings")]
    public int health = 10;
    public int maxHealth = 10;
    public float speed = 5f;
    public float deceleration = 5f;
    public float minSpeed = 0.1f;
    public float jumpForce = 5f;
    public int attackDamage = 5;
    public float attackRate = 1f;
    public float rangeRadius = 10f;
    public PlayerInput playerControls;
    public Transform model;
    public GameObject projectilePrefab;
    public GameObject particlesPrefab;

    [Header("Camera Settings")]
    public Camera currentCamera;
    public float distanceFromPlayer = 10f;
    public int angleRadius = 35;

    private PlayerController playerController;
    private CameraController cameraController;
    private bool canAttack = true;

    private void Start()
    {
        playerController = new PlayerController(speed, deceleration, minSpeed, jumpForce, gameObject.transform, gameObject.GetComponent<Rigidbody>(), model);
        cameraController = new CameraController(distanceFromPlayer, angleRadius, currentCamera);
    }

    private void FixedUpdate()
    {
        playerController.Speed = speed;
        playerController.Deceleration = deceleration;
        playerController.MinSpeed = minSpeed;
        playerController.JumpForce = jumpForce;

        playerController.ResolveMovements();

    }

    private void Update()
    {
        if (canAttack)
        {
            Vector3 pos = FindClosestMob();

            if (pos != Vector3.zero)
            {
                StartCoroutine(AttackCooldown());
                Vector3 startingPos = transform.position;
                if(startingPos.y > 1)
                    startingPos.y = 1;
                playerController.OnAttack(attackDamage, projectilePrefab, pos, startingPos, particlesPrefab);
            }
        }
    }

    private void OnEnable()
    {
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
        Debug.Log("Il Giocatore è schiattato. Porcacci");
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
}
