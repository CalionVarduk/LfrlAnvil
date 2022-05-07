using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mapping.Tests.TypeMapperTests
{
    public class TypeMapperTests : TestsBase
    {
        [Fact]
        public void Ctor_ShouldReturnCorrectResult_WhenConfigurationsIsEmpty()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            sut.GetConfiguredMappings().Should().BeEmpty();
        }

        [Fact]
        public void Ctor_ShouldReturnCorrectResult_WhenConfigurationContainsOneElement()
        {
            var configuration = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
            var expectedKey = new TypeMappingKey( configuration.SourceType, configuration.DestinationType );
            var sut = new TypeMapper( new[] { configuration } );

            sut.GetConfiguredMappings().Should().BeEquivalentTo( new[] { expectedKey } );
        }

        [Fact]
        public void Ctor_ShouldReturnCorrectResult_WhenConfigurationContainsManyDistinctElements()
        {
            var configuration1 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
            var configuration2 = new SourceTypeMappingConfiguration<int>()
                .Configure( (_, _) => string.Empty )
                .Configure( (_, _) => Guid.Empty );

            var configuration3 = new DestinationTypeMappingConfiguration<Guid>()
                .Configure<string>( (_, _) => Guid.Empty )
                .Configure<decimal>( (_, _) => Guid.Empty );

            var expectedKeys = new[]
            {
                new TypeMappingKey( configuration1.SourceType, configuration1.DestinationType ),
                new TypeMappingKey( configuration2.SourceType, typeof( string ) ),
                new TypeMappingKey( configuration2.SourceType, typeof( Guid ) ),
                new TypeMappingKey( typeof( string ), configuration3.DestinationType ),
                new TypeMappingKey( typeof( decimal ), configuration3.DestinationType )
            };

            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2, configuration3 } );

            sut.GetConfiguredMappings().Should().BeEquivalentTo( expectedKeys );
        }

        [Fact]
        public void Ctor_ShouldReturnCorrectResult_WhenConfigurationsAreDuplicated()
        {
            var configuration1 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
            var configuration2 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
            var configuration3 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );

            var expectedKeys = new[]
            {
                new TypeMappingKey( configuration2.SourceType, configuration2.DestinationType ),
                new TypeMappingKey( configuration3.SourceType, configuration3.DestinationType )
            };

            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2, configuration3 } );

            sut.GetConfiguredMappings().Should().BeEquivalentTo( expectedKeys );
        }

        [Fact]
        public void Map_ShouldUseLatestMappingDelegate_WhenConfigurationsAreDuplicated()
        {
            var configuration1 = TypeMappingConfiguration.Create<int, bool>( (_, _) => false );
            var configuration2 = TypeMappingConfiguration.Create<int, bool>( (_, _) => true );
            var sut = new TypeMapper( new[] { configuration1, configuration2 } );

            var result = sut.Map<int, bool>( 0 );

            result.Should().BeTrue();
        }

        [Fact]
        public void Map_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map<int, string>( 1234 );

            result.Should().Be( "1234" );
        }

        [Fact]
        public void Map_WithGenericSourceAndDestinationTypes_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var action = Lambda.Of( () => sut.Map<int, string>( 1234 ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void TryMap_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.TryMap<int, string>( 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( "1234" );
            }
        }

        [Fact]
        public void TryMap_WithGenericSourceAndDestinationTypes_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var result = sut.TryMap<int, string>( 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void Map_WithGenericDestinationType_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map<string>( 1234 );

            result.Should().Be( "1234" );
        }

        [Fact]
        public void Map_WithGenericDestinationType_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var action = Lambda.Of( () => sut.Map<string>( 1234 ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void TryMap_WithGenericDestinationType_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.TryMap<string>( 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( "1234" );
            }
        }

        [Fact]
        public void TryMap_WithGenericDestinationType_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var result = sut.TryMap<string>( 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void Map_WithGenericSourceType_ShouldReturnCorrectResult()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map( 1234 );

            using ( new AssertionScope() )
            {
                result.Source.Should().Be( 1234 );
                result.TypeMapper.Should().BeSameAs( sut );
            }
        }

        [Fact]
        public void Map_WithGenericSourceType_FollowedByTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map( 1234 ).To<string>();

            result.Should().Be( "1234" );
        }

        [Fact]
        public void Map_WithGenericSourceType_FollowedByTo_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var action = Lambda.Of( () => sut.Map( 1234 ).To<string>() );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void Map_WithGenericSourceType_FollowedByTryTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map( 1234 ).TryTo<string>( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( "1234" );
            }
        }

        [Fact]
        public void Map_WithGenericSourceType_FollowedByTryTo_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var result = sut.Map( 1234 ).TryTo<string>( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().Be( default );
            }
        }

        [Fact]
        public void Map_NonGeneric_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.Map( typeof( string ), 1234 );

            result.Should().Be( "1234" );
        }

        [Fact]
        public void Map_NonGeneric_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var action = Lambda.Of( () => sut.Map( typeof( string ), 1234 ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void TryMap_NonGeneric_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.TryMap( typeof( string ), 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().Be( "1234" );
            }
        }

        [Fact]
        public void TryMap_NonGeneric_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

            var result = sut.TryMap( typeof( string ), 1234, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void MapMany_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var source = new[] { 1234, 5678, 9012 };
            var expected = new[] { "1234", "5678", "9012" };
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.MapMany<int, string>( source );

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void MapMany_WithGenericSourceAndDestinationTypes_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            var source = new[] { 1234, 5678, 9012 };

            var action = Lambda.Of( () => sut.MapMany<int, string>( source ) );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void TryMapMany_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var source = new[] { 1234, 5678, 9012 };
            var expected = new[] { "1234", "5678", "9012" };
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.TryMapMany<int, string>( source, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSequentiallyEqualTo( expected );
            }
        }

        [Fact]
        public void TryMapMany_WithGenericSourceAndDestinationTypes_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            var source = new[] { 1234, 5678, 9012 };

            var result = sut.TryMapMany<int, string>( source, out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void MapMany_WithGenericSourceType_ShouldReturnCorrectResult()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var source = new[] { 1234, 5678, 9012 };
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.MapMany( source );

            using ( new AssertionScope() )
            {
                result.Source.Should().BeSameAs( source );
                result.TypeMapper.Should().Be( sut );
            }
        }

        [Fact]
        public void MapMany_WithGenericSourceType_FollowedByTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var source = new[] { 1234, 5678, 9012 };
            var expected = new[] { "1234", "5678", "9012" };
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.MapMany( source ).To<string>();

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Fact]
        public void MapMany_WithGenericSourceType_FollowedByTo_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            var source = new[] { 1234, 5678, 9012 };

            var action = Lambda.Of( () => sut.MapMany( source ).To<string>() );

            using ( new AssertionScope() )
            {
                var exception = action.Should().ThrowExactly<UndefinedTypeMappingException>().And;
                exception?.SourceType.Should().Be( typeof( int ) );
                exception?.DestinationType.Should().Be( typeof( string ) );
            }
        }

        [Fact]
        public void MapMany_WithGenericSourceType_FollowedByTryTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var source = new[] { 1234, 5678, 9012 };
            var expected = new[] { "1234", "5678", "9012" };
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.MapMany( source ).TryTo<string>( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeTrue();
                outResult.Should().BeSequentiallyEqualTo( expected );
            }
        }

        [Fact]
        public void MapMany_WithGenericSourceType_FollowedByTryTo_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            var source = new[] { 1234, 5678, 9012 };

            var result = sut.MapMany( source ).TryTo<string>( out var outResult );

            using ( new AssertionScope() )
            {
                result.Should().BeFalse();
                outResult.Should().BeNull();
            }
        }

        [Fact]
        public void IsConfigured_ShouldReturnTrue_WhenTypeMappingExists()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.IsConfigured<int, string>();

            result.Should().BeTrue();
        }

        [Fact]
        public void IsConfigured_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
        {
            var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
            var result = sut.IsConfigured<int, string>();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsConfiguredAsSourceType_ShouldReturnTrue_WhenTypeIsConfiguredAsSourceForAnyTypeMapping()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.IsConfiguredAsSourceType<int>();

            result.Should().BeTrue();
        }

        [Fact]
        public void IsConfiguredAsSourceType_ShouldReturnFalse_WhenTypeIsNotConfiguredAsSourceForAnyTypeMapping()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.IsConfiguredAsSourceType<string>();

            result.Should().BeFalse();
        }

        [Fact]
        public void IsConfiguredAsDestinationType_ShouldReturnTrue_WhenTypeIsConfiguredAsDestinationForAnyTypeMapping()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.IsConfiguredAsDestinationType<string>();

            result.Should().BeTrue();
        }

        [Fact]
        public void IsConfiguredAsDestinationType_ShouldReturnFalse_WhenTypeIsNotConfiguredAsDestinationForAnyTypeMapping()
        {
            var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new[] { configuration } );

            var result = sut.IsConfiguredAsDestinationType<int>();

            result.Should().BeFalse();
        }

        [Fact]
        public void GetConfiguredSourceTypes_WithDestinationType_ShouldReturnTypesThatCanBeMappedToTheProvidedDestinationType()
        {
            var configuration1 = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var configuration2 = TypeMappingConfiguration.Create<Guid, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2 } );
            var expected = new[] { typeof( int ), typeof( Guid ) }.AsEnumerable();

            var result = sut.GetConfiguredSourceTypes<string>();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetConfiguredDestinationTypes_WithSourceType_ShouldReturnTypesThatCanBeMappedFromTheProvidedSourceType()
        {
            var configuration1 = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var configuration2 = TypeMappingConfiguration.Create<int, double>( (s, _) => s );
            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2 } );
            var expected = new[] { typeof( string ), typeof( double ) }.AsEnumerable();

            var result = sut.GetConfiguredDestinationTypes<int>();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetConfiguredSourceTypes_ShouldReturnDistinctConfiguredSourceTypes()
        {
            var configuration1 = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var configuration2 = TypeMappingConfiguration.Create<int, double>( (s, _) => s );
            var configuration3 = TypeMappingConfiguration.Create<Guid, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2, configuration3 } );
            var expected = new[] { typeof( int ), typeof( Guid ) }.AsEnumerable();

            var result = sut.GetConfiguredSourceTypes();

            result.Should().BeEquivalentTo( expected );
        }

        [Fact]
        public void GetConfiguredDestinationTypes_ShouldReturnDistinctConfiguredDestinationTypes()
        {
            var configuration1 = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
            var configuration2 = TypeMappingConfiguration.Create<int, double>( (s, _) => s );
            var configuration3 = TypeMappingConfiguration.Create<Guid, string>( (s, _) => s.ToString() );
            var sut = new TypeMapper( new ITypeMappingConfiguration[] { configuration1, configuration2, configuration3 } );
            var expected = new[] { typeof( string ), typeof( double ) }.AsEnumerable();

            var result = sut.GetConfiguredDestinationTypes();

            result.Should().BeEquivalentTo( expected );
        }
    }
}
