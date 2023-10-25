namespace LfrlAnvil.Sql.Tests;

public class SqlRecordSetInfoTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeNonTemporaryEmpty()
    {
        var sut = default( SqlRecordSetInfo );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( default( SqlSchemaObjectName ) );
            sut.Identifier.Should().BeEmpty();
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_WithOneStringParameter_ShouldCreateNonTemporaryWithEmptySchemaName()
    {
        var sut = SqlRecordSetInfo.Create( "foo" );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "foo" ) );
            sut.Identifier.Should().Be( "foo" );
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_WithTwoStringParameters_ShouldCreateNonTemporaryWithSchemaName()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "foo", "bar" ) );
            sut.Identifier.Should().Be( "foo.bar" );
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Fact]
    public void Create_WithOneSchemaObjectNameParameter_ShouldCreateNonTemporaryWithName()
    {
        var name = SqlSchemaObjectName.Create( "foo", "bar" );
        var sut = SqlRecordSetInfo.Create( name );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( name );
            sut.Identifier.Should().Be( "foo.bar" );
            sut.IsTemporary.Should().BeFalse();
        }
    }

    [Fact]
    public void CreateTemporary_ShouldCreateTemporaryWithEmptySchemaName()
    {
        var sut = SqlRecordSetInfo.CreateTemporary( "foo" );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "foo" ) );
            sut.Identifier.Should().Be( "TEMP.foo" );
            sut.IsTemporary.Should().BeTrue();
        }
    }

    [Fact]
    public void ToString_ShouldReturnIdentifier()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );
        var result = sut.ToString();
        result.Should().Be( sut.Identifier );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlRecordSetInfo.Create( "foo", "bar" );
        var expected = HashCode.Combine( SqlSchemaObjectName.Create( "foo", "bar" ), false );

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
        var result = SqlRecordSetInfo.Create( schema1, name1 ).Equals( SqlRecordSetInfo.Create( schema2, name2 ) );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void EqualityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetInfo.Create( schema1, name1 ) == SqlRecordSetInfo.Create( schema2, name2 );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", true )]
    [InlineData( "foo", "bar", "fox", "bar", true )]
    [InlineData( "foo", "bar", "foo", "bar", false )]
    [InlineData( "FOO", "BAR", "foo", "bar", false )]
    public void InequalityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlRecordSetInfo.Create( schema1, name1 ) != SqlRecordSetInfo.Create( schema2, name2 );
        result.Should().Be( expected );
    }
}
