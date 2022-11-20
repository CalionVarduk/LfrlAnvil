using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies;

public class DependencyContainerBuilder : IDependencyContainerBuilder
{
    private readonly DependencyLocatorBuilderStore _locatorBuilderStore;

    public DependencyContainerBuilder()
    {
        _locatorBuilderStore = DependencyLocatorBuilderStore.Create();
        InjectablePropertyType = typeof( Injected<> );
        OptionalDependencyAttributeType = typeof( AllowNullAttribute );
    }

    public Type InjectablePropertyType { get; private set; }
    public Type OptionalDependencyAttributeType { get; private set; }
    public DependencyLifetime DefaultLifetime => _locatorBuilderStore.Global.DefaultLifetime;
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy => _locatorBuilderStore.Global.DefaultDisposalStrategy;

    Type? IDependencyLocatorBuilder.KeyType => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).KeyType;
    object? IDependencyLocatorBuilder.Key => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).Key;
    bool IDependencyLocatorBuilder.IsKeyed => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).IsKeyed;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.AddSharedImplementor( type );
    }

    public IDependencyBuilder Add(Type type)
    {
        return _locatorBuilderStore.Global.Add( type );
    }

    public DependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        _locatorBuilderStore.Global.SetDefaultLifetime( lifetime );
        return this;
    }

    public DependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        _locatorBuilderStore.Global.SetDefaultDisposalStrategy( strategy );
        return this;
    }

    public DependencyContainerBuilder SetInjectablePropertyType(Type openGenericType)
    {
        if ( ! IsInjectablePropertyTypeCorrect( openGenericType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidInjectablePropertyType( openGenericType ),
                nameof( openGenericType ) );
        }

        InjectablePropertyType = openGenericType;
        return this;
    }

    public DependencyContainerBuilder SetOptionalDependencyAttributeType(Type attributeType)
    {
        if ( ! IsOptionalDependencyAttributeTypeCorrect( attributeType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidOptionalDependencyAttributeType( attributeType ),
                nameof( attributeType ) );
        }

        OptionalDependencyAttributeType = attributeType;
        return this;
    }

    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.TryGetSharedImplementor( type );
    }

    [Pure]
    public IDependencyBuilder? TryGetDependency(Type type)
    {
        return _locatorBuilderStore.Global.TryGetDependency( type );
    }

    public IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return _locatorBuilderStore.GetOrAddKeyed( key );
    }

    [Pure]
    public DependencyContainerBuildResult<DependencyContainer> TryBuild()
    {
        var buildParams = DependencyLocatorBuilderParams.Create( _locatorBuilderStore );
        var globalResult = _locatorBuilderStore.Global.Build( buildParams );
        var messages = globalResult.Messages;

        var keyedLocatorBuilders = _locatorBuilderStore.GetAllKeyed();
        foreach ( var locatorBuilder in keyedLocatorBuilders )
            messages = messages.Extend( locatorBuilder.BuildKeyed( buildParams ) );

        foreach ( var message in messages )
        {
            if ( message.Errors.Count > 0 )
                return new DependencyContainerBuildResult<DependencyContainer>( null, messages );
        }

        var result = new DependencyContainer( globalResult.Resolvers, buildParams.KeyedResolversStore );
        return new DependencyContainerBuildResult<DependencyContainer>( result, messages );
    }

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> IDependencyContainerBuilder.TryBuild()
    {
        var result = TryBuild();
        return new DependencyContainerBuildResult<IDisposableDependencyContainer>( result.Container, result.Messages );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return SetDefaultLifetime( lifetime );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultLifetime( lifetime );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return SetDefaultDisposalStrategy( strategy );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultDisposalStrategy( strategy );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetInjectablePropertyType(Type openGenericType)
    {
        return SetInjectablePropertyType( openGenericType );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetOptionalDependencyAttributeType(Type attributeType)
    {
        return SetOptionalDependencyAttributeType( attributeType );
    }

    [Pure]
    private static bool IsInjectablePropertyTypeCorrect(Type type)
    {
        if ( ! type.IsGenericTypeDefinition )
            return false;

        var genericArgs = type.GetGenericArguments();
        if ( genericArgs.Length != 1 )
            return false;

        var instanceType = genericArgs[0];
        var ctor = type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .FirstOrDefault(
                c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == instanceType;
                } );

        return ctor is not null;
    }

    [Pure]
    private static bool IsOptionalDependencyAttributeTypeCorrect(Type type)
    {
        if ( type.IsGenericTypeDefinition )
            return false;

        if ( type.Visit( t => t.BaseType ).All( t => t != typeof( Attribute ) ) )
            return false;

        var attributeUsage = type.Visit( t => t.BaseType )
            .Prepend( type )
            .Select( t => t.GetAttribute<AttributeUsageAttribute>( inherit: false ) )
            .FirstOrDefault( a => a is not null );

        return attributeUsage is not null && (attributeUsage.ValidOn & AttributeTargets.Parameter) == AttributeTargets.Parameter;
    }
}
