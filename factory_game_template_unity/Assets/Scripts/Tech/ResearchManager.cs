using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public List<TechNode> nodes = new List<TechNode>();

    private readonly HashSet<string> _unlocked = new HashSet<string>();
    public int CurrentTier { get; private set; }

    public void SeedDefaultNodes(List<TechNode> defaultNodes)
    {
        if (defaultNodes == null || defaultNodes.Count == 0)
        {
            return;
        }

        nodes.Clear();
        nodes.AddRange(defaultNodes);
        RecalculateTier();
    }

    public List<TechNode> GetNodes()
    {
        return nodes;
    }

    public bool IsUnlocked(string id)
    {
        return !string.IsNullOrEmpty(id) && _unlocked.Contains(id);
    }

    public bool CanResearch(TechNode node, BuildCostProvider costProvider)
    {
        if (node == null || string.IsNullOrEmpty(node.id))
        {
            return false;
        }

        if (IsUnlocked(node.id))
        {
            return false;
        }

        if (!ArePrerequisitesMet(node))
        {
            return false;
        }

        if (costProvider == null)
        {
            return true;
        }

        return costProvider.CanAfford(node.costs, Vector3.zero);
    }

    public bool TryResearch(string id, BuildCostProvider costProvider)
    {
        TechNode node = GetNode(id);
        if (node == null)
        {
            return false;
        }

        if (!CanResearch(node, costProvider))
        {
            return false;
        }

        if (costProvider != null && !costProvider.Spend(node.costs, Vector3.zero))
        {
            return false;
        }

        _unlocked.Add(node.id);
        RecalculateTier();
        return true;
    }

    public TechNode GetNode(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (TechNode node in nodes)
        {
            if (node != null && node.id == id)
            {
                return node;
            }
        }

        return null;
    }

    public List<TechNode> GetLockedNodes()
    {
        List<TechNode> locked = new List<TechNode>();
        foreach (TechNode node in nodes)
        {
            if (node != null && !IsUnlocked(node.id))
            {
                locked.Add(node);
            }
        }
        return locked;
    }

    public ResearchSaveData GetSaveData()
    {
        ResearchSaveData data = new ResearchSaveData
        {
            unlockedIds = new List<string>()
        };
        foreach (string id in _unlocked)
        {
            data.unlockedIds.Add(id);
        }
        data.currentTier = CurrentTier;
        return data;
    }

    public void RestoreSaveData(ResearchSaveData data)
    {
        _unlocked.Clear();
        if (data != null && data.unlockedIds != null)
        {
            foreach (string id in data.unlockedIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _unlocked.Add(id);
                }
            }
        }
        RecalculateTier();
    }

    private bool ArePrerequisitesMet(TechNode node)
    {
        if (node.prerequisites == null || node.prerequisites.Count == 0)
        {
            return true;
        }

        foreach (string prereq in node.prerequisites)
        {
            if (!IsUnlocked(prereq))
            {
                return false;
            }
        }

        return true;
    }

    private void RecalculateTier()
    {
        int tier = 0;
        foreach (TechNode node in nodes)
        {
            if (node != null && IsUnlocked(node.id))
            {
                tier = Mathf.Max(tier, node.unlockTier);
            }
        }
        CurrentTier = tier;
    }
}

[System.Serializable]
public class ResearchSaveData
{
    public List<string> unlockedIds;
    public int currentTier;
}
