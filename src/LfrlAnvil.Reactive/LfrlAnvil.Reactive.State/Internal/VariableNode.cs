using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Reactive.State.Events;

namespace LfrlAnvil.Reactive.State.Internal;

public abstract class VariableNode : IVariableNode
{
    protected VariableNode()
    {
        Parent = null;
    }

    public IVariableNode? Parent { get; private set; }
    public abstract VariableState State { get; }
    public abstract IEventStream<IVariableNodeEvent> OnChange { get; }
    public abstract IEventStream<IVariableNodeEvent> OnValidate { get; }

    [Pure]
    public abstract IEnumerable<IVariableNode> GetChildren();

    protected void SetAsParentOf(VariableNode other)
    {
        if ( ! ReferenceEquals( other.Parent, this ) )
        {
            Ensure.IsNull( other.Parent, nameof( other ) + '.' + nameof( Parent ) );
            Ensure.NotRefEquals( this, other, "this" );
            other.Parent = this;
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static VariableState CreateState(VariableState current, VariableState value, bool enabled)
    {
        return enabled ? current | value : current & ~value;
    }
}
