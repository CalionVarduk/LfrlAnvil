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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredClosedGenericDependencyResolverFactory : DependencyResolverFactory
{
    private ConstructorInfo? _constructorInfo;
    private KeyValuePair<ParameterInfo, DependencyResolverFactory?>[]? _parameterResolutions;
    private KeyValuePair<MemberInfo, DependencyResolverFactory?>[]? _memberResolutions;
    private Chain<string> _errors;
    private Chain<string> _warnings;

    protected RegisteredClosedGenericDependencyResolverFactory(ImplementorKey implementorKey, OpenGenericDependencyResolverFactory @base)
        : base( implementorKey, @base.Lifetime, isOpenGeneric: false )
    {
        Base = @base;
        _constructorInfo = null;
        _parameterResolutions = null;
        _memberResolutions = null;
        _errors = Chain<string>.Empty;
        _warnings = Chain<string>.Empty;
    }

    internal OpenGenericDependencyResolverFactory Base { get; }

    [Pure]
    internal static RegisteredClosedGenericDependencyResolverFactory Create(
        OpenGenericDependencyResolverFactory @base,
        ImplementorKey implementorKey)
    {
        Assume.True( implementorKey.Value.Type.IsGenericType && ! implementorKey.Value.Type.IsGenericTypeDefinition );
        Assume.True( @base.ImplementorKey.Value.Type.IsOpenGenericAssignableTo( implementorKey.Value.Type.GetGenericTypeDefinition() ) );

        RegisteredClosedGenericDependencyResolverFactory result = @base.Lifetime switch
        {
            DependencyLifetime.Singleton => new SingletonClosedGenericDependencyResolverFactory( implementorKey, @base ),
            DependencyLifetime.ScopedSingleton => new ScopedSingletonClosedGenericDependencyResolverFactory( implementorKey, @base ),
            DependencyLifetime.Scoped => new ScopedClosedGenericDependencyResolverFactory( implementorKey, @base ),
            _ => new TransientClosedGenericDependencyResolverFactory( implementorKey, @base )
        };

        return result;
    }

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
        Base.PrepareCreationMethod( idGenerator, availableDependencies, configuration );
        if ( Base.HasState( DependencyResolverFactoryState.Invalid ) )
            return false;

        var openCtor = Base.ConstructorInfo;
        Assume.IsNotNull( openCtor );
        _constructorInfo = openCtor.TryCloseGenericCtor( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type );
        if ( _constructorInfo is not null )
            return true;

        _errors = _errors.Extend(
            Resources.FailedToFindValidCtorForClosedGenericType( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type ) );

        return false;
    }

    protected sealed override bool AreRequiredDependenciesValid(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Base.ValidateRequiredDependencies( @params, dynamicResolverFactories, configuration );
        if ( Base.HasState( DependencyResolverFactoryState.Invalid ) )
            return false;

        Assume.IsNotNull( _constructorInfo );
        var captiveDependencies = Chain<string>.Empty;

        var parameters = _constructorInfo.GetParameters();
        if ( parameters.Length > 0 )
        {
            Assume.IsNotNull( Base.ParameterResolutions );
            Assume.Equals( Base.ParameterResolutions.Length, parameters.Length );
            _parameterResolutions = new KeyValuePair<ParameterInfo, DependencyResolverFactory?>[parameters.Length];

            for ( var i = 0; i < _parameterResolutions.Length; ++i )
            {
                var parameter = parameters[i];
                var baseResolution = Base.ParameterResolutions[i];
                if ( baseResolution.Value is null )
                {
                    var implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        _parameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                        continue;
                    }

                    _parameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )null );
                }
                else if ( ! baseResolution.Value.IsOpenGeneric )
                    _parameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )baseResolution.Value );
                else
                {
                    var implementorKey = InternalImplementorKey.WithType(
                        baseResolution.Value.InternalImplementorKey.Type.CloseImplementorType( parameter.ParameterType ) );

                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        _parameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                        continue;
                    }

                    var genericParameterFactory = ReinterpretCast.To<OpenGenericDependencyResolverFactory>( baseResolution.Value );
                    parameterFactory = genericParameterFactory.Close( implementorKey, @params, dynamicResolverFactories );

                    _parameterResolutions[i] = KeyValuePair.Create( parameter, ( DependencyResolverFactory? )parameterFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                }
            }
        }

        var injectableMembers = _constructorInfo.DeclaringType?.FindInjectableMembers( configuration.InjectablePropertyType ) ?? [ ];
        if ( injectableMembers.Count > 0 )
        {
            Assume.IsNotNull( Base.MemberResolutions );
            Assume.Equals( Base.MemberResolutions.Length, injectableMembers.Count );
            _memberResolutions = new KeyValuePair<MemberInfo, DependencyResolverFactory?>[injectableMembers.Count];

            for ( var i = 0; i < _memberResolutions.Length; ++i )
            {
                var member = injectableMembers[i];
                var memberInjectableType = GetInjectableMemberType( member );
                var memberType = memberInjectableType.GetGenericArguments()[0];
                var baseResolution = Base.MemberResolutions[i];

                if ( baseResolution.Value is null )
                {
                    var implementorKey = InternalImplementorKey.WithType( memberType );
                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        _memberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, parameterFactory );
                        continue;
                    }

                    _memberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )null );
                }
                else if ( ! baseResolution.Value.IsOpenGeneric )
                    _memberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )baseResolution.Value );
                else
                {
                    var implementorKey = InternalImplementorKey.WithType(
                        baseResolution.Value.InternalImplementorKey.Type.CloseImplementorType( memberType ) );

                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        _memberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, parameterFactory );
                        continue;
                    }

                    var genericParameterFactory = ReinterpretCast.To<OpenGenericDependencyResolverFactory>( baseResolution.Value );
                    parameterFactory = genericParameterFactory.Close( implementorKey, @params, dynamicResolverFactories );

                    _memberResolutions[i] = KeyValuePair.Create( member, ( DependencyResolverFactory? )parameterFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, parameterFactory );
                }
            }
        }

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
            foreach ( var (parameter, factory) in _parameterResolutions )
            {
                if ( factory is null )
                    continue;

                path[^1] = new DependencyGraphNode( parameter, factory );
                DetectCircularDependencies( factory, path );
            }
        }

        if ( _memberResolutions is not null )
        {
            foreach ( var (member, factory) in _memberResolutions )
            {
                if ( factory is null )
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
            var (parameter, factory) = _parameterResolutions[i];
            var (instanceType, name) = (parameter.ParameterType, $"p{i}");

            if ( factory is null )
                expressionBuilder.AddDefaultResolution( instanceType, name, parameter.HasDefaultValue, parameter.DefaultValue );
            else
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator );
        }

        var memberBindings = memberCount > 0 ? new MemberBinding[memberCount] : null;
        for ( var i = 0; i < memberCount; ++i )
        {
            Assume.IsNotNull( _memberResolutions );
            Assume.IsNotNull( memberBindings );

            var (member, factory) = _memberResolutions[i];
            var memberType = GetInjectableMemberType( member );
            var (instanceType, name) = (memberType.GetGenericArguments()[0], $"m{i}");

            if ( factory is null )
                expressionBuilder.AddDefaultResolution( instanceType, name );
            else
                expressionBuilder.AddDependencyResolverFactoryResolution( instanceType, name, factory, idGenerator );

            var memberCtor = memberType.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                .First( c =>
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

            if ( ! hasRequiredValueTypeDependency
                && parameter.ParameterType.IsValueType
                && Nullable.GetUnderlyingType( parameter.ParameterType ) is null )
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
            Base.ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback );

        return (builder, parameterCount, memberCount);
    }
}
