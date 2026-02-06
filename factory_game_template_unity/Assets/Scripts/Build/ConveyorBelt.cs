using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour, IItemInput, IPlacementAware
{
    [Header("Belt")]
    public float beltLength = 2f;
    public float speed = 1.8f;
    public float itemSpacing = 0.35f;
    public float pointHeight = 0.15f;
    public Transform inputPoint;
    public Transform outputPoint;

    [Header("Linking")]
    public float connectDistance = 1.2f;
    public LayerMask connectMask = ~0;

    private readonly List<MovingItem> _items = new List<MovingItem>();
    private IItemInput _target;

    private class MovingItem
    {
        public ItemStack stack;
        public float progress;
        public GameObject visual;
    }

    private void Awake()
    {
        EnsurePoints();
        RefreshTarget();
    }

    private void Update()
    {
        UpdateItems();
        TryDeliverAtEnd();
    }

    public void OnPlaced()
    {
        RefreshTarget();
    }

    public ConveyorItemState[] GetItemStates()
    {
        ConveyorItemState[] states = new ConveyorItemState[_items.Count];
        for (int i = 0; i < _items.Count; i++)
        {
            states[i] = new ConveyorItemState(_items[i].stack.id, _items[i].progress);
        }
        return states;
    }

    public void RestoreItems(ConveyorItemState[] states)
    {
        ClearItems();
        if (states == null || states.Length == 0)
        {
            return;
        }

        EnsurePoints();
        float length = GetLength();
        Vector3 start = inputPoint.position;
        Vector3 dir = (outputPoint.position - inputPoint.position).normalized;

        List<ConveyorItemState> sorted = new List<ConveyorItemState>(states);
        sorted.Sort((a, b) => a.progress.CompareTo(b.progress));
        foreach (ConveyorItemState state in sorted)
        {
            if (string.IsNullOrEmpty(state.id))
            {
                continue;
            }

            float progress = Mathf.Clamp(state.progress, 0f, length);
            ItemStack stack = new ItemStack(state.id, 1);
            MovingItem item = new MovingItem
            {
                stack = stack,
                progress = progress,
                visual = CreateVisual(stack)
            };

            if (item.visual != null)
            {
                item.visual.transform.position = start + dir * progress;
            }

            _items.Add(item);
        }
    }

    public bool TryInsert(ItemStack stack)
    {
        if (!stack.IsValid)
        {
            return false;
        }

        if (_items.Count > 0 && _items[0].progress < itemSpacing)
        {
            return false;
        }

        ItemStack single = new ItemStack(stack.id, 1);
        MovingItem item = new MovingItem
        {
            stack = single,
            progress = 0f,
            visual = CreateVisual(single)
        };

        _items.Insert(0, item);
        return true;
    }

    private void UpdateItems()
    {
        if (_items.Count == 0)
        {
            return;
        }

        float length = GetLength();
        Vector3 start = inputPoint.position;
        Vector3 dir = (outputPoint.position - inputPoint.position).normalized;

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            MovingItem item = _items[i];
            float maxProgress = length;
            if (i < _items.Count - 1)
            {
                maxProgress = Mathf.Max(0f, _items[i + 1].progress - itemSpacing);
            }

            item.progress = Mathf.Min(item.progress + speed * Time.deltaTime, maxProgress);
            if (item.visual != null)
            {
                item.visual.transform.position = start + dir * item.progress;
            }
        }
    }

    private void TryDeliverAtEnd()
    {
        if (_items.Count == 0)
        {
            return;
        }

        if (_target == null)
        {
            RefreshTarget();
        }

        float length = GetLength();
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            MovingItem item = _items[i];
            if (item.progress < length - 0.01f)
            {
                continue;
            }

            if (_target != null && _target.TryInsert(item.stack))
            {
                if (item.visual != null)
                {
                    Destroy(item.visual);
                }
                _items.RemoveAt(i);
            }
        }
    }

    private void EnsurePoints()
    {
        if (inputPoint == null)
        {
            GameObject inputObj = new GameObject("InputPoint");
            inputObj.transform.SetParent(transform, false);
            inputPoint = inputObj.transform;
        }

        if (outputPoint == null)
        {
            GameObject outputObj = new GameObject("OutputPoint");
            outputObj.transform.SetParent(transform, false);
            outputPoint = outputObj.transform;
        }

        float half = beltLength * 0.5f;
        inputPoint.localPosition = new Vector3(0f, pointHeight, -half);
        outputPoint.localPosition = new Vector3(0f, pointHeight, half);
    }

    private float GetLength()
    {
        return Vector3.Distance(inputPoint.position, outputPoint.position);
    }

    private void RefreshTarget()
    {
        _target = ItemLinker.FindInput(outputPoint, connectDistance, connectMask, transform);
    }

    private void ClearItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].visual != null)
            {
                Destroy(_items[i].visual);
            }
        }
        _items.Clear();
    }

    private GameObject CreateVisual(ItemStack stack)
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Item_" + stack.id;
        visual.transform.localScale = Vector3.one * 0.2f;
        visual.transform.SetParent(transform, false);
        if (inputPoint != null)
        {
            visual.transform.position = inputPoint.position;
        }

        Collider col = visual.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = ItemPalette.GetColor(stack.id);
        }

        return visual;
    }
}
