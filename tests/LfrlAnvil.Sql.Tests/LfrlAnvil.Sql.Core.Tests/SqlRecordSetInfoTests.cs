namespace LfrlAnvil.Sql.Tests;

public class SqlRecordSetInfoTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeNonTemporaryEmpty()
    {
        var sut = default( SqlRecordSetInfo );

        Assertion.All(
                sut.Name.TestEquals( default( SqlSchemaObjectName ) ),
                sut.Identifier.TestEmpty(),
                sut.IsTemporary.TestFalse() )
            .Go();
    }

    [Fact]
    public void Create_WithOneStringParameter_ShouldCreateNonTemporaryWithEmptySchemaName()
    {
        var sut = SqlRecordSetInfo.Create( "foo" );

        Assertion.All(
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "foo" ) ),
                sut.Identifier.TestEquals( "foo" ),
                sut.IsTemporary.TestFalse() )
            .Go();
    }

    [Fact]
    public void Create_WithTwoStringParameters_ShouldCreateNonTemporaryWithSchemaName()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );

        Assertion.All(
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "foo", "bar" ) ),
                sut.Identifier.TestEquals( "foo.bar" ),
                sut.IsTemporary.TestFalse() )
            .Go();
    }

    [Fact]
    public void Create_WithOneSchemaObjectNameParameter_ShouldCreateNonTemporaryWithName()
    {
        var name = SqlSchemaObjectName.Create( "foo", "bar" );
        var sut = SqlRecordSetInfo.Create( name );

        Assertion.All(
                sut.Name.TestEquals( name ),
                sut.Identifier.TestEquals( "foo.bar" ),
                sut.IsTemporary.TestFalse() )
            .Go();
    }

    [Fact]
    public void CreateTemporary_ShouldCreateTemporaryWithEmptySchemaName()
    {
        var sut = SqlRecordSetInfo.CreateTemporary( "foo" );

        Assertion.All(
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "foo" ) ),
                sut.Identifier.TestEquals( "TEMP.foo" ),
                sut.IsTemporary.TestTrue() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnIdentifier()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );
        var result = sut.ToString();
        result.TestEquals( sut.Identifier ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );
        var expected = HashCode.Combine( SqlSchemaObjectName.Create( "foo", "bar" ), false );

        var result = sut.GetHashCode();

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void Equals_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetInfo.Create( schema1, name1 ).Equals( SqlRecordSetInfo.Create( schema2, name2 ) );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void EqualityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetInfo.Create( schema1, name1 ) == SqlRecordSetInfo.Create( schema2, name2 );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", true )]
    [InlineData( "foo", "bar", "fox", "bar", true )]
    [InlineData( "foo", "bar", "foo", "bar", false )]
    [InlineData( "FOO", "BAR", "foo", "bar", false )]
    public void InequalityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetInfo.Create( schema1, name1 ) != SqlRecordSetInfo.Create( schema2, name2 );
        result.TestEquals( expected ).Go();
    }
}
