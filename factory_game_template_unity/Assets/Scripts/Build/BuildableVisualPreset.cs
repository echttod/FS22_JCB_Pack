using UnityEngine;

public class BuildableVisualPreset : MonoBehaviour
{
    public BuildableVisualType preset = BuildableVisualType.Storage;
    public bool generateOnAwake = true;

    private const string VisualRootName = "VisualRoot";

    private void Awake()
    {
        if (generateOnAwake)
        {
            Generate();
        }
    }

    public void Generate()
    {
        if (transform.Find(VisualRootName) != null)
        {
            return;
        }

        Buildable buildable = GetComponent<Buildable>();
        Vector3 size = GetSizeForPreset();
        Color color = GetColorForPreset();

        if (buildable != null)
        {
            buildable.footprint = size;
            if (preset == BuildableVisualType.Miner)
            {
                buildable.allowResourceOverlap = true;
            }
        }

        Transform root = new GameObject(VisualRootName).transform;
        root.SetParent(transform, false);

        CreatePart(root, "Body", PrimitiveType.Cube, size, new Vector3(0f, size.y * 0.5f, 0f), color);

        switch (preset)
        {
            case BuildableVisualType.Miner:
                CreatePart(root, "Drill", PrimitiveType.Cylinder, new Vector3(0.25f, 0.7f, 0.25f), new Vector3(0f, 0.2f, 0f), new Color(0.2f, 0.2f, 0.2f, 1f));
                break;
            case BuildableVisualType.Smelter:
                CreatePart(root, "Chimney", PrimitiveType.Cylinder, new Vector3(0.5f, 1.2f, 0.5f), new Vector3(0.3f, 1.2f, 0.3f), new Color(0.25f, 0.25f, 0.25f, 1f));
                break;
            case BuildableVisualType.Depot:
                CreatePart(root, "Crate", PrimitiveType.Cube, new Vector3(0.8f, 0.6f, 0.8f), new Vector3(-0.35f, 0.4f, 0.2f), new Color(0.5f, 0.35f, 0.2f, 1f));
                CreatePart(root, "Beacon", PrimitiveType.Cylinder, new Vector3(0.2f, 0.8f, 0.2f), new Vector3(0.5f, 0.6f, -0.4f), new Color(0.9f, 0.8f, 0.2f, 1f));
                break;
            case BuildableVisualType.Generator:
                CreatePart(root, "Exhaust", PrimitiveType.Cylinder, new Vector3(0.25f, 0.8f, 0.25f), new Vector3(-0.4f, 0.7f, 0f), new Color(0.15f, 0.15f, 0.15f, 1f));
                break;
        }

        EnsureCollider(size);
    }

    private Vector3 GetSizeForPreset()
    {
        switch (preset)
        {
            case BuildableVisualType.Conveyor:
                return new Vector3(0.6f, 0.2f, 2f);
            case BuildableVisualType.Miner:
                return new Vector3(1.2f, 1f, 1.2f);
            case BuildableVisualType.Smelter:
                return new Vector3(1.6f, 1.4f, 1.6f);
            case BuildableVisualType.Storage:
                return new Vector3(2f, 2f, 2f);
            case BuildableVisualType.Depot:
                return new Vector3(1.8f, 1.2f, 1.8f);
            case BuildableVisualType.Generator:
                return new Vector3(1.8f, 1.2f, 1.8f);
            default:
                return Vector3.one;
        }
    }

    private Color GetColorForPreset()
    {
        switch (preset)
        {
            case BuildableVisualType.Conveyor:
                return new Color(0.1f, 0.6f, 0.9f, 1f);
            case BuildableVisualType.Miner:
                return new Color(0.2f, 0.7f, 0.3f, 1f);
            case BuildableVisualType.Smelter:
                return new Color(0.8f, 0.4f, 0.2f, 1f);
            case BuildableVisualType.Storage:
                return new Color(0.6f, 0.6f, 0.6f, 1f);
            case BuildableVisualType.Depot:
                return new Color(0.25f, 0.3f, 0.55f, 1f);
            case BuildableVisualType.Generator:
                return new Color(0.25f, 0.25f, 0.25f, 1f);
            default:
                return Color.white;
        }
    }

    private void CreatePart(Transform parent, string name, PrimitiveType type, Vector3 scale, Vector3 localPos, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localScale = scale;
        part.transform.localPosition = localPos;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Collider col = part.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }

    private void EnsureCollider(Vector3 size)
    {
        Collider existing = GetComponent<Collider>();
        if (existing != null)
        {
            return;
        }

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = new Vector3(0f, size.y * 0.5f, 0f);
    }
}

public enum BuildableVisualType
{
    Conveyor,
    Miner,
    Smelter,
    Storage,
    Depot,
    Generator
}
