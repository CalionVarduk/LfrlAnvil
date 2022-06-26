using System;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Mapping.Internal;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mapping.Tests.TypeMapperBuilderTests;

public class TypeMapperBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateEmptyBuilder()
    {
        var sut = new TypeMapperBuilder();
        sut.GetConfigurations().Should().BeEmpty();
    }

    [Fact]
    public void Configure_ShouldAddFirstConfigurationCorrectly()
    {
        var configuration = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var sut = new TypeMapperBuilder();

        var result = sut.Configure( configuration );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetConfigurations().Should().BeSequentiallyEqualTo( configuration );
        }
    }

    [Fact]
    public void Configure_ShouldAddNextConfigurationCorrectly()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var configuration2 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var sut = new TypeMapperBuilder();
        sut.Configure( configuration1 );

        var result = sut.Configure( configuration2 );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetConfigurations().Should().BeSequentiallyEqualTo( configuration1, configuration2 );
        }
    }

    [Fact]
    public void Configure_WithCollection_ShouldAddConfigurationsCorrectly()
    {
        var configuration1 = TypeMappingConfiguration.Create<int, string>( (_, _) => string.Empty );
        var configuration2 = TypeMappingConfiguration.Create<string, int>( (_, _) => default );
        var configuration3 = TypeMappingConfiguration.Create<string, Guid>( (_, _) => default );
        var sut = new TypeMapperBuilder();

        var result = sut.Configure( configuration1, configuration2, configuration3 );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetConfigurations().Should().BeSequentiallyEqualTo( configuration1, configuration2, configuration3 );
        }
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

        using ( new AssertionScope() )
        {
            result.Should().BeOfType<TypeMapper>();
            result.GetConfiguredMappings().Should().BeEquivalentTo( expectedKeys );
        }
    }
}
