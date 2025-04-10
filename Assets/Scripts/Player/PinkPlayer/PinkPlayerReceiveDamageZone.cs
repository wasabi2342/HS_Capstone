using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PinkPlayerReceiveDamageZone : MonoBehaviour
{


    private PinkPlayerController pinkPlayerController;


    private void Awake()
    {
        // SphereCollider�� �ڵ����� Trigger�� ����
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // ������ �ִ� WhitePlayerController�� ã��
        pinkPlayerController = GetComponentInParent<PinkPlayerController>();
        if (pinkPlayerController == null)
        {
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PinkPlayerReceiveDamageZone] �浹 �߻�: {other.name}, ���̾�: {LayerMask.LayerToName(other.gameObject.layer)}");
    }
}
