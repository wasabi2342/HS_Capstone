public interface INode
{
    public enum NodeState
    {
        Running,
        Success,
        Failure,
    }

    public NodeState Evaluate();
}
