using System.Collections.Generic;

public sealed class SequenceNode : INode
{
    List<INode> children;

    public SequenceNode(List<INode> children)
    {
        this.children = children;
    }

    public INode.NodeState Evaluate()
    {
        if (children == null || children.Count == 0)
            return INode.NodeState.Failure;

        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case INode.NodeState.Running:
                    return INode.NodeState.Running;
                case INode.NodeState.Success:
                    continue;
                case INode.NodeState.Failure:
                    return INode.NodeState.Failure;
            }
        }
        return INode.NodeState.Success;
    }
}
