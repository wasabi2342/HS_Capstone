using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "CreateFireArea", menuName = "Scriptable Objects/CreateFireArea")]
public class CreateFireArea : BaseSpecialEffect
{
    public override void ApplyEffect()
    {

        float moveX = playerController.animator.GetFloat("moveX");
        float moveY = playerController.animator.GetFloat("moveY");

        float rotationY = 0f;
        bool flipX = false;

        // �밢�� ó��
        if (Mathf.Abs(moveX) > 0.1f && Mathf.Abs(moveY) > 0.1f)
        {
            // �밢���� ���� ������ -45��, �Ʒ����� 45���� �ٲ�
            rotationY = (moveY > 0) ? -45f : 45f;

            if (moveX < 0)
            {
                rotationY *= -1f;
                flipX = true;
            }
        }
        else if (Mathf.Abs(moveY) > 0.1f) // �� �Ǵ� �Ʒ�
        {
            rotationY = (moveY > 0) ? -90f : 90f;
        }
        else if (Mathf.Abs(moveX) > 0.1f) // �¿�
        {
            rotationY = 0f;

            if (moveX < 0)
                flipX = true;
        }

        FlameArea flameArea;

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("�������� ����");
            flameArea = PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/Space_FlameArea", playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0)).GetComponent<FlameArea>();
            flameArea.Init(value * playerController.ReturnAbilityPower() * playerController.damageBuff, duration);
        }
        else
        {
            Debug.Log("Instantiate�� ����");
            flameArea = Instantiate(Resources.Load<FlameArea>("SkillEffect/WhitePlayer/Space_FlameArea"), playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0));
            flameArea.Init(value * playerController.ReturnAbilityPower(), duration);
        }

        // �¿� ���� ����
        Vector3 scale = flameArea.transform.localScale;
        scale.x = flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        flameArea.transform.localScale = scale;

    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);
        isInstant = true;
    }
}
