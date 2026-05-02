// Copyright 2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class OpenGenericDependencyResolverFactory : DependencyResolverFactory
{
    internal ConstructorInfo? ConstructorInfo;
    internal KeyValuePair<ParameterInfo, DependencyResolverFactory?>[]? ParameterResolutions;
    internal KeyValuePair<MemberInfo, DependencyResolverFactory?>[]? MemberResolutions;
    private Chain<string> _errors;
    private Chain<string> _warnings;

    private OpenGenericDependencyResolverFactory(
        ImplementorKey implementorKey,
        IOpenGenericDependencyImplementorBuilder implementorBuilder,
        DependencyLifetime lifetime)
        : base( implementorKey, lifetime, isOpenGeneric: true )
    {
        ImplementorBuilder = implementorBuilder;
        ConstructorInfo = null;
        ParameterResolutions = null;
        MemberResolutions = null;
        _errors = Chain<string>.Empty;
        _warnings = Chain<string>.Empty;
    }

    internal IOpenGenericDependencyImplementorBuilder ImplementorBuilder { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static OpenGenericDependencyResolverFactory Create(
        ImplementorKey implementorKey,
        DependencyLifetime lifetime,
        IOpenGenericDependencyImplementorBuilder implementorBuilder)
    {
        return new OpenGenericDependencyResolverFactory( implementorKey, implementorBuilder, lifetime );
    }

    public DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );

        if ( ImplementorKey.IsShared )
        {
            var sharedResolverFactory = CloseShared( dependencyKey, @params );
            dynamicResolverFactories.Add( dependencyKey, sharedResolverFactory );
            return sharedResolverFactory;
        }

        ref var parameterFactory
            = ref CollectionsMarshal.GetValueRefOrAddDefault( dynamicResolverFactories, dependencyKey, out var exists )!;

        if ( ! exists )
            parameterFactory = RegisteredClosedGenericDependencyResolverFactory.Create(
                this,
                ImplementorKey.Create( dependencyKey, ImplementorKey.RangeIndex ) );

        return parameterFactory;
    }

    public DependencyResolverFactory CloseShared(
        IInternalDependencyKey dependencyKey,
        DependencyLocatorBuilderExtractionParams @params)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );
        Assume.True( ImplementorKey.IsShared );

        var sharedImplementorType = ImplementorKey.Value.Type.CloseImplementorType( dependencyKey.Type );
        var sharedImplementorKey = InternalImplementorKey.WithType( sharedImplementorType );
        var sharedResolverFactory = @params.GetSharedResolverFactory( sharedImplementorKey, Lifetime );
        if ( sharedResolverFactory is null )
        {
            sharedResolverFactory = RegisteredClosedGenericDependencyResolverFactory.Create(
                this,
                ImplementorKey.CreateShared( sharedImplementorKey ) );

            @params.AddSharedResolverFactory( sharedImplementorKey, Lifetime, sharedResolverFactory );
        }

        return sharedResolverFactory;
    }

    [Pure]
    internal override Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        return IsCaptiveDependencyOf( lifetime ) ? Chain.Create<DependencyResolverFactory>( this ) : Chain<DependencyResolverFactory>.Empty;
    }

    [Pure]
    internal override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return Lifetime < lifetime;
    }

    [Pure]
    protected override Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        return _errors.Count == 0 && _warnings.Count == 0
            ? Chain<DependencyContainerBuildMessages>.Empty
            : Chain.Create( new DependencyContainerBuildMessages( ImplementorKey, _errors, _warnings ) );
    }

    protected override bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        var result = FindValidConstructor( availableDependencies, configuration );
        if ( result.Errors.Count == 0 )
        {
            ConstructorInfo = result.Info;
            return true;
        }

        _errors = _errors.Extend( result.Errors );
        return false;
    }

    protected override bool AreRequiredDependenciesValid(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( ConstructorInfo );

        var captiveDependencies = Chain<string>.Empty;
        var invocationOptions = ImplementorBuilder.Constructor?.InvocationOptions;

        var parameters = ConstructorInfo.GetParameters();
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;
        var usedExplicitResolutions = explicitResolutionsLength > 0 ? new BitArray( explicitResolutionsLength ) : null;

        if ( parameters.Length > 0 )
            ParameterResolutions = new KeyValuePair<ParameterInfo, DependencyResolverFactory?>[parameters.Length];

        for ( var i = 0; i < parameters.Length; ++i )
        {
            Assume.IsNotNull( ParameterResolutions );

            var parameter = parameters[i];
            var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

            IDependencyKey implementorKey;
            if ( customResolutionIndex == -1 )
                implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
            else
            {
                var resolution = GetResolution( explicitParameterResolutions, usedExplicitResolutions, customResolutionIndex );
                implementorKey = ValidateDependencyImplementorType( parameter, parameter.ParameterType, resolution.ImplementorKey );
            }

            if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
            {
                ParameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )parameterFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                continue;
            }

            if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( internalKey.Type.GetGenericTypeDefinition() );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out parameterFactory )
                    && parameterFactory is OpenGenericDependencyResolverFactory genericParameterFactory )
                {
                    if ( ! implementorKey.Type.ContainsGenericParameters )
                        parameterFactory = genericParameterFactory.Close( internalKey, @params, dynamicResolverFactories );

                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )parameterFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                    continue;
                }
            }

            if ( parameter.HasDefaultValue
                || parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false )
                || (parameter.ParameterType.IsGenericType
                    && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                ParameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )null );
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
            MemberResolutions = new KeyValuePair<MemberInfo, DependencyResolverFactory?>[injectableMembers.Length];

        for ( var i = 0; i < injectableMembers.Length; ++i )
        {
            Assume.IsNotNull( MemberResolutions );

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
                implementorKey = ValidateDependencyImplementorType( member, memberType, resolution.ImplementorKey );
            }

            if ( @params.ResolverFactories.TryGetValue( implementorKey, out var memberFactory ) )
            {
                MemberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )memberFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                continue;
            }

            if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( internalKey.Type.GetGenericTypeDefinition() );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out memberFactory )
                    && memberFactory is OpenGenericDependencyResolverFactory genericMemberFactory )
                {
                    if ( ! implementorKey.Type.ContainsGenericParameters )
                        memberFactory = genericMemberFactory.Close( internalKey, @params, dynamicResolverFactories );

                    MemberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )memberFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                    continue;
                }
            }

            if ( IsInjectableMemberOptional( member, configuration )
                || (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                MemberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )null );
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
            ParameterResolutions = null;
            MemberResolutions = null;
            return false;
        }

        return true;
    }

    protected override void OnCircularDependencyDetected(List<DependencyGraphNode> path)
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

    protected override void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 1 );

        if ( ParameterResolutions is not null )
        {
            foreach ( var (parameter, factory) in ParameterResolutions )
            {
                if ( factory is null )
                    continue;

                path[^1] = new DependencyGraphNode( parameter, factory );
                DetectCircularDependencies( factory, path );
            }
        }

        if ( MemberResolutions is not null )
        {
            foreach ( var (member, factory) in MemberResolutions )
            {
                if ( factory is null )
                    continue;

                path[^1] = new DependencyGraphNode( GetActualMember( member ), factory );
                DetectCircularDependencies( factory, path );
            }
        }
    }

    protected override DependencyResolver CreateResolver(UlongSequenceGenerator idGenerator)
    {
        return base.CreateResolver( idGenerator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int FindCustomResolutionIndex<T>(
        IReadOnlyList<OpenGenericInjectableDependencyResolution<T>>? resolutions,
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
    private static OpenGenericInjectableDependencyResolution<T> GetResolution<T>(
        IReadOnlyList<OpenGenericInjectableDependencyResolution<T>>? resolutions,
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

        if ( ! dependencyType.IsAnyResolvableBy( implementorKey.Type ) )
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
        IReadOnlyList<OpenGenericInjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int resolutionsLength)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( ConstructorInfo );

        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions );
            Assume.IsNotNull( usedResolutions );

            if ( usedResolutions[i] )
                continue;

            var resolution = resolutions[i];
            var message = Resources.UnusedResolution<T>( ConstructorInfo, i, resolution.ImplementorKey, factory: null );
            _warnings = _warnings.Extend( message );
        }
    }

    [Pure]
    private MemberInfo[] FindInjectableMembers(IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( ConstructorInfo );

        var result = ConstructorInfo.DeclaringType?.FindMembers(
                MemberTypes.Field | MemberTypes.Property,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                static (member, criteria) =>
                {
                    var injectablePropertyType = ReinterpretCast.To<Type>( criteria );

                    if ( member is FieldInfo field )
                        return field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == injectablePropertyType;

                    return member is PropertyInfo property
                        && property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == injectablePropertyType
                        && property.GetSetMethod( nonPublic: true ) is not null
                        && ! property.IsIndexer()
                        && property.GetBackingField() is null;
                },
                configuration.InjectablePropertyType )
            ?? Array.Empty<MemberInfo>();

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
        var errors = Chain<string>.Empty;
        var ctor = ImplementorBuilder.Constructor?.Info;

        if ( ctor is not null )
        {
            var declaringType = ctor.DeclaringType;
            if ( declaringType is null || ! declaringType.IsOpenGenericAssignableTo( ImplementorKey.Value.Type ) )
                errors = errors.Extend( Resources.ProvidedConstructorDoesNotCreateInstancesOfCorrectType( ctor ) );

            if ( declaringType?.IsAbstract != false )
                errors = errors.Extend( Resources.ProvidedConstructorBelongsToNonConstructableType( ctor ) );
        }
        else
        {
            Type type;
            var explicitType = ImplementorBuilder.Constructor?.Type;
            if ( explicitType is not null )
            {
                type = explicitType;
                if ( ! explicitType.IsOpenGenericAssignableTo( ImplementorKey.Value.Type ) )
                    errors = errors.Extend( Resources.ProvidedTypeIsIncorrect( explicitType ) );
            }
            else
                type = ImplementorKey.Value.Type;

            if ( type.IsAbstract )
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

            foreach ( var parameter in parameters )
            {
                var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

                IDependencyKey implementorKey;
                if ( customResolutionIndex == -1 )
                    implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
                else
                {
                    Assume.IsNotNull( explicitParameterResolutions );
                    var resolution = explicitParameterResolutions[customResolutionIndex];
                    if ( ! parameter.ParameterType.IsAnyResolvableBy( resolution.ImplementorKey.Type ) )
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

                if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
                {
                    var openGenericKey = internalKey.WithType( implementorKey.Type.GetGenericTypeDefinition() );
                    if ( availableDependencies.TryGetValue( openGenericKey, out parameterFactory ) )
                    {
                        if ( ! parameterFactory.IsCaptiveDependencyOf( Lifetime ) )
                            score += defaultScore * 2;

                        continue;
                    }
                }

                if ( ! parameter.HasDefaultValue
                    && ! parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false )
                    && ! (parameter.ParameterType.IsGenericType
                        && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
                {
                    score = notEligibleScore;
                    break;
                }

                score += defaultScore;
            }

            scoredConstructors[i] = (ctor, score, parameters.Length);
        }

        (ConstructorInfo Info, int Score, int ParameterCount)? result = null;
        foreach ( var other in scoredConstructors )
        {
            if ( other.Score == notEligibleScore )
                continue;

            if ( result is null
                || other.Score > result.Value.Score
                || (other.Score == result.Value.Score && other.ParameterCount > result.Value.ParameterCount) )
                result = other;
        }

        return result?.Info;
    }
}
