using UnityEngine;
using Mirror;
using System.Collections;

public class SniperModel : MobModel
{
    public GameObject projectilePrefab;
    public float shootRange = 20f;
    public float shootCooldown = 3f;
    public float projectileSpeed = 10f;
    public float minDistanceFromPlayer = 8f;
    public float retreatSpeed = 3f;

    private bool canShoot = true;
    private Transform closestPlayer;

    protected override void Update()
    {
        if (!isServer) return;

        FindClosestPlayer();

        if (closestPlayer == null) return;

        float distance = Vector3.Distance(transform.position, closestPlayer.position);

        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 5f);
        }

        if (distance < minDistanceFromPlayer)
        {
            Vector3 retreatDir = (transform.position - closestPlayer.position).normalized;
            retreatDir.y = 0f;
            transform.position += retreatDir * retreatSpeed * Time.deltaTime;
        }
        else if (distance >= minDistanceFromPlayer && distance <= shootRange && canShoot)
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
            yield break;

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        Vector3 dir = (closestPlayer.position - spawnPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, rotation);

        if (NetworkServer.active)
        {
            ProjectileSniperModel projectile = proj.GetComponent<ProjectileSniperModel>();
            if (projectile != null)
            {
                projectile.damage = 1;
                projectile.speed = projectileSpeed;
            }

            NetworkServer.Spawn(proj);
        }

        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }
}
