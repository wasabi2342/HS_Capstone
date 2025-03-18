using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerInteractionZone : MonoBehaviour
{
    [Header("��ȣ�ۿ� ���� ����")]
    [Tooltip("�� ������ �������� NPC, Trap ��� ��ȣ�ۿ��� �� �ִ� �����Դϴ�.")]
    public float interactionRange = 1.5f;

    // ���� ���� �ִ� ��ȣ�ۿ� ������ ������Ʈ ��� (NPC, Trap ��)
    //public List<GameObject> interactables = new List<GameObject>();
    public List<Action<InputAction.CallbackContext>> interactables = new List<Action<InputAction.CallbackContext>>();

    [SerializeField]
    private WhitePlayercontroller_event whitePlayercontroller_Event;

    private void Awake()
    {
        whitePlayercontroller_Event = GetComponentInParent<WhitePlayercontroller_event>();

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
            whitePlayercontroller_Event.OnInteractionEvent += other.GetComponent<IInteractable>().OnInteract;
            interactables.Add(other.GetComponent<IInteractable>().OnInteract);
            Debug.Log("�浹�ȴ�.");
        }

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.GetComponent<IInteractable>() != null)
        {
            whitePlayercontroller_Event.OnInteractionEvent -= other.GetComponent<IInteractable>().OnInteract;
            interactables.Remove(other.GetComponent<IInteractable>().OnInteract);
        }

    }


    private void OnDisable()
    {
        for (int i = 0; i < interactables.Count; i++)
        {
            whitePlayercontroller_Event.OnInteractionEvent -= interactables[i];
        }
        interactables.Clear();
    }
}

public class WhitePlayerReviveInteractable : MonoBehaviour, IInteractable
{
    private WhitePlayerController whitePlayer;

    private void Awake()
    {
        whitePlayer = GetComponentInParent<WhitePlayerController>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // ���� ���¶�� ��Ȱ ���� ����
            if (whitePlayer.currentState == WhitePlayerState.Stun)
            {
                // ü���� 20���� ȸ��
                whitePlayer.currentHealth = 20;
                // Revive() �޼��� ȣ��
                whitePlayer.Revive();
                Debug.Log("�÷��̾� ��Ȱ ��ȣ�ۿ�");
            }
        }
    }
}
