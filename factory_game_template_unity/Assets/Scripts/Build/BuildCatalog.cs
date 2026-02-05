using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuildableEntry
{
    public string id;
    public GameObject prefab;
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

    public void Register(string id, GameObject prefab)
    {
        if (string.IsNullOrEmpty(id) || prefab == null)
        {
            return;
        }

        BuildableEntry existing = buildables.Find(e => e != null && e.id == id);
        if (existing != null)
        {
            existing.prefab = prefab;
            return;
        }

        BuildableEntry entry = new BuildableEntry
        {
            id = id,
            prefab = prefab
        };
        buildables.Add(entry);
    }
}
