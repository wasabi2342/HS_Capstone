using System;

public sealed class ActionNode : INode
{
    Func<INode.NodeState> onUpdate = null;

    public ActionNode(Func<INode.NodeState> onUpdate)
    {
        this.onUpdate = onUpdate;
    }

    public INode.NodeState Evaluate() => onUpdate?.Invoke() ?? INode.NodeState.Failure;
}
