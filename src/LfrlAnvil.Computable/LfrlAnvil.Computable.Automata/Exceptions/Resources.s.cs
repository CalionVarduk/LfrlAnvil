using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Automata.Exceptions;

internal static class Resources
{
    internal const string InitialStateIsMissing = "Initial state is missing.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TransitionAlreadyExists<TState, TInput>(TState source, TInput input)
    {
        return $"Outgoing transition from '{source}' state for '{input}' input already exists.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TransitionDoesNotExist<TState, TInput>(TState source, TInput input)
    {
        return $"Outgoing transition from '{source}' state for '{input}' input does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StateDoesNotExist<TState>(TState state)
    {
        return $"State '{state}' does not exist.";
    }
}
