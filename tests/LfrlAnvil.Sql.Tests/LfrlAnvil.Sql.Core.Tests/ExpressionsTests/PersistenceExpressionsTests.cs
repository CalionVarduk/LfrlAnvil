using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class PersistenceExpressionsTests : TestsBase
{
    [Fact]
    public void DeleteFrom_ShouldCreateDeleteFromNode()
    {
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var dataSource = set1.Join( set2.InnerOn( set1["a"] == set2["b"] ) ).AndWhere( set2["c"] > SqlNode.Literal( 5 ) );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlRecordSetNode>>();
        selector.WithAnyArgs( _ => set1 );
        var sut = dataSource.ToDeleteFrom( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DeleteFrom );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set1 );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (([foo].[a] : ?) = ([bar].[b] : ?))
AND WHERE
    (([bar].[c] : ?) > (""5"" : System.Int32))
DELETE [foo]" );
        }
    }

    [Fact]
    public void DeleteFrom_ShouldCreateDeleteFromNode_FromSingleDataSource()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var sut = dataSource.ToDeleteFrom();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DeleteFrom );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (([foo].[a] : ?) > (""5"" : System.Int32))
DELETE [foo]" );
        }
    }

    [Fact]
    public void ValueAssignment_ShouldCreateValueAssignmentNode()
    {
        var dataField = SqlNode.RawRecordSet( "foo" )["a"];
        var value = SqlNode.Literal( 5 );
        var sut = dataField.Assign( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ValueAssignment );
            sut.DataField.Should().BeSameAs( dataField );
            sut.Value.Should().BeSameAs( value );
            text.Should().Be( "([foo].[a] : ?) = (\"5\" : System.Int32)" );
        }
    }

    [Fact]
    public void Update_ShouldCreateUpdateNode()
    {
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var dataSource = set1.Join( set2.InnerOn( set1["a"] == set2["b"] ) ).AndWhere( set2["c"] > SqlNode.Literal( 5 ) );
        var assignments = new[]
        {
            set1["b"].Assign( SqlNode.Literal( 10 ) ),
            set1["c"].Assign( SqlNode.Literal( "foo" ) ),
            set1["d"].Assign( SqlNode.Parameter<double>( "dVal" ) )
        };

        var setSelector = Substitute.For<Func<SqlMultiDataSourceNode, SqlRecordSetNode>>();
        setSelector.WithAnyArgs( _ => set1 );
        var assignmentsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlValueAssignmentNode>>>();
        assignmentsSelector.WithAnyArgs( _ => assignments );
        var sut = dataSource.ToUpdate( setSelector, assignmentsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            setSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            assignmentsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( set1 );
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set1 );
            sut.Assignments.ToArray().Should().BeSequentiallyEqualTo( assignments );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (([foo].[a] : ?) = ([bar].[b] : ?))
AND WHERE
    (([bar].[c] : ?) > (""5"" : System.Int32))
UPDATE [foo]
SET
    ([foo].[b] : ?) = (""10"" : System.Int32),
    ([foo].[c] : ?) = (""foo"" : System.String),
    ([foo].[d] : ?) = (@dVal : System.Double)" );
        }
    }

    [Fact]
    public void Update_ShouldCreateUpdateNode_WithEmptyAssignments()
    {
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var dataSource = set1.Join( set2.InnerOn( set1["a"] == set2["b"] ) ).AndWhere( set2["c"] > SqlNode.Literal( 5 ) );
        var sut = dataSource.ToUpdate( set1 );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set1 );
            sut.Assignments.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (([foo].[a] : ?) = ([bar].[b] : ?))
AND WHERE
    (([bar].[c] : ?) > (""5"" : System.Int32))
UPDATE [foo]
SET" );
        }
    }

    [Fact]
    public void Update_ShouldCreateUpdateNode_FromSingleDataSource()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var assignments = new[]
        {
            set["b"].Assign( SqlNode.Literal( 10 ) ),
            set["c"].Assign( SqlNode.Literal( "foo" ) ),
            set["d"].Assign( SqlNode.Parameter<double>( "dVal" ) )
        };

        var assignmentsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlValueAssignmentNode>>>();
        assignmentsSelector.WithAnyArgs( _ => assignments );
        var sut = dataSource.ToUpdate( assignmentsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            assignmentsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( set );
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set );
            sut.Assignments.ToArray().Should().BeSequentiallyEqualTo( assignments );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (([foo].[a] : ?) > (""5"" : System.Int32))
UPDATE [foo]
SET
    ([foo].[b] : ?) = (""10"" : System.Int32),
    ([foo].[c] : ?) = (""foo"" : System.String),
    ([foo].[d] : ?) = (@dVal : System.Double)" );
        }
    }

    [Fact]
    public void AndSet_ShouldCreateUpdateNode_WithAddedAssignments()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var oldAssignments = new[] { set["b"].Assign( SqlNode.Literal( 10 ) ), };
        var newAssignments = new[]
        {
            set["c"].Assign( SqlNode.Literal( "foo" ) ),
            set["d"].Assign( SqlNode.Parameter<double>( "dVal" ) )
        };

        var assignmentsSelector = Substitute.For<Func<SqlUpdateNode, IEnumerable<SqlValueAssignmentNode>>>();
        assignmentsSelector.WithAnyArgs( _ => newAssignments );
        var oldUpdate = dataSource.ToUpdate( oldAssignments );
        var sut = oldUpdate.AndSet( assignmentsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            assignmentsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( oldUpdate );
            sut.Should().NotBeSameAs( oldUpdate );
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.RecordSet.Should().BeSameAs( set );
            sut.Assignments.ToArray().Should().BeSequentiallyEqualTo( oldAssignments[0], newAssignments[0], newAssignments[1] );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (([foo].[a] : ?) > (""5"" : System.Int32))
UPDATE [foo]
SET
    ([foo].[b] : ?) = (""10"" : System.Int32),
    ([foo].[c] : ?) = (""foo"" : System.String),
    ([foo].[d] : ?) = (@dVal : System.Double)" );
        }
    }

    [Fact]
    public void AndSet_ShouldReturnOriginalUpdateNode_WhenAssignmentsAreEmpty()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var oldAssignments = new[] { set["b"].Assign( SqlNode.Literal( 10 ) ), };
        var original = dataSource.ToUpdate( oldAssignments );
        var sut = original.AndSet();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().BeSameAs( original );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (([foo].[a] : ?) > (""5"" : System.Int32))
UPDATE [foo]
SET
    ([foo].[b] : ?) = (""10"" : System.Int32)" );
        }
    }
}
