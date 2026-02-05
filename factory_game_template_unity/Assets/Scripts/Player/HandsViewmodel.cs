using UnityEngine;

public class HandsViewmodel : MonoBehaviour
{
    public Transform leftHand;
    public Transform rightHand;
    public bool createPlaceholderHands = true;
    public Vector3 leftLocalPos = new Vector3(-0.2f, -0.25f, 0.5f);
    public Vector3 rightLocalPos = new Vector3(0.2f, -0.25f, 0.5f);
    public Vector3 leftLocalRot = new Vector3(0f, 180f, 0f);
    public Vector3 rightLocalRot = new Vector3(0f, 180f, 0f);
    public Vector3 handScale = new Vector3(0.08f, 0.12f, 0.2f);
    public float bobSpeed = 8f;
    public float bobAmount = 0.02f;
    public CharacterController controller;

    private float _bobTime;

    private void Awake()
    {
        if (createPlaceholderHands)
        {
            if (leftHand == null)
            {
                leftHand = CreatePlaceholderHand("LeftHand", leftLocalPos, leftLocalRot);
            }
            if (rightHand == null)
            {
                rightHand = CreatePlaceholderHand("RightHand", rightLocalPos, rightLocalRot);
            }
        }
    }

    private void LateUpdate()
    {
        float speed = controller != null ? controller.velocity.magnitude : 0f;
        float bobFactor = Mathf.Clamp01(speed);
        _bobTime += Time.deltaTime * bobSpeed * bobFactor;
        float bobOffset = Mathf.Sin(_bobTime) * bobAmount;

        if (leftHand != null)
        {
            leftHand.localPosition = leftLocalPos + new Vector3(0f, bobOffset, 0f);
            leftHand.localRotation = Quaternion.Euler(leftLocalRot);
        }

        if (rightHand != null)
        {
            rightHand.localPosition = rightLocalPos + new Vector3(0f, bobOffset, 0f);
            rightHand.localRotation = Quaternion.Euler(rightLocalRot);
        }
    }

    private Transform CreatePlaceholderHand(string name, Vector3 localPos, Vector3 localRot)
    {
        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = name;
        hand.transform.SetParent(transform, false);
        hand.transform.localPosition = localPos;
        hand.transform.localRotation = Quaternion.Euler(localRot);
        hand.transform.localScale = handScale;

        Collider col = hand.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Renderer renderer = hand.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.8f, 0.6f, 1f);
        }

        return hand.transform;
    }
}
