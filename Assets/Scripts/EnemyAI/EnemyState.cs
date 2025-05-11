// AI의 가능한 상태들을 정의하는 열거형
public enum EnemyState
{
    Wander,     // 배회
    Idle,       // 대기
    Chase,      // 추격
    Return,     // 귀환
    WaitCool,   // 공격 준비 대기
    Attack,     // 공격
    AttackCool, // 공격 후 대기
    Hit,        // 피격
    Dead        // 사망
}