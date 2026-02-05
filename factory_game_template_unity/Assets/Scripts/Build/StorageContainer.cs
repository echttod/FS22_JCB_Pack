using UnityEngine;

public class StorageContainer : MonoBehaviour, IItemInput, IInteractable
{
    public int capacity = 100;
    private ItemInventory _inventory;

    private void Awake()
    {
        _inventory = new ItemInventory(capacity);
    }

    public bool TryInsert(ItemStack stack)
    {
        if (!stack.IsValid)
        {
            return false;
        }

        return _inventory.Add(stack.id, stack.amount) > 0;
    }

    public void Interact()
    {
        Debug.Log("Storage: " + _inventory.GetSummary());
    }
}
