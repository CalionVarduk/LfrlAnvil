using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class LogicalExpressionsTests : TestsBase
{
    [Fact]
    public void EqualTo_ShouldReturnEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left == right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.EqualTo ),
                text.TestEquals( $"({left}) == ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlEqualToConditionNode>( equalToNode => Assertion.All(
                        equalToNode.Left.TestRefEquals( left ),
                        equalToNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void EqualTo_ShouldReturnEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left == right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.EqualTo ),
                text.TestEquals( "(NULL) == (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlEqualToConditionNode>( equalToNode => Assertion.All(
                        equalToNode.Left.TestRefEquals( SqlNode.Null() ),
                        equalToNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void NotEqualTo_ShouldReturnNotEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left != right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NotEqualTo ),
                text.TestEquals( $"({left}) <> ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlNotEqualToConditionNode>( notEqualToNode => Assertion.All(
                        notEqualToNode.Left.TestRefEquals( left ),
                        notEqualToNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void NotEqualTo_ShouldReturnNotEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left != right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NotEqualTo ),
                text.TestEquals( "(NULL) <> (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlNotEqualToConditionNode>( notEqualToNode => Assertion.All(
                        notEqualToNode.Left.TestRefEquals( SqlNode.Null() ),
                        notEqualToNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void GreaterThan_ShouldReturnGreaterThanConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left > right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.GreaterThan ),
                text.TestEquals( $"({left}) > ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlGreaterThanConditionNode>( greaterThanNode => Assertion.All(
                        greaterThanNode.Left.TestRefEquals( left ),
                        greaterThanNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void GreaterThan_ShouldReturnGreaterThanConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left > right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.GreaterThan ),
                text.TestEquals( "(NULL) > (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlGreaterThanConditionNode>( greaterThanNode => Assertion.All(
                        greaterThanNode.Left.TestRefEquals( SqlNode.Null() ),
                        greaterThanNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void LessThan_ShouldReturnLessThanConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left < right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.LessThan ),
                text.TestEquals( $"({left}) < ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlLessThanConditionNode>( lessThanNode => Assertion.All(
                        lessThanNode.Left.TestRefEquals( left ),
                        lessThanNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void LessThan_ShouldReturnLessThanConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left < right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.LessThan ),
                text.TestEquals( "(NULL) < (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlLessThanConditionNode>( lessThanNode => Assertion.All(
                        lessThanNode.Left.TestRefEquals( SqlNode.Null() ),
                        lessThanNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void GreaterThanOrEqualTo_ShouldReturnGreaterThanOrEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left >= right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.GreaterThanOrEqualTo ),
                text.TestEquals( $"({left}) >= ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlGreaterThanOrEqualToConditionNode>( greaterThanOrEqualToNode => Assertion.All(
                        greaterThanOrEqualToNode.Left.TestRefEquals( left ),
                        greaterThanOrEqualToNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void GreaterThanOrEqualTo_ShouldReturnGreaterThanOrEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left >= right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.GreaterThanOrEqualTo ),
                text.TestEquals( "(NULL) >= (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlGreaterThanOrEqualToConditionNode>( greaterThanOrEqualToNode => Assertion.All(
                        greaterThanOrEqualToNode.Left.TestRefEquals( SqlNode.Null() ),
                        greaterThanOrEqualToNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void LessThanOrEqualTo_ShouldReturnLessThanOrEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left <= right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.LessThanOrEqualTo ),
                text.TestEquals( $"({left}) <= ({right})" ),
                sut.TestType()
                    .AssignableTo<SqlLessThanOrEqualToConditionNode>( lessThanOrEqualToNode => Assertion.All(
                        lessThanOrEqualToNode.Left.TestRefEquals( left ),
                        lessThanOrEqualToNode.Right.TestRefEquals( right ) ) ) )
            .Go();
    }

    [Fact]
    public void LessThanOrEqualTo_ShouldReturnLessThanOrEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = ( SqlExpressionNode? )null;
        var right = ( SqlExpressionNode? )null;
        var sut = left <= right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.LessThanOrEqualTo ),
                text.TestEquals( "(NULL) <= (NULL)" ),
                sut.TestType()
                    .AssignableTo<SqlLessThanOrEqualToConditionNode>( lessThanOrEqualToNode => Assertion.All(
                        lessThanOrEqualToNode.Left.TestRefEquals( SqlNode.Null() ),
                        lessThanOrEqualToNode.Right.TestRefEquals( SqlNode.Null() ) ) ) )
            .Go();
    }

    [Fact]
    public void Between_ShouldReturnBetweenConditionNode()
    {
        var value = SqlNode.Literal( 42 );
        var min = SqlNode.Parameter<int>( "foo", isNullable: true );
        var max = SqlNode.Parameter<int>( "bar" );
        var sut = value.IsBetween( min, max );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Between ),
                sut.Value.TestRefEquals( value ),
                sut.Min.TestRefEquals( min ),
                sut.Max.TestRefEquals( max ),
                sut.IsNegated.TestFalse(),
                text.TestEquals( $"({value}) BETWEEN ({min}) AND ({max})" ) )
            .Go();
    }

    [Fact]
    public void NotBetween_ShouldReturnNegatedBetweenConditionNode()
    {
        var value = SqlNode.Literal( 42 );
        var min = SqlNode.Parameter<int>( "foo", isNullable: true );
        var max = SqlNode.Parameter<int>( "bar" );
        var sut = value.IsNotBetween( min, max );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Between ),
                sut.Value.TestRefEquals( value ),
                sut.Min.TestRefEquals( min ),
                sut.Max.TestRefEquals( max ),
                sut.IsNegated.TestTrue(),
                text.TestEquals( $"({value}) NOT BETWEEN ({min}) AND ({max})" ) )
            .Go();
    }

    [Fact]
    public void Like_ShouldReturnLikeConditionNode()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var sut = value.Like( pattern );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Like ),
                sut.Value.TestRefEquals( value ),
                sut.Pattern.TestRefEquals( pattern ),
                sut.Escape.TestNull(),
                sut.IsNegated.TestFalse(),
                text.TestEquals( $"({value}) LIKE ({pattern})" ) )
            .Go();
    }

    [Fact]
    public void NotLike_ShouldReturnNegatedLikeConditionNode()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var sut = value.NotLike( pattern );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Like ),
                sut.Value.TestRefEquals( value ),
                sut.Pattern.TestRefEquals( pattern ),
                sut.Escape.TestNull(),
                sut.IsNegated.TestTrue(),
                text.TestEquals( $"({value}) NOT LIKE ({pattern})" ) )
            .Go();
    }

    [Fact]
    public void Like_ShouldReturnLikeConditionNode_WithEscape()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var escape = SqlNode.Literal( '\\' );
        var sut = value.Like( pattern ).Escape( escape );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Like ),
                sut.Value.TestRefEquals( value ),
                sut.Pattern.TestRefEquals( pattern ),
                sut.Escape.TestRefEquals( escape ),
                sut.IsNegated.TestFalse(),
                text.TestEquals( $"({value}) LIKE ({pattern}) ESCAPE ({escape})" ) )
            .Go();
    }

    [Fact]
    public void Exists_ShouldReturnExistsConditionNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Exists();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Exists ),
                sut.Query.TestType()
                    .AssignableTo<SqlDataSourceQueryExpressionNode>( query => Assertion.All(
                        query.Traits.ToArray().TestEmpty(),
                        query.DataSource.Joins.TestEmpty(),
                        query.DataSource.From.TestRefEquals( recordSet ),
                        query.DataSource.RecordSets.TestSequence( [ recordSet ] ) ) ),
                sut.Query.Selection.Count.TestEquals( 1 ),
                sut.Query.Selection.TestAll( (n, _) => n.NodeType.TestEquals( SqlNodeType.SelectAll ) ),
                sut.IsNegated.TestFalse(),
                text.TestEquals(
                    """
                    EXISTS (
                      FROM [foo]
                      SELECT
                        *
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void Exists_ShouldReturnExistsConditionNode_WithDataSource()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet ).AndWhere( SqlNode.True() );
        var sut = dataSource.Exists();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Exists ),
                sut.Query.TestType()
                    .AssignableTo<SqlDataSourceQueryExpressionNode>( query => Assertion.All(
                        query.Traits.ToArray().TestEmpty(),
                        query.DataSource.TestRefEquals( dataSource ) ) ),
                sut.Query.Selection.Count.TestEquals( 1 ),
                sut.Query.Selection.TestAll( (n, _) => n.NodeType.TestEquals( SqlNodeType.SelectAll ) ),
                sut.IsNegated.TestFalse(),
                text.TestEquals(
                    """
                    EXISTS (
                      FROM [foo]
                      AND WHERE TRUE
                      SELECT
                        *
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void NotExists_ShouldReturnExistsConditionNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.NotExists();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Exists ),
                sut.Query.TestType()
                    .AssignableTo<SqlDataSourceQueryExpressionNode>( query => Assertion.All(
                        query.Traits.ToArray().TestEmpty(),
                        query.DataSource.Joins.ToArray().TestEmpty(),
                        query.DataSource.From.TestRefEquals( recordSet ),
                        query.DataSource.RecordSets.TestSequence( [ recordSet ] ) ) ),
                sut.Query.Selection.Count.TestEquals( 1 ),
                sut.Query.Selection.TestAll( (n, _) => n.NodeType.TestEquals( SqlNodeType.SelectAll ) ),
                sut.IsNegated.TestTrue(),
                text.TestEquals(
                    """
                    NOT EXISTS (
                      FROM [foo]
                      SELECT
                        *
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void NotExists_ShouldReturnExistsConditionNode_WithDataSource()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet ).AndWhere( SqlNode.True() );
        var sut = dataSource.NotExists();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Exists ),
                sut.Query.TestType()
                    .AssignableTo<SqlDataSourceQueryExpressionNode>( query => Assertion.All(
                        query.Traits.ToArray().TestEmpty(),
                        query.DataSource.TestRefEquals( dataSource ) ) ),
                sut.Query.Selection.Count.TestEquals( 1 ),
                sut.Query.Selection.TestAll( (n, _) => n.NodeType.TestEquals( SqlNodeType.SelectAll ) ),
                sut.IsNegated.TestTrue(),
                text.TestEquals(
                    """
                    NOT EXISTS (
                      FROM [foo]
                      AND WHERE TRUE
                      SELECT
                        *
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void In_ShouldReturnInConditionNode_WithNonEmptyExpressions()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) }.ToList();
        var sut = value.In( expressions );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.In ),
                sut.TestType()
                    .AssignableTo<SqlInConditionNode>( inNode => Assertion.All(
                        inNode.Value.TestRefEquals( value ),
                        inNode.Expressions.ToArray().TestSequence( expressions ),
                        inNode.IsNegated.TestFalse() ) ),
                text.TestEquals( $"({value}) IN (({expressions[0]}), ({expressions[1]}))" ) )
            .Go();
    }

    [Fact]
    public void In_ShouldReturnFalseNode_WithEmptyExpressions()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.In();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.False ),
                text.TestEquals( "FALSE" ) )
            .Go();
    }

    [Fact]
    public void NotIn_ShouldReturnInConditionNode_WithNonEmptyExpressions()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) }.ToList();
        var sut = value.NotIn( expressions );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.In ),
                sut.TestType()
                    .AssignableTo<SqlInConditionNode>( inNode => Assertion.All(
                        inNode.Value.TestRefEquals( value ),
                        inNode.Expressions.ToArray().TestSequence( expressions ),
                        inNode.IsNegated.TestTrue() ) ),
                text.TestEquals( $"({value}) NOT IN (({expressions[0]}), ({expressions[1]}))" ) )
            .Go();
    }

    [Fact]
    public void NotIn_ShouldReturnTrueNode_WithEmptyExpressions()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.NotIn();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.True ),
                text.TestEquals( "TRUE" ) )
            .Go();
    }

    [Fact]
    public void InQuery_ShouldReturnInQueryNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From.GetField( "id" ).AsSelf() );
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.InQuery( query );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.InQuery ),
                sut.Value.TestRefEquals( value ),
                sut.Query.TestRefEquals( query ),
                sut.IsNegated.TestFalse(),
                text.TestEquals(
                    $"""
                     ({value}) IN (
                       FROM [foo]
                       SELECT
                         ([foo].[id] : ?)
                     )
                     """ ) )
            .Go();
    }

    [Fact]
    public void NotInQuery_ShouldReturnInQueryNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From.GetField( "id" ).AsSelf() );
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.NotInQuery( query );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.InQuery ),
                sut.Value.TestRefEquals( value ),
                sut.Query.TestRefEquals( query ),
                sut.IsNegated.TestTrue(),
                text.TestEquals(
                    $"""
                     ({value}) NOT IN (
                       FROM [foo]
                       SELECT
                         ([foo].[id] : ?)
                     )
                     """ ) )
            .Go();
    }

    [Fact]
    public void True_ShouldReturnTrueConditionNode()
    {
        var sut = SqlNode.True();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.True ),
                text.TestEquals( "TRUE" ) )
            .Go();
    }

    [Fact]
    public void False_ShouldReturnFalseConditionNode()
    {
        var sut = SqlNode.False();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.False ),
                text.TestEquals( "FALSE" ) )
            .Go();
    }

    [Fact]
    public void RawCondition_ShouldReturnRawConditionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawCondition( "@a = @b", parameters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawCondition ),
                sut.Sql.TestEquals( "@a = @b" ),
                sut.Parameters.ToArray().TestSequence( parameters ),
                text.TestEquals( "@a = @b" ) )
            .Go();
    }

    [Fact]
    public void Value_ShouldReturnConditionValueNode()
    {
        var condition = SqlNode.True();
        var sut = condition.ToValue();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ConditionValue ),
                sut.Condition.TestRefEquals( condition ),
                text.TestEquals( $"CONDITION_VALUE({condition})" ) )
            .Go();
    }

    [Fact]
    public void AliasedValue_ShouldReturnSelectFieldNode()
    {
        var condition = SqlNode.True();
        var sut = condition.As( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectField ),
                sut.Alias.TestEquals( "foo" ),
                sut.FieldName.TestEquals( "foo" ),
                sut.Expression.TestType().AssignableTo<SqlConditionValueNode>( n => n.Condition.TestRefEquals( condition ) ),
                text.TestEquals( $"(CONDITION_VALUE({condition})) AS [foo]" ) )
            .Go();
    }

    [Fact]
    public void And_ShouldReturnAndConditionNode()
    {
        var left = SqlNode.True();
        var right = SqlNode.False();
        var sut = left.And( right );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.And ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) AND ({right})" ) )
            .Go();
    }

    [Fact]
    public void Or_ShouldReturnOrConditionNode()
    {
        var left = SqlNode.True();
        var right = SqlNode.False();
        var sut = left.Or( right );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Or ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) OR ({right})" ) )
            .Go();
    }
}
