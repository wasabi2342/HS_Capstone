using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerInteractionZone : MonoBehaviour
{
    [Header("상호작용 범위 설정")]
    [Tooltip("이 영역의 반지름이 NPC, Trap 등과 상호작용할 수 있는 범위입니다.")]
    public float interactionRange = 1.5f;

    // 범위 내에 있는 상호작용 가능한 오브젝트 목록 (NPC, Trap 등)
    public List<GameObject> interactables = new List<GameObject>();

    private void Awake()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = interactionRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // NPC와 Trap 레이어만 추가
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC") ||
            other.gameObject.layer == LayerMask.NameToLayer("Trap"))
        {
            if (!interactables.Contains(other.gameObject))
            {
                interactables.Add(other.gameObject);
                Debug.Log("[WhitePlayerInteractionZone] 상호작용 대상 추가: " + other.gameObject.name);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC") ||
            other.gameObject.layer == LayerMask.NameToLayer("Trap"))
        {
            if (interactables.Contains(other.gameObject))
            {
                interactables.Remove(other.gameObject);
                Debug.Log("[WhitePlayerInteractionZone] 상호작용 대상 제거: " + other.gameObject.name);
            }
        }
    }
}
