
using UnityEngine;

public class PhysicsReset : MonoBehaviour
{
    void Start()
    {
        // Physics Layer Collision ���� ������Ʈ
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Weapon"), LayerMask.NameToLayer("Enemy"), false);
        Debug.Log("Layer Collision Matrix ���� ������Ʈ �Ϸ�");
    }
}
