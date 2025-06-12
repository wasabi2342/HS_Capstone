using UnityEngine;

public interface IMonsterAttack
{
    string AnimKey { get; } // 기존

    float WindUpRate { get; } // 0.0 ~ 1.0  (타격 발동 시점 비율)
    void Attack(Transform target);
    /* FSM이 호출할 콜라이더·이펙트 제어 */
    void EnableAttack();            // 콜라이더 ON 
    void DisableAttack();           // 콜라이더 OFF
    void SetDirection(float sign);  // +1 = Right, -1 = Left

}
