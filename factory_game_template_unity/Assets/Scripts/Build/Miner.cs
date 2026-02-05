using UnityEngine;

public class Miner : MonoBehaviour, IPlacementAware
{
    [Header("Production")]
    public float productionInterval = 1.5f;
    public int outputCapacity = 20;
    public float nodeSearchRadius = 1.2f;

    [Header("Output")]
    public float outputInterval = 0.25f;
    public Transform outputPoint;
    public float connectDistance = 1.4f;
    public LayerMask connectMask = ~0;

    private ItemInventory _buffer;
    private ResourceNode _node;
    private float _productionTimer;
    private float _outputTimer;
    private IItemInput _target;

    private void Awake()
    {
        _buffer = new ItemInventory(outputCapacity);
        EnsureOutputPoint();
        FindNode();
        RefreshTarget();
    }

    private void Update()
    {
        Produce();
        PushOutput();
    }

    public void OnPlaced()
    {
        FindNode();
        RefreshTarget();
    }

    private void Produce()
    {
        if (_node == null || _node.amount <= 0)
        {
            return;
        }

        if (_buffer.Count >= _buffer.Capacity)
        {
            return;
        }

        _productionTimer += Time.deltaTime;
        if (_productionTimer < productionInterval)
        {
            return;
        }

        _productionTimer = 0f;
        if (_node.TryExtract(1, out string id))
        {
            _buffer.Add(id, 1);
        }
    }

    private void PushOutput()
    {
        if (!_buffer.HasAny())
        {
            return;
        }

        _outputTimer += Time.deltaTime;
        if (_outputTimer < outputInterval)
        {
            return;
        }

        _outputTimer = 0f;
        if (_target == null)
        {
            RefreshTarget();
        }

        if (_target == null)
        {
            return;
        }

        if (_buffer.TryRemoveAny(out ItemStack stack))
        {
            if (!_target.TryInsert(stack))
            {
                _buffer.Add(stack.id, stack.amount);
            }
        }
    }

    private void FindNode()
    {
        _node = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, nodeSearchRadius);
        foreach (Collider hit in hits)
        {
            ResourceNode node = hit.GetComponentInParent<ResourceNode>();
            if (node != null)
            {
                _node = node;
                return;
            }
        }
    }

    private void EnsureOutputPoint()
    {
        if (outputPoint != null)
        {
            return;
        }

        GameObject outputObj = new GameObject("OutputPoint");
        outputObj.transform.SetParent(transform, false);
        outputObj.transform.localPosition = new Vector3(0f, 0.4f, 0.9f);
        outputPoint = outputObj.transform;
    }

    private void RefreshTarget()
    {
        _target = ItemLinker.FindInput(outputPoint, connectDistance, connectMask, transform);
    }
}
