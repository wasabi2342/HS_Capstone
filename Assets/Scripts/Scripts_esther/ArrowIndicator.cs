using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ArrowIndicator : MonoBehaviour
{
    [Header("���� ����")]
    // �ν����Ϳ� �Ҵ��� �ȵǾ� ������, �������� ���� �÷��̾� ����
    public Transform playerTransform;

    [Header("��ġ �� ȸ�� ������")]
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("��ȣ�ۿ� ���� ����")]
    public float interactionRadius = 1.5f;
    public LayerMask npcLayerMask;  // �ν����Ϳ��� NPC ���̾ �����ϼ���.

    [Header("���̶���Ʈ ����")]
    public Color highlightColor = Color.yellow;

    private Renderer arrowRenderer;
    private Transform npcTarget;
    private GameObject currentOutlinedNPC;

    void Start()
    {
        // Arrow Mesh�� �ڽ� ������Ʈ��� GetComponentInChildren<Renderer>() ���
        arrowRenderer = GetComponentInChildren<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("[ArrowIndicator] Renderer�� �����ϴ�. ȭ��ǥ Mesh�� Renderer�� �߰��ϼ���.");
            return;
        }
        arrowRenderer.enabled = false;

        // �÷��̾� Transform�� �Ҵ���� �ʾҴٸ� �������� ã��
        if (playerTransform == null)
            FindLocalPlayerTransform();
    }

    void Update()
    {
        // �������� ���� �÷��̾ �����Ǿ����� Ȯ��
        if (playerTransform == null)
            FindLocalPlayerTransform();

        if (playerTransform == null)
            return;

        // �÷��̾� �ֺ� NPC Ž��
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

        // NPC ������ ���� ȭ��ǥ Ȱ��ȭ/��Ȱ��ȭ
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

        // ȭ��ǥ ��ġ �� ȸ�� (�÷��̾��� offset ��ġ���� NPC�� ���ϵ���)
        transform.position = playerTransform.position + offset;
        Vector3 direction = npcTarget.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction);
        else if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        // NPC Outline ó��
        UpdateNPCOutline();
    }

    void UpdateNPCOutline()
    {
        if (npcTarget != null)
        {
            if (currentOutlinedNPC != npcTarget.gameObject)
            {
                DisableCurrentOutline();
                // NPC�� Outline ������Ʈ�� �پ� �־�� ��!
                Outline outline = npcTarget.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineColor = highlightColor;
                    outline.enabled = true;
                    currentOutlinedNPC = npcTarget.gameObject;
                }
                else
                {
                    Debug.LogWarning($"[ArrowIndicator] NPC '{npcTarget.name}'�� Outline ������Ʈ�� �����ϴ�.");
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
        // �������� PlayerController.localPlayer�� ���� ���� �÷��̾� Transform�� �Ҵ�
        if (PlayerController.localPlayer != null)
        {
            playerTransform = PlayerController.localPlayer.transform;
            Debug.Log("[ArrowIndicator] ���� �÷��̾� Transform�� �Ҵ��߽��ϴ�.");
        }
        else
        {
            // ���� ���� �������� �ʾҴٸ�, FindObjectsOfType�� �õ�
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (var p in players)
            {
                if (p.photonView != null && p.photonView.IsMine)
                {
                    playerTransform = p.transform;
                    Debug.Log("[ArrowIndicator] ���� �÷��̾� Transform�� �Ҵ��߽��ϴ� (FindObjectsOfType).");
                    return;
                }
            }
            Debug.LogWarning("[ArrowIndicator] ���� �÷��̾ ã�� ���߽��ϴ�.");
        }
    }
}
