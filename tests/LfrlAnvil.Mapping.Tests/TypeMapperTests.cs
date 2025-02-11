using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Mapping.Exceptions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping.Tests;

public class TypeMapperTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenConfigurationsIsEmpty()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        sut.GetConfiguredMappings().TestEmpty().Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenConfigurationContainsOneElement()
    {
        var configuration = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var expectedKey = new TypeMappingKey( configuration.SourceType, configuration.DestinationType );
        var sut = new TypeMapper( new[] { configuration } );

        sut.GetConfiguredMappings().TestSetEqual( [ expectedKey ] ).Go();
    }

    [Fact]
    public void Ctor_ShouldReturnCorrectResult_WhenConfigurationContainsManyDistinctElements()
    {
        var configuration1 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var configuration2 = new SourceTypeMappingConfiguration<int>().Configure( (_, _) => string.Empty )
            .Configure( (_, _) => Guid.Empty );

        var configuration3 = new DestinationTypeMappingConfiguration<Guid>().Configure<string>( (_, _) => Guid.Empty )
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

        sut.GetConfiguredMappings().TestSetEqual( expectedKeys ).Go();
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

        sut.GetConfiguredMappings().TestSetEqual( expectedKeys ).Go();
    }

    [Fact]
    public void Map_ShouldUseLatestMappingDelegate_WhenConfigurationsAreDuplicated()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, bool>( (_, _) => false );
        var configuration2 = TypeMappingConfiguration.Create<int, bool>( (_, _) => true );
        var sut = new TypeMapper( new[] { configuration1, configuration2 } );

        var result = sut.Map<int, bool>( 0 );

        result.TestTrue().Go();
    }

    [Fact]
    public void Map_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map<int, string>( 1234 );

        result.TestEquals( "1234" ).Go();
    }

    [Fact]
    public void Map_WithGenericSourceAndDestinationTypes_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var action = Lambda.Of( () => sut.Map<int, string>( 1234 ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void TryMap_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.TryMap<int, string>( 1234, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "1234" ) )
            .Go();
    }

    [Fact]
    public void TryMap_WithGenericSourceAndDestinationTypes_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var result = sut.TryMap<int, string>( 1234, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Map_WithGenericDestinationType_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map<string>( 1234 );

        result.TestEquals( "1234" ).Go();
    }

    [Fact]
    public void Map_WithGenericDestinationType_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var action = Lambda.Of( () => sut.Map<string>( 1234 ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void TryMap_WithGenericDestinationType_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.TryMap<string>( 1234, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "1234" ) )
            .Go();
    }

    [Fact]
    public void TryMap_WithGenericDestinationType_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var result = sut.TryMap<string>( 1234, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Map_WithGenericSourceType_ShouldReturnCorrectResult()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map( 1234 );

        Assertion.All(
                result.Source.TestEquals( 1234 ),
                result.TypeMapper.TestRefEquals( sut ) )
            .Go();
    }

    [Fact]
    public void Map_WithGenericSourceType_FollowedByTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map( 1234 ).To<string>();

        result.TestEquals( "1234" ).Go();
    }

    [Fact]
    public void Map_WithGenericSourceType_FollowedByTo_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var action = Lambda.Of( () => sut.Map( 1234 ).To<string>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void Map_WithGenericSourceType_FollowedByTryTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map( 1234 ).TryTo<string>( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "1234" ) )
            .Go();
    }

    [Fact]
    public void Map_WithGenericSourceType_FollowedByTryTo_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var result = sut.Map( 1234 ).TryTo<string>( out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void Map_NonGeneric_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.Map( typeof( string ), 1234 );

        result.TestEquals( "1234" ).Go();
    }

    [Fact]
    public void Map_NonGeneric_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var action = Lambda.Of( () => sut.Map( typeof( string ), 1234 ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void TryMap_NonGeneric_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.TryMap( typeof( string ), 1234, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestEquals( "1234" ) )
            .Go();
    }

    [Fact]
    public void TryMap_NonGeneric_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );

        var result = sut.TryMap( typeof( string ), 1234, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var source = new[] { 1234, 5678, 9012 };
        var expected = new[] { "1234", "5678", "9012" };
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.MapMany<int, string>( source );

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceAndDestinationTypes_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        var source = new[] { 1234, 5678, 9012 };

        var action = Lambda.Of( () => sut.MapMany<int, string>( source ) );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void TryMapMany_WithGenericSourceAndDestinationTypes_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var source = new[] { 1234, 5678, 9012 };
        var expected = new[] { "1234", "5678", "9012" };
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.TryMapMany<int, string>( source, out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestIf().NotNull( r => r.TestSequence( expected ) ) )
            .Go();
    }

    [Fact]
    public void TryMapMany_WithGenericSourceAndDestinationTypes_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        var source = new[] { 1234, 5678, 9012 };

        var result = sut.TryMapMany<int, string>( source, out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceType_ShouldReturnCorrectResult()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var source = new[] { 1234, 5678, 9012 };
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.MapMany( source );

        Assertion.All(
                result.Source.TestRefEquals( source ),
                result.TypeMapper.TestEquals( sut ) )
            .Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceType_FollowedByTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var source = new[] { 1234, 5678, 9012 };
        var expected = new[] { "1234", "5678", "9012" };
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.MapMany( source ).To<string>();

        result.TestSequence( expected ).Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceType_FollowedByTo_ShouldThrowUndefinedTypeMappingException_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        var source = new[] { 1234, 5678, 9012 };

        var action = Lambda.Of( () => sut.MapMany( source ).To<string>() );

        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<UndefinedTypeMappingException>(),
                    exc.TestIf()
                        .OfType<UndefinedTypeMappingException>(
                            e => Assertion.All(
                                e.SourceType.TestEquals( typeof( int ) ),
                                e.DestinationType.TestEquals( typeof( string ) ) ) ) ) )
            .Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceType_FollowedByTryTo_ShouldReturnCorrectResult_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var source = new[] { 1234, 5678, 9012 };
        var expected = new[] { "1234", "5678", "9012" };
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.MapMany( source ).TryTo<string>( out var outResult );

        Assertion.All(
                result.TestTrue(),
                outResult.TestNotNull(),
                outResult.TestIf().NotNull( r => r.TestSequence( expected ) ) )
            .Go();
    }

    [Fact]
    public void MapMany_WithGenericSourceType_FollowedByTryTo_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        var source = new[] { 1234, 5678, 9012 };

        var result = sut.MapMany( source ).TryTo<string>( out var outResult );

        Assertion.All(
                result.TestFalse(),
                outResult.TestNull() )
            .Go();
    }

    [Fact]
    public void IsConfigured_ShouldReturnTrue_WhenTypeMappingExists()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (s, _) => s.ToString() );
        var sut = new TypeMapper( new[] { configuration } );

        var result = sut.IsConfigured<int, string>();

        result.TestTrue().Go();
    }

    [Fact]
    public void IsConfigured_ShouldReturnFalse_WhenTypeMappingDoesNotExist()
    {
        var sut = new TypeMapper( Enumerable.Empty<ITypeMappingConfiguration>() );
        var result = sut.IsConfigured<int, string>();
        result.TestFalse().Go();
    }
}
