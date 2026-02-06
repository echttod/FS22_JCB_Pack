using System.Collections.Generic;
using UnityEngine;

public class Smelter : MonoBehaviour, IItemInput, IPlacementAware, IInteractable
{
    [Header("Processing")]
    public float processTime = 2f;
    public int inputCapacity = 10;
    public int outputCapacity = 10;

    [Header("Ports")]
    public Transform inputPoint;
    public Transform outputPoint;
    public float connectDistance = 1.4f;
    public LayerMask connectMask = ~0;

    private readonly Dictionary<string, string> _recipes = new Dictionary<string, string>();
    private ItemInventory _input;
    private ItemInventory _output;
    private float _processTimer;
    private string _currentOutput;
    private IItemInput _target;

    private void Awake()
    {
        EnsureInventories();
        BuildRecipes();
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
        Debug.Log("Smelter Input: " + _input.GetSummary() + " | Output: " + _output.GetSummary());
    }

    private void BuildRecipes()
    {
        _recipes[ItemTypes.IronOre] = ItemTypes.IronIngot;
        _recipes[ItemTypes.CopperOre] = ItemTypes.CopperIngot;
        _recipes[ItemTypes.Limestone] = ItemTypes.Concrete;
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

    public string GetCurrentOutputId()
    {
        return _currentOutput;
    }

    public float GetCurrentTimer()
    {
        return _processTimer;
    }

    public void RestoreState(ItemStack[] input, ItemStack[] output, string currentOutput, float remainingTime)
    {
        EnsureInventories();
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

        _currentOutput = currentOutput;
        _processTimer = Mathf.Max(0f, remainingTime);
    }

    private void Process()
    {
        EnsureInventories();
        if (!string.IsNullOrEmpty(_currentOutput))
        {
            _processTimer -= Time.deltaTime;
            if (_processTimer <= 0f)
            {
                if (_output.Add(_currentOutput, 1) > 0)
                {
                    _currentOutput = null;
                }
            }
            return;
        }

        string nextInput = FindNextInput();
        if (string.IsNullOrEmpty(nextInput))
        {
            return;
        }

        if (_input.TryRemove(nextInput, 1, out _))
        {
            _currentOutput = _recipes[nextInput];
            _processTimer = processTime;
        }
    }

    private string FindNextInput()
    {
        foreach (KeyValuePair<string, string> pair in _recipes)
        {
            if (_input.GetAmount(pair.Key) > 0)
            {
                return pair.Key;
            }
        }

        return null;
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
}
