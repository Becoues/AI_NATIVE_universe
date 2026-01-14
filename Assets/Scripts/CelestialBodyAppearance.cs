using UnityEngine;

[RequireComponent(typeof(Renderer))]
[DisallowMultipleComponent]
public class CelestialBodyAppearance : MonoBehaviour
{
    [SerializeField] private Color albedo = Color.white;
    [SerializeField] private Color emission = Color.black;
    [SerializeField, Range(0f, 1f)] private float metallic = 0f;
    [SerializeField, Range(0f, 1f)] private float smoothness = 0.6f;
    [SerializeField] private float emissionIntensity = 1f;

    private Material runtimeMaterial;

    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        runtimeMaterial = new Material(shader);

        runtimeMaterial.SetColor("_BaseColor", albedo);
        runtimeMaterial.SetFloat("_Metallic", metallic);
        runtimeMaterial.SetFloat("_Smoothness", smoothness);

        Color emissionColor = emission * Mathf.LinearToGammaSpace(emissionIntensity);
        runtimeMaterial.SetColor("_EmissionColor", emissionColor);
        runtimeMaterial.EnableKeyword("_EMISSION");

        renderer.material = runtimeMaterial;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeMaterial);
        }
        else
        {
            DestroyImmediate(runtimeMaterial);
        }
    }
}
