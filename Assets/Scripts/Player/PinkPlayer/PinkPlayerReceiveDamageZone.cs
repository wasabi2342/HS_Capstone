using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PinkPlayerReceiveDamageZone : MonoBehaviour
{


    private PinkPlayerController pinkPlayerController;


    private void Awake()
    {
        // SphereCollider를 자동으로 Trigger로 설정
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // 상위에 있는 WhitePlayerController를 찾음
        pinkPlayerController = GetComponentInParent<PinkPlayerController>();
        if (pinkPlayerController == null)
        {
            
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PinkPlayerReceiveDamageZone] 충돌 발생: {other.name}, 레이어: {LayerMask.LayerToName(other.gameObject.layer)}");
    }
}
