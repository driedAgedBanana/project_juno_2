using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    private Vector2 _moveInput;

    private bool _isAlive = true;
    public Rigidbody rb;

    [Header("Camera Settings")]
    public Transform camHolder;
    public float sensitivity = 15f;
    private Vector2 _lookDirection;
    [Range(0f, 90f)] private float _xClamp = 50f;
    private float _xRotation;

    [Header("Movement")]
    public float moveSpeed = 5f;
    [SerializeField] private bool _isMoving;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(_isAlive)
        {
            HandleLook();
            HandleMovement();
        }
    }

    private void HandleLook()
    {
        float mouseX = _lookDirection.x * sensitivity * Time.deltaTime;
        float mouseY = _lookDirection.y * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        _xRotation = Mathf.Clamp(_xRotation - mouseY, -_xClamp, _xClamp);
        camHolder.localRotation = Quaternion.Slerp(camHolder.localRotation, Quaternion.Euler(_xRotation, 0f, 0f), Time.deltaTime * 10f);
    }

    private void HandleMovement()
    {
        Vector3 movement = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        rb.linearVelocity = new Vector3(movement.x * moveSpeed, rb.linearVelocity.y, movement.z * moveSpeed);
        _isMoving = _moveInput.magnitude > 0.01f;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        _lookDirection = ctx.ReadValue<Vector2>();
    }
}
