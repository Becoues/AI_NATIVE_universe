using UnityEngine;

[DisallowMultipleComponent]
public class OrbitMotion : MonoBehaviour
{
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Vector3 orbitAxis = Vector3.up;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private Vector3 selfRotationAxis = Vector3.up;
    [SerializeField] private float selfRotationSpeed = 20f;

    private Vector3 initialOffset;
    private bool hasCenter;

    private void Awake()
    {
        if (orbitCenter == null && transform.parent != null)
            orbitCenter = transform.parent;

        hasCenter = orbitCenter != null;
        initialOffset = hasCenter
            ? transform.position - orbitCenter.position
            : transform.position; // orbit world origin when no explicit center
    }

    private void LateUpdate()
    {
        Vector3 centerPosition = hasCenter ? orbitCenter.position : Vector3.zero;

        if (initialOffset != Vector3.zero)
        {
            Quaternion orbitStep = Quaternion.AngleAxis(orbitSpeed * Time.deltaTime, orbitAxis.normalized);
            initialOffset = orbitStep * initialOffset;
            transform.position = centerPosition + initialOffset;
        }

        if (selfRotationSpeed != 0f)
        {
            transform.Rotate(selfRotationAxis.normalized, selfRotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void SetOrbitCenter(Transform center)
    {
        orbitCenter = center;
        hasCenter = center != null;
        initialOffset = hasCenter
            ? transform.position - orbitCenter.position
            : transform.position;
    }
}
