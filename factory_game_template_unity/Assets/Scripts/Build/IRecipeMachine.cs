public interface IRecipeMachine
{
    ItemStack[] GetInputSnapshot();
    ItemStack[] GetOutputSnapshot();
    string GetActiveRecipeId();
    string GetProcessingRecipeId();
    float GetCurrentTimer();
    void RestoreState(ItemStack[] input, ItemStack[] output, string activeRecipe, string processingRecipe, float remainingTime);
}
