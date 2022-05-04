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

namespace LfrlAnvil.Mapping.Tests.TypeMappingConfigurationTests
{
    public abstract class GenericTypeMappingConfigurationTests<TSource, TDestination> : TestsBase
    {
        [Fact]
        public void Create_ShouldReturnConfigurationWithSingleStore()
        {
            var expectedKey = new MappingKey( typeof( TSource ), typeof( TDestination ) );
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
            var sut = new TypeMappingConfiguration<TSource, TDestination>();

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
            var expectedKey = new MappingKey( typeof( TSource ), typeof( TDestination ) );
            var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );

            var sut = new TypeMappingConfiguration<TSource, TDestination>( mapping );
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
            var expectedKey = new MappingKey( typeof( TSource ), typeof( TDestination ) );
            var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
            var sut = new TypeMappingConfiguration<TSource, TDestination>();

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
            var expectedKey = new MappingKey( typeof( TSource ), typeof( TDestination ) );
            var oldMapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
            var mapping = Lambda.Of( (TSource _, ITypeMapper _) => default( TDestination )! );
            var sut = new TypeMappingConfiguration<TSource, TDestination>( oldMapping );

            var result = sut.Configure( mapping );
            var mappingStores = sut.GetMappingStores().Select( kv => KeyValuePair.Create( kv.Key, kv.Value.FastDelegate ) );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                mappingStores.Should().BeSequentiallyEqualTo( KeyValuePair.Create( expectedKey, (Delegate)mapping ) );
            }
        }
    }
}
