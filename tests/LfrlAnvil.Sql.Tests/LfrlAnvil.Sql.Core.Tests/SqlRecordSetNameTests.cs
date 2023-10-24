namespace LfrlAnvil.Sql.Tests;

public class SqlRecordSetNameTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeNonTemporaryEmpty()
    {
        var sut = default( SqlRecordSetName );

        using ( new AssertionScope() )
        {
            sut.SchemaName.Should().BeEmpty();
            sut.Name.Should().BeEmpty();
            sut.Identifier.Should().BeEmpty();
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData( "foo", "bar", "foo.bar" )]
    [InlineData( "", "bar", "bar" )]
    public void Create_ShouldCreateNonTemporaryName(string schemaName, string name, string expectedIdentifier)
    {
        var sut = SqlRecordSetName.Create( schemaName, name );

        using ( new AssertionScope() )
        {
            sut.SchemaName.Should().Be( schemaName );
            sut.Name.Should().Be( name );
            sut.Identifier.Should().Be( expectedIdentifier );
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Fact]
    public void CreateTemporary_ShouldCreateTemporaryName()
    {
        var sut = SqlRecordSetName.CreateTemporary( "foo" );

        using ( new AssertionScope() )
        {
            sut.SchemaName.Should().BeEmpty();
            sut.Name.Should().Be( "foo" );
            sut.Identifier.Should().Be( "TEMP.foo" );
            sut.IsTemporary.Should().BeTrue();
        }
    }

    [Fact]
    public void ToString_ShouldReturnIdentifier()
    {
        var sut = SqlRecordSetName.Create( "foo", "bar" );
        var result = sut.ToString();
        result.Should().Be( sut.Identifier );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlRecordSetName.Create( "foo", "bar" );
        var expected = HashCode.Combine(
            "foo".GetHashCode( StringComparison.OrdinalIgnoreCase ),
            "bar".GetHashCode( StringComparison.OrdinalIgnoreCase ),
            sut.IsTemporary );

        var result = sut.GetHashCode();

        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void Equals_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetName.Create( schema1, name1 ).Equals( SqlRecordSetName.Create( schema2, name2 ) );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void EqualityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetName.Create( schema1, name1 ) == SqlRecordSetName.Create( schema2, name2 );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", true )]
    [InlineData( "foo", "bar", "fox", "bar", true )]
    [InlineData( "foo", "bar", "foo", "bar", false )]
    [InlineData( "FOO", "BAR", "foo", "bar", false )]
    public void InequalityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetName.Create( schema1, name1 ) != SqlRecordSetName.Create( schema2, name2 );
        result.Should().Be( expected );
    }
}
