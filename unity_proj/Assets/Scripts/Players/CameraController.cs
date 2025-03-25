using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float _distanceFromPlayer;
    private int _angleRadius;
    private Camera _camera;

    public CameraController(float distanceFromPlayer, int angleRadius, Camera camera)
    {
        _distanceFromPlayer = distanceFromPlayer;
        _angleRadius = angleRadius;
        _camera = camera;

        _camera.transform.localPosition = new Vector3(0, _distanceFromPlayer * Mathf.Tan(_angleRadius * Mathf.Deg2Rad), -_distanceFromPlayer);
        _camera.transform.rotation = Quaternion.Euler(_angleRadius, 0, 0);
    }

    public float DistanceFromPlayer { get => _distanceFromPlayer; set => _distanceFromPlayer = value; }
    public int AngleRadius { get => _angleRadius; set => _angleRadius = value; }
    public Camera Camera { get => _camera; set => _camera = value; }
}
