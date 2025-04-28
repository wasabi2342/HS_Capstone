using UnityEngine;

public class PinkPlayerSoundEvent : MonoBehaviour
{
    public void PinkPlayerSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Character-pink/{source}", transform.position);
    }

    public void PinkPlayerSoundMove(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Common/{source}", transform.position);
    }
}
