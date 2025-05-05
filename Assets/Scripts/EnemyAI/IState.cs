// 모든 상태 클래스가 구현해야 하는 기본 인터페이스
public interface IState
{
    void Enter();    // 상태 진입 시 호출
    void Execute();  // 상태 활성화 중 매 프레임 또는 주기적으로 호출
    void Exit();     // 상태 탈출 시 호출
}