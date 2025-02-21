using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("참조 설정")]
   
    public Transform playerTransform;

    [Header("위치 및 회전 오프셋")]
    
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("상호작용 범위 설정")]
   
    public float interactionRadius = 1.5f;
    
    public LayerMask npcLayerMask;

    [Header("하이라이트 설정")]
    
    public Color highlightColor = Color.yellow;

    
    private Transform npcTarget;
    
    private Renderer arrowRenderer;
    
    private GameObject currentOutlinedNPC;

    void Awake()
    {
        arrowRenderer = GetComponent<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("ArrowIndicator 스크립트가 붙은 오브젝트에 Renderer 컴포넌트가 필요합니다.");
        }
        // 시작 시 화살표를 숨김 (렌더러 비활성화)
        arrowRenderer.enabled = false;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // 플레이어 주변의 NPC 탐색 (OverlapSphere 사용)
        Collider[] cols = Physics.OverlapSphere(playerTransform.position, interactionRadius, npcLayerMask);
        Transform closestNPC = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in cols)
        {
            // NPC 레이어에 해당하는지 확인
            if (col.gameObject.layer == LayerMask.NameToLayer("NPC"))
            {
                float dist = Vector3.Distance(playerTransform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestNPC = col.transform;
                }
            }
        }
        npcTarget = closestNPC;

        // NPC가 감지되면 화살표를 보이게, 없으면 숨김
        if (npcTarget != null)
        {
            if (!arrowRenderer.enabled)
                arrowRenderer.enabled = true;
        }
        else
        {
            if (arrowRenderer.enabled)
                arrowRenderer.enabled = false;
            DisableCurrentOutline();
            return;
        }

        // 플레이어 위치에 오프셋 적용하여 화살표 위치 업데이트
        transform.position = playerTransform.position + offset;

        // NPC를 향하는 방향 계산 및 회전 적용
        Vector3 direction = npcTarget.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        // NPC Outline
        UpdateNPCOutline();
    }

    void UpdateNPCOutline()
    {
        if (npcTarget != null)
        {
            // 만약 현재 하이라이트된 NPC가 새 타겟과 다르다면 전환
            if (currentOutlinedNPC != npcTarget.gameObject)
            {
                // 이전에 하이라이트된 NPC가 있다면 비활성화
                DisableCurrentOutline();

                // 새 타겟의 Outline 컴포넌트 활성화
                Outline outline = npcTarget.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineColor = highlightColor;
                    outline.enabled = true;
                    currentOutlinedNPC = npcTarget.gameObject;
                }
                else
                {
                    Debug.LogWarning("NPC 오브젝트에 Outline 컴포넌트가 없습니다: " + npcTarget.gameObject.name);
                }
            }
        }
        else
        {
            DisableCurrentOutline();
        }
    }

    void DisableCurrentOutline()
    {
        if (currentOutlinedNPC != null)
        {
            Outline outline = currentOutlinedNPC.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
            currentOutlinedNPC = null;
        }
    }
}
