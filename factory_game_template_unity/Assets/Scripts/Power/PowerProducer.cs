using UnityEngine;

public class PowerProducer : MonoBehaviour
{
    public float output = 8f;
    public bool isOnline = true;

    public float CurrentOutput => isOnline ? Mathf.Max(0f, output) : 0f;
}
