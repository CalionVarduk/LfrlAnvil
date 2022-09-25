using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State;

public interface IVariableNode
{
    IVariableNode? Parent { get; }
    VariableState State { get; }
    IEventStream<IVariableNodeEvent> OnChange { get; }
    IEventStream<IVariableNodeEvent> OnValidate { get; }

    [Pure]
    IEnumerable<IVariableNode> GetChildren();
}
