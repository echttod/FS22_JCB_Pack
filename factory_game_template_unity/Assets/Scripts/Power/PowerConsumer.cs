using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
    public float demand = 1f;
    public bool requiresPower = true;
    public PowerNode node;

    public float PowerRatio { get; private set; } = 1f;
    public bool HasPower => !requiresPower || PowerRatio > 0.01f;

    private void Awake()
    {
        EnsureNode();
    }

    public void SetPowerRatio(float ratio)
    {
        PowerRatio = requiresPower ? Mathf.Clamp01(ratio) : 1f;
    }

    private void EnsureNode()
    {
        if (node == null)
        {
            node = GetComponent<PowerNode>();
        }

        if (node == null)
        {
            node = gameObject.AddComponent<PowerNode>();
        }

        node.consumer = this;
    }
}
