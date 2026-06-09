using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed       = 5f;
    public float sprintSpeed     = 9f;
    public float jumpForce       = 5f;
    public float mouseSensitivity = 2f;

    private Rigidbody rb;
    private Camera    playerCamera;
    private float     verticalRotation = 0f;
    private bool      isGrounded;
    private bool      cursorLocked = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (!isLocalPlayer)
        {
            GetComponentInChildren<Camera>()?.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        playerCamera = GetComponentInChildren<Camera>();
        LockCursor(true);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleCursorToggle();
        HandleMouseLook();
        HandleJump();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        HandleMovement();
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            LockCursor(!cursorLocked);
    }

    void HandleMouseLook()
    {
        if (!cursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation  = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool  sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed     = sprinting ? sprintSpeed : moveSpeed;

        Vector3 move = (transform.right * h + transform.forward * v).normalized * speed;
        move.y = rb.linearVelocity.y;
        rb.linearVelocity = move;
    }

    void HandleJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void LockCursor(bool locked)
    {
        cursorLocked          = locked;
        Cursor.lockState      = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible        = !locked;
    }
}