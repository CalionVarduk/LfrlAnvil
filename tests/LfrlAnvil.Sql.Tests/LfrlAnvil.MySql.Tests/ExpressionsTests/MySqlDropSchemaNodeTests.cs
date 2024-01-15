using LfrlAnvil.MySql.Internal.Expressions;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.MySql.Tests.ExpressionsTests;

public class MySqlDropSchemaNodeTests : TestsBase
{
    [Fact]
    public void DropSchemaNode_ShouldHaveCorrectProperties()
    {
        var sut = new MySqlDropSchemaNode( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Unknown );
            sut.Name.Should().Be( "foo" );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( "DROP SCHEMA [foo]" );
        }
    }
}
