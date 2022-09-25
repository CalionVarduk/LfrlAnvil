namespace LfrlAnvil.Reactive.State.Exceptions;

internal static class Resources
{
    internal const string ParentNodeIsDisposed = "Parent node is disposed.";
    internal const string ChildNodeIsDisposed = "Child node is disposed.";
    internal const string ChildNodeAlreadyHasParent = "Child node already has a parent.";
    internal const string ParentNodeCannotRegisterSelf = "Parent node cannot register itself as a child node.";

    internal const string CannotRegisterChildNodesInParentNodeThatAlreadyHasParent =
        "Cannot register child nodes in parent node that already has a parent.";
}
