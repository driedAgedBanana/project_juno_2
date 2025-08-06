using System.Runtime.CompilerServices;
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

    [Header("Movement Mechanic")]
    public float moveSpeed = 5f;
    public float slowWalkSpeed = 2f;
    public float runSpeed = 10f;

    // Stamina for running
    public float currentStamina;
    public float maxStamina;
    public float staminaDrainRate;
    public float staminaGainRate;
    public float staminaFastGainRate;
    public float coolDownTime;

    private float _coolDownTimer;

    private bool _isSlowWalk;
    private bool _isRunning;
    private bool _runButtonPressed;
    private bool _isMoving;

    [Header("Crouching Mechanic (in development, not working right now)")]
    public bool isCrouching;
    public float crouchSpeed = 2f;

    [Header("Leaning Mechanic")]
    public float leaningAmount = 20f;
    public float leaningSpeed = 15f;

    private bool _canLean;
    private Quaternion _targetLeanRotation;
    private float _leaningDirection;


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
        _isAlive = true;
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAlive)
        {
            HandleLeaning();
            RegenerateStamina();
        }
    }

    private void FixedUpdate()
    {
        if (_isAlive)
        {
            HandleMovement();
        }
    }

    private void LateUpdate()
    {
        if (_isAlive)
        {
            HandleLook();
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

    private void HandleMovement()
    {
        bool canRun = _runButtonPressed && _isMoving && !isCrouching && currentStamina > 0;
        float speed = isCrouching ? crouchSpeed : (canRun ? runSpeed : (_isSlowWalk ? slowWalkSpeed : moveSpeed));

        Vector3 movement = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        rb.linearVelocity = new Vector3(movement.x * speed, rb.linearVelocity.y, movement.z * speed);
        _isMoving = _moveInput.magnitude > 0.01f;

        if (canRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            _coolDownTimer = 0f;
            _isRunning = true;
            Debug.Log("Running!");
        }
        else
        {
            _isRunning = false;
            Debug.Log("Or not...");
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    private void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            if (currentStamina == 0)
            {
                _coolDownTimer += Time.deltaTime;
                if (_coolDownTimer < coolDownTime) return;
            }

            float regenerateStaminaRate = _isMoving ? staminaGainRate : staminaFastGainRate;

            currentStamina += regenerateStaminaRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
    }

    private void HandleLeaning()
    {
        _canLean = true;
        if (_leaningDirection > 0) // lean right
        {
            _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, -leaningAmount);
        }
        else if (_leaningDirection < 0) // lean left
        {
            _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, leaningAmount);
        }
        else
        {
            _targetLeanRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
        }

        // smoothly transition to the target lean rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, _targetLeanRotation, Time.deltaTime * leaningSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        _lookDirection = ctx.ReadValue<Vector2>();
    }

    public void OnLean(InputAction.CallbackContext ctx)
    {
        float leaningInput = ctx.ReadValue<float>();
        _leaningDirection = leaningInput;
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _runButtonPressed = true;
        }
        if (ctx.canceled)
        {
            _runButtonPressed = false;
        }
    }

    public void OnSlowWalk(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            _isSlowWalk = true;
        }
        if (ctx.canceled)
        {
            _isSlowWalk = false;
        }
    }
}
