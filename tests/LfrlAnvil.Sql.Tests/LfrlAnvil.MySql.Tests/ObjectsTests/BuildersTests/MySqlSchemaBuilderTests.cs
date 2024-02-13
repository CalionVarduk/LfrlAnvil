using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlSchemaBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" );

        var result = sut.ToString();

        result.Should().Be( "[Schema] foo" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( name );

        var result = ((ISqlObjectBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            db.Schemas.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaDoesNotHaveAnyObjects()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( oldName );

        var startStatementCount = db.Changes.GetPendingActions().Length;

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );
        var statements = db.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 2 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( $"CREATE SCHEMA `{newName}`;" );
            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( $"DROP SCHEMA `{oldName}`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaIsOriginallyCommon()
    {
        var oldName = "common";
        var newName = Fixture.Create<string>();
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.GetSchema( oldName );

        var startStatementCount = db.Changes.GetPendingActions().Length;

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );
        var statements = db.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( $"CREATE SCHEMA `{newName}`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesToCommon()
    {
        var oldName = Fixture.Create<string>();
        var newName = "common";
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Default.SetName( "x" );
        var sut = db.Schemas.Create( oldName );

        var startStatementCount = db.Changes.GetPendingActions().Length;

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );
        var statements = db.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( $"DROP SCHEMA `{oldName}`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndSchemaHasObjects()
    {
        var (oldName, newName) = ("foo", "bar");
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( oldName );

        var t1 = sut.Objects.CreateTable( "T1" );
        var c1 = t1.Columns.Create( "C1" );
        var c2 = t1.Columns.Create( "C2" ).MarkAsNullable();
        var pk1 = t1.Constraints.SetPrimaryKey( c1.Asc() );
        var ix1 = t1.Constraints.CreateIndex( c2.Asc() );
        var fk1 = t1.Constraints.CreateForeignKey( ix1, pk1.Index );
        var chk1 = t1.Constraints.CreateCheck( "CHK_T1_0", c1.Node != SqlNode.Literal( 0 ) );

        var t2 = sut.Objects.CreateTable( "T2" );
        var c3 = t2.Columns.Create( "C3" );
        var pk2 = t2.Constraints.SetPrimaryKey( c3.Asc() );
        var fk2 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var t3 = sut.Objects.CreateTable( "T3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.Constraints.SetPrimaryKey( c4.Asc() );
        var chk2 = t3.Constraints.CreateCheck( "CHK_T3_0", c4.Node > SqlNode.Literal( 10 ) );

        var v1 = sut.Objects.CreateView(
            "V1",
            t2.ToRecordSet().Join( t3.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v2 = sut.Objects.CreateView(
            "V2",
            t1.ToRecordSet().Join( v1.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = db.Changes.GetPendingActions().Length;

        var result = ((ISqlSchemaBuilder)sut).SetName( newName );
        var statements = db.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            db.Schemas.Contains( newName ).Should().BeTrue();
            db.Schemas.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 9 );

            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V2`;" );
            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V1`;" );
            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "CREATE SCHEMA `bar`;" );

            statements.Should()
                .Contain(
                    s => s.Sql != null &&
                        s.Sql.Replace( Environment.NewLine, string.Empty ) == "ALTER TABLE `foo`.`T1` RENAME TO `bar`.`T1`;" );

            statements.Should()
                .Contain(
                    s => s.Sql != null &&
                        s.Sql.Replace( Environment.NewLine, string.Empty ) == "ALTER TABLE `foo`.`T2` RENAME TO `bar`.`T2`;" );

            statements.Should()
                .Contain(
                    s => s.Sql != null &&
                        s.Sql.Replace( Environment.NewLine, string.Empty ) == "ALTER TABLE `foo`.`T3` RENAME TO `bar`.`T3`;" );

            statements.ElementAtOrDefault( 6 ).Sql.Should().SatisfySql( "DROP SCHEMA `foo`;" );

            statements.ElementAtOrDefault( 7 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE VIEW `bar`.`V1` AS
SELECT
  *
FROM `bar`.`T2`
INNER JOIN `bar`.`T3` ON TRUE;" );

            statements.ElementAtOrDefault( 8 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE VIEW `bar`.`V2` AS
SELECT
  *
FROM `bar`.`T1`
INNER JOIN `bar`.`V1` ON TRUE;" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( Fixture.Create<string>() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenSchemaWithNameAlreadyExists()
    {
        var (name1, name2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Create( name2 );
        var sut = db.Schemas.Create( name1 );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenSchemaHasBeenRemoved()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        db.Schemas.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveSchema_WhenSchemaDoesNotHaveAnyObjects()
    {
        var name = Fixture.Create<string>();
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( name );

        sut.Remove();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenSchemaHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( name );
        sut.Remove();

        sut.Remove();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldRemoveSchemaAndAllOfItsObjects_WhenSchemaHasTablesAndViewsWithoutReferencesFromOtherSchemas()
    {
        var name = "foo";
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( name );

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

        var ix3 = t3.Constraints.CreateIndex( c6.Asc() ).SetFilter( SqlNode.True() );
        var fk5 = t3.Constraints.CreateForeignKey( ix3, pk4.Index );

        var v1 = sut.Objects.CreateView(
            "V1",
            t2.ToRecordSet().Join( t3.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v2 = sut.Objects.CreateView(
            "V2",
            t1.ToRecordSet().Join( v1.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = db.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = db.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            db.Schemas.Contains( name ).Should().BeFalse();
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

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP SCHEMA `foo`;" );
        }
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenAttemptingToRemoveDefaultSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Default.SetName( "foo" );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenAttemptingToRemoveCommonSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        db.Schemas.Default.SetName( "foo" );
        var sut = db.Schemas.Create( db.CommonSchemaName );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenAttemptingToRemoveSchemaWithTableReferencedByForeignKeyFromOtherSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        var table = sut.Objects.CreateTable( "T1" );
        var column = table.Columns.Create( "C1" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        var otherTable = db.Schemas.Default.Objects.CreateTable( "T2" );
        var otherColumn = otherTable.Columns.Create( "C2" );
        var otherColumn2 = otherTable.Columns.Create( "C3" );
        otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
        var otherIndex = otherTable.Constraints.CreateIndex( otherColumn2.Asc() );
        otherTable.Constraints.CreateForeignKey( otherTable.Constraints.GetPrimaryKey().Index, table.Constraints.GetPrimaryKey().Index );
        otherTable.Constraints.CreateForeignKey( otherIndex, table.Constraints.GetPrimaryKey().Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenAttemptingToRemoveSchemaWithTableReferencedByViewFromOtherSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        var table = sut.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        table.Constraints.SetPrimaryKey( column.Asc() );

        db.Schemas.Default.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenAttemptingToRemoveSchemaWithViewReferencedByViewFromOtherSchema()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( Fixture.Create<string>() );
        var view = sut.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );

        db.Schemas.Default.Objects.CreateView( "W", view.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenSchemaIsMySql()
    {
        var action = Substitute.For<Action<MySqlSchemaBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default;

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenSchemaIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlSchemaBuilder>>();
        var sut = Substitute.For<ISqlSchemaBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
