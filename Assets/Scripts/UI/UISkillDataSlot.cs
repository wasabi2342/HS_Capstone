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
        //skillImage.sprite = Resources.Load<Sprite>($"Blessing/Front/{(Blessings)blessingData.skillData.ID}");
        //bgImage.sprite = Resources.Load<Sprite>(bgImagePath);
    }
}
