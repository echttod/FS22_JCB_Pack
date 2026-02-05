using System.Collections.Generic;
using UnityEngine;

public class ItemInventory
{
    private readonly Dictionary<string, int> _items = new Dictionary<string, int>();
    private int _count;

    public int Capacity { get; private set; }
    public int Count => _count;

    public ItemInventory(int capacity)
    {
        Capacity = Mathf.Max(0, capacity);
    }

    public int Add(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0)
        {
            return 0;
        }

        int space = Capacity - _count;
        if (space <= 0)
        {
            return 0;
        }

        int added = Mathf.Min(space, amount);
        if (_items.TryGetValue(id, out int current))
        {
            _items[id] = current + added;
        }
        else
        {
            _items[id] = added;
        }

        _count += added;
        return added;
    }

    public bool TryRemove(string id, int amount, out ItemStack stack)
    {
        stack = default;
        if (string.IsNullOrEmpty(id) || amount <= 0)
        {
            return false;
        }

        if (!_items.TryGetValue(id, out int current) || current < amount)
        {
            return false;
        }

        int remaining = current - amount;
        if (remaining <= 0)
        {
            _items.Remove(id);
        }
        else
        {
            _items[id] = remaining;
        }

        _count -= amount;
        stack = new ItemStack(id, amount);
        return true;
    }

    public bool TryRemoveAny(out ItemStack stack)
    {
        foreach (KeyValuePair<string, int> pair in _items)
        {
            if (pair.Value > 0)
            {
                return TryRemove(pair.Key, 1, out stack);
            }
        }

        stack = default;
        return false;
    }

    public int GetAmount(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return 0;
        }

        return _items.TryGetValue(id, out int current) ? current : 0;
    }

    public bool HasAny()
    {
        return _count > 0;
    }

    public string GetSummary()
    {
        if (_items.Count == 0)
        {
            return "Empty";
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in _items)
        {
            parts.Add(pair.Key + " x" + pair.Value);
        }
        return string.Join(", ", parts);
    }
}
