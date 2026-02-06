using System;

[Serializable]
public struct ConveyorItemState
{
    public string id;
    public float progress;

    public ConveyorItemState(string id, float progress)
    {
        this.id = id;
        this.progress = progress;
    }
}
