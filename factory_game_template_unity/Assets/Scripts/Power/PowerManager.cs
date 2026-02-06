using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public float refreshInterval = 0.5f;

    public float TotalSupply { get; private set; }
    public float TotalDemand { get; private set; }
    public float PowerRatio { get; private set; } = 1f;

    private float _timer;

    private void Awake()
    {
        Recalculate();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            Recalculate();
        }
    }

    public void Recalculate()
    {
        PowerProducer[] producers = FindObjectsOfType<PowerProducer>();
        PowerConsumer[] consumers = FindObjectsOfType<PowerConsumer>();

        float supply = 0f;
        foreach (PowerProducer producer in producers)
        {
            if (producer != null)
            {
                supply += producer.CurrentOutput;
            }
        }

        float demand = 0f;
        foreach (PowerConsumer consumer in consumers)
        {
            if (consumer != null && consumer.requiresPower)
            {
                demand += Mathf.Max(0f, consumer.demand);
            }
        }

        TotalSupply = supply;
        TotalDemand = demand;
        PowerRatio = demand <= 0.01f ? 1f : Mathf.Clamp01(supply / demand);

        foreach (PowerConsumer consumer in consumers)
        {
            if (consumer != null)
            {
                consumer.SetPowerRatio(PowerRatio);
            }
        }
    }
}
