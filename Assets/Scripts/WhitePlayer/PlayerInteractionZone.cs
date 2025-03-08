using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerInteractionZone : MonoBehaviour
{
    [Header("��ȣ�ۿ� ���� ����")]
    [Tooltip("�� ������ �������� NPC, Trap ��� ��ȣ�ۿ��� �� �ִ� �����Դϴ�.")]
    public float interactionRange = 1.5f;

    // ���� ���� �ִ� ��ȣ�ۿ� ������ ������Ʈ ��� (NPC, Trap ��)
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
        // NPC�� Trap ���̾ �߰�
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC") ||
            other.gameObject.layer == LayerMask.NameToLayer("Trap"))
        {
            if (!interactables.Contains(other.gameObject))
            {
                interactables.Add(other.gameObject);
                Debug.Log("[WhitePlayerInteractionZone] ��ȣ�ۿ� ��� �߰�: " + other.gameObject.name);
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
                Debug.Log("[WhitePlayerInteractionZone] ��ȣ�ۿ� ��� ����: " + other.gameObject.name);
            }
        }
    }
}
