using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        result.Should().Be( "[PrimaryKey] foo.bar" );
    }

    [Fact]
    public void Change_ShouldMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" ).SetType<int>();
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        table.Constraints.SetPrimaryKey( c1.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        table.Constraints.SetPrimaryKey( c2.Asc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( sut.Name );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( oldName );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).SetType<int>().Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            sut.FullName.Should().Be( "foo.bar" );
            table.Constraints.GetConstraint( "bar" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `bar` PRIMARY KEY (`C` ASC);" );
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
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).SetType<int>().Asc() ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlPrimaryKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "PK_T" );
            sut.FullName.Should().Be( "foo.PK_T" );
            table.Constraints.GetConstraint( "PK_T" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "PK_T" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql
                .Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C` ASC);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).SetName( "bar" );
        table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "PK_T" );

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlPrimaryKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndUnderlyingIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            table.Constraints.Contains( sut.Index.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Index.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.Index.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldRemovePrimaryKeyAndRemoveSelfReferencingForeignKeysToItsIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.SetPrimaryKey( c1.Asc() );
        var ix = table.Constraints.CreateIndex( c2.Asc() );
        var fk = table.Constraints.CreateForeignKey( ix, sut.Index );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            table.Constraints.Contains( sut.Index.Name ).Should().BeFalse();
            table.Constraints.Contains( fk.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Index.Name ).Should().BeFalse();
            schema.Objects.Contains( fk.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.Index.IsRemoved.Should().BeTrue();
            sut.Index.PrimaryKey.Should().BeNull();
            fk.IsRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenPrimaryKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.SetPrimaryKey( c1.Asc() );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        table.Constraints.SetPrimaryKey( c2.Asc() );
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenUnderlyingIndexIsReferencedByAnotherTable()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        var sut = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var t2 = schema.Objects.CreateTable( "T2" );
        t2.Constraints.CreateForeignKey( t2.Constraints.CreateIndex( t2.Columns.Create( "C2" ).Asc() ), sut.Index );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenPrimaryKeyIsMySql()
    {
        var action = Substitute.For<Action<MySqlPrimaryKeyBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenPrimaryKeyIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlPrimaryKeyBuilder>>();
        var sut = Substitute.For<ISqlPrimaryKeyBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
