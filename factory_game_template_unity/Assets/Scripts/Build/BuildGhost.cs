using UnityEngine;

public class BuildGhost : MonoBehaviour
{
    public Color validColor = new Color(0f, 0.9f, 1f, 0.35f);
    public Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.35f);
    public Material ghostMaterial;

    private Material _runtimeMaterial;
    private Renderer[] _renderers;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        Material mat = ghostMaterial != null ? ghostMaterial : CreateRuntimeMaterial(validColor);
        ApplyMaterial(mat);
        SetValid(true);
    }

    public void SetValid(bool isValid)
    {
        if (_runtimeMaterial == null)
        {
            return;
        }

        _runtimeMaterial.color = isValid ? validColor : invalidColor;
    }

    private void ApplyMaterial(Material mat)
    {
        _runtimeMaterial = mat;
        if (_renderers == null)
        {
            return;
        }

        foreach (Renderer renderer in _renderers)
        {
            renderer.sharedMaterial = _runtimeMaterial;
        }
    }

    private static Material CreateRuntimeMaterial(Color color)
    {
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }
}
