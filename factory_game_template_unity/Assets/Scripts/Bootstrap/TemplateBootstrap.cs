using System.Collections.Generic;
using UnityEngine;

public class TemplateBootstrap : MonoBehaviour
{
    [Header("Scene")]
    public bool createGround = true;
    public Vector3 groundSize = new Vector3(40f, 1f, 40f);
    public bool createLight = true;

    [Header("Player")]
    public Vector3 playerStart = new Vector3(0f, 2f, -6f);

    private Transform _libraryRoot;
    private StorageContainer _buildDepot;

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
        BuildCostProvider costProvider = systems.AddComponent<BuildCostProvider>();
        BuildSystem buildSystem = systems.AddComponent<BuildSystem>();
        buildSystem.catalog = catalog;
        buildSystem.costProvider = costProvider;
        buildSystem.playerCamera = camera;

        HudOverlay hud = cameraObj.AddComponent<HudOverlay>();
        hud.buildSystem = buildSystem;
        hud.costProvider = costProvider;

        CreateBuildDepot();
        CreateDefaultBuildables(catalog);

        SaveLoadManager saveLoad = systems.AddComponent<SaveLoadManager>();
        saveLoad.catalog = catalog;
        saveLoad.costProvider = costProvider;
        saveLoad.buildSystem = buildSystem;
        saveLoad.libraryRoot = _libraryRoot;
        costProvider.Refresh();
    }

    private void CreateDefaultBuildables(BuildCatalog catalog)
    {
        GameObject library = new GameObject("BuildableLibrary");
        library.transform.SetParent(transform, false);
        _libraryRoot = library.transform;

        GameObject conveyor = CreateBuildablePrefab("Conveyor", new Vector3(0.6f, 0.2f, 2f), new Color(0.1f, 0.6f, 0.9f, 1f));
        conveyor.AddComponent<ConveyorBelt>();

        GameObject miner = CreateBuildablePrefab("Miner", new Vector3(1.2f, 1f, 1.2f), new Color(0.2f, 0.7f, 0.3f, 1f));
        miner.AddComponent<Miner>();
        Buildable minerBuildable = miner.GetComponent<Buildable>();
        if (minerBuildable != null)
        {
            minerBuildable.allowResourceOverlap = true;
        }
        AddDrill(miner);

        GameObject smelter = CreateBuildablePrefab("Smelter", new Vector3(1.6f, 1.4f, 1.6f), new Color(0.8f, 0.4f, 0.2f, 1f));
        smelter.AddComponent<Smelter>();

        GameObject storage = CreateBuildablePrefab("Storage", new Vector3(2f, 2f, 2f), new Color(0.6f, 0.6f, 0.6f, 1f));
        storage.AddComponent<StorageContainer>();

        GameObject depot = CreateBuildablePrefab("Depot", new Vector3(1.8f, 1.2f, 1.8f), new Color(0.25f, 0.3f, 0.55f, 1f));
        StorageContainer depotStorage = depot.AddComponent<StorageContainer>();
        depotStorage.capacity = 200;
        depotStorage.isBuildDepot = true;
        AddDepotModel(depot);

        AddChimney(smelter, new Vector3(0.5f, 1.2f, 0.5f));

        RegisterPrefab(catalog, library.transform, conveyor, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 1)
        });
        RegisterPrefab(catalog, library.transform, miner, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 5),
            new BuildCost(ItemTypes.Limestone, 2)
        });
        RegisterPrefab(catalog, library.transform, smelter, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 8),
            new BuildCost(ItemTypes.CopperOre, 4)
        });
        RegisterPrefab(catalog, library.transform, storage, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 4),
            new BuildCost(ItemTypes.Limestone, 4)
        });
        RegisterPrefab(catalog, library.transform, depot, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 6),
            new BuildCost(ItemTypes.Limestone, 6)
        });
    }

    private static void RegisterPrefab(BuildCatalog catalog, Transform parent, GameObject prefab, List<BuildCost> costs)
    {
        prefab.transform.SetParent(parent, false);
        prefab.SetActive(false);
        catalog.Register(prefab.name, prefab, costs);
    }

    private static GameObject CreateBuildablePrefab(string name, Vector3 size, Color color)
    {
        GameObject root = new GameObject(name);
        Buildable buildable = root.AddComponent<Buildable>();
        buildable.footprint = size;
        BuildableId id = root.AddComponent<BuildableId>();
        id.id = name;

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

    private static void AddDrill(GameObject target)
    {
        GameObject drill = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        drill.name = "Drill";
        drill.transform.SetParent(target.transform, false);
        drill.transform.localScale = new Vector3(0.25f, 0.7f, 0.25f);
        drill.transform.localPosition = new Vector3(0f, 0.2f, 0f);

        Renderer renderer = drill.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        Collider col = drill.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }

    private static void AddDepotModel(GameObject target)
    {
        GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        crate.transform.SetParent(target.transform, false);
        crate.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
        crate.transform.localPosition = new Vector3(-0.35f, 0.4f, 0.2f);
        Renderer crateRenderer = crate.GetComponent<Renderer>();
        if (crateRenderer != null)
        {
            crateRenderer.material.color = new Color(0.5f, 0.35f, 0.2f, 1f);
        }

        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beacon.name = "Beacon";
        beacon.transform.SetParent(target.transform, false);
        beacon.transform.localScale = new Vector3(0.2f, 0.8f, 0.2f);
        beacon.transform.localPosition = new Vector3(0.5f, 0.6f, -0.4f);
        Renderer beaconRenderer = beacon.GetComponent<Renderer>();
        if (beaconRenderer != null)
        {
            beaconRenderer.material.color = new Color(0.9f, 0.8f, 0.2f, 1f);
        }

        Collider crateCol = crate.GetComponent<Collider>();
        if (crateCol != null)
        {
            Destroy(crateCol);
        }

        Collider beaconCol = beacon.GetComponent<Collider>();
        if (beaconCol != null)
        {
            Destroy(beaconCol);
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

    private void CreateBuildDepot()
    {
        if (_buildDepot != null)
        {
            return;
        }

        GameObject depot = new GameObject("BuildDepot");
        depot.transform.position = playerStart + new Vector3(2f, 0f, 2f);

        _buildDepot = depot.AddComponent<StorageContainer>();
        _buildDepot.capacity = 200;
        _buildDepot.isBuildDepot = true;

        GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.name = "Mesh";
        mesh.transform.SetParent(depot.transform, false);
        mesh.transform.localScale = new Vector3(1.6f, 1f, 1.6f);
        mesh.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Renderer renderer = mesh.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.2f, 0.5f, 1f);
        }

        AddDepotModel(depot);

        _buildDepot.Add(new ItemStack(ItemTypes.IronOre, 40));
        _buildDepot.Add(new ItemStack(ItemTypes.CopperOre, 20));
        _buildDepot.Add(new ItemStack(ItemTypes.Limestone, 20));
    }
}
