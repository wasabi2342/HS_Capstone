using UnityEngine;

public class PinkPlayerSoundEvent : MonoBehaviour
{
    public void PinkPlayerSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Character-pink/{source}", transform.position);
    }

    public void PinkPlayerSoundMove()
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Common/walk sound", transform.position);
    }
}
