using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mapping.Tests.SourceTypeMappingConfigurationTests
{
    public abstract class GenericSourceTypeMappingConfigurationTests<TSource, TDestination1, TDestination2> : TestsBase
    {
        [Fact]
        public void Ctor_ShouldReturnEmptyConfiguration()
        {
            var sut = new SourceTypeMappingConfiguration<TSource>();

            using ( new AssertionScope() )
            {
                sut.SourceType.Should().Be( typeof( TSource ) );
                sut.GetMappingStores().Should().BeEmpty();
            }
        }

        [Fact]
        public void Configure_ShouldAddNewMapping_WhenConfigurationIsEmpty()
        {
            var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
            var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );

            var sut = new SourceTypeMappingConfiguration<TSource>();

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
            var expectedFirstKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
            var expectedSecondKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination2 ) );
            var firstMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );
            var secondMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination2 )! );

            var sut = new SourceTypeMappingConfiguration<TSource>();
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
        public void Configure_ShouldReplaceMappingForExistingSourceType()
        {
            var expectedKey = new TypeMappingKey( typeof( TSource ), typeof( TDestination1 ) );
            var firstMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );
            var secondMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination1 )! );

            var sut = new SourceTypeMappingConfiguration<TSource>();
            sut.Configure( firstMapping );

            var result = sut.Configure( secondMapping );
            var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)secondMapping ) );
            }
        }
    }
}
