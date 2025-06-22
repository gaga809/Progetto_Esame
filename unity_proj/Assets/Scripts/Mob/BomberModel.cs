using Mirror;
using System.Collections;
using UnityEngine;

public class BomberMobModel : MobModel
{
    public float explosionRadius = 3f;
    public float explosionDamage = 10f;
    public GameObject explosionEffect;

    private bool hasExploded = false;

    protected override void Update()
    {
        base.Update();

        if (hasExploded || player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= stoppingDistance)
        {
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        hasExploded = true;

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerModel playerModel = hit.GetComponent<PlayerModel>();
                if (playerModel != null)
                {
                    playerModel.Hurt((int)explosionDamage); 
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
