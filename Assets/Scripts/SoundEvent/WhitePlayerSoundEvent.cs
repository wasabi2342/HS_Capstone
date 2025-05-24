using UnityEngine;
using Photon.Pun;

public class WhitePlayerSoundEvent : MonoBehaviourPun
{
    private void PlaySound(string fullPath)
    {
        AudioManager.Instance.PlayOneShot(fullPath, transform.position);
    }

    public void WhitePlayerSoundEffect(string source)
    {
        PlaySound($"event:/Character/Character-sword/{source}");
    }

    public void WhitePlayerSoundMove(string source)
    {
        PlaySound($"event:/Character/Common/{source}");
    }
}
