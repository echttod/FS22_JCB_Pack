using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildableEntry
{
    public string id;
    public GameObject prefab;
    public List<BuildCost> costs = new List<BuildCost>();
}

public class BuildCatalog : MonoBehaviour
{
    public List<BuildableEntry> buildables = new List<BuildableEntry>();

    public string[] GetIds()
    {
        List<string> ids = new List<string>();
        foreach (BuildableEntry entry in buildables)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.id))
            {
                ids.Add(entry.id);
            }
        }
        ids.Sort(StringComparer.Ordinal);
        return ids.ToArray();
    }

    public GameObject GetPrefab(string id)
    {
        foreach (BuildableEntry entry in buildables)
        {
            if (entry != null && entry.id == id)
            {
                return entry.prefab;
            }
        }
        return null;
    }

    public List<BuildCost> GetCosts(string id)
    {
        foreach (BuildableEntry entry in buildables)
        {
            if (entry != null && entry.id == id)
            {
                return entry.costs ?? new List<BuildCost>();
            }
        }

        return new List<BuildCost>();
    }

    public void Register(string id, GameObject prefab)
    {
        Register(id, prefab, null);
    }

    public void Register(string id, GameObject prefab, List<BuildCost> costs)
    {
        if (string.IsNullOrEmpty(id) || prefab == null)
        {
            return;
        }

        BuildableEntry existing = buildables.Find(e => e != null && e.id == id);
        if (existing != null)
        {
            existing.prefab = prefab;
            existing.costs = costs ?? new List<BuildCost>();
            return;
        }

        BuildableEntry entry = new BuildableEntry
        {
            id = id,
            prefab = prefab,
            costs = costs ?? new List<BuildCost>()
        };
        buildables.Add(entry);
    }
}
