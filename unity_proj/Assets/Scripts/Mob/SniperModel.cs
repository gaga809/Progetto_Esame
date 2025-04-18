using UnityEngine;
using Mirror;
using System.Collections;

public class SniperModel : MobModel
{
    public GameObject projectilePrefab;
    public float shootRange = 15f;
    public float shootCooldown = 3f;
    public float projectileSpeed = 10f;

    private bool canShoot = true;
    private Transform closestPlayer;

    protected override void Update()
    {
        if (!isServer) return;

        FindClosestPlayer();

        if (closestPlayer == null) return;

        float distance = Vector3.Distance(transform.position, closestPlayer.position);

        // Rotate toward the player
        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }

        if (distance <= shootRange && canShoot)
        {
            StartCoroutine(Shoot());
        }

        CheckIfGrounded();
    }

    private void FindClosestPlayer()
    {
        PlayerModel[] players = FindObjectsOfType<PlayerModel>();

        float closestDistance = Mathf.Infinity;
        closestPlayer = null;

        foreach (var player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player.transform;
            }
        }
    }

    private IEnumerator Shoot()
    {
        canShoot = false;

        if (closestPlayer == null)
        {
            Debug.LogWarning("Sniper tried to shoot but no player found.");
            yield break;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (closestPlayer.position - spawnPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, rotation);
        Debug.Log("Projectile instantiated at " + spawnPos);

        if (NetworkServer.active)
        {
            ProjectileSniperModel projectile = proj.GetComponent<ProjectileSniperModel>();
            if (projectile != null)
            {
                projectile.damage = 1;
                projectile.speed = projectileSpeed;
            }

            NetworkServer.Spawn(proj);
            Debug.Log("Projectile spawned on server.");
        }

        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
}
