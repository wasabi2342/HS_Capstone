// Assets/Scripts/Common/IMovable.cs
public interface IMovable
{
    /// <summary>����� ���� �̵��ӵ��� �Ͻ������� ������ �� ���</summary>
    float MoveSpeed { get; set; }

    /// <summary>true -> �̵�/�Է� ���, false -> ���� ����</summary>
    void StopMove(bool stop);
}
