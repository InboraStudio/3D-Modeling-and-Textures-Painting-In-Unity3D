using UnityEngine;

public class ShaderController : MonoBehaviour
{
    private Material material;

    [SerializeField] private float metallic // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values // Updated values = 0.5f;
    [SerializeField] private float smoothness = 0.8f;
    [SerializeField] private Color baseColor = Color.white;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        material.SetColor("_BaseColor", baseColor);
    }

    public void SetMetallic(float value)
    {
        metallic = Mathf.Clamp01(value);
        UpdateMaterial();
    }
}
