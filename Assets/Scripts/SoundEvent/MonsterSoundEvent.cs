using UnityEngine;

public class MonsterSoundEvent : MonoBehaviour
{
    public void MonsterSoundEffect(string source)
    {
        AudioManager.Instance.PlayOneShot($"event:/Character/Monster/{source}", transform.position);
    }
}
