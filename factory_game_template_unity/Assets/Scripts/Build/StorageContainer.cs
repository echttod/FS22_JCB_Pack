using UnityEngine;

public class StorageContainer : MonoBehaviour, IItemInput, IInteractable
{
    public int capacity = 100;
    public bool isBuildDepot = false;
    private ItemInventory _inventory;

    private void Awake()
    {
        EnsureInventory();
    }

    public bool TryInsert(ItemStack stack)
    {
        if (!stack.IsValid)
        {
            return false;
        }

        EnsureInventory();
        return _inventory.Add(stack.id, stack.amount) > 0;
    }

    public void Interact()
    {
        EnsureInventory();
        Debug.Log("Storage: " + _inventory.GetSummary());
    }

    public ItemStack[] GetSnapshot()
    {
        EnsureInventory();
        return _inventory.GetStacks().ToArray();
    }

    public void Restore(ItemStack[] stacks)
    {
        EnsureInventory();
        _inventory.Clear();
        if (stacks == null)
        {
            return;
        }

        foreach (ItemStack stack in stacks)
        {
            _inventory.Add(stack.id, stack.amount);
        }
    }

    public int GetAmount(string id)
    {
        EnsureInventory();
        return _inventory.GetAmount(id);
    }

    public int Remove(string id, int amount)
    {
        EnsureInventory();
        int available = _inventory.GetAmount(id);
        int take = Mathf.Min(available, amount);
        if (take <= 0)
        {
            return 0;
        }

        _inventory.TryRemove(id, take, out _);
        return take;
    }

    public void Add(ItemStack stack)
    {
        EnsureInventory();
        _inventory.Add(stack.id, stack.amount);
    }

    private void EnsureInventory()
    {
        if (_inventory == null)
        {
            _inventory = new ItemInventory(capacity);
        }
    }
}
