using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
    public float demand = 1f;
    public bool requiresPower = true;

    public float PowerRatio { get; private set; } = 1f;
    public bool HasPower => !requiresPower || PowerRatio > 0.01f;

    public void SetPowerRatio(float ratio)
    {
        PowerRatio = requiresPower ? Mathf.Clamp01(ratio) : 1f;
    }
}
