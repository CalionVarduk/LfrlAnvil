namespace LfrlAnvil.Sqlite.Tests;

public class SqliteNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = SqliteNodeInterpreterOptions.Default;

        Assertion.All(
                sut.TypeDefinitions.TestNull(),
                sut.IsStrictModeEnabled.TestFalse(),
                sut.IsUpdateFromEnabled.TestTrue(),
                sut.IsUpdateOrDeleteLimitEnabled.TestTrue(),
                sut.IsAggregateFunctionOrderingEnabled.TestFalse(),
                sut.ArePositionalParametersEnabled.TestFalse(),
                sut.UpsertOptions.TestEquals( SqliteUpsertOptions.Supported ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new SqliteColumnTypeDefinitionProviderBuilder().Build();
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( typeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableStrictMode_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableStrictMode( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( enabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableUpdateFrom_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableUpdateFrom( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( enabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableUpdateOrDeleteLimit_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableUpdateOrDeleteLimit( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( enabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableAggregateFunctionOrdering_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnableAggregateFunctionOrdering( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( enabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnablePositionalParameters_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = SqliteNodeInterpreterOptions.Default;
        var result = sut.EnablePositionalParameters( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( enabled ),
                result.UpsertOptions.TestEquals( sut.UpsertOptions ) )
            .Go();
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

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsStrictModeEnabled.TestEquals( sut.IsStrictModeEnabled ),
                result.IsUpdateFromEnabled.TestEquals( sut.IsUpdateFromEnabled ),
                result.IsUpdateOrDeleteLimitEnabled.TestEquals( sut.IsUpdateOrDeleteLimitEnabled ),
                result.IsAggregateFunctionOrderingEnabled.TestEquals( sut.IsAggregateFunctionOrderingEnabled ),
                result.ArePositionalParametersEnabled.TestEquals( sut.ArePositionalParametersEnabled ),
                result.UpsertOptions.TestEquals( expected ) )
            .Go();
    }
}
