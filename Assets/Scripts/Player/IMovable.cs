// Assets/Scripts/Common/IMovable.cs
public interface IMovable
{
    /// <summary>디버프 등이 이동속도를 일시적으로 조정할 때 사용</summary>
    float MoveSpeed { get; set; }

    /// <summary>true -> 이동/입력 잠금, false -> 정상 해제</summary>
    void StopMove(bool stop);
}
