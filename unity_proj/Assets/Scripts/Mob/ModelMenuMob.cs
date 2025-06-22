using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MobModelMenu : MonoBehaviour
{
    public Transform colorTransform;
    public Transform modelTransform;
    public Transform target;
    public float rotationSpeed = 5f;
    public float visualJumpHeight = 1.5f;
    public float visualJumpDuration = 0.5f;
    public float jumpCooldown = 1f;
    public float bounceAmplitude = 0.25f;
    public float bounceFrequency = 2f;

    private Renderer modelRenderer;
    private float initialY;

    private void Start()
    {
        ApplyRandomColor();

        if (colorTransform != null)
        {
            Transform sphere = colorTransform.Find("Sphere");
            if (sphere != null)
                modelRenderer = sphere.GetComponent<Renderer>();
        }

        if (modelTransform != null)
            initialY = modelTransform.localPosition.y;

        StartCoroutine(JumpLoop());
    }

    private void Update()
    {
        if (modelTransform != null)
        {
            Vector3 pos = modelTransform.localPosition;
            pos.y = initialY + Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
            modelTransform.localPosition = pos;
        }

        if (target != null)
        {
            Vector3 dir = target.position - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void ApplyRandomColor()
    {
        string[] materialNames = { "Grass", "Stone", "Green", "LightBlue", "Orange", "Blue", "Yellow", "Purple" };
        string selectedMatName = materialNames[Random.Range(0, materialNames.Length)];
        Material randomMat = Resources.Load<Material>("Materials/" + selectedMatName);

        if (randomMat == null) return;

        if (colorTransform != null)
        {
            Transform sphere = colorTransform.Find("Sphere");
            if (sphere != null)
            {
                var rend = sphere.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = randomMat;
                }
            }
        }
    }

    private IEnumerator JumpLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(jumpCooldown);

        while (true)
        {
            if (modelTransform != null)
            {
                float halfDur = visualJumpDuration / 2f;

                Sequence seq = DOTween.Sequence();

                seq.Append(modelTransform.DOLocalMoveY(initialY + visualJumpHeight, halfDur).SetEase(Ease.OutQuad));
                seq.Join(modelTransform.DOScaleY(0.7f, halfDur));
                seq.Join(modelTransform.DOScaleX(1.2f, halfDur));
                seq.Join(modelTransform.DOScaleZ(1.2f, halfDur));

                seq.Append(modelTransform.DOLocalMoveY(initialY, halfDur).SetEase(Ease.InQuad));
                seq.Join(modelTransform.DOScaleY(1f, halfDur));
                seq.Join(modelTransform.DOScaleX(1f, halfDur));
                seq.Join(modelTransform.DOScaleZ(1f, halfDur));

                yield return seq.WaitForCompletion();
            }
            yield return wait;
        }
    }
}
