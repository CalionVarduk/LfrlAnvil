namespace LfrlAnvil.Sqlite.Tests;

public class SqliteNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = SqliteNodeInterpreterOptions.Default;

        using ( new AssertionScope() )
        {
            sut.TypeDefinitions.Should().BeNull();
            sut.IsStrictModeEnabled.Should().BeFalse();
            sut.IsUpdateFromEnabled.Should().BeTrue();
            sut.IsUpdateOrDeleteLimitEnabled.Should().BeTrue();
        }
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( typeDefinitions );
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
        }
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeNull();
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableStrictMode_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableStrictMode( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsStrictModeEnabled.Should().Be( enabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableUpdateFrom_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableUpdateFrom( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( enabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableUpdateOrDeleteLimit_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableUpdateOrDeleteLimit( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( enabled );
        }
    }
}
