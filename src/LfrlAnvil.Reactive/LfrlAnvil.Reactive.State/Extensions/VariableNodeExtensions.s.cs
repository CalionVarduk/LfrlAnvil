using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.State.Extensions;

public static class VariableNodeExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsChanged(this IVariableNode node)
    {
        return (node.State & VariableState.Changed) != VariableState.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInvalid(this IVariableNode node)
    {
        return (node.State & VariableState.Invalid) != VariableState.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsWarning(this IVariableNode node)
    {
        return (node.State & VariableState.Warning) != VariableState.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsReadOnly(this IVariableNode node)
    {
        return (node.State & VariableState.ReadOnly) != VariableState.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDisposed(this IVariableNode node)
    {
        return (node.State & VariableState.Disposed) != VariableState.Default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDirty(this IVariableNode node)
    {
        return (node.State & VariableState.Dirty) != VariableState.Default;
    }
}
