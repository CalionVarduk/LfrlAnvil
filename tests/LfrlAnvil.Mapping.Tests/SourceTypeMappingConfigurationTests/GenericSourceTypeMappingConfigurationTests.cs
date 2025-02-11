using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping.Tests.SourceTypeMappingConfigurationTests;

public abstract class GenericSourceTypeMappingConfigurationTests<TSource, TDestination1, TDestination2> : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnEmptyConfiguration()
    {
        var sut = new SourceTypeMappingConfiguration<TSource>();

        Assertion.All(
                sut.SourceType.TestEquals( typeof( TSource ) ),
                sut.GetMappingStores().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsEmpty()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
        var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );

        var sut = new SourceTypeMappingConfiguration<TSource>();

        var result = sut.Configure( mapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        Assertion.All(
                result.TestRefEquals( sut ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )mapping ) ] ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldAddNewMapping_WhenConfigurationIsNotEmpty()
    {
        var expectedFirstKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
        var expectedSecondKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination2 ) );
        var firstMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );
        var secondMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination2 )! );

        var sut = new SourceTypeMappingConfiguration<TSource>();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        Assertion.All(
                result.TestRefEquals( sut ),
                mappingStores.TestSequence(
                [
                    KeyValuePair.Create( expectedFirstKey, ( Delegate )firstMapping ),
                    KeyValuePair.Create( expectedSecondKey, ( Delegate )secondMapping )
                ] ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldReplaceMappingForExistingSourceType()
    {
        var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
        var firstMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );
        var secondMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );

        var sut = new SourceTypeMappingConfiguration<TSource>();
        sut.Configure( firstMapping );

        var result = sut.Configure( secondMapping );
        var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

        Assertion.All(
                result.TestRefEquals( sut ),
                mappingStores.TestSequence( [ KeyValuePair.Create( expectedKey, ( Delegate )secondMapping ) ] ) )
            .Go();
    }
}
