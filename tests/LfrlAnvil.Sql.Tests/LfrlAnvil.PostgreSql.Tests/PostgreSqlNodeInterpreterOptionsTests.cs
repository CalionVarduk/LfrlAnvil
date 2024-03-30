namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;

        using ( new AssertionScope() )
        {
            sut.TypeDefinitions.Should().BeNull();
            sut.IsVirtualGeneratedColumnStorageParsingEnabled.Should().BeFalse();
        }
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( typeDefinitions );
            result.IsVirtualGeneratedColumnStorageParsingEnabled.Should().Be( sut.IsVirtualGeneratedColumnStorageParsingEnabled );
        }
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.IsVirtualGeneratedColumnStorageParsingEnabled.Should().Be( sut.IsVirtualGeneratedColumnStorageParsingEnabled );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableVirtualGeneratedColumnStorageParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.EnableVirtualGeneratedColumnStorageParsing( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsVirtualGeneratedColumnStorageParsingEnabled.Should().Be( enabled );
        }
    }
}
