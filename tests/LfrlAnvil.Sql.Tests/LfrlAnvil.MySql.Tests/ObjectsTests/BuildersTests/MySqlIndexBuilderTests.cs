using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlIndexBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Index] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldNotMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();

        var startStatementsCount = schema.Database.GetPendingStatements().Length;

        table.Constraints.CreateIndex( c2.Asc() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementsCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Constraints.CreateIndex( c2.Asc() );
        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldNotCreateIndex_WhenIndexIsAttachedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var sut = table.Constraints.CreateUniqueIndex( c2.Asc() );
        table.Constraints.SetPrimaryKey( sut );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( sut.Name );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlConstraintBuilder)sut).SetName( oldName );

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            table.Constraints.GetConstraint( "bar" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.GetConstraint( "bar" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `IX_T_C2A` TO `bar`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateNameAndNotRecreateOriginatingForeignKeys()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `IX_T_C2A` TO `bar`;" );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        var result = ((ISqlConstraintBuilder)sut).SetDefaultName();

        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index.SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            table.Constraints.GetConstraint( "UIX_T_C1A" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "UIX_T_C1A" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            table.Constraints.GetConstraint( "IX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `bar` TO `IX_T_C2A`;" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndIndexIsUnique()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" ).MarkAsUnique();
        var oldName = sut.Name;

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "UIX_T_C2A" );
            table.Constraints.GetConstraint( "UIX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.GetObject( "UIX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `bar` TO `UIX_T_C2A`;" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        pk.SetName( "IX_T_C2A" );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsUnique_ShouldDoNothing_WhenUniquenessFlagDoesNotChange(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( value );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique( value );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().MarkAsUnique( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsUnique();

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique( false );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldRecreateOriginatingForeignKeys_WhenValueChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.MarkAsUnique();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyIndexUniquenessChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique( false ) );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowMySqlObjectBuilderException_WhenUniquenessChangesToFalseAndIndexIsReferencedByForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique( false ) );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).MarkAsUnique() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUniqueAndNameCorrectly_WhenIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `bar` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueDoesNotChange()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() ).SetFilter( null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNonNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = sut.SetFilter( t => t["C2"] != null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            result.ReferencedFilterColumns.Should().BeSequentiallyEqualTo( column );
            column.ReferencingIndexFilters.Should().BeSequentiallyEqualTo( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC) WHERE (`C2` IS NOT NULL);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C2"] != null );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).SetFilter( null );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeNull();
            result.ReferencedFilterColumns.Should().BeEmpty();
            column.ReferencingIndexFilters.Should().BeEmpty();
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldThrowMySqlObjectBuilderException_WhenFilterIsInvalid()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of(
            () => ((ISqlIndexBuilder)sut).SetFilter( _ => SqlNode.Functions.RecordsAffected() == SqlNode.Literal( 0 ) ) );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowMySqlObjectBuilderException_WhenPrimaryKeyIndexFilterChangesToNonNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() ) );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowMySqlObjectBuilderException_WhenReferencedIndexFilterChangesToNonNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetFilter( SqlNode.True() ) );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowMySqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlIndexBuilder)sut).SetFilter( null ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldRecreateOriginatingForeignKeys_WhenValueChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetFilter( SqlNode.True() );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC) WHERE TRUE;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndIsUniqueAndNameCorrectly_WhenFilterAndIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        var result = ((ISqlIndexBuilder)sut).MarkAsUnique().SetFilter( t => t["C2"] != null ).SetName( "bar" );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `bar` ON `foo`.`T` (`C2` ASC) WHERE (`C2` IS NOT NULL);" );
        }
    }

    [Fact]
    public void PrimaryKeyAssignment_ShouldRecreateOriginatingForeignKeys()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var ix = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).SetType<int>().Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( sut, ix );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        table.Constraints.SetPrimaryKey( sut );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C3_REF_T`;",
                    "DROP INDEX `IX_T_C3A` ON `foo`.`T`;",
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C3` ASC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C3_REF_T` FOREIGN KEY (`C3`) REFERENCES `foo`.`T` (`C2`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void AssigningToPrimaryKey_ShouldDropIndexByItsOldName_WhenIndexNameAlsoChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" );
        table.Constraints.SetPrimaryKey( sut );
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `UIX_T_C2A` ON `foo`.`T`;",
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC);" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndSelfReferencingForeignKeys()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( c2.Asc() ).MarkAsUnique();
        var fk1 = table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );
        var fk2 = table.Constraints.CreateForeignKey( sut, pk.Index );

        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.SetName( "bar" ).Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            fk1.IsRemoved.Should().BeTrue();
            fk2.IsRemoved.Should().BeTrue();
            c2.ReferencingIndexes.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`,
                      DROP FOREIGN KEY `FK_T_C3_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndAssignedPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            table.Constraints.Contains( pk.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( pk.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            column.ReferencingIndexes.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndClearAssignedFilterColumns()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C"] != null );

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            sut.OriginatingForeignKeys.Should().BeEmpty();
            sut.ReferencingForeignKeys.Should().BeEmpty();
            sut.ReferencedFilterColumns.Should().BeEmpty();
            column.ReferencingIndexes.Should().BeEmpty();
            column.ReferencingIndexFilters.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        _ = schema.Database.GetPendingStatements();
        sut.Remove();
        var startStatementCount = schema.Database.GetPendingStatements().Length;

        sut.Remove();
        var statements = schema.Database.GetPendingStatements().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowMySqlObjectBuilderException_WhenIndexHasExternalReferences()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Constraints.CreateIndex( t1.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C3" ).Asc() ).Index;
        t2.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<MySqlObjectBuilderException>();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenIndexIsMySql()
    {
        var action = Substitute.For<Action<MySqlIndexBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenIndexIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlIndexBuilder>>();
        var sut = Substitute.For<ISqlIndexBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
