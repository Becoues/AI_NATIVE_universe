using UnityEngine;

[DisallowMultipleComponent]
public class ChaseCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.5f, -10f);
    [SerializeField] private float followSmoothing = 6f;
    [SerializeField] private float lookSmoothing = 10f;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            var found = FindObjectOfType<PlayerShipController>();
            if (found != null)
            {
                target = found.transform;
            }
        }

        if (target == null)
            return;

        Vector3 desiredPosition = target.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / followSmoothing);

        Quaternion desiredRotation = Quaternion.LookRotation(target.forward, target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lookSmoothing * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
