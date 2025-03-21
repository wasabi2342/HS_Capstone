using System.Collections.Generic;

public sealed class SelectorNode : INode
{
    List<INode> children;

    public SelectorNode(List<INode> children)
    {
        this.children = children;
    }

    public INode.NodeState Evaluate()
    {
        if (children == null) return INode.NodeState.Failure;

        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case INode.NodeState.Running:
                    return INode.NodeState.Running;
                case INode.NodeState.Success:
                    return INode.NodeState.Success;
            }
        }
        return INode.NodeState.Failure;
    }
}
