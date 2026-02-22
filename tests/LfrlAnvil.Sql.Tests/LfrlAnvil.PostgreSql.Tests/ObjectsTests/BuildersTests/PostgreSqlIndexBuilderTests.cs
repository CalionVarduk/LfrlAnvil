using System.Linq;
using System.Text.RegularExpressions;
using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests.BuildersTests;

public class PostgreSqlIndexBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[Index] foo.bar" ).Go();
    }

    [Fact]
    public void Creation_ShouldNotMarkTableForAlteration()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var ixc2 = c2.Asc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( ixc2 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestMatch( new Regex( "IX_T_C2A" ) ),
                sut.Columns.Expressions.TestSequence( [ ixc2 ] ),
                sut.ReferencedColumns.TestSequence( [ c2 ] ),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "CREATE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC);" ) ] ) )
            .Go();
    }

    [Fact]
    public void Creation_ShouldNotMarkTableForAlteration_WhenIndexContainsExpressions()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var c3 = table.Columns.Create( "C3" ).SetType<int>();
        var ixc1 = c2.Asc();
        var ixc2 = (c3.Node + SqlNode.Literal( 1 )).Desc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( ixc1, ixc2 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestMatch( new Regex( "IX_T_C2A_E1D" ) ),
                sut.Columns.Expressions.TestSequence( [ ixc1, ixc2 ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql( "CREATE INDEX \"IX_T_C2A_E1D\" ON \"foo\".\"T\" (\"C2\" ASC, (\"C3\" + 1) DESC);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( c2.Asc() );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Creation_ShouldNotCreateIndex_WhenIndexIsAttachedToPrimaryKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var ixc2 = c2.Asc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateUniqueIndex( ixc2 );
        table.Constraints.SetPrimaryKey( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestMatch( new Regex( "UIX_T_C2A" ) ),
                sut.Columns.Expressions.TestSequence( [ ixc2 ] ),
                sut.ReferencedColumns.TestSequence( [ c2 ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE "foo"."T"
                                DROP CONSTRAINT "PK_T";
                            """,
                            """
                            ALTER TABLE "foo"."T"
                                ADD CONSTRAINT "PK_T" PRIMARY KEY ("C2");
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER INDEX \"foo\".\"IX_T_C2A\" RENAME TO \"bar\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldUpdateNameAndNotRecreateOriginatingForeignKeys()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER INDEX \"foo\".\"IX_T_C2A\" RENAME TO \"bar\";" ) ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "\"" )]
    [InlineData( "'" )]
    [InlineData( "f\"oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index.SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "UIX_T_C1A" ),
                table.Constraints.TryGet( "UIX_T_C1A" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "UIX_T_C1A" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "IX_T_C2A" ),
                table.Constraints.TryGet( "IX_T_C2A" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "IX_T_C2A" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER INDEX \"foo\".\"bar\" RENAME TO \"IX_T_C2A\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndIndexIsUnique()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "UIX_T_C2A" ),
                table.Constraints.TryGet( "UIX_T_C2A" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "UIX_T_C2A" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "ALTER INDEX \"foo\".\"bar\" RENAME TO \"UIX_T_C2A\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        pk.SetName( "IX_T_C2A" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsUnique_ShouldDoNothing_WhenUniquenessFlagDoesNotChange(bool value)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsUnique();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsUnique.TestTrue(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE UNIQUE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToFalse()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsUnique.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexUniquenessChangesToFalse()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenVirtualIndexUniquenessChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexWithExpressionsUniquenessChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( (table.Columns.Create( "C2" ).Node + SqlNode.Literal( 1 )).Asc() );

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenUniquenessChangesToFalseAndIndexIsReferencedByForeignKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUniqueAndNameCorrectly_WhenIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique().SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsUnique.TestTrue(),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE UNIQUE INDEX \"bar\" ON \"foo\".\"T\" (\"C2\" ASC);" )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsVirtual_ShouldDoNothing_WhenVirtualityFlagDoesNotChange(bool value)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsVirtual();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.TestEmpty() )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsVirtual.TestTrue(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP INDEX \"foo\".\"IX_T_C2A\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToFalse()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsVirtual();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsVirtual.TestFalse(),
                actions.Select( a => a.Sql )
                    .TestSequence( [ (sql, _) => sql.TestSatisfySql( "CREATE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC);" ) ] ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexVirtualityChangesToFalse()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsVirtual( false ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPartialIndexVirtualityChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetFilter( SqlNode.True() );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenUniqueIndexVirtualityChangesToTrue()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsUnique();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenVirtualityChangesToTrueAndIndexIsReferencedByForeignKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 2 ) ) ) )
            .Go();
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueDoesNotChange()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( SqlNode.True() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.Length.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetFilter( SqlNode.True() );
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                actions.Length.TestEquals( 0 ) )
            .Go();
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNonNull()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( t => t["C2"] != null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Filter.TestType()
                    .AssignableTo<SqlNotEqualToConditionNode>( n => Assertion.All(
                        n.Left.TestType()
                            .AssignableTo<SqlColumnBuilderNode>( cn => Assertion.All(
                                cn.Name.TestEquals( "C2" ),
                                cn.RecordSet.TestRefEquals( table.Node ) ) ),
                        n.Right.TestType().AssignableTo<SqlNullNode>() ) ),
                result.ReferencedFilterColumns.TestSequence( [ column ] ),
                column.ReferencingObjects.Count.TestEquals( 2 ),
                column.ReferencingObjects.TestSetEqual(
                [
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Filter" ), column )
                ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC) WHERE (\"C2\" IS NOT NULL);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNull()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                result.Filter.TestNull(),
                result.ReferencedFilterColumns.TestEmpty(),
                column.ReferencingObjects.TestSequence(
                    [ SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) ] ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE INDEX \"IX_T_C2A\" ON \"foo\".\"T\" (\"C2\" ASC);" )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenFilterIsInvalid(SqlOptionalFunctionalityResolution indexFilterResolution)
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create( indexFilterResolution ).Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetFilter( _ => SqlNode.WindowFunctions.RowNumber() == SqlNode.Literal( 0 ) ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexFilterChangesToNonNull()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexFilterChangesToNonNull()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsVirtual()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetFilter( null ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void
        SetFilter_ShouldUpdateFilterAndIsUniqueAndNameCorrectly_WhenFilterAndIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique().SetFilter( t => t["C2"] != null ).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.IsUnique.TestTrue(),
                result.Filter.TestType()
                    .AssignableTo<SqlNotEqualToConditionNode>( n => Assertion.All(
                        n.Left.TestType()
                            .AssignableTo<SqlColumnBuilderNode>( cn => Assertion.All(
                                cn.Name.TestEquals( "C2" ),
                                cn.RecordSet.TestRefEquals( table.Node ) ) ),
                        n.Right.TestType().AssignableTo<SqlNullNode>() ) ),
                sut.Name.TestEquals( "bar" ),
                table.Constraints.TryGet( "bar" ).TestRefEquals( sut ),
                table.Constraints.TryGet( oldName ).TestNull(),
                schema.Objects.TryGet( "bar" ).TestRefEquals( sut ),
                schema.Objects.TryGet( oldName ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            "DROP INDEX \"foo\".\"IX_T_C2A\";",
                            "CREATE UNIQUE INDEX \"bar\" ON \"foo\".\"T\" (\"C2\" ASC) WHERE (\"C2\" IS NOT NULL);" )
                    ] ) )
            .Go();
    }

    [Fact]
    public void PrimaryKeyAssignment_ShouldDropIndexByItsOldName_WhenIndexNameAlsoChanges()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        table.Constraints.SetPrimaryKey( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Select( a => a.Sql )
            .TestSequence(
            [
                (sql, _) => sql.TestSatisfySql(
                    "DROP INDEX \"foo\".\"UIX_T_C2A\";",
                    """
                    ALTER TABLE "foo"."T"
                        DROP CONSTRAINT "PK_T";
                    """,
                    """
                    ALTER TABLE "foo"."T"
                        ADD CONSTRAINT "PK_T" PRIMARY KEY ("C2");
                    """ )
            ] )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndClearReferencedColumnsAndReferencedFilterColumns()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( c2.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.Columns.Expressions.TestEmpty(),
                sut.ReferencedColumns.TestEmpty(),
                sut.ReferencedFilterColumns.TestEmpty(),
                sut.Filter.TestNull(),
                c2.ReferencingObjects.TestEmpty(),
                actions.Select( a => a.Sql ).TestSequence( [ (sql, _) => sql.TestSatisfySql( "DROP INDEX \"foo\".\"IX_T_C2A\";" ) ] ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndAssignedPrimaryKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        sut.Remove();

        Assertion.All(
                table.Constraints.TryGetPrimaryKey().TestNull(),
                table.Constraints.TryGet( sut.Name ).TestNull(),
                table.Constraints.TryGet( pk.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( pk.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.PrimaryKey.TestNull(),
                sut.Columns.Expressions.TestEmpty(),
                sut.ReferencedColumns.TestEmpty(),
                pk.IsRemoved.TestTrue(),
                column.ReferencingObjects.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasOriginatingForeignKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, ix );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType().Exact<SqlObjectBuilderException>() ).Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasReferencingForeignKey()
    {
        var schema = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( PostgreSqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldInvokeAction_WhenIndexIsPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlIndexBuilder>>();
        var table = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForPostgreSql_ShouldNotInvokeAction_WhenIndexIsNotPostgreSql()
    {
        var action = Substitute.For<Action<PostgreSqlIndexBuilder>>();
        var sut = Substitute.For<ISqlIndexBuilder>();

        var result = sut.ForPostgreSql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
