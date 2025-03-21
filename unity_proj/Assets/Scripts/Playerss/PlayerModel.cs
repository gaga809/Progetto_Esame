using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerModel : MonoBehaviour
{
    [Header("Player Settings")]
    public float speed = 5f;
    public float deceleration = 5f;
    public float minSpeed = 0.1f;
    public float jumpForce = 5f;
    public PlayerInput playerControls;

    [Header("Camera Settings")]
    public Camera currentCamera;
    public float distanceFromPlayer = 10f;
    public int angleRadius = 35;

    private PlayerController playerController;
    private CameraController cameraController;

    private void Start()
    {
        playerController = new PlayerController(speed, deceleration, minSpeed, jumpForce, gameObject.transform, gameObject.GetComponent<Rigidbody>());
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

    private void OnEnable()
    {
        playerControls.actions.Enable();

        playerControls.actions["Move"].performed += ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled += ctx => playerController.OnFinishMove();

        playerControls.actions["Jump"].performed += ctx => playerController.OnJump();
        playerControls.actions["Attack"].performed += ctx => playerController.OnAttack();
        playerControls.actions["Interact"].performed += ctx => playerController.OnInteract();
    }

    private void OnDisable()
    {
        playerControls.actions["Move"].performed -= ctx => playerController.OnMove(ctx.ReadValue<Vector2>());
        playerControls.actions["Move"].canceled -= ctx => playerController.OnFinishMove();
        playerControls.actions["Jump"].performed -= ctx => playerController.OnJump();
        playerControls.actions["Attack"].performed -= ctx => playerController.OnAttack();
        playerControls.actions["Interact"].performed -= ctx => playerController.OnInteract();
    }
}
