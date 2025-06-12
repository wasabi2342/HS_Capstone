using System.Collections.Generic;
using UnityEngine;

public class LabelController : MonoBehaviour
{
    public float speed = 100f; // 이동 속도
    public float imageWidth = 1850; // 이미지 한 장의 너비
    public List<RectTransform> imageList; // 3개의 이미지
    public bool moveLeft = true;

    private bool isScrolling = false; // 스크롤 시작 여부

    private void Update()
    {
        if (!isScrolling) return;

        float direction = moveLeft ? -1f : 1f;
        float move = direction * speed * Time.deltaTime;

        // 이미지 이동
        foreach (RectTransform img in imageList)
        {
            img.anchoredPosition += new Vector2(move, 0f);
        }

        // 이미지 재배치 (하나씩만 검사)
        foreach (RectTransform img in imageList)
        {
            if (moveLeft && img.anchoredPosition.x <= -imageWidth)
            {
                RectTransform rightMost = GetRightMostImage();
                img.anchoredPosition = new Vector2(rightMost.anchoredPosition.x + imageWidth, img.anchoredPosition.y);
                break;
            }
            else if (!moveLeft && img.anchoredPosition.x >= imageWidth)
            {
                RectTransform leftMost = GetLeftMostImage();
                img.anchoredPosition = new Vector2(leftMost.anchoredPosition.x - imageWidth, img.anchoredPosition.y);
                break;
            }
        }
    }

    private RectTransform GetRightMostImage()
    {
        RectTransform result = null;
        float maxX = float.MinValue;
        foreach (RectTransform img in imageList)
        {
            if (img.anchoredPosition.x > maxX)
            {
                maxX = img.anchoredPosition.x;
                result = img;
            }
        }
        return result;
    }

    private RectTransform GetLeftMostImage()
    {
        RectTransform result = null;
        float minX = float.MaxValue;
        foreach (RectTransform img in imageList)
        {
            if (img.anchoredPosition.x < minX)
            {
                minX = img.anchoredPosition.x;
                result = img;
            }
        }
        return result;
    }

    // 외부에서 호출하면 시작됨
    public void StartScroll()
    {
        isScrolling = true;
    }

    // 필요하면 멈추게도 가능
    public void StopScroll()
    {
        isScrolling = false;
    }

}
