using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class PinkPlayerInteractionZone : MonoBehaviour
{
    [Header("��ȣ�ۿ� ���� ����")]
    [Tooltip("�� ������ �������� NPC, Trap ��� ��ȣ�ۿ��� �� �ִ� �����Դϴ�.")]
    public float interactionRange = 1.5f;

    // ���� ���� �ִ� ��ȣ�ۿ� ������ ������Ʈ ��� (NPC, Trap ��)
    public List<Action<InputAction.CallbackContext>> interactables = new List<Action<InputAction.CallbackContext>>();

    [SerializeField]
    private PinkPlayercontroller_event pinkPlayercontroller_Event;

    private void Awake()
    {
        pinkPlayercontroller_Event = GetComponentInParent<PinkPlayercontroller_event>();

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = interactionRange;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<IInteractable>() != null)
        {
            pinkPlayercontroller_Event.OnInteractionEvent += other.GetComponent<IInteractable>().OnInteract;
            interactables.Add(other.GetComponent<IInteractable>().OnInteract);
            Debug.Log("�浹�ȴ�.");
        }

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.GetComponent<IInteractable>() != null)
        {
            pinkPlayercontroller_Event.OnInteractionEvent -= other.GetComponent<IInteractable>().OnInteract;
            interactables.Remove(other.GetComponent<IInteractable>().OnInteract);
        }

    }


    private void OnDisable()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            pinkPlayercontroller_Event.OnInteractionEvent -= interactables[i];
        }
        interactables.Clear();
    }
}