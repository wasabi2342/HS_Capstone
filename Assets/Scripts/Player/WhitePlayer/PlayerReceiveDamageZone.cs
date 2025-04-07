using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerReceiveDamageZone :  MonoBehaviour
{
   

    private WhitePlayerController whitePlayerController;
   

    private void Awake()
    {
        // SphereCollider를 자동으로 Trigger로 설정
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 상위에 있는 WhitePlayerController를 찾음
        whitePlayerController = GetComponentInParent<WhitePlayerController>();
        if (whitePlayerController == null)
        {
            Debug.LogWarning("[WhitePlayerReceiveDamageZone] 상위에 WhitePlayerController가 없습니다!");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[WhitePlayerReceiveDamageZone] 충돌 발생: {other.name}, 레이어: {LayerMask.LayerToName(other.gameObject.layer)}");
    }
}
