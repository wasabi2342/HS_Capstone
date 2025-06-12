using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISkillIcon : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Outline outline;
    [SerializeField]
    private Button skillButton;
    [SerializeField]
    private Image cooldownImage;
    [SerializeField]
    private Image skillImage;

    private float cooldownTimer;

    private Coroutine cooldownCoroutine;
    //public void Init(Color outlineColr) // 스킬 정보 가져와 skillButton의 이미지와 클릭 이벤트 설정하기
    //{
    //    outline.effectColor = outlineColr;
    //    //skillButton.image.sprite = Resources.Load<Sprite>("경로");
    //}

    public void SetOutlineColor(Color outlineColr)
    {
        if (!outline.enabled)
        {
            outline.enabled = true;
        }
        outline.effectColor = outlineColr;
    }

    public void StartUpdateSkillCooldown(float leftCooldown, float maxCooldown)
    {
        cooldownImage.fillAmount = Mathf.Max(leftCooldown / maxCooldown, 0);
    }

    public void SetImage(string playerCharacter, Skills skills, Blessings blessings)
    {
        skillImage.sprite = Resources.Load<Sprite>($"SkillIcon/{playerCharacter}/{skills}_{blessings}");
    }

    //public void UpdateCooldownTimer(float time)
    //{
    //    cooldownTimer = time;
    //}
    //
    //private IEnumerator UpdateSkillCooldown(float leftCooldown, float maxCooldown)
    //{
    //    cooldownTimer = leftCooldown;
    //    cooldownImage.fillAmount = cooldownTimer / maxCooldown;
    //
    //    while (cooldownTimer >= 0)
    //    {
    //        cooldownTimer -= Time.deltaTime;
    //        cooldownImage.fillAmount = Mathf.Max(cooldownTimer / maxCooldown, 0);
    //        yield return null;
    //    }
    //    cooldownImage.fillAmount = 0;
    //}

}
