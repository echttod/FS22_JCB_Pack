using System.Collections.Generic;
using UnityEngine;

public class RecipeBook : MonoBehaviour
{
    public List<RecipeDefinition> recipes = new List<RecipeDefinition>();

    public RecipeDefinition GetRecipe(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (RecipeDefinition recipe in recipes)
        {
            if (recipe != null && recipe.id == id)
            {
                return recipe;
            }
        }

        return null;
    }

    public List<RecipeDefinition> GetRecipesForMachine(string machineId, int maxTier)
    {
        List<RecipeDefinition> list = new List<RecipeDefinition>();
        foreach (RecipeDefinition recipe in recipes)
        {
            if (recipe == null)
            {
                continue;
            }

            if (recipe.machineId == machineId && recipe.requiredTier <= maxTier)
            {
                list.Add(recipe);
            }
        }

        return list;
    }
}
