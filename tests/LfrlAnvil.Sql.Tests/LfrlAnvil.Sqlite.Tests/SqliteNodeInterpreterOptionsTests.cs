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
            sut.IsAggregateFunctionOrderingEnabled.Should().BeFalse();
            sut.UpsertOptions.Should().Be( SqliteUpsertOptions.Supported );
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
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
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
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
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
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
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
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
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
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableAggregateFunctionOrdering_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableAggregateFunctionOrdering( enabled );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
            result.IsAggregateFunctionOrderingEnabled.Should().Be( enabled );
            result.UpsertOptions.Should().Be( sut.UpsertOptions );
        }
    }

    [Theory]
    [InlineData( SqliteUpsertOptions.Disabled, SqliteUpsertOptions.Disabled )]
    [InlineData( SqliteUpsertOptions.Supported, SqliteUpsertOptions.Supported )]
    [InlineData(
        SqliteUpsertOptions.AllowEmptyConflictTarget,
        SqliteUpsertOptions.Supported | SqliteUpsertOptions.AllowEmptyConflictTarget )]
    [InlineData(
        SqliteUpsertOptions.Supported | SqliteUpsertOptions.AllowEmptyConflictTarget,
        SqliteUpsertOptions.Supported | SqliteUpsertOptions.AllowEmptyConflictTarget )]
    public void SetUpsertOptions_ShouldReturnCorrectResult(SqliteUpsertOptions options, SqliteUpsertOptions expected)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.SetUpsertOptions( options );

        using ( new AssertionScope() )
        {
            result.TypeDefinitions.Should().BeSameAs( sut.TypeDefinitions );
            result.IsStrictModeEnabled.Should().Be( sut.IsStrictModeEnabled );
            result.IsUpdateFromEnabled.Should().Be( sut.IsUpdateFromEnabled );
            result.IsUpdateOrDeleteLimitEnabled.Should().Be( sut.IsUpdateOrDeleteLimitEnabled );
            result.IsAggregateFunctionOrderingEnabled.Should().Be( sut.IsAggregateFunctionOrderingEnabled );
            result.UpsertOptions.Should().Be( expected );
        }
    }
}
