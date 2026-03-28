using System.Collections.Generic;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DeleteFrom ),
                sut.DataSource.TestRefEquals( dataSource ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    DELETE FROM [foo]
                    INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
                    AND WHERE ([bar].[c] : ?) > ("5" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void DeleteFrom_ShouldCreateDeleteFromNode_FromSingleDataSource()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var sut = dataSource.ToDeleteFrom();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DeleteFrom ),
                sut.DataSource.TestRefEquals( dataSource ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    DELETE FROM [foo]
                    AND WHERE ([foo].[a] : ?) > ("5" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateNode()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var sut = set.ToTruncate();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Truncate ),
                sut.Table.TestRefEquals( set ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( "TRUNCATE [foo]" ) )
            .Go();
    }

    [Fact]
    public void ValueAssignment_ShouldCreateValueAssignmentNode()
    {
        var dataField = SqlNode.RawRecordSet( "foo" )["a"];
        var value = SqlNode.Literal( 5 );
        var sut = dataField.Assign( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ValueAssignment ),
                sut.DataField.TestRefEquals( dataField ),
                sut.Value.TestRefEquals( value ),
                text.TestEquals( "([foo].[a] : ?) = (\"5\" : System.Int32)" ) )
            .Go();
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

        Assertion.All(
                assignmentsSelector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.Update ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Assignments.TestSequence( assignments ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPDATE FROM [foo]
                    INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
                    AND WHERE ([bar].[c] : ?) > ("5" : System.Int32)
                    SET
                      ([foo].[b] : ?) = ("10" : System.Int32),
                      ([foo].[c] : ?) = ("foo" : System.String),
                      ([foo].[d] : ?) = (@dVal : System.Double)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Update_ShouldCreateUpdateNode_WithEmptyAssignments()
    {
        var set1 = SqlNode.RawRecordSet( "foo" );
        var set2 = SqlNode.RawRecordSet( "bar" );
        var dataSource = set1.Join( set2.InnerOn( set1["a"] == set2["b"] ) ).AndWhere( set2["c"] > SqlNode.Literal( 5 ) );
        var sut = dataSource.ToUpdate();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Update ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Assignments.TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPDATE FROM [foo]
                    INNER JOIN [bar] ON ([foo].[a] : ?) == ([bar].[b] : ?)
                    AND WHERE ([bar].[c] : ?) > ("5" : System.Int32)
                    SET
                    """ ) )
            .Go();
    }

    [Fact]
    public void AndSet_ShouldCreateUpdateNode_WithAddedAssignments()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var dataSource = set.ToDataSource().AndWhere( set["a"] > SqlNode.Literal( 5 ) );
        var oldAssignments = new[] { set["b"].Assign( SqlNode.Literal( 10 ) ), };
        var newAssignments = new[] { set["c"].Assign( SqlNode.Literal( "foo" ) ), set["d"].Assign( SqlNode.Parameter<double>( "dVal" ) ) };

        var assignmentsSelector = Substitute.For<Func<SqlUpdateNode, IEnumerable<SqlValueAssignmentNode>>>();
        assignmentsSelector.WithAnyArgs( _ => newAssignments );
        var oldUpdate = dataSource.ToUpdate( oldAssignments );
        var sut = oldUpdate.AndSet( assignmentsSelector );
        var text = sut.ToString();

        Assertion.All(
                assignmentsSelector.CallAt( 0 ).Arguments.TestSequence( [ oldUpdate ] ),
                sut.TestNotRefEquals( oldUpdate ),
                sut.NodeType.TestEquals( SqlNodeType.Update ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Assignments.TestSequence( [ oldAssignments[0], newAssignments[0], newAssignments[1] ] ),
                text.TestEquals(
                    """
                    UPDATE FROM [foo]
                    AND WHERE ([foo].[a] : ?) > ("5" : System.Int32)
                    SET
                      ([foo].[b] : ?) = ("10" : System.Int32),
                      ([foo].[c] : ?) = ("foo" : System.String),
                      ([foo].[d] : ?) = (@dVal : System.Double)
                    """ ) )
            .Go();
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

        Assertion.All(
                sut.TestRefEquals( original ),
                text.TestEquals(
                    """
                    UPDATE FROM [foo]
                    AND WHERE ([foo].[a] : ?) > ("5" : System.Int32)
                    SET
                      ([foo].[b] : ?) = ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_FromQuery()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var dataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => dataFields );
        var sut = query.ToInsertInto( set, dataFieldsSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.InsertInto ),
                sut.Source.TestRefEquals( query ),
                sut.RecordSet.TestRefEquals( set ),
                sut.DataFields.TestSequence( dataFields ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    INSERT INTO [foo] ([foo].[x] : ?, [foo].[y] : ?)
                    SELECT a, b FROM bar
                    """ ) )
            .Go();
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_FromValues()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var values = SqlNode.Values( SqlNode.Literal( 5 ), SqlNode.Parameter<string>( "a" ) );
        var dataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => dataFields );
        var sut = values.ToInsertInto( set, dataFieldsSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.InsertInto ),
                sut.Source.TestRefEquals( values ),
                sut.RecordSet.TestRefEquals( set ),
                sut.DataFields.TestSequence( dataFields ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    INSERT INTO [foo] ([foo].[x] : ?, [foo].[y] : ?)
                    VALUES
                    (("5" : System.Int32), (@a : System.String))
                    """ ) )
            .Go();
    }

    [Fact]
    public void InsertInto_ShouldCreateInsertIntoNode_WithEmptyDataFields()
    {
        var set = SqlNode.RawRecordSet( "foo" ).As( "qux" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var sut = query.ToInsertInto( set );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.InsertInto ),
                sut.Source.TestRefEquals( query ),
                sut.RecordSet.TestRefEquals( set ),
                sut.DataFields.TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    INSERT INTO [foo] AS [qux] ()
                    SELECT a, b FROM bar
                    """ ) )
            .Go();
    }

    [Fact]
    public void Upsert_ShouldCreateUpsertNode_FromQuery()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var insertDataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var updateAssignments = new[] { set["x"].Assign( SqlNode.Literal( 10 ) ), set["y"].Assign( SqlNode.Parameter<double>( "dVal" ) ) };

        var conflictTarget = new SqlDataFieldNode[] { set["x"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => insertDataFields );
        var updateAssignmentsSelector =
            Substitute.For<Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>>>();

        updateAssignmentsSelector.WithAnyArgs( _ => updateAssignments );
        var conflictTargetSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        conflictTargetSelector.WithAnyArgs( _ => conflictTarget );
        var sut = query.ToUpsert( set, dataFieldsSelector, updateAssignmentsSelector, conflictTargetSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                updateAssignmentsSelector.CallAt( 0 ).Arguments.TestSequence( [ set, sut.UpdateSource ] ),
                conflictTargetSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.Upsert ),
                sut.Source.TestRefEquals( query ),
                sut.RecordSet.TestRefEquals( set ),
                sut.UpdateSource.Base.TestRefEquals( sut.RecordSet ),
                sut.InsertDataFields.TestSequence( insertDataFields ),
                sut.UpdateAssignments.TestSequence( updateAssignments ),
                sut.ConflictTarget.TestSequence( conflictTarget ),
                sut.UpdateFilter.TestNull(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPSERT [foo] USING
                    SELECT a, b FROM bar
                    WITH CONFLICT TARGET ([foo].[x] : ?)
                    INSERT ([foo].[x] : ?, [foo].[y] : ?)
                    ON CONFLICT SET
                      ([foo].[x] : ?) = ("10" : System.Int32),
                      ([foo].[y] : ?) = (@dVal : System.Double)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Upsert_ShouldCreateUpsertNode_FromValues()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var values = SqlNode.Values( SqlNode.Literal( 5 ), SqlNode.Parameter<string>( "a" ) );
        var insertDataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var updateAssignments = new[] { set["x"].Assign( SqlNode.Literal( 10 ) ), set["y"].Assign( SqlNode.Parameter<double>( "dVal" ) ) };

        var conflictTarget = new SqlDataFieldNode[] { set["x"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => insertDataFields );
        var updateAssignmentsSelector =
            Substitute.For<Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>>>();

        updateAssignmentsSelector.WithAnyArgs( _ => updateAssignments );
        var conflictTargetSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        conflictTargetSelector.WithAnyArgs( _ => conflictTarget );
        var sut = values.ToUpsert( set, dataFieldsSelector, updateAssignmentsSelector, conflictTargetSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                updateAssignmentsSelector.CallAt( 0 ).Arguments.TestSequence( [ set, sut.UpdateSource ] ),
                conflictTargetSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.Upsert ),
                sut.Source.TestRefEquals( values ),
                sut.RecordSet.TestRefEquals( set ),
                sut.UpdateSource.Base.TestRefEquals( sut.RecordSet ),
                sut.InsertDataFields.TestSequence( insertDataFields ),
                sut.UpdateAssignments.TestSequence( updateAssignments ),
                sut.ConflictTarget.TestSequence( conflictTarget ),
                sut.UpdateFilter.TestNull(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPSERT [foo] USING
                    VALUES
                    (("5" : System.Int32), (@a : System.String))
                    WITH CONFLICT TARGET ([foo].[x] : ?)
                    INSERT ([foo].[x] : ?, [foo].[y] : ?)
                    ON CONFLICT SET
                      ([foo].[x] : ?) = ("10" : System.Int32),
                      ([foo].[y] : ?) = (@dVal : System.Double)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Upsert_ShouldCreateUpsertNode_FromQuery_WithUpdateFilter()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var query = SqlNode.RawQuery( "SELECT a, b FROM bar" );
        var insertDataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var updateAssignments = new[] { set["x"].Assign( SqlNode.Literal( 10 ) ), set["y"].Assign( SqlNode.Parameter<double>( "dVal" ) ) };
        var updateFilter = set["x"].IsGreaterThan( SqlNode.Literal( 11 ) );

        var conflictTarget = new SqlDataFieldNode[] { set["x"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => insertDataFields );
        var updateSelector = Substitute.For<Func<SqlRecordSetNode, SqlInternalRecordSetNode, SqlUpsertNodeUpdatePart>>();
        updateSelector.WithAnyArgs( _ => new SqlUpsertNodeUpdatePart( updateAssignments, updateFilter ) );
        var conflictTargetSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        conflictTargetSelector.WithAnyArgs( _ => conflictTarget );
        var sut = query.ToUpsert( set, dataFieldsSelector, updateSelector, conflictTargetSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                updateSelector.CallAt( 0 ).Arguments.TestSequence( [ set, sut.UpdateSource ] ),
                conflictTargetSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.Upsert ),
                sut.Source.TestRefEquals( query ),
                sut.RecordSet.TestRefEquals( set ),
                sut.UpdateSource.Base.TestRefEquals( sut.RecordSet ),
                sut.InsertDataFields.TestSequence( insertDataFields ),
                sut.UpdateAssignments.TestSequence( updateAssignments ),
                sut.ConflictTarget.TestSequence( conflictTarget ),
                sut.UpdateFilter.TestRefEquals( updateFilter ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPSERT [foo] USING
                    SELECT a, b FROM bar
                    WITH CONFLICT TARGET ([foo].[x] : ?)
                    INSERT ([foo].[x] : ?, [foo].[y] : ?)
                    ON CONFLICT SET
                      ([foo].[x] : ?) = ("10" : System.Int32),
                      ([foo].[y] : ?) = (@dVal : System.Double)
                    WHERE ([foo].[x] : ?) > ("11" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Upsert_ShouldCreateUpsertNode_FromValues_WithUpdateFilter()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var values = SqlNode.Values( SqlNode.Literal( 5 ), SqlNode.Parameter<string>( "a" ) );
        var insertDataFields = new SqlDataFieldNode[] { set["x"], set["y"] };

        var updateAssignments = new[] { set["x"].Assign( SqlNode.Literal( 10 ) ), set["y"].Assign( SqlNode.Parameter<double>( "dVal" ) ) };
        var updateFilter = set["x"].IsGreaterThan( SqlNode.Literal( 11 ) );

        var conflictTarget = new SqlDataFieldNode[] { set["x"] };

        var dataFieldsSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        dataFieldsSelector.WithAnyArgs( _ => insertDataFields );
        var updateSelector = Substitute.For<Func<SqlRecordSetNode, SqlInternalRecordSetNode, SqlUpsertNodeUpdatePart>>();
        updateSelector.WithAnyArgs( _ => new SqlUpsertNodeUpdatePart( updateAssignments, updateFilter ) );
        var conflictTargetSelector = Substitute.For<Func<SqlRecordSetNode, IEnumerable<SqlDataFieldNode>>>();
        conflictTargetSelector.WithAnyArgs( _ => conflictTarget );
        var sut = values.ToUpsert( set, dataFieldsSelector, updateSelector, conflictTargetSelector );
        var text = sut.ToString();

        Assertion.All(
                dataFieldsSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                updateSelector.CallAt( 0 ).Arguments.TestSequence( [ set, sut.UpdateSource ] ),
                conflictTargetSelector.CallAt( 0 ).Arguments.TestSequence( [ set ] ),
                sut.NodeType.TestEquals( SqlNodeType.Upsert ),
                sut.Source.TestRefEquals( values ),
                sut.RecordSet.TestRefEquals( set ),
                sut.UpdateSource.Base.TestRefEquals( sut.RecordSet ),
                sut.InsertDataFields.TestSequence( insertDataFields ),
                sut.UpdateAssignments.TestSequence( updateAssignments ),
                sut.ConflictTarget.TestSequence( conflictTarget ),
                sut.UpdateFilter.TestRefEquals( updateFilter ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    UPSERT [foo] USING
                    VALUES
                    (("5" : System.Int32), (@a : System.String))
                    WITH CONFLICT TARGET ([foo].[x] : ?)
                    INSERT ([foo].[x] : ?, [foo].[y] : ?)
                    ON CONFLICT SET
                      ([foo].[x] : ?) = ("10" : System.Int32),
                      ([foo].[y] : ?) = (@dVal : System.Double)
                    WHERE ([foo].[x] : ?) > ("11" : System.Int32)
                    """ ) )
            .Go();
    }
}
