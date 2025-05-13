using UnityEngine;
using UnityEngine.UI;

public class UISkillDataSlot : MonoBehaviour
{
    [SerializeField]
    private Text skillInfoText;
    [SerializeField]
    private Text skillKeyText;
    [SerializeField]
    private Image skillImage;
    [SerializeField]
    private Image bgImage;

    public void Init(SkillWithLevel blessingData)
    {
        skillInfoText.text = blessingData.skillData.Bless_Discript;
        skillKeyText.text = blessingData.skillData.Blessing_name + blessingData.level.ToString();
        Sprite sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)blessingData.skillData.ID}"); //나중에 바꾸기 인덱스로 바뀔 수도
        if (sprite != null)
            skillImage.sprite = sprite;
        else
            skillImage.sprite = Resources.Load<Sprite>("Blessing/Front/card_image");  // 이미지 이름 나중에 맞는 가호로 바꾸기
        //skillImage.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)blessingData.skillData.ID}");
        //bgImage.sprite = Resources.Load<Sprite>(bgImagePath);
    }
}
