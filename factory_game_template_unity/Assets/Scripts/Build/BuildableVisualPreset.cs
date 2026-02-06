using UnityEngine;

public class BuildableVisualPreset : MonoBehaviour
{
    public BuildableVisualType preset = BuildableVisualType.Storage;
    public bool generateOnAwake = true;

    private const string VisualRootName = "VisualRoot";
    private const string BoxMeshName = "Meshes/Box";
    private const string CylinderMeshName = "Meshes/Cylinder";
    private static Mesh _boxMesh;
    private static Mesh _cylinderMesh;

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

        CreatePart(root, "Body", false, size, new Vector3(0f, size.y * 0.5f, 0f), color);

        switch (preset)
        {
            case BuildableVisualType.Miner:
                CreatePart(root, "Drill", true, new Vector3(0.25f, 0.7f, 0.25f), new Vector3(0f, 0.2f, 0f), new Color(0.2f, 0.2f, 0.2f, 1f));
                break;
            case BuildableVisualType.Smelter:
                CreatePart(root, "Chimney", true, new Vector3(0.5f, 1.2f, 0.5f), new Vector3(0.3f, 1.2f, 0.3f), new Color(0.25f, 0.25f, 0.25f, 1f));
                break;
            case BuildableVisualType.Depot:
                CreatePart(root, "Crate", false, new Vector3(0.8f, 0.6f, 0.8f), new Vector3(-0.35f, 0.4f, 0.2f), new Color(0.5f, 0.35f, 0.2f, 1f));
                CreatePart(root, "Beacon", true, new Vector3(0.2f, 0.8f, 0.2f), new Vector3(0.5f, 0.6f, -0.4f), new Color(0.9f, 0.8f, 0.2f, 1f));
                break;
            case BuildableVisualType.Generator:
                CreatePart(root, "Exhaust", true, new Vector3(0.25f, 0.8f, 0.25f), new Vector3(-0.4f, 0.7f, 0f), new Color(0.15f, 0.15f, 0.15f, 1f));
                break;
            case BuildableVisualType.PowerPole:
                CreatePart(root, "Pole", true, new Vector3(0.2f, 2.4f, 0.2f), new Vector3(0f, 1.2f, 0f), new Color(0.3f, 0.3f, 0.35f, 1f));
                CreatePart(root, "Cross", false, new Vector3(1f, 0.12f, 0.2f), new Vector3(0f, 2.2f, 0f), new Color(0.4f, 0.4f, 0.45f, 1f));
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
            case BuildableVisualType.PowerPole:
                return new Vector3(0.6f, 2.6f, 0.6f);
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
            case BuildableVisualType.PowerPole:
                return new Color(0.35f, 0.35f, 0.4f, 1f);
            default:
                return Color.white;
        }
    }

    private void CreatePart(Transform parent, string name, bool useCylinder, Vector3 scale, Vector3 localPos, Color color)
    {
        Mesh mesh = useCylinder ? GetCylinderMesh() : GetBoxMesh();
        if (mesh == null)
        {
            Debug.LogWarning("Missing mesh asset for " + name);
            return;
        }

        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localScale = scale;
        part.transform.localPosition = localPos;

        MeshFilter filter = part.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = part.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;
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

    private static Mesh GetBoxMesh()
    {
        if (_boxMesh == null)
        {
            _boxMesh = Resources.Load<Mesh>(BoxMeshName);
        }
        return _boxMesh;
    }

    private static Mesh GetCylinderMesh()
    {
        if (_cylinderMesh == null)
        {
            _cylinderMesh = Resources.Load<Mesh>(CylinderMeshName);
        }
        return _cylinderMesh;
    }
}

public enum BuildableVisualType
{
    Conveyor,
    Miner,
    Smelter,
    Storage,
    Depot,
    Generator,
    PowerPole
}
