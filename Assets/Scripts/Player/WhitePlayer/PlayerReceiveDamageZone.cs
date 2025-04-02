using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerReceiveDamageZone :  MonoBehaviour
{
   

    private WhitePlayerController whitePlayerController;
   

    private void Awake()
    {
        // SphereCollider�� �ڵ����� Trigger�� ����
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // ������ �ִ� WhitePlayerController�� ã��
        whitePlayerController = GetComponentInParent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogWarning("[WhitePlayerReceiveDamageZone] ������ WhitePlayerController�� �����ϴ�!");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[WhitePlayerReceiveDamageZone] �浹 �߻�: {other.name}, ���̾�: {LayerMask.LayerToName(other.gameObject.layer)}");
    }
}
