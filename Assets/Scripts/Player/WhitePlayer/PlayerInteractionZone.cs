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

    [SerializeField]
    private string layerName = "원하는레이어이름"; // 예시: "Weapon"
    
    // 범위 내에 있는 상호작용 가능한 오브젝트 목록 (NPC, Trap 등)
    public List<Action<InputAction.CallbackContext>> interactables = new List<Action<InputAction.CallbackContext>>();

    [SerializeField]
    private WhitePlayercontroller_event pinkPlayercontroller_Event;

    private void Awake()
    {
        pinkPlayercontroller_Event = GetComponentInParent<WhitePlayercontroller_event>();

        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = interactionRange;
        }
    }

    private void Start()
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"{layerName} 레이어를 찾을 수 없습니다. (ForceLayerSetter)");
            return;
        }

        gameObject.layer = layer;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponent<IInteractable>() != null)
        {
            pinkPlayercontroller_Event.OnInteractionEvent += other.GetComponent<IInteractable>().OnInteract;
            interactables.Add(other.GetComponent<IInteractable>().OnInteract);
            Debug.Log("충돌된다.");
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