using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public string resourceId = "IronOre";
    public int amount = 1000;

    public void Interact()
    {
        Debug.Log("ResourceNode: " + resourceId + " amount=" + amount);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}
