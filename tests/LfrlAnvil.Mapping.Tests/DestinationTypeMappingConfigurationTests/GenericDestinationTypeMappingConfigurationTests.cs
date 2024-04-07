using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Mapping.Tests.DestinationTypeMappingConfigurationTests;

public abstract class GenericDestinationTypeMappingConfigurationTests<TDestination, TSource1, TSource2> : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyConfiguration()
    {
        var sut = new DestinationTypeMappingConfiguration<TDestination>();

        using ( new AssertionScope() )
        {
            sut.DestinationType.Should().Be( typeof( TDestination ) );
            sut.GetMappingStores().Should().BeEmpty();
        }
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource1 ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource1 _, ITypeMapper _) => default( TDestination )! );

        var sut = new DestinationTypeMappingConfiguration<TDestination>();

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, ( Delegate )mapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsNotEmpty()
    {
        var expectedFirstKey = new TypeMappingKey( typeof( TSource1 ), typeof( TDestination ) );
        var expectedSecondKey = new TypeMappingKey( typeof( TSource2 ), typeof( TDestination ) );
        var firstMapping = Lambda.Of( (TSource1 _, ITypeMapper _) => default( TDestination )! );
        var secondMapping = Lambda.Of( (TSource2 _, ITypeMapper _) => default( TDestination )! );

        var sut = new DestinationTypeMappingConfiguration<TDestination>();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should()
                .BeSequentiallyEqualTo(
                    KeyValuePair.Create( expectedFirstKey, ( Delegate )firstMapping ),
                    KeyValuePair.Create( expectedSecondKey, ( Delegate )secondMapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldReplaceMappingForExistingDestinationType()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource1 ), typeof( TDestination ) );
        var firstMapping = Lambda.Of( (TSource1 _, ITypeMapper _) => default( TDestination )! );
        var secondMapping = Lambda.Of( (TSource1 _, ITypeMapper _) => default( TDestination )! );

        var sut = new DestinationTypeMappingConfiguration<TDestination>();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, ( Delegate )secondMapping ) );
        }
    }
}
