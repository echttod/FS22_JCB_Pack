using System;

[Serializable]
public struct BuildCost
{
    public string id;
    public int amount;

    public BuildCost(string id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}
