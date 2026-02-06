using UnityEngine;

public class PowerProducer : MonoBehaviour
{
    public float output = 8f;
    public bool isOnline = true;
    public PowerNode node;

    public float CurrentOutput => isOnline ? Mathf.Max(0f, output) : 0f;

    private void Awake()
    {
        EnsureNode();
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

        node.producer = this;
    }
}
