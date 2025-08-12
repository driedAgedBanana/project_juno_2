using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchingWeapon : MonoBehaviour
{
    public WeaponSwayAndBob swayAndBob;
    public PlayerWeaponIKHandler playerWeaponIK;

    [Header("Settings")]
    public Transform defaultPosition;
    public Transform switchingWeaponPosition;

    [Header("Switching Properties")]
    [SerializeField] private float _switchingTime = 0.5f;
    private bool _isSwitching = false;

    private Transform[] _weapons;
    private int _currentWeaponIndex = 0;
    private float _timeSinceLastSwitch = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        SetWeapons();
        Select(_currentWeaponIndex);
    }

    // Update is called once per frame
    void Update()
    {
        _timeSinceLastSwitch += Time.deltaTime;
    }

    private void SetWeapons()
    {
        _weapons = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _weapons[i] = transform.GetChild(i);
        }
    }

    // Switching weapon by mouse wheel
    public void OnSwitchingWeaponWithMouseWheel(InputAction.CallbackContext ctx)
    {
        if (_timeSinceLastSwitch < _switchingTime) return; // preventing too fast switching
        Vector2 scrollValue = ctx.ReadValue<Vector2>();

        if (scrollValue.y > 0.1f)
        {
            NextWeapon();
        }
        else if (scrollValue.y < -0.1f)
        {
            PreviousWeapon();
        }
    }

    //// Called by Input System for number keys
    //public void OnSelectWeaponByNumber(InputAction.CallbackContext ctx)
    //{
    //    if (!ctx.performed) return;
    //    if (_timeSinceLastSwitch < switchTime) return;

    //    // Assuming you bind keys 1, 2, 3... to send the index as float
    //    float numberPressed = ctx.ReadValue<float>(); // e.g., pressing "1" gives 1
    //    int weaponIndex = Mathf.RoundToInt(numberPressed) - 1;

    //    if (weaponIndex >= 0 && weaponIndex < _weapons.Length)
    //    {
    //        Select(weaponIndex);
    //    }
    //}

    private void NextWeapon()
    {
        if (_isSwitching) return;
        int nextIndex = (_currentWeaponIndex + 1) % _weapons.Length;
        StartCoroutine(SwitchingWeaponRoutine(nextIndex));
    }

    private void PreviousWeapon()
    {
        if (_isSwitching) return;
        int prevIndex = (_currentWeaponIndex - 1 + _weapons.Length) % _weapons.Length;
        StartCoroutine(SwitchingWeaponRoutine(prevIndex));
    }

    private void Select(int weaponIndex)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            _weapons[i].gameObject.SetActive(i == weaponIndex);
        }

        _timeSinceLastSwitch = 0;
        _currentWeaponIndex = weaponIndex;

        WeaponGrip weaponGrip = _weapons[weaponIndex].GetComponent<WeaponGrip>();
        if(weaponGrip != null)
        {
            playerWeaponIK.EquippingWeapon(weaponGrip);
        }

        EquippingWeapon(weaponIndex);

    }

    public void EquippingWeapon(int index)
    {
        IWeapon weapon = _weapons[index].GetComponent<IWeapon>();
        if (weapon != null)
        {
            swayAndBob.SetCurrentWeapon(weapon);
        }
    }

    private IEnumerator SwitchingWeaponRoutine(int newIndex)
    {
        _isSwitching = true;
        _timeSinceLastSwitch = 0f;

        Transform currentWeapon = _weapons[_currentWeaponIndex];
        Transform nextWeapon = _weapons[newIndex];

        // Lower the current weapon to switching to new weapon
        yield return StartCoroutine(MoveWeapon(currentWeapon, switchingWeaponPosition.position));

        // Disable the current weapon
        currentWeapon.gameObject.SetActive(false);
        nextWeapon.gameObject.SetActive(true);

        // Update the sway/IK after enabling
        WeaponGrip weaponGrip = nextWeapon.GetComponent<WeaponGrip>();
        if(weaponGrip != null)
        {
            playerWeaponIK.EquippingWeapon(weaponGrip);
        }
        EquippingWeapon(newIndex);

        _currentWeaponIndex = newIndex;

        // Raise the new weapon from the switching position
        nextWeapon.position = switchingWeaponPosition.position;
        yield return StartCoroutine(MoveWeapon(nextWeapon, defaultPosition.position));

        _isSwitching = false;
    }

    private IEnumerator MoveWeapon(Transform weapon, Vector3 targetPosition)
    {
        float elapsedTime = 0f; // Timer for lerping animation
        Vector3 startPosition = weapon.position; // Store initial position

        while (elapsedTime < _switchingTime)
        {
            float progress = Mathf.Clamp01(elapsedTime / _switchingTime); // Normalize progress
            weapon.position = Vector3.Slerp(startPosition, targetPosition, progress); // Smoothly move weapon
            elapsedTime += Time.deltaTime; // Increase elapsed time
            yield return null; // Wait for the next frame
        }

        weapon.position = targetPosition; // Ensure the weapon reaches its final position
    }
}
