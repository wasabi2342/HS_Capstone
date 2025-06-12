using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField]
    private List<GameObject> items = new List<GameObject>();
    [SerializeField]
    private Sprite changeSprite;
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Image buttonImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(true);
        }

        buttonImage.sprite = changeSprite;
        buttonImage.SetNativeSize();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(false);
        }
        buttonImage.sprite = defaultSprite;
        buttonImage.SetNativeSize();
    }

}
