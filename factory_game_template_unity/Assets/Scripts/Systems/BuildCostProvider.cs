using System.Collections.Generic;
using UnityEngine;

public class BuildCostProvider : MonoBehaviour
{
    public bool useAllStorages = false;
    public float refreshInterval = 1f;

    private readonly List<StorageContainer> _storages = new List<StorageContainer>();
    private float _refreshTimer;

    private void Awake()
    {
        Refresh();
    }

    private void Update()
    {
        _refreshTimer += Time.deltaTime;
        if (_refreshTimer >= refreshInterval)
        {
            _refreshTimer = 0f;
            Refresh();
        }
    }

    public void Refresh()
    {
        _storages.Clear();
        StorageContainer[] all = FindObjectsOfType<StorageContainer>();
        foreach (StorageContainer storage in all)
        {
            if (storage == null)
            {
                continue;
            }

            if (useAllStorages || storage.isBuildDepot)
            {
                _storages.Add(storage);
            }
        }

        if (_storages.Count == 0 && !useAllStorages)
        {
            foreach (StorageContainer storage in all)
            {
                if (storage != null)
                {
                    _storages.Add(storage);
                }
            }
        }
    }

    public bool CanAfford(List<BuildCost> costs)
    {
        if (costs == null || costs.Count == 0)
        {
            return true;
        }

        EnsureStorages();
        foreach (BuildCost cost in costs)
        {
            int available = 0;
            foreach (StorageContainer storage in _storages)
            {
                available += storage.GetAmount(cost.id);
            }

            if (available < cost.amount)
            {
                return false;
            }
        }

        return true;
    }

    public bool Spend(List<BuildCost> costs)
    {
        if (costs == null || costs.Count == 0)
        {
            return true;
        }

        EnsureStorages();
        if (!CanAfford(costs))
        {
            return false;
        }

        foreach (BuildCost cost in costs)
        {
            int remaining = cost.amount;
            foreach (StorageContainer storage in _storages)
            {
                if (remaining <= 0)
                {
                    break;
                }

                int removed = storage.Remove(cost.id, remaining);
                remaining -= removed;
            }
        }

        return true;
    }

    public string GetSummary()
    {
        EnsureStorages();
        Dictionary<string, int> totals = new Dictionary<string, int>();
        foreach (StorageContainer storage in _storages)
        {
            ItemStack[] stacks = storage.GetSnapshot();
            foreach (ItemStack stack in stacks)
            {
                if (totals.TryGetValue(stack.id, out int current))
                {
                    totals[stack.id] = current + stack.amount;
                }
                else
                {
                    totals[stack.id] = stack.amount;
                }
            }
        }

        if (totals.Count == 0)
        {
            return "Empty";
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in totals)
        {
            parts.Add(pair.Key + " x" + pair.Value);
        }

        return string.Join(", ", parts);
    }

    private void EnsureStorages()
    {
        if (_storages.Count == 0)
        {
            Refresh();
        }
    }
}
