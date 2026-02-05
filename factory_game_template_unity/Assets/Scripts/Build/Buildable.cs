using UnityEngine;

public class Buildable : MonoBehaviour
{
    [Header("Placement")]
    public Vector3 footprint = new Vector3(1f, 1f, 1f);
    public bool requiresFlatGround = true;

    public Bounds GetWorldBounds()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        return new Bounds(transform.position, footprint);
    }
}
