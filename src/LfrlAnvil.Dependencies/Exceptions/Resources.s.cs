using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Exceptions;

internal static class Resources
{
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
    internal static string InvalidDependencyType(Type expected, Type? actual)
    {
        if ( actual is null )
            return $"Cannot cast resolved object to expected dependency type '{expected.GetDebugString()}'.";

        return
            $"Cannot cast resolved object of type '{actual.GetDebugString()}' to expected dependency type '{expected.GetDebugString()}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string SharedImplementorIsMissing(IDependencyKey implementorKey)
    {
        return $"Expected shared implementor {implementorKey} does not exist.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ProvidedTypeIsNonConstructable(Type? type)
    {
        return type is null
            ? "Type is not constructable."
            : $"Type '{type.GetDebugString()}' provided as a creation detail is not constructable.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FailedToFindValidCtorForType(Type? type)
    {
        return type is null
            ? "Failed to find a valid constructor."
            : $"Failed to find a valid constructor for type '{type.GetDebugString()}' provided as a creation detail.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ProvidedConstructorDoesNotCreateInstancesOfCorrectType(ConstructorInfo ctor)
    {
        return
            $"Constructor '{ctor.GetDebugString( includeDeclaringType: true )}' provided as a creation detail does not create instances assignable to expected type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ProvidedTypeIsIncorrect(Type type)
    {
        return $"Type '{type.GetDebugString()}' provided as a creation detail is not assignable to expected type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ProvidedConstructorBelongsToNonConstructableType(ConstructorInfo ctor)
    {
        return
            $"Constructor '{ctor.GetDebugString( includeDeclaringType: true )}' provided as a creation detail belongs to a type that is not constructable.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ProvidedImplementorTypeIsNotAssignableToDependencyType<T>(T target, IDependencyKey implementorKey)
        where T : notnull
    {
        if ( typeof( T ) == typeof( ParameterInfo ) )
        {
            var parameter = ReinterpretCast.To<ParameterInfo>( target );
            var ctor = ReinterpretCast.To<ConstructorInfo>( parameter.Member );
            return
                $"Provided implementor {implementorKey} is not assignable to explicitly resolved '{parameter.Name}' parameter of '{ctor.GetDebugString( includeDeclaringType: true )}' constructor.";
        }

        var member = ReinterpretCast.To<MemberInfo>( target );
        var memberText = member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).GetDebugString( includeDeclaringType: true )
            : ReinterpretCast.To<PropertyInfo>( member ).GetDebugString( includeDeclaringType: true );

        return
            $"Provided implementor {implementorKey} is not assignable to explicitly resolved '{memberText}' member.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string RequiredDependencyCannotBeResolved<T>(T target, IDependencyKey implementorKey)
        where T : notnull
    {
        if ( typeof( T ) == typeof( ParameterInfo ) )
        {
            var parameter = ReinterpretCast.To<ParameterInfo>( target );
            var ctor = ReinterpretCast.To<ConstructorInfo>( parameter.Member );
            return
                $"Parameter '{parameter.Name}' of '{ctor.GetDebugString( includeDeclaringType: true )}' constructor is unresolvable because used implementor {implementorKey} is not configured.";
        }

        var member = ReinterpretCast.To<MemberInfo>( target );
        var memberText = member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).GetDebugString( includeDeclaringType: true )
            : ReinterpretCast.To<PropertyInfo>( member ).GetDebugString( includeDeclaringType: true );

        return $"Member '{memberText}' is unresolvable because used implementor {implementorKey} is not configured.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string CaptiveDependencyDetected<T>(
        T target,
        DependencyLifetime implementorLifetime,
        IDependencyKey implementorKey,
        DependencyLifetime caughtLifetime)
        where T : notnull
    {
        if ( typeof( T ) == typeof( ParameterInfo ) )
        {
            var parameter = ReinterpretCast.To<ParameterInfo>( target );
            var ctor = ReinterpretCast.To<ConstructorInfo>( parameter.Member );
            return
                $"Parameter '{parameter.Name}' of '{ctor.GetDebugString( includeDeclaringType: true )}' constructor resolved with implementor {implementorKey} ({caughtLifetime}) is a captive dependency of {implementorLifetime} dependency.";
        }

        var member = ReinterpretCast.To<MemberInfo>( target );
        var memberText = member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).GetDebugString( includeDeclaringType: true )
            : ReinterpretCast.To<PropertyInfo>( member ).GetDebugString( includeDeclaringType: true );

        return
            $"Member '{memberText}' resolved with implementor {implementorKey} ({caughtLifetime}) is a captive dependency of {implementorLifetime} dependency.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnusedResolution<T>(ConstructorInfo ctor, int index, InjectableDependencyResolution<T> resolution)
        where T : class, ICustomAttributeProvider
    {
        var resolutionText = resolution.Factory is not null ? "from factory" : $"from implementor {resolution.ImplementorKey}";

        return typeof( T ) == typeof( ParameterInfo )
            ? $"Explicit parameter resolution {resolutionText} (index: {index}) in '{ctor.GetDebugString( includeDeclaringType: true )}' constructor is unused."
            : $"Explicit member resolution {resolutionText} (index: {index}) is unused.";
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
        var expectedTargets = $"{AttributeTargets.Parameter} | {AttributeTargets.Field} | {AttributeTargets.Property}";

        return
            $@"Type '{type.GetDebugString()}' is not a valid optional dependency attribute type because it doesn't satisfy the following requirements:
- Type cannot be an open generic definition,
- Type must extend a {typeof( Attribute ).GetDebugString()} class,
- Type must have a {typeof( AttributeUsageAttribute ).GetDebugString()} attribute with '{expectedTargets}' target.";
    }

    [Pure]
    internal static string ContainerIsNotConfiguredCorrectly(Chain<DependencyContainerBuildMessages> messages)
    {
        var implementorCount = 0;
        foreach ( var message in messages )
        {
            if ( message.Errors.Count > 0 )
                ++implementorCount;
        }

        var builder = new StringBuilder( "Dependency container is not configured correctly. Encountered errors in " )
            .Append( implementorCount )
            .Append( " implementor(s):" )
            .AppendLine();

        var implementorIndex = 1;
        foreach ( var message in messages )
        {
            if ( message.Errors.Count == 0 )
                continue;

            builder
                .Append( implementorIndex++ )
                .Append( ". " )
                .Append( message.ImplementorKey.ToString() )
                .Append( ", found " )
                .Append( message.Errors.Count )
                .Append( " error(s):" )
                .AppendLine();

            var errorIndex = 1;
            foreach ( var error in message.Errors )
            {
                builder
                    .Append( "    " )
                    .Append( errorIndex++ )
                    .Append( ". " )
                    .Append( error.Replace( Environment.NewLine, Environment.NewLine + "    " ) )
                    .AppendLine();
            }
        }

        return builder.ToString();
    }

    [Pure]
    internal static string CircularDependenciesDetected(
        ReadOnlySpan<(object? ReachedFrom, ImplementorBasedDependencyResolverFactory Node)> path)
    {
        var builder = new StringBuilder( "Circular dependency detected:" ).AppendLine();

        for ( var i = 0; i < path.Length; ++i )
        {
            var pathNode = path[i];
            Assume.IsNotNull( pathNode.ReachedFrom, nameof( pathNode.ReachedFrom ) );

            builder
                .Append( "    " )
                .Append( i + 1 )
                .Append( ". " )
                .Append( pathNode.Node.ImplementorKey.ToString() )
                .Append( " resolved through '" );

            if ( pathNode.ReachedFrom is ParameterInfo parameter )
            {
                var ctor = ReinterpretCast.To<ConstructorInfo>( parameter.Member );
                builder
                    .Append( parameter.Name )
                    .Append( "' parameter of '" )
                    .Append( ctor.GetDebugString( includeDeclaringType: true ) )
                    .Append( "' constructor." );
            }
            else
            {
                var member = ReinterpretCast.To<MemberInfo>( pathNode.ReachedFrom );
                if ( member.MemberType == MemberTypes.Field )
                {
                    var field = ReinterpretCast.To<FieldInfo>( member );
                    builder.Append( field.GetDebugString( includeDeclaringType: true ) ).Append( "' field." );
                }
                else
                {
                    var property = ReinterpretCast.To<PropertyInfo>( member );
                    builder.Append( property.GetDebugString( includeDeclaringType: true ) ).Append( "' property." );
                }
            }

            builder.AppendLine();
        }

        return builder.ToString();
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
