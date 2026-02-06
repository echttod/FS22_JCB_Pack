using UnityEngine;

public class ResourceNode : MonoBehaviour, IInteractable
{
    public string resourceId = "IronOre";
    public int amount = 1000;

    public int Amount => amount;

    public void Interact()
    {
        Debug.Log("ResourceNode: " + resourceId + " amount=" + amount);
    }

    public bool TryExtract(int requestedAmount, out string id)
    {
        id = resourceId;
        if (amount <= 0 || requestedAmount <= 0)
        {
            return false;
        }

        int taken = Mathf.Min(requestedAmount, amount);
        amount -= taken;
        return taken > 0;
    }

    public void SetAmount(int value)
    {
        amount = Mathf.Max(0, value);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}
