using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "CreateFireArea", menuName = "Scriptable Objects/CreateFireArea")]
public class CreateFireArea : BaseSpecialEffect
{
    public override void ApplyEffect()
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate("SkillEffect/WhitePlayer/Space_FlameArea", playerController.transform.position - new Vector3(0, 2, 0), Quaternion.Euler(90, 0, 0)).GetComponent<FlameArea>().Init(value * playerController.ReturnAbilityPower(), duration);
        }
        else
        {
            Instantiate(Resources.Load<FlameArea>("SkillEffect/WhitePlayer/Space_FlameArea"), playerController.transform.position - new Vector3(0, 2, 0), Quaternion.Euler(90, 0, 0)).Init(value * playerController.ReturnAbilityPower(), duration);
        }
    }

    public override void Init(float value, float duration, ParentPlayerController playerController)
    {
        base.Init(value, duration, playerController);
        isInstant = true;
    }
}
