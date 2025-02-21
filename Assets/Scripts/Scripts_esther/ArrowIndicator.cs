using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [Header("���� ����")]
   
    public Transform playerTransform;

    [Header("��ġ �� ȸ�� ������")]
    
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("��ȣ�ۿ� ���� ����")]
   
    public float interactionRadius = 1.5f;
    
    public LayerMask npcLayerMask;

    [Header("���̶���Ʈ ����")]
    
    public Color highlightColor = Color.yellow;

    
    private Transform npcTarget;
    
    private Renderer arrowRenderer;
    
    private GameObject currentOutlinedNPC;

    void Awake()
    {
        arrowRenderer = GetComponent<Renderer>();
        if (arrowRenderer == null)
        {
            Debug.LogError("ArrowIndicator ��ũ��Ʈ�� ���� ������Ʈ�� Renderer ������Ʈ�� �ʿ��մϴ�.");
        }
        // ���� �� ȭ��ǥ�� ���� (������ ��Ȱ��ȭ)
        arrowRenderer.enabled = false;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        // �÷��̾� �ֺ��� NPC Ž�� (OverlapSphere ���)
        Collider[] cols = Physics.OverlapSphere(playerTransform.position, interactionRadius, npcLayerMask);
        Transform closestNPC = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider col in cols)
        {
            // NPC ���̾ �ش��ϴ��� Ȯ��
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

        // NPC�� �����Ǹ� ȭ��ǥ�� ���̰�, ������ ����
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

        // �÷��̾� ��ġ�� ������ �����Ͽ� ȭ��ǥ ��ġ ������Ʈ
        transform.position = playerTransform.position + offset;

        // NPC�� ���ϴ� ���� ��� �� ȸ�� ����
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
            // ���� ���� ���̶���Ʈ�� NPC�� �� Ÿ�ٰ� �ٸ��ٸ� ��ȯ
            if (currentOutlinedNPC != npcTarget.gameObject)
            {
                // ������ ���̶���Ʈ�� NPC�� �ִٸ� ��Ȱ��ȭ
                DisableCurrentOutline();

                // �� Ÿ���� Outline ������Ʈ Ȱ��ȭ
                Outline outline = npcTarget.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.OutlineColor = highlightColor;
                    outline.enabled = true;
                    currentOutlinedNPC = npcTarget.gameObject;
                }
                else
                {
                    Debug.LogWarning("NPC ������Ʈ�� Outline ������Ʈ�� �����ϴ�: " + npcTarget.gameObject.name);
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
