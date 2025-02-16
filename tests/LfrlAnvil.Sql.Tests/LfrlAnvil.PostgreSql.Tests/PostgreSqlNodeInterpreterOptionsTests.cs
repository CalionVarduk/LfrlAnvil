namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlNodeInterpreterOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectOptions()
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;

        Assertion.All(
                sut.TypeDefinitions.TestNull(),
                sut.IsVirtualGeneratedColumnStorageParsingEnabled.TestFalse() )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_ShouldReturnCorrectResult()
    {
        var typeDefinitions = new PostgreSqlColumnTypeDefinitionProviderBuilder().Build();
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( typeDefinitions );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( typeDefinitions ),
                result.IsVirtualGeneratedColumnStorageParsingEnabled.TestEquals( sut.IsVirtualGeneratedColumnStorageParsingEnabled ) )
            .Go();
    }

    [Fact]
    public void SetTypeDefinitions_WithNull_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.SetTypeDefinitions( null );

        Assertion.All(
                result.TypeDefinitions.TestNull(),
                result.IsVirtualGeneratedColumnStorageParsingEnabled.TestEquals( sut.IsVirtualGeneratedColumnStorageParsingEnabled ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void EnableVirtualGeneratedColumnStorageParsing_ShouldReturnCorrectResult(bool enabled)
    {
        var sut = PostgreSqlNodeInterpreterOptions.Default;
        var result = sut.EnableVirtualGeneratedColumnStorageParsing( enabled );

        Assertion.All(
                result.TypeDefinitions.TestRefEquals( sut.TypeDefinitions ),
                result.IsVirtualGeneratedColumnStorageParsingEnabled.TestEquals( enabled ) )
            .Go();
    }
}
