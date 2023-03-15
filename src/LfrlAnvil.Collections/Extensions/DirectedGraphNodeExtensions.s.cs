using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections.Extensions;

public static class DirectedGraphNodeExtensions
{
    [Pure]
    public static bool IsRoot<TKey, TNodeValue, TEdgeValue>(this IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
        where TKey : notnull
    {
        foreach ( var edge in node.Edges )
        {
            var info = edge.GetInfo( node );
            if ( info is not null && info.Value.CanBeReached )
                return false;
        }

        return true;
    }
}
