using LfrlAnvil.MySql.Internal.Expressions;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.MySql.Tests.ExpressionsTests;

public class MySqlCreateSchemaNodeTests : TestsBase
{
    [Theory]
    [InlineData( true, "CREATE SCHEMA IF NOT EXISTS [foo]" )]
    [InlineData( false, "CREATE SCHEMA [foo]" )]
    public void CreateSchemaNode_ShouldHaveCorrectProperties(bool ifNotExists, string expected)
    {
        var sut = new MySqlCreateSchemaNode( "foo", ifNotExists );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Unknown );
            sut.Name.Should().Be( "foo" );
            sut.IfNotExists.Should().Be( ifNotExists );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expected );
        }
    }
}
