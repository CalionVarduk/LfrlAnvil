using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.EqualTo );
            text.Should().Be( $"({left}) = ({right})" );
            var equalToNode = sut as SqlEqualToConditionNode;
            (equalToNode?.Left).Should().BeSameAs( left );
            (equalToNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void EqualTo_ShouldReturnEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left == right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.EqualTo );
            text.Should().Be( "(NULL) = (NULL)" );
            var equalToNode = sut as SqlEqualToConditionNode;
            (equalToNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (equalToNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void NotEqualTo_ShouldReturnNotEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left != right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.NotEqualTo );
            text.Should().Be( $"({left}) <> ({right})" );
            var notEqualToNode = sut as SqlNotEqualToConditionNode;
            (notEqualToNode?.Left).Should().BeSameAs( left );
            (notEqualToNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void NotEqualTo_ShouldReturnNotEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left != right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.NotEqualTo );
            text.Should().Be( "(NULL) <> (NULL)" );
            var notEqualToNode = sut as SqlNotEqualToConditionNode;
            (notEqualToNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (notEqualToNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void GreaterThan_ShouldReturnGreaterThanConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left > right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.GreaterThan );
            text.Should().Be( $"({left}) > ({right})" );
            var greaterThanNode = sut as SqlGreaterThanConditionNode;
            (greaterThanNode?.Left).Should().BeSameAs( left );
            (greaterThanNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void GreaterThan_ShouldReturnGreaterThanConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left > right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.GreaterThan );
            text.Should().Be( "(NULL) > (NULL)" );
            var greaterThanNode = sut as SqlGreaterThanConditionNode;
            (greaterThanNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (greaterThanNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void LessThan_ShouldReturnLessThanConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left < right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LessThan );
            text.Should().Be( $"({left}) < ({right})" );
            var lessThanNode = sut as SqlLessThanConditionNode;
            (lessThanNode?.Left).Should().BeSameAs( left );
            (lessThanNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void LessThan_ShouldReturnLessThanConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left < right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LessThan );
            text.Should().Be( "(NULL) < (NULL)" );
            var lessThanNode = sut as SqlLessThanConditionNode;
            (lessThanNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (lessThanNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void GreaterThanOrEqualTo_ShouldReturnGreaterThanOrEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left >= right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.GreaterThanOrEqualTo );
            text.Should().Be( $"({left}) >= ({right})" );
            var greaterThanOrEqualToNode = sut as SqlGreaterThanOrEqualToConditionNode;
            (greaterThanOrEqualToNode?.Left).Should().BeSameAs( left );
            (greaterThanOrEqualToNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void GreaterThanOrEqualTo_ShouldReturnGreaterThanOrEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left >= right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.GreaterThanOrEqualTo );
            text.Should().Be( "(NULL) >= (NULL)" );
            var greaterThanOrEqualToNode = sut as SqlGreaterThanOrEqualToConditionNode;
            (greaterThanOrEqualToNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (greaterThanOrEqualToNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void LessThanOrEqualTo_ShouldReturnLessThanOrEqualToConditionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left <= right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LessThanOrEqualTo );
            text.Should().Be( $"({left}) <= ({right})" );
            var lessThanOrEqualToNode = sut as SqlLessThanOrEqualToConditionNode;
            (lessThanOrEqualToNode?.Left).Should().BeSameAs( left );
            (lessThanOrEqualToNode?.Right).Should().BeSameAs( right );
        }
    }

    [Fact]
    public void LessThanOrEqualTo_ShouldReturnLessThanOrEqualToConditionNode_AndReplaceNullParametersWithNullNode()
    {
        var left = (SqlExpressionNode?)null;
        var right = (SqlExpressionNode?)null;
        var sut = left <= right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LessThanOrEqualTo );
            text.Should().Be( "(NULL) <= (NULL)" );
            var lessThanOrEqualToNode = sut as SqlLessThanOrEqualToConditionNode;
            (lessThanOrEqualToNode?.Left).Should().BeSameAs( SqlNode.Null() );
            (lessThanOrEqualToNode?.Right).Should().BeSameAs( SqlNode.Null() );
        }
    }

    [Fact]
    public void Between_ShouldReturnBetweenConditionNode()
    {
        var value = SqlNode.Literal( 42 );
        var min = SqlNode.Parameter<int>( "foo", isNullable: true );
        var max = SqlNode.Parameter<int>( "bar" );
        var sut = value.IsBetween( min, max );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Between );
            sut.Value.Should().BeSameAs( value );
            sut.Min.Should().BeSameAs( min );
            sut.Max.Should().BeSameAs( max );
            sut.IsNegated.Should().BeFalse();
            text.Should().Be( $"({value}) BETWEEN ({min}) AND ({max})" );
        }
    }

    [Fact]
    public void NotBetween_ShouldReturnNegatedBetweenConditionNode()
    {
        var value = SqlNode.Literal( 42 );
        var min = SqlNode.Parameter<int>( "foo", isNullable: true );
        var max = SqlNode.Parameter<int>( "bar" );
        var sut = value.IsNotBetween( min, max );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Between );
            sut.Value.Should().BeSameAs( value );
            sut.Min.Should().BeSameAs( min );
            sut.Max.Should().BeSameAs( max );
            sut.IsNegated.Should().BeTrue();
            text.Should().Be( $"({value}) NOT BETWEEN ({min}) AND ({max})" );
        }
    }

    [Fact]
    public void Like_ShouldReturnLikeConditionNode()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var sut = value.Like( pattern );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Like );
            sut.Value.Should().BeSameAs( value );
            sut.Pattern.Should().BeSameAs( pattern );
            sut.Escape.Should().BeNull();
            sut.IsNegated.Should().BeFalse();
            text.Should().Be( $"({value}) LIKE ({pattern})" );
        }
    }

    [Fact]
    public void NotLike_ShouldReturnNegatedLikeConditionNode()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var sut = value.NotLike( pattern );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Like );
            sut.Value.Should().BeSameAs( value );
            sut.Pattern.Should().BeSameAs( pattern );
            sut.Escape.Should().BeNull();
            sut.IsNegated.Should().BeTrue();
            text.Should().Be( $"({value}) NOT LIKE ({pattern})" );
        }
    }

    [Fact]
    public void Like_ShouldReturnLikeConditionNode_WithEscape()
    {
        var value = SqlNode.Literal( "foo" );
        var pattern = SqlNode.Parameter<string>( "pattern" );
        var escape = SqlNode.Literal( '\\' );
        var sut = value.Like( pattern ).Escape( escape );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Like );
            sut.Value.Should().BeSameAs( value );
            sut.Pattern.Should().BeSameAs( pattern );
            sut.Escape.Should().BeSameAs( escape );
            sut.IsNegated.Should().BeFalse();
            text.Should().Be( $"({value}) LIKE ({pattern}) ESCAPE ({escape})" );
        }
    }

    [Fact]
    public void Exists_ShouldReturnExistsConditionNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Exists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeNull();
            (query?.DataSource.Joins.ToArray()).Should().BeEmpty();
            (query?.DataSource.From).Should().BeSameAs( recordSet );
            (query?.DataSource.RecordSets).Should().BeSequentiallyEqualTo( recordSet );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeFalse();
            text.Should()
                .Be(
                    @"EXISTS (
    FROM [foo]
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void Exists_ShouldReturnExistsConditionNode_WithDataSource()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet );
        var sut = dataSource.Exists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeNull();
            (query?.DataSource).Should().BeSameAs( dataSource );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeFalse();
            text.Should()
                .Be(
                    @"EXISTS (
    FROM [foo]
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void Exists_ShouldReturnExistsConditionNode_WithDataSourceDecorator()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet );
        var decorator = SqlNode.Filtered( dataSource, SqlNode.True() );
        var sut = decorator.Exists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeSameAs( decorator );
            (query?.DataSource).Should().BeSameAs( dataSource );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeFalse();
            text.Should()
                .Be(
                    @"EXISTS (
    FROM [foo]
    AND WHERE
        (TRUE)
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void NotExists_ShouldReturnExistsConditionNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.NotExists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeNull();
            (query?.DataSource.Joins.ToArray()).Should().BeEmpty();
            (query?.DataSource.From).Should().BeSameAs( recordSet );
            (query?.DataSource.RecordSets).Should().BeSequentiallyEqualTo( recordSet );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeTrue();
            text.Should()
                .Be(
                    @"NOT EXISTS (
    FROM [foo]
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void NotExists_ShouldReturnExistsConditionNode_WithDataSource()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet );
        var sut = dataSource.NotExists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeNull();
            (query?.DataSource).Should().BeSameAs( dataSource );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeTrue();
            text.Should()
                .Be(
                    @"NOT EXISTS (
    FROM [foo]
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void NotExists_ShouldReturnExistsConditionNode_WithDataSourceDecorator()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var dataSource = SqlNode.SingleDataSource( recordSet );
        var decorator = SqlNode.Filtered( dataSource, SqlNode.True() );
        var sut = decorator.NotExists();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Exists );
            var query = sut.Query as SqlDataSourceQueryExpressionNode;
            (query?.Decorator).Should().BeSameAs( decorator );
            (query?.DataSource).Should().BeSameAs( dataSource );
            sut.Query.Selection.ToArray().Should().HaveCount( 1 );
            (sut.Query.Selection.ToArray().ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SelectAll );
            sut.IsNegated.Should().BeTrue();
            text.Should()
                .Be(
                    @"NOT EXISTS (
    FROM [foo]
    AND WHERE
        (TRUE)
    SELECT
        *
)" );
        }
    }

    [Fact]
    public void In_ShouldReturnInConditionNode_WithNonEmptyCollection()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) }.ToList();
        var sut = value.In( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.In );
            var inNode = sut as SqlInConditionNode;
            (inNode?.Value).Should().BeSameAs( value );
            (inNode?.Expressions.ToArray()).Should().BeSequentiallyEqualTo( expressions );
            (inNode?.IsNegated).Should().BeFalse();
            text.Should().Be( $"({value}) IN (({expressions[0]}), ({expressions[1]}))" );
        }
    }

    [Fact]
    public void In_ShouldReturnInConditionNode_WithNonEmptyArray()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) };
        var sut = value.In( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.In );
            var inNode = sut as SqlInConditionNode;
            (inNode?.Value).Should().BeSameAs( value );
            (inNode?.Expressions.ToArray()).Should().BeSequentiallyEqualTo( expressions );
            (inNode?.IsNegated).Should().BeFalse();
            text.Should().Be( $"({value}) IN (({expressions[0]}), ({expressions[1]}))" );
        }
    }

    [Fact]
    public void In_ShouldReturnFalseNode_WithEmptyCollection()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.In( Enumerable.Empty<SqlExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.False );
            text.Should().Be( "FALSE" );
        }
    }

    [Fact]
    public void In_ShouldReturnFalseNode_WithEmptyArray()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.In();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.False );
            text.Should().Be( "FALSE" );
        }
    }

    [Fact]
    public void NotIn_ShouldReturnInConditionNode_WithNonEmptyCollection()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) }.ToList();
        var sut = value.NotIn( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.In );
            var inNode = sut as SqlInConditionNode;
            (inNode?.Value).Should().BeSameAs( value );
            (inNode?.Expressions.ToArray()).Should().BeSequentiallyEqualTo( expressions );
            (inNode?.IsNegated).Should().BeTrue();
            text.Should().Be( $"({value}) NOT IN (({expressions[0]}), ({expressions[1]}))" );
        }
    }

    [Fact]
    public void NotIn_ShouldReturnInConditionNode_WithNonEmptyArray()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var expressions = new[] { SqlNode.Literal( 42 ), SqlNode.Literal( 123 ) };
        var sut = value.NotIn( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.In );
            var inNode = sut as SqlInConditionNode;
            (inNode?.Value).Should().BeSameAs( value );
            (inNode?.Expressions.ToArray()).Should().BeSequentiallyEqualTo( expressions );
            (inNode?.IsNegated).Should().BeTrue();
            text.Should().Be( $"({value}) NOT IN (({expressions[0]}), ({expressions[1]}))" );
        }
    }

    [Fact]
    public void NotIn_ShouldReturnTrueNode_WithEmptyCollection()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.NotIn( Enumerable.Empty<SqlExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.True );
            text.Should().Be( "TRUE" );
        }
    }

    [Fact]
    public void NotIn_ShouldReturnTrueNode_WithEmptyArray()
    {
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.NotIn();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.True );
            text.Should().Be( "TRUE" );
        }
    }

    [Fact]
    public void InQuery_ShouldReturnInQueryNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From.GetField( "id" ).AsSelf() );
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.InQuery( query );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.InQuery );
            sut.Value.Should().BeSameAs( value );
            sut.Query.Should().BeSameAs( query );
            sut.IsNegated.Should().BeFalse();
            text.Should()
                .Be(
                    $@"({value}) IN (
    FROM [foo]
    SELECT
        ([foo].[id] : ?)
)" );
        }
    }

    [Fact]
    public void NotInQuery_ShouldReturnInQueryNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From.GetField( "id" ).AsSelf() );
        var value = SqlNode.Parameter<int>( "foo" );
        var sut = value.NotInQuery( query );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.InQuery );
            sut.Value.Should().BeSameAs( value );
            sut.Query.Should().BeSameAs( query );
            sut.IsNegated.Should().BeTrue();
            text.Should()
                .Be(
                    $@"({value}) NOT IN (
    FROM [foo]
    SELECT
        ([foo].[id] : ?)
)" );
        }
    }

    [Fact]
    public void True_ShouldReturnTrueConditionNode()
    {
        var sut = SqlNode.True();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.True );
            text.Should().Be( "TRUE" );
        }
    }

    [Fact]
    public void False_ShouldReturnFalseConditionNode()
    {
        var sut = SqlNode.False();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.False );
            text.Should().Be( "FALSE" );
        }
    }

    [Fact]
    public void RawCondition_ShouldReturnRawConditionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) }.ToList();
        var sut = SqlNode.RawCondition( "@a = @b", parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawCondition );
            sut.Sql.Should().Be( "@a = @b" );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            text.Should().Be( "@a = @b" );
        }
    }

    [Fact]
    public void Value_ShouldReturnConditionValueNode()
    {
        var condition = SqlNode.True();
        var sut = condition.ToValue();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ConditionValue );
            sut.Type.Should().Be( SqlExpressionType.Create<bool>() );
            sut.Condition.Should().BeSameAs( condition );
            text.Should().Be( $"VALUE({condition})" );
        }
    }

    [Fact]
    public void And_ShouldReturnAndConditionNode()
    {
        var left = SqlNode.True();
        var right = SqlNode.False();
        var sut = left & right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.And );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) AND ({right})" );
        }
    }

    [Fact]
    public void Or_ShouldReturnOrConditionNode()
    {
        var left = SqlNode.True();
        var right = SqlNode.False();
        var sut = left | right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Or );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) OR ({right})" );
        }
    }
}
