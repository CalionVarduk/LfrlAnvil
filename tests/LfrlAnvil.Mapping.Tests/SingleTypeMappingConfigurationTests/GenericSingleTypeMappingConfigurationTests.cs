using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;

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

        Assertion.All(
                sut.SourceType.TestEquals( typeof( TSource ) ),
                sut.DestinationType.TestEquals( typeof( TDestination ) ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )mapping ) ] ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldReturnEmptyConfiguration()
    {
        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>();

        Assertion.All(
                sut.SourceType.TestEquals( typeof( TSource ) ),
                sut.DestinationType.TestEquals( typeof( TDestination ) ),
                sut.GetMappingStores().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Ctor_WithMappingDelegate_ShouldReturnConfigurationWithSingleStore()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );

        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        Assertion.All(
                sut.SourceType.TestEquals( typeof( TSource ) ),
                sut.DestinationType.TestEquals( typeof( TDestination ) ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )mapping ) ] ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldUpdateConfigurationCorrectly_WhenConfigurationIsEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
        var sut = new SingleTypeMappingConfiguration<TSource, TDestination>();

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        Assertion.All(
                result.TestRefEquals( sut ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )mapping ) ] ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )mapping ) ] ) )
            .Go();
    }
}
