using LfrlAnvil.Functional;

namespace LfrlAnvil.MySql.Tests;

public class MySqlNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = MySqlNodeInterpreterOptions.Default;

        Assertion.All(
                sut.TypeDefinitions.TestNull(),
                sut.CommonSchemaName.TestNull(),
                sut.IndexPrefixLength.TestEquals( 500 ),
                sut.IsFullJoinParsingEnabled.TestFalse(),
                sut.IsIndexFilterParsingEnabled.TestFalse(),
                sut.AreTemporaryViewsForbidden.TestFalse(),
                sut.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new MySqlColumnTypeDefinitionProviderBuilder().Build();
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( typeDefinitions ),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Fact]
    public void SetCommonSchemaName_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetCommonSchemaName( "foo" );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestEquals( "foo" ),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Fact]
    public void SetCommonSchemaName_WithNull_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetCommonSchemaName( null );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Fact]
    public void EnableIndexPrefixes_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableIndexPrefixes( 400 );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 400 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 0 )]
    public void EnableIndexPrefixes_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanOne(int length)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var action = Lambda.Of( () => sut.EnableIndexPrefixes( length ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void DisableIndexPrefixes_ShouldReturnCorrectResult()
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.DisableIndexPrefixes();

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestNull(),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void EnableFullJoinParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableFullJoinParsing( enabled );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestEquals( enabled ),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void EnableIndexFilterParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.EnableIndexFilterParsing( enabled );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestEquals( enabled ),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void ForbidTemporaryViews_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.ForbidTemporaryViews( enabled );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestEquals( enabled ),
                result.UpsertSourceAlias.TestNull() )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "foo" )]
    [InlineData( null )]
    public void SetUpdateSourceAlias_ShouldReturnCorrectResult(string? alias)
    {
        var sut = MySqlNodeInterpreterOptions.Default;
        var result = sut.SetUpdateSourceAlias( alias );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.CommonSchemaName.TestNull(),
                result.IndexPrefixLength.TestEquals( 500 ),
                result.IsFullJoinParsingEnabled.TestFalse(),
                result.IsIndexFilterParsingEnabled.TestFalse(),
                result.AreTemporaryViewsForbidden.TestFalse(),
                result.UpsertSourceAlias.TestRefEquals( alias ) )
            .Go();
    }
}
