
using UnityEngine;

public class PhysicsReset : MonoBehaviour
{
    void Start()
    {
        // Physics Layer Collision 강제 업데이트
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Weapon"), LayerMask.NameToLayer("Enemy"), false);
        Debug.Log("Layer Collision Matrix 강제 업데이트 완료");
    }
}
