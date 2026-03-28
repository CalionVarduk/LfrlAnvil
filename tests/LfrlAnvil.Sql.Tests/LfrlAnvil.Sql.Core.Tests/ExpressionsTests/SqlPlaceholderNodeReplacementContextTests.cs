using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlPlaceholderNodeReplacementContextTests : TestsBase
{
    [Fact]
    public void Visit_ShouldReplacePlaceholders()
    {
        var ph1 = SqlNode.Placeholders.Expression();
        var ph2 = SqlNode.Placeholders.Expression();
        var ph3 = SqlNode.Placeholders.Condition();
        var ph4 = SqlNode.Placeholders.Condition();
        var tree = (SqlNode.Parameter<int>( "a" ) > ph1).Or( ph2 == SqlNode.Literal( "foo" ) ).Or( ph3.And( ph4 ) );

        var sut = new SqlPlaceholderNodeReplacementContext.Builder( capacity: 2 )
            .Add( ph1, SqlNode.Literal( 123 ) )
            .Add( ph2, SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) )
            .Add( ph3, SqlNode.True() )
            .Add( ph4, SqlNode.RawDataField( SqlNode.RawRecordSet( "r" ), "val" ) > SqlNode.Literal( 10 ) )
            .Build();

        var replaced = sut.Visit( tree );
        var result = new SqlNodeDebugInterpreter().Interpret( replaced );

        Assertion.All(
                result.Sql.ToString()
                    .TestEquals(
                        "(((@a : System.Int32) > (\"123\" : System.Int32)) OR (([r].[val] : ?) == (\"foo\" : System.String))) OR ((TRUE) AND (([r].[val] : ?) > (\"10\" : System.Int32)))" ),
                result.Parameters.TestSequence( [ new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) ] ) )
            .Go();
    }
}
