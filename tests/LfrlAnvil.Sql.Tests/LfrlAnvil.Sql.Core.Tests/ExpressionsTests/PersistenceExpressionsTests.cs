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
        var sut = dataSource.ToDeleteFrom();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DeleteFrom );
            sut.DataSource.Should().BeSameAs( dataSource );
            text.Should()
                .Be(
                    @"DELETE FROM [foo]
INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
AND WHERE ([bar].[c] : ?) > (""5"" : System.Int32)" );
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
            text.Should()
                .Be(
                    @"DELETE FROM [foo]
AND WHERE ([foo].[a] : ?) > (""5"" : System.Int32)" );
        }
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateNode()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var sut = set.ToTruncate();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Truncate );
            sut.Table.Should().BeSameAs( set );
            text.Should().Be( "TRUNCATE [foo]" );
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

        var assignmentsSelector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlValueAssignmentNode>>>();
        assignmentsSelector.WithAnyArgs( _ => assignments );
        var sut = dataSource.ToUpdate( assignmentsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            assignmentsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Assignments.ToArray().Should().BeSequentiallyEqualTo( assignments );
            text.Should()
                .Be(
                    @"UPDATE FROM [foo]
INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
AND WHERE ([bar].[c] : ?) > (""5"" : System.Int32)
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
        var sut = dataSource.ToUpdate();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Update );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Assignments.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"UPDATE FROM [foo]
INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
AND WHERE ([bar].[c] : ?) > (""5"" : System.Int32)
SET" );
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
            sut.Assignments.ToArray().Should().BeSequentiallyEqualTo( oldAssignments[0], newAssignments[0], newAssignments[1] );
            text.Should()
                .Be(
                    @"UPDATE FROM [foo]
AND WHERE ([foo].[a] : ?) > (""5"" : System.Int32)
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
                    @"UPDATE FROM [foo]
AND WHERE ([foo].[a] : ?) > (""5"" : System.Int32)
SET
  ([foo].[b] : ?) = (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_FromQuery()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var dataFields = new SqlDataFieldNode[]
        {
            set["x"],
            set["y"]
        };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => dataFields );
        var sut = query.ToInsertInto( set, dataFieldsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            dataFieldsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( set );
            sut.NodeType.Should().Be( SqlNodeType.InsertInto );
            sut.Source.Should().BeSameAs( query );
            sut.RecordSet.Should().BeSameAs( set );
            sut.DataFields.ToArray().Should().BeSequentiallyEqualTo( dataFields );
            text.Should()
                .Be(
                    @"INSERT INTO [foo] ([foo].[x] : ?, [foo].[y] : ?)
SELECT a, b FROM bar" );
        }
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_FromValues()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var values = SqlNode.Values( SqlNode.Literal( 5 ), SqlNode.Parameter<string>( "a" ) );
        var dataFields = new SqlDataFieldNode[]
        {
            set["x"],
            set["y"]
        };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => dataFields );
        var sut = values.ToInsertInto( set, dataFieldsSelector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            dataFieldsSelector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( set );
            sut.NodeType.Should().Be( SqlNodeType.InsertInto );
            sut.Source.Should().BeSameAs( values );
            sut.RecordSet.Should().BeSameAs( set );
            sut.DataFields.ToArray().Should().BeSequentiallyEqualTo( dataFields );
            text.Should()
                .Be(
                    @"INSERT INTO [foo] ([foo].[x] : ?, [foo].[y] : ?)
VALUES
((""5"" : System.Int32), (@a : System.String))" );
        }
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_WithEmptyDataFields()
    {
        var set = SqlNode.RawRecordSet( "foo" ).As( "qux" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var sut = query.ToInsertInto( set );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.InsertInto );
            sut.Source.Should().BeSameAs( query );
            sut.RecordSet.Should().BeSameAs( set );
            sut.DataFields.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"INSERT INTO [foo] AS [qux] ()
SELECT a, b FROM bar" );
        }
    }
}
