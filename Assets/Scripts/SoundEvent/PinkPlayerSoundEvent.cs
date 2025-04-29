using UnityEngine;
using Photon.Pun;

public class PinkPlayerSoundEvent : MonoBehaviourPun
{
    private void PlaySound(string fullPath)
    {
        if (photonView.IsMine)
        {
            AudioManager.Instance.PlayOneShot(fullPath, transform.position);
        }
    }

    public void PinkPlayerSoundEffect(string source)
    {
        PlaySound($"event:/Character/Character-pink/{source}");
    }

    public void PinkPlayerSoundMove(string source)
    {
        PlaySound($"event:/Character/Common/{source}");
    }
}
