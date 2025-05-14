using UnityEngine;
using System.Collections;
using Mirror;

public class ProjectileModel : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public float speed = 1f;
    public float lifeTime = 5f;
    public PlayerModel playerModel;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(DestroyProjectile());
        }
        //StartCoroutine(DestroyProjectile());
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        MobModel mob = other.gameObject.GetComponent<MobModel>();

        if (mob != null)
        {
            mob.Hurt(damage, playerModel);
            NetworkServer.Destroy(gameObject);
        }
    }

}