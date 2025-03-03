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

    public void Init(string skillInfo, string key, string skillImagePath = "images", string bgImagePath = "images")
    {
        skillInfoText.text = skillInfo;
        skillKeyText.text = key;
        skillImage.sprite = Resources.Load<Sprite>(skillImagePath);
        bgImage.sprite = Resources.Load<Sprite>(bgImagePath);
    }
}
