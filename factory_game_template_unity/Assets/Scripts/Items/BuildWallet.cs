using System.Collections.Generic;
using UnityEngine;

public class BuildWallet : MonoBehaviour
{
    public int capacity = 500;
    public bool freeBuild = false;

    private ItemInventory _inventory;

    public int Count => _inventory != null ? _inventory.Count : 0;

    private void Awake()
    {
        _inventory = new ItemInventory(capacity);
    }

    public void Add(string id, int amount)
    {
        if (_inventory == null)
        {
            _inventory = new ItemInventory(capacity);
        }

        _inventory.Add(id, amount);
    }

    public bool CanAfford(List<BuildCost> costs)
    {
        if (freeBuild)
        {
            return true;
        }

        if (costs == null || costs.Count == 0)
        {
            return true;
        }

        foreach (BuildCost cost in costs)
        {
            if (_inventory.GetAmount(cost.id) < cost.amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool Spend(List<BuildCost> costs)
    {
        if (freeBuild)
        {
            return true;
        }

        if (!CanAfford(costs))
        {
            return false;
        }

        foreach (BuildCost cost in costs)
        {
            if (!_inventory.TryRemove(cost.id, cost.amount, out _))
            {
                return false;
            }
        }

        return true;
    }

    public string GetSummary()
    {
        return _inventory != null ? _inventory.GetSummary() : "Empty";
    }

    public List<ItemStack> GetSnapshot()
    {
        return _inventory != null ? _inventory.GetStacks() : new List<ItemStack>();
    }

    public void RestoreSnapshot(IEnumerable<ItemStack> stacks)
    {
        if (_inventory == null)
        {
            _inventory = new ItemInventory(capacity);
        }

        _inventory.Clear();
        if (stacks == null)
        {
            return;
        }

        foreach (ItemStack stack in stacks)
        {
            _inventory.Add(stack.id, stack.amount);
        }
    }
}
