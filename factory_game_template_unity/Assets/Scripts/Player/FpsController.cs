using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FpsController : MonoBehaviour
{
    public Transform cameraPivot;
    public float mouseSensitivity = 1.5f;
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    public float maxPitch = 85f;

    private CharacterController _controller;
    private float _pitch;
    private Vector3 _verticalVelocity;

    public Vector3 Velocity => _controller != null ? _controller.velocity : Vector3.zero;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        LockCursor(true);
    }

    private void Update()
    {
        HandleLook();
        HandleMove();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            LockCursor(true);
        }
    }

    private void HandleLook()
    {
        if (cameraPivot == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        if (_controller == null)
        {
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        _controller.Move(move * speed * Time.deltaTime);

        if (_controller.isGrounded && _verticalVelocity.y < 0f)
        {
            _verticalVelocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _verticalVelocity.y += gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);
    }

    private static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
