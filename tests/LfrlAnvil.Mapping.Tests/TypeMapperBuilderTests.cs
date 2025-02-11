using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping.Tests;

public class TypeMapperBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEmptyBuilder()
    {
        var sut = new TypeMapperBuilder();
        sut.GetConfigurations().TestEmpty().Go();
    }

    [Fact]
    public void Configure_ShouldAddFirstConfigurationCorrectly()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var sut = new TypeMapperBuilder();

        var result = sut.Configure( configuration );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetConfigurations().TestSequence( [ configuration ] ) )
            .Go();
    }

    [Fact]
    public void Configure_ShouldAddNextConfigurationCorrectly()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var configuration2 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var sut = new TypeMapperBuilder();
        sut.Configure( configuration1 );

        var result = sut.Configure( configuration2 );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetConfigurations().TestSequence( [ configuration1, configuration2 ] ) )
            .Go();
    }

    [Fact]
    public void Configure_WithCollection_ShouldAddConfigurationsCorrectly()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var configuration2 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var configuration3 = TypeMappingConfiguration.Create<string, Guid>( (_, _) => default );
        var sut = new TypeMapperBuilder();

        var result = sut.Configure( configuration1, configuration2, configuration3 );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetConfigurations().TestSequence( [ configuration1, configuration2, configuration3 ] ) )
            .Go();
    }

    [Fact]
    public void Build_ShouldReturnCorrectResult()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var configuration2 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var configuration3 = TypeMappingConfiguration.Create<string, Guid>( (_, _) => default );
        var sut = new TypeMapperBuilder();
        sut.Configure( configuration1, configuration2, configuration3 );
        var expectedKeys = new[]
        {
            new TypeMappingKey( typeof( int ), typeof( string ) ),
            new TypeMappingKey( typeof( string ), typeof( int ) ),
            new TypeMappingKey( typeof( string ), typeof( Guid ) )
        };

        var result = sut.Build();

        Assertion.All(
                result.TestType().AssignableTo<TypeMapper>(),
                result.GetConfiguredMappings().TestSetEqual( expectedKeys ) )
            .Go();
    }
}
