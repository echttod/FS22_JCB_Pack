using System;
using System.Collections.Generic;

[Serializable]
public class RecipeDefinition
{
    public string id;
    public string displayName;
    public string machineId;
    public int requiredTier;
    public float processTime = 2f;
    public List<BuildCost> inputs = new List<BuildCost>();
    public List<ItemStack> outputs = new List<ItemStack>();
}
