using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]
public class WhitePlayerInteractionZone : MonoBehaviour
{
    [Header("상호작용 범위 설정")]
    [Tooltip("이 영역의 반지름이 NPC, Trap 등과 상호작용할 수 있는 범위입니다.")]
    public float interactionRange = 1.5f;

    // 범위 내에 있는 상호작용 가능한 오브젝트 목록 (NPC, Trap 등)
    //public List<GameObject> interactables = new List<GameObject>();
    public List<Action<InputAction.CallbackContext>> interactables = new List<Action<InputAction.CallbackContext>>();

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
        interactables.Add(other.GetComponent<IInteractable>().OnInteract);
        Debug.Log("충돌된다.");
       
    }

    private void OnTriggerExit(Collider other)
    {

        whitePlayercontroller_Event.OnInteractionEvent -= other.GetComponent<IInteractable>().OnInteract;
        interactables.Remove(other.GetComponent<IInteractable>().OnInteract);

    }

    private void OnDisable()
    {
        for(int i = 0; i < interactables.Count; i++)
        {
            whitePlayercontroller_Event.OnInteractionEvent -= interactables[i];
        }
        interactables.Clear();
    }
}
