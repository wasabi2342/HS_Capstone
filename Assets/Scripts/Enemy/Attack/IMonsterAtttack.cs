using UnityEngine;

public interface IMonsterAttack
{
    void Attack(Transform target);
    /* FSM이 호출할 콜라이더·이펙트 제어 */
    void EnableAttack();            // 콜라이더 ON
    void DisableAttack();           // 콜라이더 OFF
    void SetDirection(float sign);  // +1 = Right, -1 = Left
}
