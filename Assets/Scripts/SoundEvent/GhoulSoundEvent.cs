using UnityEngine;

public class GhoulSoundEvent : MonoBehaviour
{
    public void GhoulSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Monster/{source}", transform.position);
    }
}
