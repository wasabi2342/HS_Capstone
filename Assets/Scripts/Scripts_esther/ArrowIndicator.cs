using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ArrowIndicator : MonoBehaviour
{
    [Header("참조 설정")]
    // 인스펙터에 할당이 안되어 있으면, 동적으로 로컬 플레이어 참조
    public Transform playerTransform;

    [Header("위치 및 회전 오프셋")]
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("상호작용 범위 설정")]
    public float interactionRadius = 1.5f;
    public LayerMask npcLayerMask;  // 인스펙터에서 NPC 레이어를 지정하세요.

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;

    private Renderer arrowRenderer;
    private Transform npcTarget;
    private GameObject currentOutlinedNPC;

    void Start()
    {
        // Arrow Mesh가 자식 오브젝트라면 GetComponentInChildren<Renderer>() 사용
        arrowRenderer = GetComponentInChildren<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("[ArrowIndicator] Renderer가 없습니다. 화살표 Mesh에 Renderer를 추가하세요.");
            return;
        }
        arrowRenderer.enabled = false;

        // 플레이어 Transform이 할당되지 않았다면 동적으로 찾기
        if (playerTransform == null)
            FindLocalPlayerTransform();
    }

    void Update()
    {
        // 동적으로 로컬 플레이어가 스폰되었는지 확인
        if (playerTransform == null)
            FindLocalPlayerTransform();

        if (playerTransform == null)
            return;

        // 플레이어 주변 NPC 탐색
        Collider[] cols = Physics.OverlapSphere(playerTransform.position, interactionRadius, npcLayerMask);
        Transform closestNPC = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in cols)
        {
            float dist = Vector3.Distance(playerTransform.position, col.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestNPC = col.transform;
            }
        }
        npcTarget = closestNPC;

        // NPC 유무에 따라 화살표 활성화/비활성화
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

        // 화살표 위치 및 회전 (플레이어의 offset 위치에서 NPC를 향하도록)
        transform.position = playerTransform.position + offset;
        Vector3 direction = npcTarget.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
        else if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        // NPC Outline 처리
        UpdateNPCOutline();
    }

    void UpdateNPCOutline()
    {
        if (npcTarget != null)
        {
            if (currentOutlinedNPC != npcTarget.gameObject)
            {
                DisableCurrentOutline();
                // NPC에 Outline 컴포넌트가 붙어 있어야 함!
                Outline outline = npcTarget.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineColor = highlightColor;
                    outline.enabled = true;
                    currentOutlinedNPC = npcTarget.gameObject;
                }
                else
                {
                    Debug.LogWarning($"[ArrowIndicator] NPC '{npcTarget.name}'에 Outline 컴포넌트가 없습니다.");
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
                outline.enabled = false;
            currentOutlinedNPC = null;
        }
    }

    private void FindLocalPlayerTransform()
    {
        // 동적으로 PlayerController.localPlayer를 통해 로컬 플레이어 Transform을 할당
        if (PlayerController.localPlayer != null)
        {
            playerTransform = PlayerController.localPlayer.transform;
            Debug.Log("[ArrowIndicator] 로컬 플레이어 Transform을 할당했습니다.");
        }
        else
        {
            // 만약 아직 스폰되지 않았다면, FindObjectsOfType로 시도
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (var p in players)
            {
                if (p.photonView != null && p.photonView.IsMine)
                {
                    playerTransform = p.transform;
                    Debug.Log("[ArrowIndicator] 로컬 플레이어 Transform을 할당했습니다 (FindObjectsOfType).");
                    return;
                }
            }
            Debug.LogWarning("[ArrowIndicator] 로컬 플레이어를 찾지 못했습니다.");
        }
    }
}
