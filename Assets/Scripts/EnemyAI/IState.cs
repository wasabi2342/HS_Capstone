// ��� ���� Ŭ������ �����ؾ� �ϴ� �⺻ �������̽�
public interface IState
{
    void Enter();    // ���� ���� �� ȣ��
    void Execute();  // ���� Ȱ��ȭ �� �� ������ �Ǵ� �ֱ������� ȣ��
    void Exit();     // ���� Ż�� �� ȣ��
}