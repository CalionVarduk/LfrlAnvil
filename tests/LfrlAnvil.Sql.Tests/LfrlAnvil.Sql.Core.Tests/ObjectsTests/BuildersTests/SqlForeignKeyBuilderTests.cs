using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlForeignKeyBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[ForeignKey] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            sut.OriginIndex.Should().BeSameAs( ix2 );
            sut.ReferencedIndex.Should().BeSameAs( ix1 );

            ix1.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            ix2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) );

            table.ReferencingObjects.Should().BeEmpty();
            schema.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  CREATE [ForeignKey] foo.FK_T_C2_REF_T;" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            t2.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T2_C2_REF_T" );
            sut.OriginIndex.Should().BeSameAs( ix2 );
            sut.ReferencedIndex.Should().BeSameAs( ix1 );

            ix1.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            ix2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) );

            table.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            schema.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T2
  CREATE [ForeignKey] foo.FK_T2_C2_REF_T;" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            t2.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            s2.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T2_C2_REF_foo_T" );
            sut.OriginIndex.Should().BeSameAs( ix2 );
            sut.ReferencedIndex.Should().BeSameAs( ix1 );

            ix1.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            ix2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) );

            table.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            schema.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] bar.T2
  CREATE [ForeignKey] bar.FK_T2_C2_REF_foo_T;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "'" )]
    [InlineData( "f\'oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            table.Constraints.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );
        ix1.SetName( "FK_T_C2_REF_T" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Cascade );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([9] : 'OnDeleteBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));" );
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var result = sut.SetOnDeleteBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnDeleteBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnDeleteBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldUpdateBehavior_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Cascade );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([10] : 'OnUpdateBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));" );
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var result = sut.SetOnUpdateBehavior( ReferenceBehavior.Restrict );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetOnUpdateBehavior_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetOnUpdateBehavior( ReferenceBehavior.Cascade ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var sut = table.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            ix1.ReferencingObjects.Should().BeEmpty();
            ix2.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  REMOVE [ForeignKey] foo.FK_T_C2_REF_T;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentTableFromTheSameSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var t2 = schema.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            t2.Constraints.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            ix1.ReferencingObjects.Should().BeEmpty();
            ix2.ReferencingObjects.Should().BeEmpty();
            table.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T2
  REMOVE [ForeignKey] foo.FK_T2_C2_REF_T;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveForeignKey_WhenForeignKeyReferencesDifferentSchema()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            t2.Constraints.TryGet( sut.Name ).Should().BeNull();
            s2.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            ix1.ReferencingObjects.Should().BeEmpty();
            ix2.ReferencingObjects.Should().BeEmpty();
            table.ReferencingObjects.Should().BeEmpty();
            schema.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] bar.T2
  REMOVE [ForeignKey] bar.FK_T2_C2_REF_foo_T;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenForeignKeyIsReferenced()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        schema.Database.Changes.CompletePendingChanges();
        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjects_WhenForeignKeyReferencesTheSameTable()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();

            ix2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) );

            ix1.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix1 ) );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndReferencedIndex()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var s2 = schema.Database.Schemas.Create( "bar" );
        var t2 = s2.Objects.CreateTable( "T2" );
        var ix2 = t2.Constraints.SetPrimaryKey( t2.Columns.Create( "C2" ).Asc() ).Index;
        var sut = t2.Constraints.CreateForeignKey( ix2, ix1 );
        SqlDatabaseBuilderMock.AddReference( sut, SqlObjectBuilderReferenceSource.Create( table ) );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            t2.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            s2.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.IsRemoved.Should().BeTrue();
            sut.ReferencingObjects.Should().BeEmpty();

            ix2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), ix2 ) );

            ix1.ReferencingObjects.Should().BeEmpty();
            table.ReferencingObjects.Should().BeEmpty();
            schema.ReferencingObjects.Should().BeEmpty();

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenForeignKeyIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlForeignKeyBuilder)sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);" );
        }
    }

    [Fact]
    public void ISqlConstraintBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlConstraintBuilder)sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);" );
        }
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlObjectBuilder)sut).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.bar ([1] : 'Name' (System.String) FROM FK_T_C2_REF_T);" );
        }
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlForeignKeyBuilder)sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            table.Constraints.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void ISqlConstraintBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlConstraintBuilder)sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "FK_T_C2_REF_T" );
            table.Constraints.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "FK_T_C2_REF_T" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetOnDeleteBehavior_ShouldBeEquivalentToSetOnDeleteBehavior()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlForeignKeyBuilder)sut).SetOnDeleteBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnDeleteBehavior.Should().Be( ReferenceBehavior.Cascade );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([9] : 'OnDeleteBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));" );
        }
    }

    [Fact]
    public void ISqlForeignKeyBuilder_SetOnUpdateBehavior_ShouldBeEquivalentToSetOnUpdateBehavior()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix1 = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var ix2 = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateForeignKey( ix1, ix2 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlForeignKeyBuilder)sut).SetOnUpdateBehavior( ReferenceBehavior.Cascade );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.OnUpdateBehavior.Should().Be( ReferenceBehavior.Cascade );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [ForeignKey] foo.FK_T_C2_REF_T ([10] : 'OnUpdateBehavior' (LfrlAnvil.Sql.ReferenceBehavior) FROM 'RESTRICT' (Restrict));" );
        }
    }
}
