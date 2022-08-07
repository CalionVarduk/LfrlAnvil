using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Mapping.Tests.SingleTypeMappingConfigurationTests;

public abstract class GenericSingleTypeMappingConfigurationTests<TSource, TDestination> : TestsBase
{
    [Fact]
    public void Create_ShouldReturnConfigurationWithSingleStore()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );

        var sut = TypeMappingConfiguration.Create( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            sut.SourceType.Should().Be( typeof( TSource ) );
            sut.DestinationType.Should().Be( typeof( TDestination ) );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
        }
    }

    [Fact]
    public void Ctor_ShouldReturnEmptyConfiguration()
    {
        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>();

        using ( new AssertionScope() )
        {
            sut.SourceType.Should().Be( typeof( TSource ) );
            sut.DestinationType.Should().Be( typeof( TDestination ) );
            sut.GetMappingStores().Should().BeEmpty();
        }
    }

    [Fact]
    public void Ctor_WithMappingDelegate_ShouldReturnConfigurationWithSingleStore()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );

        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            sut.SourceType.Should().Be( typeof( TSource ) );
            sut.DestinationType.Should().Be( typeof( TDestination ) );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldUpdateConfigurationCorrectly_WhenConfigurationIsEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>();

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldUpdateConfigurationCorrectly_WhenConfigurationIsNotEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var oldMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>( oldMapping );

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
        }
    }
}
