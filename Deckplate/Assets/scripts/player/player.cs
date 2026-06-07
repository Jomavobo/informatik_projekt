using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;

    private Rigidbody rb;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Nur für den eigenen Spieler die Kamera aktivieren
        if (!isLocalPlayer)
        {
            // Fremde Spieler: Kamera deaktivieren, fertig
            GetComponentInChildren<Camera>().enabled = false;
            return;
        }

        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Escape togglet die Maus
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        HandleMouseLook();
        HandleJump();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        HandleMovement();
    }
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Spieler horizontal drehen
        transform.Rotate(Vector3.up * mouseX);

        // Kamera vertikal drehen (begrenzt auf -90 bis +90 Grad)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = sprinting ? sprintSpeed : moveSpeed;

        Vector3 move = (transform.right * h + transform.forward * v).normalized * speed;
        move.y = rb.linearVelocity.y; // Schwerkraft beibehalten
        rb.linearVelocity = move;
    }

    void HandleJump()
    {
        // Einfacher Grounded-Check via Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}