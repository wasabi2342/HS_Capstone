using UnityEngine;

public class WhitePlayerSoundEvent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void WhitePlayerSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Character-sword/{source}", transform.position);
    }

}
