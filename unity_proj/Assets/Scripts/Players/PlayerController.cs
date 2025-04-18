using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour
{

    private float _speed, _deceleration, _minSpeed, _jumpForce;
    private Transform _trs;
    private Rigidbody _rb;
    private Transform _trsModel;

    public PlayerController(float speed, float decel, float minS, float jumpF, Transform trs, Rigidbody rb, Transform trsModel)
    {
        _speed = speed;
        _deceleration = decel;
        _minSpeed = minS;
        _jumpForce = jumpF;
        _trs = trs;
        _rb = rb;
        _trsModel = trsModel;
    }

    private Vector3 playerMovementDirection = Vector3.zero;
    private Vector3 currentVelocity = Vector3.zero;

    public float Speed { get => _speed; set => _speed = value; }
    public float Deceleration { get => _deceleration; set => _deceleration = value; }
    public float MinSpeed { get => _minSpeed; set => _minSpeed = value; }
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }

    public void ResolveMovements()
    {
        Vector3 targetVelocity = playerMovementDirection * _speed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * _deceleration);

        if (currentVelocity.magnitude < _minSpeed && playerMovementDirection == Vector3.zero)
        {
            currentVelocity = Vector3.zero;
        }

        _trs.Translate(currentVelocity * Time.deltaTime, Space.World);
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);

        if (playerMovementDirection != Vector3.zero)
        {

            _trsModel.rotation = Quaternion.Slerp(
                _trsModel.rotation,
                Quaternion.LookRotation(playerMovementDirection),
                Time.deltaTime * 10f
            );
        }
    }

    public void OnMove(Vector2 direction)
    {
        playerMovementDirection = new Vector3(direction.x, 0, direction.y);
    }

    public void OnFinishMove()
    {
        playerMovementDirection = Vector3.zero;
    }

    public void OnJump()
    {
        if (IsGrounded())
        {
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    public void OnAttack(int damage, GameObject prefab, Vector3 closestMobPos, Vector3 startingPoint, GameObject particlesPrefab)
    {
        if (prefab == null || particlesPrefab == null)
        {
            Debug.LogError("Attack prefabs are null!");
            return;
        }

        Vector3 dir = closestMobPos - startingPoint;
        dir.y = 1;
        Quaternion quaternion = Quaternion.LookRotation(dir);
        quaternion.x = 0;
        quaternion.z = 0;



        //GameObject effect = Instantiate(particlesPrefab, startingPoint, quaternion);
        //Destroy(effect, 2f);
        GameObject proj = Instantiate(prefab, startingPoint, quaternion);

        if (NetworkServer.active)
        {
            NetworkServer.Spawn(proj);
            //NetworkServer.Spawn(effect);
            //NetworkServer.UnSpawn(effect);
        }

        proj.GetComponent<ProjectileModel>().damage = damage;

    }

    public void OnInteract()
    {
        Debug.Log("Interact");
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(_trs.transform.position, Vector3.down, 1.1f);
    }

}