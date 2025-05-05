using UnityEngine;

public class VisualRotationLock : MonoBehaviour
{
    readonly Quaternion zero = Quaternion.identity;
    void LateUpdate() => transform.localRotation = zero;
}
