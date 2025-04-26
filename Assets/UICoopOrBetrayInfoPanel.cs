using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UICoopOrBetrayInfoPanel : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> objectList = new List<GameObject>();
    [SerializeField]
    private Vector3 targetPos;
    [SerializeField]
    private Vector3 startPos;
    [SerializeField]
    private RectTransform myRectTransform;

    private void OnEnable()
    {
        Debug.Log("UICoopOrBetrayInfoPanel Enable ½ÇÇàµÊ");

        myRectTransform.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            foreach (var obj in objectList)
            {
                obj.SetActive(true);
            }
        });
    }

    private void OnDisable()
    {
        myRectTransform.anchoredPosition = startPos;

        foreach (var obj in objectList)
        {
            obj.SetActive(false);
        }
    }
}
