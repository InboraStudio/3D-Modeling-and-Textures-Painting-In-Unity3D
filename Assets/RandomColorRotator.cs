using UnityEngine;

public class RGBColorRotator : MonoBehaviour
{
    public float rotationSpeed = 50f;   // Cube rotation speed
    public float colorSpeed = 1f;       // RGB color change speed

    private Renderer cubeRenderer;
    private float t;                    // Timer for color cycling

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        // Smooth rotation
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // Cycle through RGB colors
        t += Time.deltaTime * colorSpeed;

        // Convert time into a color that cycles smoothly (RGB rainbow)
        Color rgb = Color.HSVToRGB((Mathf.Sin(t) * 0.5f + 0.5f), 1f, 1f);

        // Apply color
        cubeRenderer.material.color = rgb;
    }
}
