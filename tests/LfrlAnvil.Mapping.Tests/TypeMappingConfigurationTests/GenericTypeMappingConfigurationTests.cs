using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Mapping.Tests.TypeMappingConfigurationTests;

public abstract class GenericTypeMappingConfigurationTests<T1, T2, T3> : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEmptyConfiguration()
    {
        var sut = new TypeMappingConfiguration();
        sut.GetMappingStores().Should().BeEmpty();
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( T1 ), typeof( T2 ) );
        var mapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T2 )! );

        var sut = new TypeMappingConfiguration();

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsNotEmpty()
    {
        var expectedFirstKey = new TypeMappingKey( typeof( T1 ), typeof( T2 ) );
        var expectedSecondKey = new TypeMappingKey( typeof( T1 ), typeof( T3 ) );
        var firstMapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T2 )! );
        var secondMapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T3 )! );

        var sut = new TypeMappingConfiguration();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should()
                .BeSequentiallyEqualTo(
                    KeyValuePair.Create( expectedFirstKey, (Delegate)firstMapping ),
                    KeyValuePair.Create( expectedSecondKey, (Delegate)secondMapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldReplaceMappingForExistingTypes()
    {
        var expectedKey = new TypeMappingKey( typeof( T1 ), typeof( T2 ) );
        var firstMapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T2 )! );
        var secondMapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T2 )! );

        var sut = new TypeMappingConfiguration();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)secondMapping ) );
        }
    }

    [Fact]
    public void Configure_ShouldAddReverseMappingCorrectly()
    {
        var expectedFirstKey = new TypeMappingKey( typeof( T1 ), typeof( T2 ) );
        var expectedSecondKey = new TypeMappingKey( typeof( T2 ), typeof( T1 ) );
        var firstMapping = Lambda.Of( (T1 _, ITypeMapper _) => default( T2 )! );
        var secondMapping = Lambda.Of( (T2 _, ITypeMapper _) => default( T1 )! );

        var sut = new TypeMappingConfiguration();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            mappingStores.Should()
                .BeSequentiallyEqualTo(
                    KeyValuePair.Create( expectedFirstKey, (Delegate)firstMapping ),
                    KeyValuePair.Create( expectedSecondKey, (Delegate)secondMapping ) );
        }
    }
}
