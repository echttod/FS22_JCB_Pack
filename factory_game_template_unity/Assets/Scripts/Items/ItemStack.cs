using System;

[Serializable]
public struct ItemStack
{
    public string id;
    public int amount;

    public ItemStack(string id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }

    public bool IsValid => !string.IsNullOrEmpty(id) && amount > 0;
}
