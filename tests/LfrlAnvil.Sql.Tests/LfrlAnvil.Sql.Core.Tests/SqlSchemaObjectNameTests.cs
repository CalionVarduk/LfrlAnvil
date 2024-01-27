namespace LfrlAnvil.Sql.Tests;

public class SqlSchemaObjectNameTests : TestsBase
{
    [Fact]
    public void Default_ShouldBeEmpty()
    {
        var sut = default( SqlSchemaObjectName );

        using ( new AssertionScope() )
        {
            sut.Schema.Should().BeEmpty();
            sut.Object.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_WithOneParameter_ShouldCreateWithEmptySchemaName()
    {
        var sut = SqlSchemaObjectName.Create( "foo" );

        using ( new AssertionScope() )
        {
            sut.Schema.Should().BeEmpty();
            sut.Object.Should().Be( "foo" );
        }
    }

    [Fact]
    public void Create_WithTwoParameters_ShouldCreateWithSchemaName()
    {
        var sut = SqlSchemaObjectName.Create( "foo", "bar" );

        using ( new AssertionScope() )
        {
            sut.Schema.Should().Be( "foo" );
            sut.Object.Should().Be( "bar" );
        }
    }

    [Theory]
    [InlineData( "foo", "bar", "foo.bar" )]
    [InlineData( "", "bar", "bar" )]
    public void ToString_ShouldReturnCorrectText(string schema, string obj, string expected)
    {
        var sut = SqlSchemaObjectName.Create( schema, obj );
        var result = sut.ToString();
        result.Should().Be( expected );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = SqlSchemaObjectName.Create( "foo", "bar" );
        var expected = HashCode.Combine(
            "foo".GetHashCode( StringComparison.OrdinalIgnoreCase ),
            "bar".GetHashCode( StringComparison.OrdinalIgnoreCase ) );

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
        var result = SqlSchemaObjectName.Create( schema1, name1 ).Equals( SqlSchemaObjectName.Create( schema2, name2 ) );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", false )]
    [InlineData( "foo", "bar", "fox", "bar", false )]
    [InlineData( "foo", "bar", "foo", "bar", true )]
    [InlineData( "FOO", "BAR", "foo", "bar", true )]
    public void EqualityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlSchemaObjectName.Create( schema1, name1 ) == SqlSchemaObjectName.Create( schema2, name2 );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", "bar", "foo", "baz", true )]
    [InlineData( "foo", "bar", "fox", "bar", true )]
    [InlineData( "foo", "bar", "foo", "bar", false )]
    [InlineData( "FOO", "BAR", "foo", "bar", false )]
    public void InequalityOperator_ShouldReturnCorrectResult(string schema1, string name1, string schema2, string name2, bool expected)
    {
        var result = SqlSchemaObjectName.Create( schema1, name1 ) != SqlSchemaObjectName.Create( schema2, name2 );
        result.Should().Be( expected );
    }
}
