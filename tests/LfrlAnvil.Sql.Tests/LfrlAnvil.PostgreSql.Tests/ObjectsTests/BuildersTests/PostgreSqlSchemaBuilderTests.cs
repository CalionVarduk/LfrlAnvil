using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public partial class PostgreSqlSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Schema] foo" );
    }

    [Fact]
    public void Creation_ShouldAddCorrectStatement()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();

        var actionCount = db.GetPendingActionCount();
        var sut = db.Schemas.Create( "foo" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "foo" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "CREATE SCHEMA \"foo\";" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChanges()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER SCHEMA \"foo\" RENAME TO \"bar\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaHasObjects()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        t1.Constraints.SetPrimaryKey( c1.Asc() );
        t1.Constraints.CreateIndex( c2.Asc() );
        var recordSet1 = t1.Node;
        _ = t1.Info;

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        t2.Constraints.SetPrimaryKey( c3.Asc() );
        t2.Constraints.CreateCheck( SqlNode.True() );
        var recordSet2 = t2.Node;
        _ = t2.Info;

        var v1 = sut.Objects.CreateView( "V1", SqlNode.RawQuery( "SELECT * FROM bar" ) );
        var recordSet3 = v1.Node;
        _ = v1.Info;

        var v2 = sut.Objects.CreateView( "V2", SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var recordSet4 = v2.Node;
        _ = v2.Info;

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            t1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T1" ) );
            recordSet1.Info.Should().Be( t1.Info );
            t2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T2" ) );
            recordSet2.Info.Should().Be( t2.Info );
            v1.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V1" ) );
            recordSet3.Info.Should().Be( v1.Info );
            v2.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V2" ) );
            recordSet4.Info.Should().Be( v2.Info );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER SCHEMA \"foo\" RENAME TO \"bar\";" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndAnotherSchemaContainsReferencingView()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Default;

        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var recordSet1 = table.Node;
        _ = table.Info;

        var view = sut.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var recordSet2 = view.Node;
        _ = view.Info;

        other.Objects.CreateView( "V", view.Node.Join( table.Node.InnerOn( SqlNode.True() ) ).Select( d => new[] { d.GetAll() } ) );

        var actionCount = db.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            db.Schemas.TryGet( "bar" ).Should().BeSameAs( sut );
            db.Schemas.TryGet( "foo" ).Should().BeNull();

            table.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "T" ) );
            recordSet1.Info.Should().Be( table.Info );
            view.Info.Should().Be( SqlRecordSetInfo.Create( "bar", "V" ) );
            recordSet2.Info.Should().Be( view.Info );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER SCHEMA \"foo\" RENAME TO \"bar\";" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenSchemaIsRemoved()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemas()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var other = db.Schemas.Create( "bar" );

        var action = Lambda.Of( () => sut.SetName( other.Name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAddStatement()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP SCHEMA \"foo\" CASCADE;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAllOfItsObjects_WhenSchemaHasTablesAndViewsWithoutReferencesFromOtherSchemas()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        var pk1 = t1.Constraints.SetPrimaryKey( c1.Asc() );
        var ix1 = t1.Constraints.CreateIndex( c2.Asc() );
        var fk1 = t1.Constraints.CreateForeignKey( ix1, pk1.Index );
        var chk1 = t1.Constraints.CreateCheck( c1.Node != null );

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        var c4 = t2.Columns.Create( "C4" );
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );
        var ix2 = t2.Constraints.CreateIndex( c4.Asc() );
        var fk2 = t2.Constraints.CreateForeignKey( ix2, pk1.Index );

        var t3 = sut.Objects.CreateTable( "T3" );
        var c5 = t3.Columns.Create( "C5" );
        var c6 = t3.Columns.Create( "C6" );
        var pk3 = t3.Constraints.SetPrimaryKey( c5.Asc() );
        var fk3 = t3.Constraints.CreateForeignKey( pk3.Index, pk2.Index );
        var chk2 = t3.Constraints.CreateCheck( c5.Node > SqlNode.Literal( 0 ) );

        var t4 = sut.Objects.CreateTable( "T4" );
        var c7 = t4.Columns.Create( "C7" );
        var pk4 = t4.Constraints.SetPrimaryKey( c7.Asc() );
        var fk4 = t4.Constraints.CreateForeignKey( pk4.Index, pk3.Index );

        var ix3 = t3.Constraints.CreateIndex( c6.Asc() );
        var fk5 = t3.Constraints.CreateForeignKey( ix3, pk4.Index );

        var v1 = sut.Objects.CreateView( "V1", t2.Node.Join( t3.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );
        var v2 = sut.Objects.CreateView( "V2", t1.Node.Join( v1.Node.InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            db.Schemas.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            t1.IsRemoved.Should().BeTrue();
            t2.IsRemoved.Should().BeTrue();
            t3.IsRemoved.Should().BeTrue();
            t4.IsRemoved.Should().BeTrue();
            c1.IsRemoved.Should().BeTrue();
            c2.IsRemoved.Should().BeTrue();
            c3.IsRemoved.Should().BeTrue();
            c4.IsRemoved.Should().BeTrue();
            c5.IsRemoved.Should().BeTrue();
            c6.IsRemoved.Should().BeTrue();
            c7.IsRemoved.Should().BeTrue();
            pk1.IsRemoved.Should().BeTrue();
            pk2.IsRemoved.Should().BeTrue();
            pk3.IsRemoved.Should().BeTrue();
            pk4.IsRemoved.Should().BeTrue();
            pk1.Index.IsRemoved.Should().BeTrue();
            pk2.Index.IsRemoved.Should().BeTrue();
            pk3.Index.IsRemoved.Should().BeTrue();
            pk4.Index.IsRemoved.Should().BeTrue();
            ix1.IsRemoved.Should().BeTrue();
            ix2.IsRemoved.Should().BeTrue();
            ix3.IsRemoved.Should().BeTrue();
            fk1.IsRemoved.Should().BeTrue();
            fk2.IsRemoved.Should().BeTrue();
            fk3.IsRemoved.Should().BeTrue();
            fk4.IsRemoved.Should().BeTrue();
            fk5.IsRemoved.Should().BeTrue();
            v1.IsRemoved.Should().BeTrue();
            v2.IsRemoved.Should().BeTrue();
            chk1.IsRemoved.Should().BeTrue();
            chk2.IsRemoved.Should().BeTrue();
            sut.Objects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP SCHEMA \"foo\" CASCADE;" );
        }
    }

    [Fact]
    public void Remove_ShouldNotAddAnyStatements_WhenSchemaIsRemoved()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        db.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = db.GetPendingActionCount();
        sut.Remove();
        var actions = db.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsDefault()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default.SetName( "foo" );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByForeignKeyFromAnotherSchema()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T1" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherTable.Columns.Create( "C2" ).Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenSchemaIsReferencedByViewFromAnotherSchema()
    {
        var db = PostgreSqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );
        var table = sut.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        db.Schemas.Default.Objects.CreateView( "V", table.Node.ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == PostgreSqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenSchemaIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlSchemaBuilder>>();
        var sut = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenSchemaIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlSchemaBuilder>>();
        var sut = Substitute.For<ISqlSchemaBuilder>();

        var result = sut.ForPostgreSql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
