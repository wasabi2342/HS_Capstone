using UnityEngine;

public class WhitePlayerSoundEvent : MonoBehaviour
{
    public void WhitePlayerSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Character-sword/{source}", transform.position);
    }

    public void WhitePlayerSoundMove()
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Common/walk sound", transform.position);
    } 
}
