using UnityEngine;

public class TemplateBootstrap : MonoBehaviour
{
    [Header("Scene")]
    public bool createGround = true;
    public Vector3 groundSize = new Vector3(40f, 1f, 40f);
    public bool createLight = true;

    [Header("Player")]
    public Vector3 playerStart = new Vector3(0f, 2f, -6f);

    private void Start()
    {
        SetupEnvironment();
        SetupPlayerAndSystems();
        SpawnResourceNodes();
    }

    private void SetupEnvironment()
    {
        if (createGround && GameObject.Find("TemplateGround") == null)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "TemplateGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(groundSize.x / 10f, 1f, groundSize.z / 10f);
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.25f, 0.28f, 0.3f, 1f);
            }
        }

        if (createLight && FindObjectOfType<Light>() == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    private void SetupPlayerAndSystems()
    {
        if (FindObjectOfType<FpsController>() != null)
        {
            return;
        }

        GameObject player = new GameObject("Player");
        player.transform.position = playerStart;

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        GameObject pivot = new GameObject("CameraPivot");
        pivot.transform.SetParent(player.transform, false);
        pivot.transform.localPosition = new Vector3(0f, 0.7f, 0f);

        GameObject cameraObj = new GameObject("PlayerCamera");
        cameraObj.transform.SetParent(pivot.transform, false);
        cameraObj.transform.localPosition = Vector3.zero;
        Camera camera = cameraObj.AddComponent<Camera>();
        cameraObj.AddComponent<AudioListener>();

        GameObject handsRoot = new GameObject("HandsRoot");
        handsRoot.transform.SetParent(cameraObj.transform, false);
        HandsViewmodel hands = handsRoot.AddComponent<HandsViewmodel>();
        hands.controller = controller;

        FpsController fps = player.AddComponent<FpsController>();
        fps.cameraPivot = pivot.transform;

        PlayerInteractor interactor = player.AddComponent<PlayerInteractor>();
        interactor.playerCamera = camera;

        GameObject systems = new GameObject("GameSystems");
        BuildCatalog catalog = systems.AddComponent<BuildCatalog>();
        BuildSystem buildSystem = systems.AddComponent<BuildSystem>();
        buildSystem.catalog = catalog;
        buildSystem.playerCamera = camera;

        HudOverlay hud = cameraObj.AddComponent<HudOverlay>();
        hud.buildSystem = buildSystem;

        CreateDefaultBuildables(catalog);
    }

    private void CreateDefaultBuildables(BuildCatalog catalog)
    {
        GameObject library = new GameObject("BuildableLibrary");
        library.transform.SetParent(transform, false);

        GameObject conveyor = CreateBuildablePrefab("Conveyor", new Vector3(2f, 0.2f, 0.6f), new Color(0.1f, 0.6f, 0.9f, 1f));
        GameObject smelter = CreateBuildablePrefab("Smelter", new Vector3(1.5f, 1.5f, 1.5f), new Color(0.8f, 0.4f, 0.2f, 1f));
        GameObject storage = CreateBuildablePrefab("Storage", new Vector3(2f, 2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));

        AddChimney(smelter, new Vector3(0.5f, 1.2f, 0.5f));

        RegisterPrefab(catalog, library.transform, conveyor);
        RegisterPrefab(catalog, library.transform, smelter);
        RegisterPrefab(catalog, library.transform, storage);
    }

    private static void RegisterPrefab(BuildCatalog catalog, Transform parent, GameObject prefab)
    {
        prefab.transform.SetParent(parent, false);
        prefab.SetActive(false);
        catalog.Register(prefab.name, prefab);
    }

    private static GameObject CreateBuildablePrefab(string name, Vector3 size, Color color)
    {
        GameObject root = new GameObject(name);
        Buildable buildable = root.AddComponent<Buildable>();
        buildable.footprint = size;

        GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.name = "Mesh";
        mesh.transform.SetParent(root.transform, false);
        mesh.transform.localScale = size;
        mesh.transform.localPosition = new Vector3(0f, size.y / 2f, 0f);

        Renderer renderer = mesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        Collider meshCol = mesh.GetComponent<Collider>();
        if (meshCol != null)
        {
            Destroy(meshCol);
        }

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = size;
        col.center = new Vector3(0f, size.y / 2f, 0f);

        return root;
    }

    private static void AddChimney(GameObject target, Vector3 size)
    {
        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        chimney.name = "Chimney";
        chimney.transform.SetParent(target.transform, false);
        chimney.transform.localScale = size;
        chimney.transform.localPosition = new Vector3(0.3f, size.y, 0.3f);

        Renderer renderer = chimney.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        }

        Collider col = chimney.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }

    private void SpawnResourceNodes()
    {
        if (FindObjectsOfType<ResourceNode>().Length > 0)
        {
            return;
        }

        CreateResourceNode("IronNode", new Vector3(6f, 0f, 4f), "IronOre", new Color(0.8f, 0.4f, 0.2f, 1f));
        CreateResourceNode("CopperNode", new Vector3(-6f, 0f, 5f), "CopperOre", new Color(0.9f, 0.6f, 0.2f, 1f));
        CreateResourceNode("LimestoneNode", new Vector3(2f, 0f, -8f), "Limestone", new Color(0.85f, 0.85f, 0.8f, 1f));
    }

    private static void CreateResourceNode(string name, Vector3 position, string resourceId, Color color)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        node.name = name;
        node.transform.position = position;
        node.transform.localScale = new Vector3(1.2f, 0.6f, 1.2f);

        Renderer renderer = node.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        ResourceNode resourceNode = node.AddComponent<ResourceNode>();
        resourceNode.resourceId = resourceId;
        resourceNode.amount = 1000;
    }
}
