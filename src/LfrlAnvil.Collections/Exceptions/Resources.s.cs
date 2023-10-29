namespace LfrlAnvil.Collections.Exceptions;

internal static class Resources
{
    internal const string KeyExistenceInForwardDictionary = "key's existence in forward dictionary";
    internal const string KeyExistenceInReverseDictionary = "key's existence in reverse dictionary.";
    internal const string NodeHasBeenRemovedFromGraph = "Node has been removed from the graph.";
    internal const string EdgeHasBeenRemovedFromGraph = "Edge has been removed from the graph.";
    internal const string NoneIsNotValidDirection = nameof( GraphDirection.None ) + " is not a valid direction.";
    internal const string NodesMustBelongToTheSameGraph = "Nodes must belong to the same graph.";
    internal const string NodeCannotBeMovedToItself = "Node cannot be moved to itself.";
    internal const string SubtreeCannotBeMovedToItself = "Subtree root node cannot be moved to itself.";
    internal const string SubtreeCannotBeMovedToOneOfItsNodes = "Subtree root node cannot be moved to one of its descendants.";
    internal const string NodeIsOfIncorrectType = "Node is of incorrect type.";
    internal const string NodeDoesNotBelongToTree = "Node doesn't belong to this tree.";
    internal const string NodeAlreadyBelongsToTree = "Node already belongs to a tree.";
    internal const string SubtreeAlreadyBelongsToTree = "Subtree already belongs to this tree.";
    internal const string SomeSubtreeNodeKeysAlreadyExistInTree = "Some of the subtree's node keys already exist in this tree.";
}
