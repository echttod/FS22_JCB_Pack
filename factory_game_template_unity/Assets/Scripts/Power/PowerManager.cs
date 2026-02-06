using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public float refreshInterval = 0.5f;
    public float connectionDistance = 6f;
    public bool drawConnections = true;

    public float TotalSupply { get; private set; }
    public float TotalDemand { get; private set; }
    public float PowerRatio { get; private set; } = 1f;
    public int NetworkCount { get; private set; }

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
        PowerNode[] nodes = FindObjectsOfType<PowerNode>();
        TotalSupply = 0f;
        TotalDemand = 0f;
        NetworkCount = 0;

        if (nodes == null || nodes.Length == 0)
        {
            PowerRatio = 1f;
            return;
        }

        bool[] visited = new bool[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            if (visited[i] || nodes[i] == null)
            {
                continue;
            }

            NetworkCount++;
            float supply = 0f;
            float demand = 0f;
            var indices = new System.Collections.Generic.List<int> { i };

            for (int idx = 0; idx < indices.Count; idx++)
            {
                int nodeIndex = indices[idx];
                if (visited[nodeIndex])
                {
                    continue;
                }

                PowerNode node = nodes[nodeIndex];
                if (node == null)
                {
                    continue;
                }

                visited[nodeIndex] = true;
                supply += node.Supply;
                demand += node.Demand;

                for (int j = 0; j < nodes.Length; j++)
                {
                    if (visited[j] || nodes[j] == null)
                    {
                        continue;
                    }

                    if (IsConnected(node, nodes[j]))
                    {
                        indices.Add(j);
                    }
                }
            }

            float ratio = demand <= 0.01f ? 1f : Mathf.Clamp01(supply / demand);
            foreach (int nodeIndex in indices)
            {
                PowerNode node = nodes[nodeIndex];
                if (node != null && node.consumer != null)
                {
                    node.consumer.SetPowerRatio(ratio);
                }
            }

            TotalSupply += supply;
            TotalDemand += demand;
        }

        PowerRatio = TotalDemand <= 0.01f ? 1f : Mathf.Clamp01(TotalSupply / TotalDemand);
    }

    private bool IsConnected(PowerNode a, PowerNode b)
    {
        float maxDistance = connectionDistance;
        if (a != null && b != null)
        {
            float radius = (a.linkRadius + b.linkRadius) * 0.5f;
            maxDistance = Mathf.Max(maxDistance, radius);
        }

        return Vector3.Distance(a.Position, b.Position) <= maxDistance;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawConnections)
        {
            return;
        }

        PowerNode[] nodes = FindObjectsOfType<PowerNode>();
        if (nodes == null)
        {
            return;
        }

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.4f);
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null)
            {
                continue;
            }

            for (int j = i + 1; j < nodes.Length; j++)
            {
                if (nodes[j] == null)
                {
                    continue;
                }

                if (IsConnected(nodes[i], nodes[j]))
                {
                    Gizmos.DrawLine(nodes[i].Position, nodes[j].Position);
                }
            }
        }
    }
}
