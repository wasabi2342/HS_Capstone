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

    private float cooldownTimer;

    public void Init(Color outlineColr) // ��ų ���� ������ skillButton�� �̹����� Ŭ�� �̺�Ʈ �����ϱ�
    {
        outline.effectColor = outlineColr;
        //skillButton.image.sprite = Resources.Load<Sprite>("���");
    }

    public void StartUpdateSkillCooldown(float cooldown)
    {
        StartCoroutine(UpdateSkillCooldown(cooldown));
    }

    public void UpdateCooldownTimer(float time)
    {
        cooldownTimer = time;
    }

    private IEnumerator UpdateSkillCooldown(float cooldown)
    {
        cooldownTimer = cooldown;
        cooldownImage.fillAmount = 1;

        while (cooldownTimer >= 0)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownImage.fillAmount = Mathf.Max(cooldownTimer / cooldown, 0);
            yield return null;
        }
        cooldownImage.fillAmount = 0;
    }

}
