using UnityEngine;
using System.Collections;

public class ProjectileModel : MonoBehaviour
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
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        MobModel mob = other.gameObject.GetComponent<MobModel>();

        if (mob != null)
        {
            mob.Hurt(damage);
            Debug.Log("Hittato Mob: " + other.gameObject.name);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("L'oggetto colpito NON è un Mob: " + other.gameObject.name);
        }
    }

}
