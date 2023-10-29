using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredDependencyResolverFactory : DependencyResolverFactory
{
    private ConstructorInfo? _constructorInfo;
    private KeyValuePair<ParameterInfo, object?>[]? _parameterResolutions;
    private KeyValuePair<MemberInfo, object?>[]? _memberResolutions;
    private Chain<string> _errors;
    private Chain<string> _warnings;

    protected RegisteredDependencyResolverFactory(
        ImplementorKey implementorKey,
        IDependencyImplementorBuilder implementorBuilder,
        DependencyLifetime lifetime)
        : base( implementorKey, lifetime )
    {
        ImplementorBuilder = implementorBuilder;
        _constructorInfo = null;
        _parameterResolutions = null;
        _memberResolutions = null;
        _errors = Chain<string>.Empty;
        _warnings = Chain<string>.Empty;
    }

    internal IDependencyImplementorBuilder ImplementorBuilder { get; }

    [Pure]
    internal sealed override Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        return IsCaptiveDependencyOf( lifetime ) ? Chain.Create<DependencyResolverFactory>( this ) : Chain<DependencyResolverFactory>.Empty;
    }

    [Pure]
    internal sealed override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return Lifetime < lifetime;
    }

    [Pure]
    protected sealed override Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        return _errors.Count == 0 && _warnings.Count == 0
            ? Chain<DependencyContainerBuildMessages>.Empty
            : Chain.Create( new DependencyContainerBuildMessages( ImplementorKey, _errors, _warnings ) );
    }

    protected sealed override bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( ImplementorBuilder.Factory is not null )
        {
            var resolver = CreateFromFactory( idGenerator );
            Finish( resolver );
            return true;
        }

        var result = FindValidConstructor( availableDependencies, configuration );
        if ( result.Errors.Count == 0 )
        {
            _constructorInfo = result.Info;
            return true;
        }

        _errors = _errors.Extend( result.Errors );
        return false;
    }

    protected sealed override bool AreRequiredDependenciesValid(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( _constructorInfo );

        var captiveDependencies = Chain<string>.Empty;
        var invocationOptions = ImplementorBuilder.Constructor?.InvocationOptions;

        var parameters = _constructorInfo.GetParameters();
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;
        var usedExplicitResolutions = explicitResolutionsLength > 0 ? new BitArray( explicitResolutionsLength ) : null;

        if ( parameters.Length > 0 )
            _parameterResolutions = new KeyValuePair<ParameterInfo, object?>[parameters.Length];

        for ( var i = 0; i < parameters.Length; ++i )
        {
            Assume.IsNotNull( _parameterResolutions );

            var parameter = parameters[i];
            var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

            IDependencyKey implementorKey;
            if ( customResolutionIndex == -1 )
                implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
            else
            {
                var resolution = GetResolution( explicitParameterResolutions, usedExplicitResolutions, customResolutionIndex );
                if ( resolution.Factory is not null )
                {
                    _parameterResolutions[i] = KeyValuePair.Create( parameter, (object?)resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( parameter, parameter.ParameterType, resolution.ImplementorKey );
            }

            if ( availableDependencies.TryGetValue( implementorKey, out var parameterFactory ) )
            {
                _parameterResolutions[i] = KeyValuePair.Create( parameter, (object?)parameterFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                continue;
            }

            if ( parameter.HasDefaultValue ||
                parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false ) ||
                (parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                _parameterResolutions[i] = KeyValuePair.Create( parameter, (object?)null );
                continue;
            }

            _errors = _errors.Extend( Resources.RequiredDependencyCannotBeResolved( parameter, implementorKey ) );
        }

        ValidateUnusedResolutions( explicitParameterResolutions, usedExplicitResolutions, explicitResolutionsLength );

        var injectableMembers = FindInjectableMembers( configuration );
        var explicitMemberResolutions = invocationOptions?.MemberResolutions;
        explicitResolutionsLength = explicitMemberResolutions?.Count ?? 0;
        usedExplicitResolutions = ReuseBitArray( usedExplicitResolutions, explicitResolutionsLength );

        if ( injectableMembers.Length > 0 )
            _memberResolutions = new KeyValuePair<MemberInfo, object?>[injectableMembers.Length];

        for ( var i = 0; i < injectableMembers.Length; ++i )
        {
            Assume.IsNotNull( _memberResolutions );

            var member = injectableMembers[i];
            var customResolutionIndex = FindCustomResolutionIndex( explicitMemberResolutions, explicitResolutionsLength, member );

            var memberInjectableType = GetInjectableMemberType( member );
            var memberType = memberInjectableType.GetGenericArguments()[0];

            IDependencyKey implementorKey;
            if ( customResolutionIndex == -1 )
                implementorKey = InternalImplementorKey.WithType( memberType );
            else
            {
                var resolution = GetResolution( explicitMemberResolutions, usedExplicitResolutions, customResolutionIndex );
                if ( resolution.Factory is not null )
                {
                    _memberResolutions[i] = KeyValuePair.Create( member, (object?)resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( member, memberType, resolution.ImplementorKey );
            }

            if ( availableDependencies.TryGetValue( implementorKey, out var memberFactory ) )
            {
                _memberResolutions[i] = KeyValuePair.Create( member, (object?)memberFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                continue;
            }

            if ( IsInjectableMemberOptional( member, configuration ) ||
                (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                _memberResolutions[i] = KeyValuePair.Create( member, (object?)null );
                continue;
            }

            _errors = _errors.Extend( Resources.RequiredDependencyCannotBeResolved( member, implementorKey ) );
        }

        ValidateUnusedResolutions( explicitMemberResolutions, usedExplicitResolutions, explicitResolutionsLength );

        if ( configuration.TreatCaptiveDependenciesAsErrors )
            _errors = _errors.Extend( captiveDependencies );
        else
            _warnings = _warnings.Extend( captiveDependencies );

        if ( _errors.Count > 0 )
        {
            _parameterResolutions = null;
            _memberResolutions = null;
            return false;
        }

        return true;
    }

    protected sealed override void OnCircularDependencyDetected(List<DependencyGraphNode> path)
    {
        var pathSpan = CollectionsMarshal.AsSpan( path );

        var startIndex = pathSpan.Length - 2;
        while ( ! ReferenceEquals( pathSpan[startIndex].Factory, this ) )
            --startIndex;

        pathSpan = pathSpan.Slice( startIndex + 1 );

        foreach ( var pathNode in pathSpan )
            AddState( pathNode.Factory, DependencyResolverFactoryState.CircularDependenciesDetected );

        _errors = _errors.Extend( Resources.CircularDependenciesDetected( pathSpan ) );
    }

    protected sealed override void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 1 );

        if ( _parameterResolutions is not null )
        {
            foreach ( var (parameter, resolution) in _parameterResolutions )
            {
                if ( resolution is not DependencyResolverFactory factory )
                    continue;

                path[^1] = new DependencyGraphNode( parameter, factory );
                DetectCircularDependencies( factory, path );
            }
        }

        if ( _memberResolutions is not null )
        {
            foreach ( var (member, resolution) in _memberResolutions )
            {
                if ( resolution is not DependencyResolverFactory factory )
                    continue;

                path[^1] = new DependencyGraphNode( GetActualMember( member ), factory );
                DetectCircularDependencies( factory, path );
            }
        }
    }

    protected sealed override DependencyResolver CreateResolver(UlongSequenceGenerator idGenerator)
    {
        Assume.IsNotNull( _constructorInfo );
        var (expressionBuilder, parameterCount, memberCount) = CreateExpressionBuilder();

        for ( var i = 0; i < parameterCount; ++i )
        {
            Assume.IsNotNull( _parameterResolutions );
            var (parameter, resolution) = _parameterResolutions[i];
            var (instanceType, name) = (parameter.ParameterType, $"p{i}");

            if ( resolution is null )
                expressionBuilder.AddDefaultResolution( instanceType, name, parameter.HasDefaultValue, parameter.DefaultValue );
            else if ( resolution is Expression<Func<IDependencyScope, object>> expression )
                expressionBuilder.AddExpressionResolution( instanceType, name, expression );
            else
            {
                var factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator );
            }
        }

        var memberBindings = memberCount > 0 ? new MemberBinding[memberCount] : null;
        for ( var i = 0; i < memberCount; ++i )
        {
            Assume.IsNotNull( _memberResolutions );
            Assume.IsNotNull( memberBindings );

            var (member, resolution) = _memberResolutions[i];
            var memberType = GetInjectableMemberType( member );
            var (instanceType, name) = (memberType.GetGenericArguments()[0], $"m{i}");

            if ( resolution is null )
                expressionBuilder.AddDefaultResolution( instanceType, name );
            else if ( resolution is Expression<Func<IDependencyScope, object>> expression )
                expressionBuilder.AddExpressionResolution( instanceType, name, expression );
            else
            {
                var factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator );
            }

            var memberCtor = memberType.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .First(
                    c =>
                    {
                        var parameters = c.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType == instanceType;
                    } );

            memberBindings[i] = expressionBuilder.CreateMemberBindingForLastVariable( member, memberCtor );
        }

        var ctorParameters = expressionBuilder.GetVariableRange( parameterCount );
        var ctorCall = parameterCount > 0 ? Expression.New( _constructorInfo, ctorParameters ) : Expression.New( _constructorInfo );
        Expression instance = memberBindings is not null ? Expression.MemberInit( ctorCall, memberBindings ) : ctorCall;
        var result = expressionBuilder.Build( instance );
        return CreateFromExpression( result, idGenerator );
    }

    protected abstract DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator);

    protected abstract DependencyResolver CreateFromFactory(UlongSequenceGenerator idGenerator);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int FindCustomResolutionIndex<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        int resolutionsLength,
        T target)
        where T : class, ICustomAttributeProvider
    {
        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions );
            if ( resolutions[i].Predicate( target ) )
                return i;
        }

        return -1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static InjectableDependencyResolution<T> GetResolution<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int index)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( resolutions );
        Assume.IsNotNull( usedResolutions );
        usedResolutions[index] = true;
        return resolutions[index];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IDependencyKey ValidateDependencyImplementorType<T>(T target, Type dependencyType, IDependencyKey? implementorKey)
        where T : notnull
    {
        Assume.IsNotNull( implementorKey );

        if ( ! implementorKey.Type.IsAssignableTo( dependencyType ) )
        {
            var message = Resources.ProvidedImplementorTypeIsNotAssignableToDependencyType( target, implementorKey );
            _errors = _errors.Extend( message );
        }

        return implementorKey;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Chain<string> ValidateCaptiveDependency<T>(
        Chain<string> currentMessages,
        T target,
        IDependencyKey implementorKey,
        DependencyResolverFactory resolverFactory)
        where T : notnull
    {
        var captiveFactories = resolverFactory.GetCaptiveDependencyFactories( Lifetime );
        foreach ( var f in captiveFactories )
        {
            var message = Resources.CaptiveDependencyDetected( target, Lifetime, implementorKey, f.Lifetime, f.ImplementorKey.RangeIndex );
            currentMessages = currentMessages.Extend( message );
        }

        return currentMessages;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ValidateUnusedResolutions<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int resolutionsLength)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( _constructorInfo );

        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions );
            Assume.IsNotNull( usedResolutions );

            if ( usedResolutions[i] )
                continue;

            var message = Resources.UnusedResolution( _constructorInfo, i, resolutions[i] );
            _warnings = _warnings.Extend( message );
        }
    }

    [Pure]
    private MemberInfo[] FindInjectableMembers(IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( _constructorInfo );

        var result = _constructorInfo.DeclaringType?.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                static (member, criteria) =>
                {
                    var injectablePropertyType = ReinterpretCast.To<Type>( criteria );

                    if ( member is FieldInfo field )
                        return field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == injectablePropertyType;

                    return member is PropertyInfo property &&
                        property.PropertyType.IsGenericType &&
                        property.PropertyType.GetGenericTypeDefinition() == injectablePropertyType &&
                        property.GetSetMethod( nonPublic: true ) is not null &&
                        ! property.IsIndexer() &&
                        property.GetBackingField() is null;
                },
                configuration.InjectablePropertyType ) ??
            Array.Empty<MemberInfo>();

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type GetInjectableMemberType(MemberInfo member)
    {
        return member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).FieldType
            : ReinterpretCast.To<PropertyInfo>( member ).PropertyType;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static MemberInfo GetActualMember(MemberInfo member)
    {
        return member.MemberType == MemberTypes.Field
            ? ReinterpretCast.To<FieldInfo>( member ).GetBackedProperty() ?? member
            : member;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsInjectableMemberOptional(MemberInfo member, IDependencyContainerConfigurationBuilder configuration)
    {
        member = GetActualMember( member );
        return member.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static BitArray? ReuseBitArray(BitArray? current, int expectedLength)
    {
        if ( expectedLength == 0 )
            return null;

        if ( current is null )
            return new BitArray( expectedLength );

        current.Length = expectedLength;
        current.SetAll( false );
        return current;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ConstructorInfo? Info, Chain<string> Errors) FindValidConstructor(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNull( ImplementorBuilder.Factory );

        var errors = Chain<string>.Empty;
        var ctor = ImplementorBuilder.Constructor?.Info;

        if ( ctor is not null )
        {
            if ( ctor.DeclaringType?.IsAssignableTo( ImplementorKey.Value.Type ) != true )
                errors = errors.Extend( Resources.ProvidedConstructorDoesNotCreateInstancesOfCorrectType( ctor ) );

            if ( ctor.DeclaringType?.IsConstructable() != true )
                errors = errors.Extend( Resources.ProvidedConstructorBelongsToNonConstructableType( ctor ) );
        }
        else
        {
            Type type;
            var explicitType = ImplementorBuilder.Constructor?.Type;
            if ( explicitType is not null )
            {
                type = explicitType;
                if ( ! explicitType.IsAssignableTo( ImplementorKey.Value.Type ) )
                    errors = errors.Extend( Resources.ProvidedTypeIsIncorrect( explicitType ) );
            }
            else
                type = ImplementorKey.Value.Type;

            if ( ! type.IsConstructable() )
                errors = errors.Extend( Resources.ProvidedTypeIsNonConstructable( explicitType ) );

            if ( errors.Count == 0 )
            {
                ctor = FindBestSuitedCtor( type, availableDependencies, configuration );
                if ( ctor is null )
                    errors = errors.Extend( Resources.FailedToFindValidCtorForType( explicitType ) );
            }
        }

        return (ctor, errors);
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private (ExpressionBuilder Builder, int ParameterCount, int MemberCount) CreateExpressionBuilder()
    {
        var parameterCount = _parameterResolutions?.Length ?? 0;
        var memberCount = _memberResolutions?.Length ?? 0;
        var defaultResolutionCount = 0;
        var hasRequiredValueTypeDependency = false;

        for ( var i = 0; i < parameterCount; ++i )
        {
            Assume.IsNotNull( _parameterResolutions );
            var (parameter, resolution) = _parameterResolutions[i];

            if ( resolution is null )
            {
                ++defaultResolutionCount;
                continue;
            }

            if ( ! hasRequiredValueTypeDependency &&
                parameter.ParameterType.IsValueType &&
                Nullable.GetUnderlyingType( parameter.ParameterType ) is null )
                hasRequiredValueTypeDependency = true;
        }

        for ( var i = 0; i < memberCount; ++i )
        {
            Assume.IsNotNull( _memberResolutions );
            var (member, resolution) = _memberResolutions[i];

            if ( resolution is null )
            {
                ++defaultResolutionCount;
                continue;
            }

            if ( hasRequiredValueTypeDependency )
                continue;

            var memberType = GetInjectableMemberType( member ).GetGenericArguments()[0];
            if ( memberType.IsValueType && Nullable.GetUnderlyingType( memberType ) is null )
                hasRequiredValueTypeDependency = true;
        }

        var builder = new ExpressionBuilder(
            parameterCount + memberCount,
            defaultResolutionCount,
            hasRequiredValueTypeDependency,
            ImplementorKey.Value.Type,
            ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback );

        return (builder, parameterCount, memberCount);
    }

    private ConstructorInfo? FindBestSuitedCtor(
        Type type,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        const int notEligibleScore = -1;
        const int defaultScore = 1;

        var invocationOptions = ImplementorBuilder.Constructor?.InvocationOptions;
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;

        var constructors = type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        var scoredConstructors = new (ConstructorInfo Info, int Score, int ParameterCount)[constructors.Length];

        for ( var i = 0; i < constructors.Length; ++i )
        {
            var ctor = constructors[i];
            var parameters = ctor.GetParameters();
            var score = ctor.IsPublic ? defaultScore : 0;

            for ( var j = 0; j < parameters.Length; ++j )
            {
                var parameter = parameters[j];
                var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

                IDependencyKey implementorKey;
                if ( customResolutionIndex == -1 )
                    implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
                else
                {
                    Assume.IsNotNull( explicitParameterResolutions );
                    var resolution = explicitParameterResolutions[customResolutionIndex];
                    if ( resolution.Factory is not null )
                    {
                        score += defaultScore * 3;
                        continue;
                    }

                    Assume.IsNotNull( resolution.ImplementorKey );
                    if ( ! resolution.ImplementorKey.Type.IsAssignableTo( parameter.ParameterType ) )
                    {
                        score = notEligibleScore;
                        break;
                    }

                    score += defaultScore;
                    implementorKey = resolution.ImplementorKey;
                }

                if ( availableDependencies.TryGetValue( implementorKey, out var parameterFactory ) )
                {
                    if ( ! parameterFactory.IsCaptiveDependencyOf( Lifetime ) )
                        score += defaultScore * 2;

                    continue;
                }

                if ( ! parameter.HasDefaultValue &&
                    ! parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false ) &&
                    ! (parameter.ParameterType.IsGenericType &&
                        parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
                {
                    score = notEligibleScore;
                    break;
                }

                score += defaultScore;
            }

            scoredConstructors[i] = (ctor, score, parameters.Length);
        }

        (ConstructorInfo Info, int Score, int ParameterCount)? result = null;
        for ( var i = 0; i < scoredConstructors.Length; ++i )
        {
            var other = scoredConstructors[i];
            if ( other.Score == notEligibleScore )
                continue;

            if ( result is null ||
                other.Score > result.Value.Score ||
                (other.Score == result.Value.Score && other.ParameterCount > result.Value.ParameterCount) )
                result = other;
        }

        return result?.Info;
    }
}
