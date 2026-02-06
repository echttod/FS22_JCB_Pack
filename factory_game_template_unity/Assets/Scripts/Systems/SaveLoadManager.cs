using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public BuildCatalog catalog;
    public BuildSystem buildSystem;
    public BuildWallet wallet;
    public Transform libraryRoot;
    public string saveFileName = "factory_save.json";
    public KeyCode saveKey = KeyCode.F5;
    public KeyCode loadKey = KeyCode.F9;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Update()
    {
        if (Input.GetKeyDown(saveKey))
        {
            Save();
        }

        if (Input.GetKeyDown(loadKey))
        {
            Load();
        }
    }

    public void Save()
    {
        SaveData data = new SaveData
        {
            buildables = new List<BuildableSaveData>(),
            resources = new List<ResourceNodeSaveData>(),
            wallet = wallet != null ? wallet.GetSnapshot().ToArray() : new ItemStack[0]
        };

        BuildableId[] buildables = FindObjectsOfType<BuildableId>();
        foreach (BuildableId buildable in buildables)
        {
            if (buildable == null || !buildable.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (buildable.GetComponent<BuildGhost>() != null)
            {
                continue;
            }

            if (libraryRoot != null && buildable.transform.IsChildOf(libraryRoot))
            {
                continue;
            }

            BuildableSaveData entry = new BuildableSaveData
            {
                id = buildable.id,
                position = buildable.transform.position,
                rotation = buildable.transform.eulerAngles
            };

            StorageContainer storage = buildable.GetComponent<StorageContainer>();
            if (storage != null)
            {
                entry.storage = storage.GetSnapshot();
            }

            data.buildables.Add(entry);
        }

        ResourceNode[] nodes = FindObjectsOfType<ResourceNode>();
        foreach (ResourceNode node in nodes)
        {
            if (node == null)
            {
                continue;
            }

            data.resources.Add(new ResourceNodeSaveData
            {
                name = node.name,
                amount = node.Amount
            });
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("Saved to: " + SavePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Save failed: " + ex.Message);
        }
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found: " + SavePath);
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("Save file is empty or invalid.");
                return;
            }

            RestoreBuildables(data.buildables);
            RestoreResources(data.resources);

            if (wallet != null)
            {
                wallet.RestoreSnapshot(data.wallet);
            }

            if (buildSystem != null)
            {
                buildSystem.RefreshCatalog();
            }

            Debug.Log("Loaded from: " + SavePath);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Load failed: " + ex.Message);
        }
    }

    private void RestoreBuildables(List<BuildableSaveData> buildables)
    {
        BuildableId[] existing = FindObjectsOfType<BuildableId>();
        foreach (BuildableId buildable in existing)
        {
            if (buildable == null || !buildable.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (buildable.GetComponent<BuildGhost>() != null)
            {
                continue;
            }

            if (libraryRoot != null && buildable.transform.IsChildOf(libraryRoot))
            {
                continue;
            }

            Destroy(buildable.gameObject);
        }

        if (buildables == null || catalog == null)
        {
            return;
        }

        foreach (BuildableSaveData entry in buildables)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                continue;
            }

            GameObject prefab = catalog.GetPrefab(entry.id);
            if (prefab == null)
            {
                continue;
            }

            GameObject placed = Instantiate(prefab, entry.position, Quaternion.Euler(entry.rotation));
            placed.name = prefab.name;
            placed.SetActive(true);

            BuildableId id = placed.GetComponent<BuildableId>();
            if (id == null)
            {
                id = placed.AddComponent<BuildableId>();
            }
            id.id = entry.id;

            StorageContainer storage = placed.GetComponent<StorageContainer>();
            if (storage != null && entry.storage != null)
            {
                storage.Restore(entry.storage);
            }

            NotifyPlaced(placed);
        }
    }

    private void RestoreResources(List<ResourceNodeSaveData> resources)
    {
        if (resources == null)
        {
            return;
        }

        ResourceNode[] nodes = FindObjectsOfType<ResourceNode>();
        Dictionary<string, ResourceNode> lookup = new Dictionary<string, ResourceNode>();
        foreach (ResourceNode node in nodes)
        {
            if (node != null && !lookup.ContainsKey(node.name))
            {
                lookup.Add(node.name, node);
            }
        }

        foreach (ResourceNodeSaveData entry in resources)
        {
            if (entry == null || string.IsNullOrEmpty(entry.name))
            {
                continue;
            }

            if (lookup.TryGetValue(entry.name, out ResourceNode node))
            {
                node.SetAmount(entry.amount);
            }
        }
    }

    private static void NotifyPlaced(GameObject placed)
    {
        if (placed == null)
        {
            return;
        }

        MonoBehaviour[] behaviours = placed.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPlacementAware aware)
            {
                aware.OnPlaced();
            }
        }
    }

    [Serializable]
    private class SaveData
    {
        public List<BuildableSaveData> buildables;
        public List<ResourceNodeSaveData> resources;
        public ItemStack[] wallet;
    }

    [Serializable]
    private class BuildableSaveData
    {
        public string id;
        public Vector3 position;
        public Vector3 rotation;
        public ItemStack[] storage;
    }

    [Serializable]
    private class ResourceNodeSaveData
    {
        public string name;
        public int amount;
    }
}
