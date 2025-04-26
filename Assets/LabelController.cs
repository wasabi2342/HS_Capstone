using System.Collections.Generic;
using UnityEngine;

public class LabelController : MonoBehaviour
{
    public float speed = 100f; // �̵� �ӵ�
    public float imageWidth = 1850; // �̹��� �� ���� �ʺ�
    public List<RectTransform> imageList; // 3���� �̹���
    public bool moveLeft = true;

    private bool isScrolling = false; // ��ũ�� ���� ����

    private void Update()
    {
        if (!isScrolling) return;

        float direction = moveLeft ? -1f : 1f;
        float move = direction * speed * Time.deltaTime;

        // �̹��� �̵�
        foreach (RectTransform img in imageList)
        {
            img.anchoredPosition += new Vector2(move, 0f);
        }

        // �̹��� ���ġ (�ϳ����� �˻�)
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

    // �ܺο��� ȣ���ϸ� ���۵�
    public void StartScroll()
    {
        isScrolling = true;
    }

    // �ʿ��ϸ� ���߰Ե� ����
    public void StopScroll()
    {
        isScrolling = false;
    }

}
