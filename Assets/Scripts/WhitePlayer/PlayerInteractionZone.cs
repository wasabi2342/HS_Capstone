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

    [SerializeField]
    private WhitePlayercontroller_event whitePlayercontroller_Event;

    private void Awake()
    {
        whitePlayercontroller_Event  = GetComponentInParent<WhitePlayercontroller_event>();

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = interactionRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        whitePlayercontroller_Event.OnInteractionEvent += other.GetComponent<IInteractable>().OnInteract;
        Debug.Log("�浹�ȴ�.");
       
    }

    private void OnTriggerExit(Collider other)
    {

        whitePlayercontroller_Event.OnInteractionEvent -= other.GetComponent<IInteractable>().OnInteract;

       
    }
}
