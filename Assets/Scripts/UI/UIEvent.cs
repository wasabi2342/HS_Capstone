using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField]
    private List<GameObject> items = new List<GameObject>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(false);
        }
    }

}
