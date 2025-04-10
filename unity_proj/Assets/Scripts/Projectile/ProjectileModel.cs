using UnityEngine;
using System.Collections;
using Mirror;

public class ProjectileModel : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public float speed = 1f;
    public float lifeTime = 5f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void Start()
    {
        StartCoroutine(DestroyProjectile());
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        SyncDestroy();
    }

    [Command]
    private void SyncDestroy()
    {
        Destroy(gameObject);
        if(isServer)
            NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        MobModel mob = other.gameObject.GetComponent<MobModel>();

        if (mob != null)
        {
            mob.Hurt(damage);
            SyncDestroy();
        }
    }

}
