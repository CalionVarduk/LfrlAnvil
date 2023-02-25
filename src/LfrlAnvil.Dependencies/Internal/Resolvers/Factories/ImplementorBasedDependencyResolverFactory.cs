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

internal abstract class ImplementorBasedDependencyResolverFactory : DependencyResolverFactory
{
    private static readonly MethodInfo ResolverCreateMethod =
        typeof( DependencyResolver ).GetMethod( nameof( DependencyResolver.Create ), BindingFlags.Instance | BindingFlags.NonPublic )!;

    private static readonly ConstructorInfo ExceptionCtor =
        typeof( InvalidDependencyCastException ).GetConstructor( new[] { typeof( Type ) } )!;

    private ConstructorInfo? _constructorInfo;
    private KeyValuePair<ParameterInfo, object?>[]? _parameterResolutions;
    private KeyValuePair<MemberInfo, object?>[]? _memberResolutions;
    private Chain<string> _errors;
    private Chain<string> _warnings;

    protected ImplementorBasedDependencyResolverFactory(
        ImplementorKey implementorKey,
        IDependencyImplementorBuilder? implementorBuilder,
        DependencyLifetime lifetime)
        : base( lifetime )
    {
        ImplementorKey = implementorKey;
        ImplementorBuilder = implementorBuilder;
        _constructorInfo = null;
        _parameterResolutions = null;
        _memberResolutions = null;
        _errors = Chain<string>.Empty;
        _warnings = Chain<string>.Empty;
    }

    internal ImplementorKey ImplementorKey { get; }
    internal IDependencyImplementorBuilder? ImplementorBuilder { get; }
    internal IInternalDependencyKey InternalImplementorKey => ReinterpretCast.To<IInternalDependencyKey>( ImplementorKey.Value );

    [Pure]
    internal sealed override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return Lifetime < lifetime;
    }

    [Pure]
    internal sealed override DependencyContainerBuildMessages? GetMessages()
    {
        if ( _errors.Count == 0 && _warnings.Count == 0 )
            return null;

        return new DependencyContainerBuildMessages( ImplementorKey, _errors, _warnings );
    }

    internal void PrepareCreationMethod(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies)
    {
        if ( State != DependencyResolverFactoryState.Created )
            return;

        if ( ImplementorBuilder?.Factory is not null )
        {
            var resolver = CreateFromFactory( idGenerator );
            Finish( resolver );
            return;
        }

        var result = FindValidConstructor( availableDependencies );
        if ( result.Errors.Count > 0 )
        {
            _errors = _errors.Extend( result.Errors );
            FinishAsInvalid();
        }
        else
        {
            _constructorInfo = result.Info;
            SetState( DependencyResolverFactoryState.Validatable );
        }
    }

    internal void ValidateRequiredDependencies(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( State != DependencyResolverFactoryState.Validatable )
            return;

        Assume.IsNotNull( _constructorInfo, nameof( _constructorInfo ) );

        var captiveDependencies = Chain<string>.Empty;
        var invocationOptions = ImplementorBuilder?.Constructor?.InvocationOptions;

        var parameters = _constructorInfo.GetParameters();
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;
        var usedExplicitResolutions = explicitResolutionsLength > 0 ? new BitArray( explicitResolutionsLength ) : null;

        if ( parameters.Length > 0 )
            _parameterResolutions = new KeyValuePair<ParameterInfo, object?>[parameters.Length];

        for ( var i = 0; i < parameters.Length; ++i )
        {
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
                    _parameterResolutions![i] = KeyValuePair.Create( parameter, (object?)resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( parameter, parameter.ParameterType, resolution.ImplementorKey );
            }

            if ( availableDependencies.TryGetValue( implementorKey, out var parameterFactory ) )
            {
                _parameterResolutions![i] = KeyValuePair.Create( parameter, (object?)parameterFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                continue;
            }

            if ( parameter.HasDefaultValue || parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false ) )
            {
                _parameterResolutions![i] = KeyValuePair.Create( parameter, (object?)null );
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
                    _memberResolutions![i] = KeyValuePair.Create( member, (object?)resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( member, memberType, resolution.ImplementorKey );
            }

            if ( availableDependencies.TryGetValue( implementorKey, out var memberFactory ) )
            {
                _memberResolutions![i] = KeyValuePair.Create( member, (object?)memberFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                continue;
            }

            if ( IsInjectableMemberOptional( member, configuration ) )
            {
                _memberResolutions![i] = KeyValuePair.Create( member, (object?)null );
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
            FinishAsInvalid();
        }
        else
            SetState( DependencyResolverFactoryState.ValidatedRequiredDependencies );
    }

    internal void ValidateCircularDependencies(List<(object? ReachedFrom, ImplementorBasedDependencyResolverFactory Node)> pathBuffer)
    {
        if ( State != DependencyResolverFactoryState.ValidatedRequiredDependencies )
            return;

        Assume.IsEmpty( pathBuffer, nameof( pathBuffer ) );

        pathBuffer.Add( (null, this) );
        DetectCircularDependencies( pathBuffer );

        Assume.ContainsExactly( pathBuffer, 1, nameof( pathBuffer ) );
        pathBuffer.Clear();
    }

    internal void Build(UlongSequenceGenerator idGenerator)
    {
        if ( IsFinished )
            return;

        var resolver = CreateResolver( idGenerator );
        Finish( resolver );
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
        Assume.IsNotNull( resolutions, nameof( resolutions ) );

        for ( var i = 0; i < resolutionsLength; ++i )
        {
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
        Assume.IsNotNull( resolutions, nameof( resolutions ) );
        Assume.IsNotNull( usedResolutions, nameof( usedResolutions ) );

        usedResolutions[index] = true;
        return resolutions[index];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IDependencyKey ValidateDependencyImplementorType<T>(T target, Type dependencyType, IDependencyKey? implementorKey)
        where T : notnull
    {
        Assume.IsNotNull( implementorKey, nameof( implementorKey ) );

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
        if ( ! resolverFactory.IsCaptiveDependencyOf( Lifetime ) )
            return currentMessages;

        var message = Resources.CaptiveDependencyDetected( target, Lifetime, implementorKey, resolverFactory.Lifetime );
        return currentMessages.Extend( message );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ValidateUnusedResolutions<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int resolutionsLength)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( _constructorInfo, nameof( _constructorInfo ) );

        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions, nameof( resolutions ) );
            Assume.IsNotNull( usedResolutions, nameof( usedResolutions ) );

            if ( usedResolutions[i] )
                continue;

            var message = Resources.UnusedResolution( _constructorInfo, i, resolutions[i] );
            _warnings = _warnings.Extend( message );
        }
    }

    [Pure]
    private MemberInfo[] FindInjectableMembers(IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( _constructorInfo, nameof( _constructorInfo ) );

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
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies)
    {
        Assume.IsNull( ImplementorBuilder?.Factory, nameof( ImplementorBuilder.Factory ) );

        var errors = Chain<string>.Empty;
        var ctor = ImplementorBuilder?.Constructor?.Info;

        if ( ctor is null )
        {
            if ( ! ImplementorKey.Value.Type.IsConstructable() )
                errors = errors.Extend( Resources.FailedToFindValidCtor );
            else
            {
                // TODO: try to find best possible ctor for Type

                if ( ctor is null )
                    errors = errors.Extend( Resources.FailedToFindValidCtor );
            }
        }
        else
        {
            if ( ctor.DeclaringType?.IsAssignableTo( ImplementorKey.Value.Type ) != true )
                errors = errors.Extend( Resources.ProvidedConstructorDoesNotCreateInstancesOfCorrectType( ctor ) );

            if ( ctor.DeclaringType?.IsConstructable() != true )
                errors = errors.Extend( Resources.ProvidedConstructorBelongsToNonConstructableType( ctor ) );
        }

        return (ctor, errors);
    }

    private void DetectCircularDependencies(List<(object? ReachedFrom, ImplementorBasedDependencyResolverFactory Node)> path)
    {
        if ( HasAnyState( DependencyResolverFactoryState.CanRegisterCircularDependency ) )
        {
            Assume.ContainsAtLeast( path, 2, nameof( path ) );
            var pathSpan = CollectionsMarshal.AsSpan( path );

            var startIndex = pathSpan.Length - 2;
            while ( ! ReferenceEquals( pathSpan[startIndex].Node, this ) )
                --startIndex;

            pathSpan = pathSpan.Slice( startIndex + 1 );

            foreach ( var pathNode in pathSpan )
                pathNode.Node.AddState( DependencyResolverFactoryState.CircularDependenciesDetected );

            _errors = _errors.Extend( Resources.CircularDependenciesDetected( pathSpan ) );
            return;
        }

        Assume.Equals( State, DependencyResolverFactoryState.ValidatedRequiredDependencies, nameof( State ) );
        SetState( DependencyResolverFactoryState.ValidatingCircularDependencies );
        path.Add( default );

        if ( _parameterResolutions is not null )
        {
            foreach ( var (parameter, resolution) in _parameterResolutions )
            {
                if ( resolution is not ImplementorBasedDependencyResolverFactory factory ||
                    factory.HasAnyState( DependencyResolverFactoryState.Validated | DependencyResolverFactoryState.Finished ) )
                    continue;

                path[^1] = (parameter, factory);
                factory.DetectCircularDependencies( path );
            }
        }

        if ( _memberResolutions is not null )
        {
            foreach ( var (member, resolution) in _memberResolutions )
            {
                if ( resolution is not ImplementorBasedDependencyResolverFactory factory ||
                    factory.HasAnyState( DependencyResolverFactoryState.Validated | DependencyResolverFactoryState.Finished ) )
                    continue;

                path[^1] = (GetActualMember( member ), factory);
                factory.DetectCircularDependencies( path );
            }
        }

        path.RemoveLast();

        if ( HasState( DependencyResolverFactoryState.CircularDependenciesDetected ) )
            FinishWithCircularDependencies();
        else
            SetState( DependencyResolverFactoryState.Validated );
    }

    private DependencyResolver CreateResolver(UlongSequenceGenerator idGenerator)
    {
        Assume.Equals( IsFinished, false, nameof( IsFinished ) );
        Assume.IsNotNull( _constructorInfo, nameof( _constructorInfo ) );

        var scopeParameter = Expression.Parameter( typeof( DependencyScope ), "scope" );
        var (parameterCount, memberCount, defaultResolutionCount, valueTypeBuffer) = GetInitialExpressionInfo();
        var dependencyCount = parameterCount + memberCount;
        Expression<Func<DependencyScope, object>> result;

        if ( dependencyCount == 0 )
        {
            result = Expression.Lambda<Func<DependencyScope, object>>( Expression.New( _constructorInfo ), scopeParameter );
            return CreateFromExpression( result, idGenerator );
        }

        Expression? abstractScopeParameter = null;
        var nullConst = Expression.Constant( null );

        var variableIndex = 0;
        var variables = new ParameterExpression[dependencyCount + (valueTypeBuffer is not null ? 1 : 0)];
        if ( valueTypeBuffer is not null )
            variables[^1] = valueTypeBuffer;

        var blockIndex = 0;
        var block = new Expression[defaultResolutionCount + (dependencyCount - defaultResolutionCount) * 2 + 1];

        for ( var i = 0; i < parameterCount; ++i )
        {
            var (parameter, resolution) = _parameterResolutions![i];
            var variable = Expression.Variable( parameter.ParameterType, $"p{i}" );
            variables[variableIndex++] = variable;

            if ( resolution is null )
            {
                AddDefaultValueAssignment( block, blockIndex++, variable, parameter.HasDefaultValue, parameter.DefaultValue );
                continue;
            }

            var rawValue = GetRawValueExpression(
                idGenerator,
                resolution,
                parameter.ParameterType,
                scopeParameter,
                ref abstractScopeParameter );

            AddResolvedValueAssignment( block, blockIndex, variable, valueTypeBuffer, rawValue, nullConst );
            blockIndex += 2;
        }

        var memberBindings = memberCount > 0 ? new MemberBinding[memberCount] : null;
        for ( var i = 0; i < memberCount; ++i )
        {
            var (member, resolution) = _memberResolutions![i];
            var memberType = GetInjectableMemberType( member );
            var instanceType = memberType.GetGenericArguments()[0];
            var variable = Expression.Variable( instanceType, $"m{i}" );
            variables[variableIndex++] = variable;

            if ( resolution is null )
                AddDefaultValueAssignment( block, blockIndex++, variable );
            else
            {
                var rawValue = GetRawValueExpression( idGenerator, resolution, instanceType, scopeParameter, ref abstractScopeParameter );
                AddResolvedValueAssignment( block, blockIndex, variable, valueTypeBuffer, rawValue, nullConst );
                blockIndex += 2;
            }

            var memberCtor = memberType.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .First(
                    c =>
                    {
                        var parameters = c.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType == instanceType;
                    } );

            memberBindings![i] = Expression.Bind( member, Expression.New( memberCtor, variable ) );
        }

        var ctorCall = parameterCount > 0
            ? Expression.New( _constructorInfo, parameterCount == variables.Length ? variables : variables.Take( parameterCount ) )
            : Expression.New( _constructorInfo );

        block[^1] = memberBindings is null ? ctorCall : Expression.MemberInit( ctorCall, memberBindings );
        result = Expression.Lambda<Func<DependencyScope, object>>( Expression.Block( variables, block ), scopeParameter );
        return CreateFromExpression( result, idGenerator );
    }

    [Pure]
    private (int ParameterCount, int MemberCount, int DefaultResolutionCount, ParameterExpression? ValueTypeBuffer)
        GetInitialExpressionInfo()
    {
        var parameterCount = _parameterResolutions?.Length ?? 0;
        var memberCount = _memberResolutions?.Length ?? 0;
        var defaultResolutionCount = 0;
        ParameterExpression? valueTypeBuffer = null;

        for ( var i = 0; i < parameterCount; ++i )
        {
            var (parameter, resolution) = _parameterResolutions![i];

            if ( resolution is null )
                ++defaultResolutionCount;

            if ( valueTypeBuffer is null &&
                parameter.ParameterType.IsValueType &&
                Nullable.GetUnderlyingType( parameter.ParameterType ) is null )
                valueTypeBuffer = Expression.Variable( typeof( object ), "b" );
        }

        for ( var i = 0; i < memberCount; ++i )
        {
            var (member, resolution) = _memberResolutions![i];

            if ( resolution is null )
                ++defaultResolutionCount;

            if ( valueTypeBuffer is not null )
                continue;

            var memberType = GetInjectableMemberType( member ).GetGenericArguments()[0];
            if ( memberType.IsValueType && Nullable.GetUnderlyingType( memberType ) is null )
                valueTypeBuffer = Expression.Variable( typeof( object ), "b" );
        }

        return (parameterCount, memberCount, defaultResolutionCount, valueTypeBuffer);
    }

    private static void AddDefaultValueAssignment(
        Expression[] block,
        int index,
        ParameterExpression variable,
        bool hasDefaultValue = false,
        object? defaultValue = null)
    {
        Expression value = hasDefaultValue ? Expression.Constant( defaultValue, variable.Type ) : Expression.Default( variable.Type );
        var assignment = Expression.Assign( variable, value );
        block[index] = assignment;
    }

    private static void AddResolvedValueAssignment(
        Expression[] block,
        int index,
        ParameterExpression variable,
        ParameterExpression? valueTypeBuffer,
        Expression rawValue,
        ConstantExpression nullConstant)
    {
        var exception = Expression.New( ExceptionCtor, Expression.Constant( variable.Type, typeof( Type ) ) );

        if ( ! variable.Type.IsValueType )
        {
            var value = Expression.TypeAs( rawValue, variable.Type );
            var assignment = Expression.Assign( variable, value );
            var nullEquality = Expression.ReferenceEqual( variable, nullConstant );
            var ifNull = Expression.IfThen( nullEquality, Expression.Throw( exception ) );

            block[index] = assignment;
            block[index + 1] = ifNull;
        }
        else if ( Nullable.GetUnderlyingType( variable.Type ) is not null )
        {
            var value = Expression.TypeAs( rawValue, variable.Type );
            var assignment = Expression.Assign( variable, value );
            var nullEquality = Expression.Equal( variable, Expression.Constant( null, variable.Type ) );
            var ifNull = Expression.IfThen( nullEquality, Expression.Throw( exception ) );

            block[index] = assignment;
            block[index + 1] = ifNull;
        }
        else
        {
            Assume.IsNotNull( valueTypeBuffer, nameof( valueTypeBuffer ) );

            var bufferAssignment = Expression.Assign( valueTypeBuffer, rawValue );
            var typeCheck = Expression.TypeIs( valueTypeBuffer, variable.Type );
            var value = Expression.Condition(
                typeCheck,
                Expression.Convert( valueTypeBuffer, variable.Type ),
                Expression.Throw( exception, variable.Type ) );

            var assignment = Expression.Assign( variable, value );

            block[index] = bufferAssignment;
            block[index + 1] = assignment;
        }
    }

    private static Expression GetRawValueExpression(
        UlongSequenceGenerator idGenerator,
        object resolution,
        Type dependencyType,
        ParameterExpression scopeParameter,
        ref Expression? abstractScopeParameter)
    {
        if ( resolution is Expression<Func<IDependencyScope, object>> expression )
        {
            var expressionParameter = expression.Parameters[0];
            abstractScopeParameter ??= Expression.Convert( scopeParameter, typeof( IDependencyScope ) );

            return expressionParameter.Name is not null
                ? expression.Body.ReplaceParameters(
                    new Dictionary<string, Expression> { { expressionParameter.Name, abstractScopeParameter } } )
                : Expression.Invoke( expression, abstractScopeParameter );
        }

        var factory = ReinterpretCast.To<DependencyResolverFactory>( resolution );
        if ( ! factory.IsInternal )
            ReinterpretCast.To<ImplementorBasedDependencyResolverFactory>( factory ).Build( idGenerator );

        var resolver = factory.GetResolver();

        return Expression.Call(
            Expression.Constant( resolver, typeof( DependencyResolver ) ),
            ResolverCreateMethod,
            scopeParameter,
            Expression.Constant( dependencyType, typeof( Type ) ) );
    }
}
