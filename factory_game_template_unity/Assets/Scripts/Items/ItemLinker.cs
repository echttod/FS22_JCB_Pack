using System;
using UnityEngine;

public static class ItemLinker
{
    public static IItemInput FindInput(Transform origin, float distance, LayerMask mask, Transform ignoreRoot)
    {
        if (origin == null)
        {
            return null;
        }

        Ray ray = new Ray(origin.position, origin.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, mask);
        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (ignoreRoot != null && hit.collider.transform.IsChildOf(ignoreRoot))
            {
                continue;
            }

            IItemInput target = hit.collider.GetComponentInParent<IItemInput>();
            if (target != null)
            {
                return target;
            }
        }

        return null;
    }
}
