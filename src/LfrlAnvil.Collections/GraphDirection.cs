using System;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents direction of <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/>.
/// </summary>
[Flags]
public enum GraphDirection : byte
{
    /// <summary>
    /// Represents a lack of connection.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents an incoming connection.
    /// </summary>
    In = 1,

    /// <summary>
    /// Represents an outgoing connection.
    /// </summary>
    Out = 2,

    /// <summary>
    /// Represents incoming and outgoing connection.
    /// </summary>
    Both = In | Out
}
