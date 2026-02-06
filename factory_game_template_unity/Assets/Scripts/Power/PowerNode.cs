using UnityEngine;

public class PowerNode : MonoBehaviour
{
    public float linkRadius = 6f;
    public bool isPole = false;

    [HideInInspector] public PowerProducer producer;
    [HideInInspector] public PowerConsumer consumer;

    public Vector3 Position => transform.position;

    public float Supply => producer != null ? producer.CurrentOutput : 0f;
    public float Demand => consumer != null && consumer.requiresPower ? Mathf.Max(0f, consumer.demand) : 0f;
}
