using System;
using System.Collections.Generic;

[Serializable]
public class TechNode
{
    public string id;
    public string displayName;
    public string description;
    public int unlockTier;
    public List<BuildCost> costs = new List<BuildCost>();
    public List<string> prerequisites = new List<string>();
}
