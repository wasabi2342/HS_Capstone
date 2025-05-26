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

        // 대각선 처리
        if (Mathf.Abs(moveX) > 0.1f && Mathf.Abs(moveY) > 0.1f)
        {
            // 대각선일 때도 위쪽은 -45도, 아래쪽은 45도로 바꿈
            rotationY = (moveY > 0) ? -45f : 45f;

            if (moveX < 0)
            {
                rotationY *= -1f;
                flipX = true;
            }
        }
        else if (Mathf.Abs(moveY) > 0.1f) // 위 또는 아래
        {
            rotationY = (moveY > 0) ? -90f : 90f;
        }
        else if (Mathf.Abs(moveX) > 0.1f) // 좌우
        {
            rotationY = 0f;

            if (moveX < 0)
                flipX = true;
        }

        FlameArea flameArea;

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("포톤으로 생성");
            flameArea = PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/Space_FlameArea", playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0)).GetComponent<FlameArea>();
            flameArea.Init(value * playerController.ReturnAbilityPower() * playerController.damageBuff, duration);
        }
        else
        {
            Debug.Log("Instantiate로 생성");
            flameArea = Instantiate(Resources.Load<FlameArea>("SkillEffect/WhitePlayer/Space_FlameArea"), playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0));
            flameArea.Init(value * playerController.ReturnAbilityPower(), duration);
        }

        // 좌우 반전 적용
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
