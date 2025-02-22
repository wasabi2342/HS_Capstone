using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ArrowIndicator : MonoBehaviour
{
    [Header("참조 설정")]
    public Transform playerTransform;

    [Header("위치 및 회전 오프셋")]
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("상호작용 범위 설정")]
    public float interactionRadius = 1.5f;
    public LayerMask npcLayerMask;  // NPC 레이어 체크

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;

    private Renderer arrowRenderer;
    private Transform npcTarget;
    private GameObject currentOutlinedNPC;

    void Start()
    {
        // (1) Arrow Mesh가 자식 오브젝트라면 GetComponentInChildren<Renderer>()를 쓰세요.
        //     만약 ArrowIndicator 오브젝트 자체에 MeshRenderer가 있다면 기존 코드 그대로.
        arrowRenderer = GetComponentInChildren<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("[ArrowIndicator] Renderer가 없습니다. 화살표 Mesh에 Renderer를 추가하세요.");
            return;
        }
        arrowRenderer.enabled = false;

        // (2) 플레이어 Transform이 인스펙터에 없으면, 로컬 플레이어 탐색
        if (playerTransform == null)
        {
            FindLocalPlayerTransform();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // (3) 플레이어 주변 NPC 탐색
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

        // NPC 유무에 따라 Arrow On/Off
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

        // (4) Arrow 위치 & 회전
        transform.position = playerTransform.position + offset;
        Vector3 direction = npcTarget.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            if (Camera.main != null)
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        }

        // (5) Outline 처리
        UpdateNPCOutline();
    }

    void UpdateNPCOutline()
    {
        if (npcTarget != null)
        {
            if (currentOutlinedNPC != npcTarget.gameObject)
            {
                DisableCurrentOutline();

                // (6) NPC에 Outline 컴포넌트가 붙어 있어야 함!
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
            {
                outline.enabled = false;
            }
            currentOutlinedNPC = null;
        }
    }

    private void FindLocalPlayerTransform()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var p in players)
        {
            if (p.photonView != null && p.photonView.IsMine)
            {
                playerTransform = p.transform;
                Debug.Log("[ArrowIndicator] 로컬 플레이어 Transform을 할당했습니다.");
                return;
            }
        }
        Debug.LogWarning("[ArrowIndicator] 로컬 플레이어를 찾지 못했습니다.");
    }
}
