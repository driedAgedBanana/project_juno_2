using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponIKHandler : MonoBehaviour
{
    public RigBuilder rigBuilder;

    [Header("IK Constraints (assign in Inspector")]
    public TwoBoneIKConstraint rightHandIK;
    public TwoBoneIKConstraint leftHandIK;

    [Header("Default (unarmed) anchors")]
    public Transform defaultRightGrip;
    public Transform defaultLeftGrip;

    [Header("Blend Settings")]
    public float timer = 0.12f;

    private Coroutine _switchCoroutine;

    public void EquippingWeapon(WeaponGrip weapon)
    {
        Transform newRight = weapon != null && weapon.rightHandGrip != null ? weapon.rightHandGrip : defaultRightGrip;
        Transform newLeft = weapon != null && weapon.leftHandGrip != null ? weapon.leftHandGrip : defaultLeftGrip;

        Debug.Log($"Equipping Weapon: RightGrip = {(newRight != null ? newRight.name : "null")}, LeftGrip = {(newLeft != null ? newLeft.name : "null")}");

        if (_switchCoroutine != null) StopCoroutine(_switchCoroutine);
        _switchCoroutine = StartCoroutine(SwitchIKTarget(newRight, newLeft, timer));

        rightHandIK.data.target = newRight;
        leftHandIK.data.target = newLeft;
        rigBuilder.Build();
    }

    private IEnumerator SwitchIKTarget(Transform newRight, Transform newLeft, float duration)
    {
        // Fade the IK weight to zero
        float timeElapsed = 0;

        float startR = rightHandIK.weight;
        float startL = leftHandIK.weight;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float p = timeElapsed / duration;
            rightHandIK.weight = Mathf.Lerp(startR, 0f, p);
            leftHandIK.weight = Mathf.Lerp(startL, 0f, p);
            yield return null;
        }

        // Assign new targets while IK is effectively off to avoid snapping
        rightHandIK.data.target = newRight;
        leftHandIK.data.target = newLeft;

        // Fade IK back in
        timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float p = timeElapsed / duration;
            rightHandIK.weight = Mathf.Lerp(0f, 1f, p);
            leftHandIK.weight = Mathf.Lerp(0f, 1f, p);
            yield return null;
        }

        rightHandIK.weight = 1f;
        leftHandIK.weight = 1f;
        _switchCoroutine = null;
    }
}
