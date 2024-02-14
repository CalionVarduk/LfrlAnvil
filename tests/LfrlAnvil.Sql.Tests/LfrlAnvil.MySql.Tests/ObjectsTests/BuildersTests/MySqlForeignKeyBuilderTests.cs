using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlForeignKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[ForeignKey] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesAnotherTable()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );

        var t1 = schema.Objects.CreateTable( "T1" );
        var ix2 = t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() ).Index;

        var t2 = schema.Objects.CreateTable( "T2" );
        var ix1 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        t2.Constraints.CreateForeignKey( ix1, ix2 );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T2`
                      ADD CONSTRAINT `FK_T2_C2_REF_T1` FOREIGN KEY (`C2`) REFERENCES `foo`.`T1` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyIsSelfReference()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        table.Constraints.CreateForeignKey( ix1, ix2 );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetName( sut.Name );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "bar" );
        var result = ((ISqlForeignKeyBuilder)sut).SetName( oldName );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetName( "bar" );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.Get( "bar" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.Get( "bar" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `bar` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( name ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( "bar" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetName( "T" ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetName( "bar" );
        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

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
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        var oldName = sut.Name;

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            table.Constraints.Get( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            table.Constraints.Contains( oldName ).Should().BeFalse();
            schema.Objects.Get( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            schema.Objects.Contains( oldName ).Should().BeFalse();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `bar`;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowMySqlObjectBuilderException_WhenNewNameAlreadyExistsInSchema()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        ix1.SetName( "FK_T_C2_REF_T" );

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetDefaultName() );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Cascade );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE CASCADE ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Restrict );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowMySqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Cascade );

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE CASCADE;" );
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Restrict );

        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            statements.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowMySqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<MySqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        using ( new AssertionScope() )
        {
            table.Constraints.Contains( sut.Name ).Should().BeFalse();
            schema.Objects.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();
            ix1.OriginatingForeignKeys.Should().BeEmpty();
            ix2.ReferencingForeignKeys.Should().BeEmpty();

            statements.Should().HaveCount( 1 );
            statements.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        _ = schema.Database.Changes.GetPendingActions();
        sut.Remove();
        var startStatementCount = schema.Database.Changes.GetPendingActions().Length;

        sut.Remove();
        var statements = schema.Database.Changes.GetPendingActions().Slice( startStatementCount ).ToArray();

        statements.Should().BeEmpty();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenForeignKeyIsMySql()
    {
        var action = Substitute.For<Action<MySqlForeignKeyBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenForeignKeyIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlForeignKeyBuilder>>();
        var sut = Substitute.For<ISqlForeignKeyBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
