using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlPrimaryKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.TestEquals( "[PrimaryKey] foo.bar" ).Go();
    }

    [Fact]
    public void Change_ShouldMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" ).SetType<int>();
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        table.Constraints.SetPrimaryKey( c1.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.SetPrimaryKey( c2.Asc() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                table.Constraints.TryGet( sut.Name ).TestRefEquals( sut ),
                schema.Objects.TryGet( sut.Name ).TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP PRIMARY KEY,
                                ADD CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC);
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
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
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).SetType<int>().Asc() );
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
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP PRIMARY KEY,
                                ADD CONSTRAINT `bar` PRIMARY KEY (`C` ASC);
                            """ )
                    ] ) )
            .Go();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

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
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).SetType<int>().Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.Name.TestEquals( "PK_T" ),
                table.Constraints.TryGet( "PK_T" ).TestRefEquals( sut ),
                table.Constraints.TryGet( "bar" ).TestNull(),
                schema.Objects.TryGet( "PK_T" ).TestRefEquals( sut ),
                schema.Objects.TryGet( "bar" ).TestNull(),
                actions.Select( a => a.Sql )
                    .TestSequence(
                    [
                        (sql, _) => sql.TestSatisfySql(
                            """
                            ALTER TABLE `foo`.`T`
                                DROP PRIMARY KEY,
                                ADD CONSTRAINT `PK_T` PRIMARY KEY (`C` ASC);
                            """ )
                    ] ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );
        table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "PK_T" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndUnderlyingIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C1" );
        var sut = table.Constraints.SetPrimaryKey( column.Asc() );

        sut.Remove();

        Assertion.All(
                table.Constraints.TryGetPrimaryKey().TestNull(),
                table.Constraints.TryGet( sut.Name ).TestNull(),
                table.Constraints.TryGet( sut.Index.Name ).TestNull(),
                schema.Objects.TryGet( sut.Name ).TestNull(),
                schema.Objects.TryGet( sut.Index.Name ).TestNull(),
                sut.IsRemoved.TestTrue(),
                sut.Index.IsRemoved.TestTrue(),
                sut.Index.PrimaryKey.TestNull(),
                sut.Index.Columns.Expressions.TestEmpty(),
                column.ReferencingObjects.TestEmpty() )
            .Go();
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();
        table.Constraints.SetPrimaryKey( table.Columns.Get( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.TestEmpty().Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenUnderlyingIndexIsReferencedByOriginatingForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        var target = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() );
        t1.Constraints.CreateForeignKey( sut.Index, target.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenUnderlyingIndexIsReferencedByReferencingForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        var target = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() );
        t2.Constraints.CreateForeignKey( target.Index, sut.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Test( exc => exc.TestType()
                .Exact<SqlObjectBuilderException>( e => Assertion.All(
                    e.Dialect.TestEquals( MySqlDialect.Instance ),
                    e.Errors.Count.TestEquals( 1 ) ) ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenPrimaryKeyIsMySql()
    {
        var action = Substitute.For<Action<MySqlPrimaryKeyBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenPrimaryKeyIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlPrimaryKeyBuilder>>();
        var sut = Substitute.For<ISqlPrimaryKeyBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
