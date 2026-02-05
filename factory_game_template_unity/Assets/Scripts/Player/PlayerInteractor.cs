using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 2.5f;
    public KeyCode interactKey = KeyCode.F;
    public LayerMask interactMask = ~0;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            ResourceNode node = hit.collider.GetComponentInParent<ResourceNode>();
            if (node != null)
            {
                node.Interact();
            }
        }
    }
}
