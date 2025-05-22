using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "CreateFireArea", menuName = "Scriptable Objects/CreateFireArea")]
public class CreateFireArea : BaseSpecialEffect
{
    public override void ApplyEffect()
    {

        float rotationY = playerController.animator.GetFloat("moveX") > 0 ? 0f : 180f;

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("포톤으로 생성");
            PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/Space_FlameArea", playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0)).GetComponent<FlameArea>().Init(value * playerController.ReturnAbilityPower() * playerController.damageBuff, duration);
        }
        else
        {
            Debug.Log("Instantiate로 생성");
            Instantiate(Resources.Load<FlameArea>("SkillEffect/WhitePlayer/Space_FlameArea"), playerController.footPivot.position, Quaternion.Euler(90, rotationY, 0)).Init(value * playerController.ReturnAbilityPower(), duration);
        }
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);
        isInstant = true;
    }
}
