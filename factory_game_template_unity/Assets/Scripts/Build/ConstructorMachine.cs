using UnityEngine;

public class ConstructorMachine : MonoBehaviour, IItemInput, IPlacementAware, IInteractable, IRecipeMachine
{
    [Header("Processing")]
    public int inputCapacity = 20;
    public int outputCapacity = 20;
    public string machineId = "Constructor";
    public string activeRecipeId;
    public RecipeBook recipeBook;
    public ResearchManager researchManager;
    public PowerConsumer powerConsumer;

    [Header("Ports")]
    public Transform inputPoint;
    public Transform outputPoint;
    public float connectDistance = 1.4f;
    public LayerMask connectMask = ~0;

    private ItemInventory _input;
    private ItemInventory _output;
    private float _processTimer;
    private RecipeDefinition _processingRecipe;
    private IItemInput _target;

    private void Awake()
    {
        EnsureInventories();
        EnsureDependencies();
        EnsurePorts();
        RefreshTarget();
    }

    private void Update()
    {
        Process();
        PushOutput();
    }

    public void OnPlaced()
    {
        EnsureDependencies();
        RefreshTarget();
    }

    public bool TryInsert(ItemStack stack)
    {
        if (!stack.IsValid)
        {
            return false;
        }

        EnsureInventories();
        return _input.Add(stack.id, stack.amount) > 0;
    }

    public void Interact()
    {
        EnsureInventories();
        CycleRecipe();
        string recipeName = string.IsNullOrEmpty(activeRecipeId) ? "Auto" : activeRecipeId;
        Debug.Log("Constructor [" + recipeName + "] Input: " + _input.GetSummary() + " | Output: " + _output.GetSummary());
    }

    public ItemStack[] GetInputSnapshot()
    {
        EnsureInventories();
        return _input.GetStacks().ToArray();
    }

    public ItemStack[] GetOutputSnapshot()
    {
        EnsureInventories();
        return _output.GetStacks().ToArray();
    }

    public string GetActiveRecipeId()
    {
        return activeRecipeId;
    }

    public string GetProcessingRecipeId()
    {
        return _processingRecipe != null ? _processingRecipe.id : null;
    }

    public float GetCurrentTimer()
    {
        return _processTimer;
    }

    public void RestoreState(ItemStack[] input, ItemStack[] output, string activeRecipe, string processingRecipe, float remainingTime)
    {
        EnsureInventories();
        EnsureDependencies();
        _input.Clear();
        _output.Clear();

        if (input != null)
        {
            foreach (ItemStack stack in input)
            {
                _input.Add(stack.id, stack.amount);
            }
        }

        if (output != null)
        {
            foreach (ItemStack stack in output)
            {
                _output.Add(stack.id, stack.amount);
            }
        }

        activeRecipeId = activeRecipe;
        _processingRecipe = recipeBook != null ? recipeBook.GetRecipe(processingRecipe) : null;
        _processTimer = Mathf.Max(0f, remainingTime);
    }

    private void Process()
    {
        EnsureInventories();
        float powerFactor = GetPowerFactor();
        if (powerFactor <= 0.01f)
        {
            return;
        }

        if (_processingRecipe != null)
        {
            _processTimer -= Time.deltaTime * powerFactor;
            if (_processTimer <= 0f)
            {
                if (CanFitOutputs(_processingRecipe))
                {
                    AddOutputs(_processingRecipe);
                    _processingRecipe = null;
                }
                else
                {
                    _processTimer = 0.1f;
                }
            }
            return;
        }

        RecipeDefinition recipe = SelectRecipe();
        if (recipe == null)
        {
            return;
        }

        if (!HasInputs(recipe))
        {
            return;
        }

        if (!CanFitOutputs(recipe))
        {
            return;
        }

        ConsumeInputs(recipe);
        _processingRecipe = recipe;
        _processTimer = Mathf.Max(0.1f, recipe.processTime);
    }

    private RecipeDefinition SelectRecipe()
    {
        EnsureDependencies();
        if (recipeBook == null)
        {
            return null;
        }

        int tier = researchManager != null ? researchManager.CurrentTier : int.MaxValue;
        var recipes = recipeBook.GetRecipesForMachine(machineId, tier);
        if (recipes.Count == 0)
        {
            return null;
        }

        RecipeDefinition active = null;
        if (!string.IsNullOrEmpty(activeRecipeId))
        {
            foreach (RecipeDefinition recipe in recipes)
            {
                if (recipe.id == activeRecipeId)
                {
                    active = recipe;
                    break;
                }
            }
        }

        if (active == null)
        {
            active = recipes[0];
            activeRecipeId = active.id;
        }

        if (HasInputs(active))
        {
            return active;
        }

        foreach (RecipeDefinition recipe in recipes)
        {
            if (HasInputs(recipe))
            {
                return recipe;
            }
        }

        return active;
    }

    private void CycleRecipe()
    {
        EnsureDependencies();
        if (recipeBook == null)
        {
            return;
        }

        int tier = researchManager != null ? researchManager.CurrentTier : int.MaxValue;
        var recipes = recipeBook.GetRecipesForMachine(machineId, tier);
        if (recipes.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(activeRecipeId))
        {
            activeRecipeId = recipes[0].id;
            return;
        }

        int index = recipes.FindIndex(r => r.id == activeRecipeId);
        index = (index + 1) % recipes.Count;
        activeRecipeId = recipes[index].id;
    }

    private bool HasInputs(RecipeDefinition recipe)
    {
        if (recipe == null || recipe.inputs == null)
        {
            return false;
        }

        foreach (BuildCost cost in recipe.inputs)
        {
            if (_input.GetAmount(cost.id) < cost.amount)
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeInputs(RecipeDefinition recipe)
    {
        foreach (BuildCost cost in recipe.inputs)
        {
            _input.TryRemove(cost.id, cost.amount, out _);
        }
    }

    private bool CanFitOutputs(RecipeDefinition recipe)
    {
        if (recipe == null || recipe.outputs == null)
        {
            return false;
        }

        int total = 0;
        foreach (ItemStack stack in recipe.outputs)
        {
            total += stack.amount;
        }

        return _output.Count + total <= _output.Capacity;
    }

    private void AddOutputs(RecipeDefinition recipe)
    {
        foreach (ItemStack stack in recipe.outputs)
        {
            _output.Add(stack.id, stack.amount);
        }
    }

    private void PushOutput()
    {
        EnsureInventories();
        if (!_output.HasAny())
        {
            return;
        }

        if (_target == null)
        {
            RefreshTarget();
        }

        if (_target == null)
        {
            return;
        }

        if (_output.TryRemoveAny(out ItemStack stack))
        {
            if (!_target.TryInsert(stack))
            {
                _output.Add(stack.id, stack.amount);
            }
        }
    }

    private void EnsurePorts()
    {
        if (inputPoint == null)
        {
            GameObject inputObj = new GameObject("InputPoint");
            inputObj.transform.SetParent(transform, false);
            inputPoint = inputObj.transform;
        }

        if (outputPoint == null)
        {
            GameObject outputObj = new GameObject("OutputPoint");
            outputObj.transform.SetParent(transform, false);
            outputPoint = outputObj.transform;
        }

        inputPoint.localPosition = new Vector3(0f, 0.4f, -0.9f);
        outputPoint.localPosition = new Vector3(0f, 0.4f, 0.9f);
    }

    private void EnsureInventories()
    {
        if (_input == null)
        {
            _input = new ItemInventory(inputCapacity);
        }

        if (_output == null)
        {
            _output = new ItemInventory(outputCapacity);
        }
    }

    private void RefreshTarget()
    {
        _target = ItemLinker.FindInput(outputPoint, connectDistance, connectMask, transform);
    }

    private void EnsureDependencies()
    {
        if (recipeBook == null)
        {
            recipeBook = FindObjectOfType<RecipeBook>();
        }

        if (researchManager == null)
        {
            researchManager = FindObjectOfType<ResearchManager>();
        }

        if (powerConsumer == null)
        {
            powerConsumer = GetComponent<PowerConsumer>();
        }
    }

    private float GetPowerFactor()
    {
        return powerConsumer != null ? powerConsumer.PowerRatio : 1f;
    }
}
