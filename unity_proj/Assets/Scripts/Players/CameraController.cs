using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerT;
    public float distanceFromPlayer;
    public int angleRadius;
    public Camera currentCamera;

    public void Update()
    {
        if (playerT != null)
        {
            currentCamera.transform.position = new Vector3(playerT.position.x, playerT.position.y + distanceFromPlayer * Mathf.Tan(angleRadius * Mathf.Deg2Rad), playerT.position.z - distanceFromPlayer);
            currentCamera.transform.rotation = Quaternion.Euler(angleRadius, 0, 0);
        }

    }
}
