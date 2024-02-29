﻿using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlTableBuilderTests : TestsBase
{
    [Fact]
    public void Creation_ShouldPrepareCorrectStatement()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var schema = db.Schemas.Create( "foo" );
        db.Changes.ClearStatements();
        var sut = schema.Objects.CreateTable( "T" );
        var ix1 = sut.Constraints.CreateIndex( sut.Columns.Create( "C1" ).SetType<int>().Asc() );
        var ix2 = sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C2" ).SetType<int>().Asc() ).Index;
        sut.Constraints.CreateIndex( sut.Columns.Create( "C3" ).SetType<long>().Asc(), sut.Columns.Create( "C4" ).SetType<long>().Desc() );
        sut.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Constraints.CreateCheck( sut.Node["C1"] > SqlNode.Literal( 0 ) );

        var statements = db.Changes.GetPendingActions().ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE TABLE `foo`.`T` (
                      `C1` INT NOT NULL,
                      `C2` INT NOT NULL,
                      `C3` BIGINT NOT NULL,
                      `C4` BIGINT NOT NULL,
                      CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC),
                      CONSTRAINT `CHK_T_{GUID}` CHECK (`C1` > 0)
                    );",
                    "CREATE INDEX `IX_T_C1A` ON `foo`.`T` (`C1` ASC);",
                    "CREATE INDEX `IX_T_C3A_C4D` ON `foo`.`T` (`C3` ASC, `C4` DESC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C1_REF_T` FOREIGN KEY (`C1`) REFERENCES `foo`.`T` (`C2`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var schema = db.Schemas.Create( "foo" );
        db.Changes.ClearStatements();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var statements = db.Changes.GetPendingActions().ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldThrowMySqlObjectBuilderException_WhenTableDoesNotHavePrimaryKeyDuringScriptResolution()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "T" );
        sut.Columns.Create( "C" );

        var action = Lambda.Of(
            () => { _ = db.Changes.GetPendingActions(); } );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var db = MySqlDatabaseBuilderMock.Create();
        var sut = db.Schemas.Create( "foo" ).Objects.CreateTable( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Table] foo.bar" );
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var name = Fixture.Create<string>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( name );

        var result = ((ISqlTableBuilder)sut).SetName( name );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( name );
            schema.Objects.Contains( name ).Should().BeTrue();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangesToNewNameAndThenChangesToOldName()
    {
        var (oldName, newName) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( oldName );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName ).SetName( oldName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( oldName );
            schema.Objects.Contains( oldName ).Should().BeTrue();
            schema.Objects.Contains( newName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableDoesNotHaveAnyExternalReferences()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );
        var recordSet = sut.Node;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            sut.Info.Should().Be( SqlRecordSetInfo.Create( "s", "bar" ) );
            recordSet.Info.Should().Be( sut.Info );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "ALTER TABLE `s`.`foo` RENAME TO `s`.`bar`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasSelfReference()
    {
        var (oldName, newName) = ("foo", "bar");
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "s" );
        var sut = schema.Objects.CreateTable( oldName );
        var c1 = sut.Columns.Create( "C1" );
        var c2 = sut.Columns.Create( "C2" );
        var pk = sut.Constraints.SetPrimaryKey( c1.Asc() );
        var fk = sut.Constraints.CreateForeignKey( sut.Constraints.CreateIndex( c2.Asc() ), pk.Index );
        sut.Constraints.CreateCheck( c1.Node > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( newName );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( newName );
            schema.Objects.Contains( newName ).Should().BeTrue();
            schema.Objects.Contains( oldName ).Should().BeFalse();
            fk.IsRemoved.Should().BeFalse();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql( "ALTER TABLE `s`.`foo` RENAME TO `s`.`bar`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );

        var t3 = schema.Objects.CreateTable( "T3" );
        var c3 = t3.Columns.Create( "C3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.Constraints.SetPrimaryKey( c3.Asc() );

        var fk1 = t3.Constraints.CreateForeignKey( pk3.Index, pk1.Index );
        var fk2 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
        var fk3 = t3.Constraints.CreateForeignKey( t3.Constraints.CreateIndex( c4.Asc() ), pk1.Index );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesWithOneOfReferencingTablesUnderChange()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );

        var t3 = schema.Objects.CreateTable( "T3" );
        var c3 = t3.Columns.Create( "C3" );
        var c4 = t3.Columns.Create( "C4" );
        var pk3 = t3.Constraints.SetPrimaryKey( c3.Asc() );

        var t4 = schema.Objects.CreateTable( "T4" );
        var c5 = t4.Columns.Create( "C5" );
        var pk4 = t4.Constraints.SetPrimaryKey( c5.Asc() );

        var fk1 = t3.Constraints.CreateForeignKey( pk3.Index, pk1.Index );
        var fk2 = t4.Constraints.CreateForeignKey( pk4.Index, pk1.Index );
        var fk3 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );
        var fk4 = t3.Constraints.CreateForeignKey( t3.Constraints.CreateIndex( c4.Asc() ), pk1.Index );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        t3.Columns.Create( "C6" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();
            fk2.IsRemoved.Should().BeFalse();
            fk3.IsRemoved.Should().BeFalse();
            fk4.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 2 );

            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T3`
                      ADD COLUMN `C6` INT NOT NULL DEFAULT 0;" );

            statements.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Columns.Create( "C3" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;",
                    @"ALTER TABLE `foo`.`U`
                      ADD COLUMN `C3` INT NOT NULL DEFAULT 0;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasExternalForeignKeyReferencesAndUnrelatedTableHasPendingChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T1" );
        var c1 = sut.Columns.Create( "C1" );
        var pk1 = sut.Constraints.SetPrimaryKey( c1.Asc() );

        var t2 = schema.Objects.CreateTable( "T2" );
        var c2 = t2.Columns.Create( "C2" );
        var pk2 = t2.Constraints.SetPrimaryKey( c2.Asc() );
        var fk1 = t2.Constraints.CreateForeignKey( pk2.Index, pk1.Index );

        var t3 = schema.Objects.CreateTable( "T3" );
        t3.Constraints.SetPrimaryKey( t3.Columns.Create( "C3" ).Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        t3.Columns.Create( "C4" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T1" ).Should().BeFalse();
            fk1.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 2 );

            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T3`
                      ADD COLUMN `C4` INT NOT NULL DEFAULT 0;" );

            statements.ElementAtOrDefault( 1 )
                .Sql.Should()
                .SatisfySql( "ALTER TABLE `foo`.`T1` RENAME TO `foo`.`U`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var v2 = schema.Objects.CreateView(
            "V2",
            v1.ToRecordSet().Join( sut.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T" ).Should().BeFalse();
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 5 );

            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V2`;" );
            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V1`;" );

            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;" );

            statements.ElementAtOrDefault( 3 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW `foo`.`V1` AS
                    SELECT
                      `foo`.`U`.`C`
                    FROM `foo`.`U`;" );

            statements.ElementAtOrDefault( 4 )
                .Sql
                .Should()
                .SatisfySql(
                    @"CREATE VIEW `foo`.`V2` AS
                    SELECT
                      *
                    FROM `foo`.`V1`
                    INNER JOIN `foo`.`U` ON TRUE;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNameChangesAndTableHasViewReferencesAndChangedTableHasOtherPendingChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C" ).Asc() );

        var v1 = schema.Objects.CreateView( "V1", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C"].AsSelf() } ) );

        var v2 = schema.Objects.CreateView(
            "V2",
            v1.ToRecordSet().Join( sut.ToRecordSet().InnerOn( SqlNode.True() ) ).Select( s => new[] { s.GetAll() } ) );

        var v3 = schema.Objects.CreateView( "V3", v1.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Columns.Create( "D" ).SetType<int>();
        var result = ((ISqlTableBuilder)sut).SetName( "U" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "U" );
            schema.Objects.Contains( "U" ).Should().BeTrue();
            schema.Objects.Contains( "T" ).Should().BeFalse();
            v1.IsRemoved.Should().BeFalse();
            v2.IsRemoved.Should().BeFalse();
            v3.IsRemoved.Should().BeFalse();

            statements.Should().HaveCount( 6 );

            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD COLUMN `D` INT NOT NULL DEFAULT 0;" );

            statements.ElementAtOrDefault( 1 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V2`;" );
            statements.ElementAtOrDefault( 2 ).Sql.Should().SatisfySql( "DROP VIEW `foo`.`V1`;" );

            statements.ElementAtOrDefault( 3 ).Sql.Should().SatisfySql( "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;" );

            statements.ElementAtOrDefault( 4 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE VIEW `foo`.`V1` AS
                    SELECT
                      `foo`.`U`.`C`
                    FROM `foo`.`U`;" );

            statements.ElementAtOrDefault( 5 )
                .Sql.Should()
                .SatisfySql(
                    @"CREATE VIEW `foo`.`V2` AS
                    SELECT
                      *
                    FROM `foo`.`V1`
                    INNER JOIN `foo`.`U` ON TRUE;" );
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
        var sut = db.Schemas.Default.Objects.CreateTable( Fixture.Create<string>() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenObjectWithNameAlreadyExists()
    {
        var (name1, name2) = Fixture.CreateDistinctCollection<string>( count: 2 );
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var other = schema.Objects.CreateTable( name2 );
        other.Constraints.SetPrimaryKey( other.Columns.Create( "C" ).Asc() );
        var sut = schema.Objects.CreateTable( name1 );

        var action = Lambda.Of( () => sut.SetName( name2 ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenTableHasBeenRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( Fixture.Create<string>() );
        schema.Objects.Remove( sut.Name );

        var action = Lambda.Of( () => sut.SetName( Fixture.Create<string>() ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveTable_WhenTableDoesNotHaveAnyExternalReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var otherColumn = sut.Columns.Create( "D" );
        var pk = sut.Constraints.SetPrimaryKey( column.Asc() );
        var ix = sut.Constraints.CreateIndex( otherColumn.Asc() );
        var fk = sut.Constraints.CreateForeignKey( ix, pk.Index );
        var chk = sut.Constraints.CreateCheck( column.Node > SqlNode.Literal( 0 ) );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            schema.Objects.Count.Should().Be( 0 );
            sut.IsRemoved.Should().BeTrue();
            column.IsRemoved.Should().BeTrue();
            otherColumn.IsRemoved.Should().BeTrue();
            pk.IsRemoved.Should().BeTrue();
            pk.Index.IsRemoved.Should().BeTrue();
            ix.IsRemoved.Should().BeTrue();
            fk.IsRemoved.Should().BeTrue();
            chk.IsRemoved.Should().BeTrue();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP TABLE `foo`.`T`;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenTableHasAlreadyBeenRemoved()
    {
        var name = Fixture.Create<string>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var sut = schema.Objects.CreateTable( name );
        sut.Remove();

        sut.Remove();

        using ( new AssertionScope() )
        {
            schema.Objects.Contains( name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenTableHasExternalForeignKeyReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        var pk = sut.Constraints.SetPrimaryKey( column.Asc() );

        var otherTable = schema.Objects.CreateTable( "U" );
        var otherColumn = otherTable.Columns.Create( "D" );
        var otherPk = otherTable.Constraints.SetPrimaryKey( otherColumn.Asc() );
        otherTable.Constraints.CreateForeignKey( otherPk.Index, pk.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenTableHasViewReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        var column = sut.Columns.Create( "C" );
        sut.Constraints.SetPrimaryKey( column.Asc() );
        schema.Objects.CreateView( "V", sut.ToRecordSet().ToDataSource().Select( s => new[] { s.GetAll() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ColumnNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "C" );
        b.SetName( "A" );
        a.SetName( "B" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `A` `B` LONGBLOB NOT NULL,
                      CHANGE COLUMN `B` `A` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void ColumnChainNameSwap_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );
        var d = sut.Columns.Create( "D" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "E" );
        b.SetName( "A" );
        c.SetName( "B" );
        d.SetName( "C" );
        a.SetName( "D" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `A` `D` LONGBLOB NOT NULL,
                      CHANGE COLUMN `B` `A` LONGBLOB NOT NULL,
                      CHANGE COLUMN `C` `B` LONGBLOB NOT NULL,
                      CHANGE COLUMN `D` `C` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void MultipleColumnNameChange_ShouldGenerateCorrectScript_WhenThereAreTemporaryNameConflicts()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schema.Database.Changes.ClearStatements();
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "P" ).Asc() );
        var a = sut.Columns.Create( "A" );
        var b = sut.Columns.Create( "B" );
        var c = sut.Columns.Create( "C" );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        a.SetName( "X" );
        b.SetName( "Y" );
        c.SetName( "D" );
        b.SetName( "C" );
        a.SetName( "B" );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `A` `B` LONGBLOB NOT NULL,
                      CHANGE COLUMN `B` `C` LONGBLOB NOT NULL,
                      CHANGE COLUMN `C` `D` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void MultipleSimpleTableChanges_ShouldGenerateCorrectScript()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var sut = schema.Objects.CreateTable( "T" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "C1" ).SetType<int>().Asc() );
        var c2 = sut.Columns.Create( "C2" ).SetType<int>();
        var c3 = sut.Columns.Create( "C3" ).SetType<int>();
        var c4 = sut.Columns.Create( "C4" ).SetType<int>();
        var ix = sut.Constraints.CreateIndex( c2.Asc() );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "U" );
        c3.SetName( "X" );
        c4.Remove();
        ix.Remove();
        sut.Constraints.CreateIndex( c2.Asc(), c3.Desc() );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "ALTER TABLE `foo`.`T` RENAME TO `foo`.`U`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`U`;",
                    @"ALTER TABLE `foo`.`U`
                      DROP COLUMN `C4`,
                      CHANGE COLUMN `C3` `X` INT NOT NULL;",
                    "CREATE INDEX `IX_U_C2A_XD` ON `foo`.`U` (`C2` ASC, `X` DESC);" );
        }
    }

    [Fact]
    public void NameChange_ThenRemoval_ShouldDropTableByUsingItsOldName()
    {
        var builder = MySqlDatabaseBuilderMock.Create();
        var sut = builder.Schemas.Create( "s" ).Objects.CreateTable( "foo" );
        sut.Constraints.SetPrimaryKey( sut.Columns.Create( "a" ).Asc() );
        _ = builder.Changes.GetPendingActions();

        sut.SetName( "bar" );
        sut.Remove();

        var result = builder.Changes.GetPendingActions()[^1].Sql;

        result.Should().SatisfySql( "DROP TABLE `s`.`foo`;" );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenTableIsMySql()
    {
        var action = Substitute.For<Action<MySqlTableBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenTableIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlTableBuilder>>();
        var sut = Substitute.For<ISqlTableBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}