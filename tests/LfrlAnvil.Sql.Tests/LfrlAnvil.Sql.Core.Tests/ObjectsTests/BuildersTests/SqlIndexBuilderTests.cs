using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public class SqlIndexBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Index] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        SqlColumnBuilder c2 = table.Columns.Create( "C2" );
        var ixc2 = c2.Asc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( ixc2 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "IX_T_C2A" );
            sut.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc2 );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );

            c2.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), c2 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  CREATE [Index] foo.IX_T_C2A;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( c2.Asc() );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
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
  ALTER [Index] foo.bar ([1] : 'Name' (System.String) FROM IX_T_C2A);" );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

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
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            table.Constraints.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndIndexIsUnique()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "UIX_T_C2A" );
            table.Constraints.TryGet( "UIX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "UIX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.UIX_T_C2A ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
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
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        pk.SetName( "IX_T_C2A" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsUnique_ShouldDoNothing_WhenUniquenessFlagDoesNotChange(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsUnique();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([6] : 'IsUnique' (System.Boolean) FROM False);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToFalse()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([6] : 'IsUnique' (System.Boolean) FROM True);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexUniquenessChangesToFalse()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenVirtualIndexUniquenessChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexWithExpressionsUniquenessChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( (table.Columns.Create( "C2" ).Node + SqlNode.Literal( 1 )).Asc() );

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenUniquenessChangesToFalseAndIndexIsReferencedByForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsVirtual_ShouldDoNothing_WhenVirtualityFlagDoesNotChange(bool value)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsVirtual();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([7] : 'IsVirtual' (System.Boolean) FROM False);" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToFalse()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([7] : 'IsVirtual' (System.Boolean) FROM True);" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexVirtualityChangesToFalse()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsVirtual( false ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPartialIndexVirtualityChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetFilter( SqlNode.True() );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenUniqueIndexVirtualityChangesToTrue()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsUnique();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenVirtualityChangesToTrueAndIndexIsReferencedByForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 2 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueDoesNotChange()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( SqlNode.True() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetFilter( SqlNode.True() );
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNonNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var column = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( t => t["C2"] != null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            result.ReferencedFilterColumns.Should().BeSequentiallyEqualTo( column );

            column.ReferencingObjects.Should().HaveCount( 2 );
            column.ReferencingObjects.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Filter" ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([8] : 'Filter' (LfrlAnvil.Sql.Expressions.Logical.SqlConditionNode) FROM <null>);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var column = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeNull();
            result.ReferencedFilterColumns.Should().BeEmpty();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([8] : 'Filter' (LfrlAnvil.Sql.Expressions.Logical.SqlConditionNode) FROM ([foo].[T].[C2] : System.Object) <> (NULL));" );
        }
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenFilterIsInvalid()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetFilter( _ => SqlNode.WindowFunctions.RowNumber() == SqlNode.Literal( 0 ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexFilterChangesToNonNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 2 );
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexFilterChangesToNonNull()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsVirtual()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetFilter( null ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndClearReferencedColumnsAndReferencedFilterColumns()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( c2.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            sut.ReferencedFilterColumns.Should().BeEmpty();
            sut.Filter.Should().BeNull();
            c2.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  REMOVE [Index] foo.IX_T_C2A;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndAssignedPrimaryKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            table.Constraints.TryGet( pk.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            sut.PrimaryKey.Should().BeNull();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            column.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  REMOVE [Index] foo.UIX_T_CA
  REMOVE [PrimaryKey] foo.PK_T;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasOriginatingForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, ix );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasReferencingForeignKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void QuickRemove_ShouldClearReferencingObjectsAndReferencedColumnsAndReferencedFilterColumns()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var column = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C2"] != null );
        table.Constraints.CreateForeignKey( sut, ix );

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.IsRemoved.Should().BeTrue();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            sut.ReferencedFilterColumns.Should().BeEmpty();
            sut.Filter.Should().BeNull();
            sut.ReferencingObjects.Should().BeEmpty();

            column.ReferencingObjects.Should().HaveCount( 2 );
            column.ReferencingObjects.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Filter" ), column ) );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldClearPrimaryKey()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.IsRemoved.Should().BeTrue();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            sut.PrimaryKey.Should().BeNull();
            sut.ReferencingObjects.Should().BeEmpty();
            pk.IsRemoved.Should().BeFalse();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void QuickRemove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        SqlDatabaseBuilderMock.QuickRemove( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Theory]
    [InlineData( true, true )]
    [InlineData( true, false )]
    [InlineData( false, true )]
    [InlineData( false, false )]
    public void ToCreateNode_ShouldReturnCorrectNode(bool isUnique, bool replaceIfExists)
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var filter = SqlNode.True();
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( isUnique ).SetFilter( filter );

        var result = sut.ToCreateNode( replaceIfExists );

        using ( new AssertionScope() )
        {
            result.Table.Should().BeSameAs( table.Node );
            result.Columns.Should().BeSequentiallyEqualTo( sut.Columns.Expressions );
            result.Name.Should().Be( SqlSchemaObjectName.Create( "foo", "IX_T_C2A" ) );
            result.Filter.Should().BeSameAs( filter );
            result.IsUnique.Should().Be( isUnique );
            result.ReplaceIfExists.Should().Be( replaceIfExists );
        }
    }

    [Fact]
    public void ISqlIndexBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlIndexBuilder)sut).SetName( "bar" );
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
  ALTER [Index] foo.bar ([1] : 'Name' (System.String) FROM IX_T_C2A);" );
        }
    }

    [Fact]
    public void ISqlConstraintBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
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
  ALTER [Index] foo.bar ([1] : 'Name' (System.String) FROM IX_T_C2A);" );
        }
    }

    [Fact]
    public void ISqlObjectBuilder_SetName_ShouldBeEquivalentToSetName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
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
  ALTER [Index] foo.bar ([1] : 'Name' (System.String) FROM IX_T_C2A);" );
        }
    }

    [Fact]
    public void ISqlIndexBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlIndexBuilder)sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            table.Constraints.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void ISqlConstraintBuilder_SetDefaultName_ShouldBeEquivalentToSetDefaultName()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlConstraintBuilder)sut).SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            table.Constraints.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([1] : 'Name' (System.String) FROM bar);" );
        }
    }

    [Fact]
    public void ISqlIndexBuilder_MarkAsUnique_ShouldBeEquivalentToMarkAsUnique()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlIndexBuilder)sut).MarkAsUnique();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([6] : 'IsUnique' (System.Boolean) FROM False);" );
        }
    }

    [Fact]
    public void ISqlIndexBuilder_MarkAsVirtual_ShouldBeEquivalentToMarkAsVirtual()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlIndexBuilder)sut).MarkAsVirtual();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([7] : 'IsVirtual' (System.Boolean) FROM False);" );
        }
    }

    [Fact]
    public void ISqlIndexBuilder_SetFilter_ShouldBeEquivalentToSetFilter()
    {
        var schema = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var column = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = ((ISqlIndexBuilder)sut).SetFilter( t => t["C2"] != null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            result.ReferencedFilterColumns.Should().BeSequentiallyEqualTo( column );

            column.ReferencingObjects.Should().HaveCount( 2 );
            column.ReferencingObjects.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Filter" ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"ALTER [Table] foo.T
  ALTER [Index] foo.IX_T_C2A ([8] : 'Filter' (LfrlAnvil.Sql.Expressions.Logical.SqlConditionNode) FROM <null>);" );
        }
    }
}
