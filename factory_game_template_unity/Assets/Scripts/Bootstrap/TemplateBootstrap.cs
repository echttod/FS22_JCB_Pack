using System.Collections.Generic;
using UnityEngine;

public class TemplateBootstrap : MonoBehaviour
{
    [Header("Scene")]
    public bool createGround = true;
    public Vector3 groundSize = new Vector3(40f, 1f, 40f);
    public bool createLight = true;
    public bool usePrefabs = true;

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
        ResearchManager researchManager = systems.AddComponent<ResearchManager>();
        RecipeBook recipeBook = systems.AddComponent<RecipeBook>();
        PowerManager powerManager = systems.AddComponent<PowerManager>();
        BuildSystem buildSystem = systems.AddComponent<BuildSystem>();
        buildSystem.catalog = catalog;
        buildSystem.costProvider = costProvider;
        buildSystem.researchManager = researchManager;
        buildSystem.playerCamera = camera;

        HudOverlay hud = cameraObj.AddComponent<HudOverlay>();
        hud.buildSystem = buildSystem;
        hud.costProvider = costProvider;
        hud.researchManager = researchManager;
        hud.powerManager = powerManager;

        CreateBuildDepot();
        SeedRecipes(recipeBook);
        SeedTechTree(researchManager);
        CreateDefaultBuildables(catalog);

        SaveLoadManager saveLoad = systems.AddComponent<SaveLoadManager>();
        saveLoad.catalog = catalog;
        saveLoad.costProvider = costProvider;
        saveLoad.researchManager = researchManager;
        saveLoad.buildSystem = buildSystem;
        saveLoad.libraryRoot = _libraryRoot;
        costProvider.Refresh();
    }

    private void CreateDefaultBuildables(BuildCatalog catalog)
    {
        GameObject library = new GameObject("BuildableLibrary");
        library.transform.SetParent(transform, false);
        _libraryRoot = library.transform;

        GameObject conveyor = GetOrCreatePrefab("Conveyor", BuildableVisualType.Conveyor, go =>
        {
            go.AddComponent<ConveyorBelt>();
            PowerConsumer consumer = go.AddComponent<PowerConsumer>();
            consumer.demand = 0.2f;
        });

        GameObject miner = GetOrCreatePrefab("Miner", BuildableVisualType.Miner, go =>
        {
            go.AddComponent<Miner>();
            PowerConsumer consumer = go.AddComponent<PowerConsumer>();
            consumer.demand = 1.2f;
        });

        GameObject smelter = GetOrCreatePrefab("Smelter", BuildableVisualType.Smelter, go =>
        {
            go.AddComponent<Smelter>();
            PowerConsumer consumer = go.AddComponent<PowerConsumer>();
            consumer.demand = 1.4f;
        });

        GameObject storage = GetOrCreatePrefab("Storage", BuildableVisualType.Storage, go =>
        {
            go.AddComponent<StorageContainer>();
        });

        GameObject depot = GetOrCreatePrefab("Depot", BuildableVisualType.Depot, go =>
        {
            StorageContainer depotStorage = go.AddComponent<StorageContainer>();
            depotStorage.capacity = 200;
            depotStorage.isBuildDepot = true;
        });

        GameObject generator = GetOrCreatePrefab("Generator", BuildableVisualType.Generator, go =>
        {
            PowerProducer producer = go.AddComponent<PowerProducer>();
            producer.output = 12f;
        });

        GameObject pole = GetOrCreatePrefab("PowerPole", BuildableVisualType.PowerPole, go =>
        {
            PowerNode node = go.AddComponent<PowerNode>();
            node.linkRadius = 6f;
            node.isPole = true;
        });

        RegisterPrefab(catalog, library.transform, conveyor, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 1)
        }, 0);
        RegisterPrefab(catalog, library.transform, miner, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 5),
            new BuildCost(ItemTypes.Limestone, 2)
        }, 1);
        RegisterPrefab(catalog, library.transform, smelter, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 8),
            new BuildCost(ItemTypes.CopperOre, 4)
        }, 1);
        RegisterPrefab(catalog, library.transform, storage, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 4),
            new BuildCost(ItemTypes.Limestone, 4)
        }, 0);
        RegisterPrefab(catalog, library.transform, depot, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 6),
            new BuildCost(ItemTypes.Limestone, 6)
        }, 0);
        RegisterPrefab(catalog, library.transform, generator, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 8),
            new BuildCost(ItemTypes.CopperOre, 6)
        }, 1);
        RegisterPrefab(catalog, library.transform, pole, new List<BuildCost>
        {
            new BuildCost(ItemTypes.IronOre, 2),
            new BuildCost(ItemTypes.CopperOre, 2)
        }, 1);
    }

    private void RegisterPrefab(BuildCatalog catalog, Transform parent, GameObject prefab, List<BuildCost> costs, int requiredTier)
    {
        if (prefab == null)
        {
            return;
        }

        if (parent != null && prefab.scene.IsValid())
        {
            prefab.transform.SetParent(parent, false);
            prefab.SetActive(false);
        }

        catalog.Register(prefab.name, prefab, costs, requiredTier);
    }

    private GameObject GetOrCreatePrefab(string name, BuildableVisualType preset, System.Action<GameObject> setup)
    {
        GameObject prefab = null;
        if (usePrefabs)
        {
            prefab = Resources.Load<GameObject>("Buildables/" + name);
        }

        if (prefab != null)
        {
            return prefab;
        }

        GameObject runtime = new GameObject(name);
        Buildable buildable = runtime.AddComponent<Buildable>();
        BuildableId id = runtime.AddComponent<BuildableId>();
        id.id = name;

        BuildableVisualPreset visual = runtime.AddComponent<BuildableVisualPreset>();
        visual.preset = preset;

        setup?.Invoke(runtime);
        return runtime;
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

        BuildableVisualPreset visual = depot.AddComponent<BuildableVisualPreset>();
        visual.preset = BuildableVisualType.Depot;

        _buildDepot.Add(new ItemStack(ItemTypes.IronOre, 40));
        _buildDepot.Add(new ItemStack(ItemTypes.CopperOre, 20));
        _buildDepot.Add(new ItemStack(ItemTypes.Limestone, 20));
    }

    private static void SeedRecipes(RecipeBook recipeBook)
    {
        if (recipeBook == null)
        {
            return;
        }

        recipeBook.recipes.Clear();
        recipeBook.recipes.Add(new RecipeDefinition
        {
            id = "IronIngot",
            displayName = "Iron Ingot",
            machineId = "Smelter",
            requiredTier = 1,
            processTime = 2f,
            inputs = new List<BuildCost> { new BuildCost(ItemTypes.IronOre, 1) },
            outputs = new List<ItemStack> { new ItemStack(ItemTypes.IronIngot, 1) }
        });

        recipeBook.recipes.Add(new RecipeDefinition
        {
            id = "CopperIngot",
            displayName = "Copper Ingot",
            machineId = "Smelter",
            requiredTier = 1,
            processTime = 2f,
            inputs = new List<BuildCost> { new BuildCost(ItemTypes.CopperOre, 1) },
            outputs = new List<ItemStack> { new ItemStack(ItemTypes.CopperIngot, 1) }
        });

        recipeBook.recipes.Add(new RecipeDefinition
        {
            id = "Concrete",
            displayName = "Concrete",
            machineId = "Smelter",
            requiredTier = 1,
            processTime = 1.5f,
            inputs = new List<BuildCost> { new BuildCost(ItemTypes.Limestone, 2) },
            outputs = new List<ItemStack> { new ItemStack(ItemTypes.Concrete, 1) }
        });

        recipeBook.recipes.Add(new RecipeDefinition
        {
            id = "IronPlate",
            displayName = "Iron Plate",
            machineId = "Smelter",
            requiredTier = 2,
            processTime = 2.5f,
            inputs = new List<BuildCost> { new BuildCost(ItemTypes.IronIngot, 2) },
            outputs = new List<ItemStack> { new ItemStack(ItemTypes.IronPlate, 1) }
        });

        recipeBook.recipes.Add(new RecipeDefinition
        {
            id = "CopperWire",
            displayName = "Copper Wire",
            machineId = "Smelter",
            requiredTier = 2,
            processTime = 1.8f,
            inputs = new List<BuildCost> { new BuildCost(ItemTypes.CopperIngot, 1) },
            outputs = new List<ItemStack> { new ItemStack(ItemTypes.CopperWire, 2) }
        });
    }

    private static void SeedTechTree(ResearchManager researchManager)
    {
        if (researchManager == null)
        {
            return;
        }

        List<TechNode> nodes = new List<TechNode>
        {
            new TechNode
            {
                id = "tier1_automation",
                displayName = "Tier 1: Automation",
                description = "Unlocks Miner, Smelter and Generator.",
                unlockTier = 1,
                costs = new List<BuildCost>
                {
                    new BuildCost(ItemTypes.IronOre, 10),
                    new BuildCost(ItemTypes.Limestone, 5)
                }
            },
            new TechNode
            {
                id = "tier1_power",
                displayName = "Tier 1: Power Grid",
                description = "Unlocks Power Poles and stabilizes power.",
                unlockTier = 1,
                costs = new List<BuildCost>
                {
                    new BuildCost(ItemTypes.CopperOre, 6),
                    new BuildCost(ItemTypes.IronOre, 6)
                },
                prerequisites = new List<string> { "tier1_automation" }
            },
            new TechNode
            {
                id = "tier2_efficiency",
                displayName = "Tier 2: Efficiency",
                description = "Future tech tier placeholder.",
                unlockTier = 2,
                costs = new List<BuildCost>
                {
                    new BuildCost(ItemTypes.IronIngot, 10),
                    new BuildCost(ItemTypes.CopperIngot, 5)
                },
                prerequisites = new List<string> { "tier1_automation" }
            },
            new TechNode
            {
                id = "tier2_logistics",
                displayName = "Tier 2: Logistics",
                description = "Unlocks advanced recipes.",
                unlockTier = 2,
                costs = new List<BuildCost>
                {
                    new BuildCost(ItemTypes.IronPlate, 4),
                    new BuildCost(ItemTypes.CopperWire, 6)
                },
                prerequisites = new List<string> { "tier2_efficiency" }
            }
        };

        researchManager.SeedDefaultNodes(nodes);
    }
}
