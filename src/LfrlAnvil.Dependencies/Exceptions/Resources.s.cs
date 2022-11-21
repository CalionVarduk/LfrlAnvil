using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Exceptions;

internal static class Resources
{
    internal const string ContainerIsNotConfiguredCorrectly = "Container is not configured correctly.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ScopeIsDisposed(IDependencyScope scope)
    {
        var scopeText = GetScopeString( scope, capitalize: true );
        return $"{scopeText} is disposed.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ScopeCannotBeginNewScopeForCurrentThread(IDependencyScope scope, IDependencyScope expected, int threadId)
    {
        var scopeText = GetScopeString( scope, capitalize: true );
        var expectedText = GetScopeString( expected, capitalize: false );
        return $"{scopeText} cannot begin new scope for thread {threadId}, expected {expectedText}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NamedScopeAlreadyExists(IDependencyScope parentScope, string name)
    {
        var scopeText = GetScopeString( parentScope, capitalize: false );
        return $"Could not begin a named scope from {scopeText} because scope with name '{name}' already exists.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CannotDisposeScopeFromThisThread(IDependencyScope scope, int actualThreadId)
    {
        var scopeText = GetScopeString( scope, capitalize: false );
        return $"Child {scopeText} can only be disposed by a thread that created it (current thread: {actualThreadId}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string OwnedDependencyHasThrownExceptionDuringDisposal(IDependencyScope scope)
    {
        var scopeText = GetScopeString( scope, capitalize: false );
        return $"Dependency owned by {scopeText} has thrown an exception during its disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SomeOwnedDependenciesHaveThrownExceptionsDuringDisposal(IDependencyScope scope)
    {
        var scopeText = GetScopeString( scope, capitalize: false );
        return $"Some owned dependencies have thrown exceptions during {scopeText} disposal.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string MissingDependency(Type type)
    {
        return $"Dependency of type '{type.GetDebugString()}' is missing.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CircularDependencyReference(Type dependencyType, Type implementorType)
    {
        return
            $"Detected circular dependency reference during '{dependencyType.GetDebugString()}' resolution using '{implementorType.GetDebugString()}' implementor.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDependencyType(Type expected, Type actual)
    {
        return
            $"Cannot cast resolved object of type '{actual.GetDebugString()}' to expected dependency type '{expected.GetDebugString()}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ImplementorDoesNotExist(Type dependencyType, IDependencyImplementorKey implementorKey)
    {
        return
            $"Dependency type '{dependencyType.GetDebugString()}' was configured to use a shared implementor '{implementorKey.ToString()}' but such an implementor wasn't registered.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidInjectablePropertyType(Type type)
    {
        return
            $@"Type '{type.GetDebugString()}' is not a valid injectable property type because it doesn't satisfy the following requirements:
- Type must be an open generic definition,
- Type must have exactly one generic argument,
- Type must contain a constructor that accepts exactly one parameter of type equal to the generic argument.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidOptionalDependencyAttributeType(Type type)
    {
        return
            $@"Type '{type.GetDebugString()}' is not a valid optional dependency attribute type because it doesn't satisfy the following requirements:
- Type cannot be an open generic definition,
- Type must extend a {typeof( Attribute ).GetDebugString()} class,
- Type must have a {typeof( AttributeUsageAttribute ).GetDebugString()} attribute with '{AttributeTargets.Parameter}' target.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string GetScopeString(IDependencyScope scope, bool capitalize)
    {
        if ( capitalize )
            return scope.IsRoot ? "Scope [root]" : $"Scope [level: {scope.Level}, thread: {scope.ThreadId}]";

        return scope.IsRoot ? "scope [root]" : $"scope [level: {scope.Level}, thread: {scope.ThreadId}]";
    }
}
