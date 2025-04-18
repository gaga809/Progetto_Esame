using UnityEngine;
using System.Collections;
using Mirror;

public class ProjectileSniperModel : NetworkBehaviour
{
    [Header("Projectile Sniper Settings")]
    public int damage = 1;
    public float speed = 1f;
    public float lifeTime = 5f;

    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(DestroyProjectile());
        }
        StartCoroutine(DestroyProjectile());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private IEnumerator DestroyProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        PlayerModel player = other.gameObject.GetComponent<PlayerModel>();

        if (player != null)
        {
            player.Hurt(damage);
            NetworkServer.Destroy(gameObject);
        }
    }
}
