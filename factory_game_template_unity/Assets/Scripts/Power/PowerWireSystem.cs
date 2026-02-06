using System.Collections.Generic;
using UnityEngine;

public class PowerWireSystem : MonoBehaviour
{
    public PowerManager powerManager;
    public float refreshInterval = 0.5f;
    public float lineWidth = 0.03f;
    public Color lineColor = new Color(0.2f, 0.9f, 1f, 0.8f);
    public bool drawWires = true;

    private readonly List<LineRenderer> _lines = new List<LineRenderer>();
    private Material _lineMaterial;
    private float _timer;

    private void Awake()
    {
        if (powerManager == null)
        {
            powerManager = FindObjectOfType<PowerManager>();
        }
        EnsureMaterial();
        RebuildLines();
    }

    private void Update()
    {
        if (!drawWires)
        {
            SetLineCount(0);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            RebuildLines();
        }
    }

    private void RebuildLines()
    {
        PowerNode[] nodes = FindObjectsOfType<PowerNode>();
        if (nodes == null || nodes.Length == 0)
        {
            SetLineCount(0);
            return;
        }

        EnsureMaterial();
        HashSet<long> pairs = new HashSet<long>();
        List<Vector3[]> connections = new List<Vector3[]>();

        for (int i = 0; i < nodes.Length; i++)
        {
            PowerNode node = nodes[i];
            if (node == null)
            {
                continue;
            }

            PowerNode target = FindPreferredConnection(node, nodes);
            if (target == null)
            {
                continue;
            }

            long key = GetPairKey(node.GetInstanceID(), target.GetInstanceID());
            if (pairs.Add(key))
            {
                connections.Add(new[] { node.Position, target.Position });
            }
        }

        SetLineCount(connections.Count);
        for (int i = 0; i < connections.Count; i++)
        {
            LineRenderer line = _lines[i];
            Vector3[] points = connections[i];
            line.positionCount = 2;
            line.SetPosition(0, points[0]);
            line.SetPosition(1, points[1]);
        }
    }

    private PowerNode FindPreferredConnection(PowerNode node, PowerNode[] nodes)
    {
        PowerNode best = null;
        float bestDist = float.MaxValue;
        bool bestIsPole = false;

        for (int i = 0; i < nodes.Length; i++)
        {
            PowerNode other = nodes[i];
            if (other == null || other == node)
            {
                continue;
            }

            if (!IsConnected(node, other))
            {
                continue;
            }

            float dist = (other.Position - node.Position).sqrMagnitude;
            bool isPole = other.isPole;
            if (best == null || (isPole && !bestIsPole) || (isPole == bestIsPole && dist < bestDist))
            {
                best = other;
                bestDist = dist;
                bestIsPole = isPole;
            }
        }

        return best;
    }

    private bool IsConnected(PowerNode a, PowerNode b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        float baseDistance = powerManager != null ? powerManager.connectionDistance : 6f;
        float radius = (a.linkRadius + b.linkRadius) * 0.5f;
        float maxDistance = Mathf.Max(baseDistance, radius);
        return Vector3.Distance(a.Position, b.Position) <= maxDistance;
    }

    private void SetLineCount(int count)
    {
        for (int i = _lines.Count; i < count; i++)
        {
            _lines.Add(CreateLineRenderer(i));
        }

        for (int i = 0; i < _lines.Count; i++)
        {
            _lines[i].gameObject.SetActive(i < count);
        }
    }

    private LineRenderer CreateLineRenderer(int index)
    {
        GameObject obj = new GameObject("PowerWire_" + index);
        obj.transform.SetParent(transform, false);
        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.material = _lineMaterial;
        line.widthMultiplier = lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.startColor = lineColor;
        line.endColor = lineColor;
        return line;
    }

    private void EnsureMaterial()
    {
        if (_lineMaterial != null)
        {
            return;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        _lineMaterial = new Material(shader);
        _lineMaterial.color = lineColor;
    }

    private static long GetPairKey(int a, int b)
    {
        int min = Mathf.Min(a, b);
        int max = Mathf.Max(a, b);
        return ((long)min << 32) | (uint)max;
    }
}
