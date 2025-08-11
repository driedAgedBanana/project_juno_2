using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;

    private Vector2 _moveInput;

    public bool isAlive = true;
    public Rigidbody rb;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public GameObject groundChecker;
    public float groundCheckDistance = 0.2f;

    [Header("Camera Settings")]
    public Transform camHolder;
    public float sensitivity = 15f;
    private Vector2 _lookDirection;
    [SerializeField][Range(0f, 90f)] private float _xClamp = 50f;
    private float _xRotation;
    private Vector3 _originalCamPosition;
    [SerializeField] private Transform _camHolder;

    public Vector2 GetLookInput() => _lookDirection;


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

        //_playerCollider = GetComponent<CapsuleCollider>();
        _originalCamPosition = camHolder.localPosition;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isAlive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isAlive)
        {
            IsGrounded();
        }
    }

    private void FixedUpdate()
    {
        HandleLook();
    }

    private void LateUpdate()
    {
        if (isAlive)
        {
        }
    }

    private void HandleLook()
    {
        float mouseX = _lookDirection.x * sensitivity * Time.deltaTime;
        float mouseY = _lookDirection.y * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        _xRotation = Mathf.Clamp(_xRotation - mouseY, -_xClamp, _xClamp);
        camHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

    }

    public bool IsGrounded()
    {
        return Physics.Raycast(groundChecker.transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        _lookDirection = ctx.ReadValue<Vector2>();
    }
}
