using System.Collections.Generic;
using System.Data;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ObjectsTests.BuildersTests;

public partial class SqlDatabaseBuilderTests
{
    public class Changes : TestsBase
    {
        [Fact]
        public void CompletePendingChanges_ShouldMaterializePendingCreateChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = ((ISqlDatabaseChangeTracker)sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                activeObject.Should().BeNull();
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
            }
        }

        [Fact]
        public void CompletePendingChanges_ShouldMaterializePendingRemoveChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();

            var actionCount = sut.Database.GetPendingActionCount();
            table.Remove();
            var result = ((ISqlDatabaseChangeTracker)sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                activeObject.Should().BeNull();
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "REMOVE [Table] common.T;" );
            }
        }

        [Fact]
        public void CompletePendingChanges_ShouldMaterializePendingAlterChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();

            var actionCount = sut.Database.GetPendingActionCount();
            table.SetName( "U" );
            var result = ((ISqlDatabaseChangeTracker)sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                activeObject.Should().BeNull();
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 )
                    .Sql.Should()
                    .Be(
                        @"ALTER [Table] common.U
  ALTER [Table] common.U ([1] : 'Name' (System.String) FROM T);" );
            }
        }

        [Fact]
        public void CompletePendingChanges_ShouldDoNothing_WhenThereAreNoPendingChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            var result = sut.CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                activeObject.Should().BeNull();
                actions.Should().BeEmpty();
            }
        }

        [Fact]
        public void GetPendingActions_ShouldMaterializePendingCreateChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var previous = sut.GetPendingActions().ToArray();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = sut.GetPendingActions().ToArray();

            using ( new AssertionScope() )
            {
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                result.Should().HaveCount( previous.Length + 1 );
                result.ElementAtOrDefault( result.Length - 1 ).Sql.Should().Be( "CREATE [Table] common.T;" );
            }
        }

        [Fact]
        public void GetPendingActions_ShouldReturnPreviousChanges_WhenThereAreNoPendingChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var expected = sut.GetPendingActions().ToArray();

            var result = sut.GetPendingActions().ToArray();

            result.Should().BeSequentiallyEqualTo( expected );
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddAction_ShouldAddNewAction(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var actionCallback = Substitute.For<Action<IDbCommand>>();
            var setupCallback = Substitute.For<Action<IDbCommand>>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetMode( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = ((ISqlDatabaseChangeTracker)sut).AddAction( actionCallback, setupCallback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().BeNull();
                actions.ElementAtOrDefault( 0 ).OnCommandSetup.Should().BeSameAs( setupCallback );
                actions.ElementAtOrDefault( 0 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddAction_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var actionCallback = Substitute.For<Action<IDbCommand>>();
            var setupCallback = Substitute.For<Action<IDbCommand>>();
            var sut = SqlDatabaseBuilderMock.Create().Changes.SetActionTimeout( timeout );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = sut.AddAction( actionCallback, setupCallback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 2 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
                actions.ElementAtOrDefault( 1 ).Sql.Should().BeNull();
                actions.ElementAtOrDefault( 1 ).OnCommandSetup.Should().BeSameAs( setupCallback );
                actions.ElementAtOrDefault( 1 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddAction_ShouldDoNothing_WhenBuilderIsDetached()
        {
            var callback = Substitute.For<Action<IDbCommand>>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( false );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddAction( callback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddAction_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var callback = Substitute.For<Action<IDbCommand>>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetMode( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddAction( callback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddStatement_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetMode( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = ((ISqlDatabaseChangeTracker)sut).AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 0 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddStatement_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes.SetActionTimeout( timeout );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = sut.AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 2 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
                actions.ElementAtOrDefault( 1 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 1 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddStatement_ShouldDoNothing_WhenBuilderIsDetached()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( false );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddStatement_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetMode( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddStatement_ShouldThrowSqlObjectBuilderException_WhenStatementContainsParameters()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var action = Lambda.Of(
                () => sut.AddStatement( SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter( "a" ) ) ) );

            action.Should()
                .ThrowExactly<SqlObjectBuilderException>()
                .AndMatch( e => e.Dialect == SqlDialectMock.Instance && e.Errors.Count == 1 );
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetMode( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = ((ISqlDatabaseChangeTracker)sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 0 ).OnCommandSetup.Should().NotBeNull();
                actions.ElementAtOrDefault( 0 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes.SetActionTimeout( timeout );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = ((ISqlDatabaseChangeTracker)sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 2 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
                actions.ElementAtOrDefault( 1 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 1 ).OnCommandSetup.Should().NotBeNull();
                actions.ElementAtOrDefault( 1 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddParameterizedStatement_TypeErased_ShouldDoNothing_WhenBuilderIsDetached()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( false );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddParameterizedStatement_TypeErased_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetMode( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { KeyValuePair.Create( "a", (object?)1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddParameterizedStatement_Generic_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetMode( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = ((ISqlDatabaseChangeTracker)sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ),
                new Source { A = 1 } );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 0 ).OnCommandSetup.Should().NotBeNull();
                actions.ElementAtOrDefault( 0 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddParameterizedStatement_Generic_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetMode( SqlDatabaseCreateMode.Commit );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = ((ISqlDatabaseChangeTracker)sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ),
                new Source { A = 1 } );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                actions.Should().HaveCount( 2 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
                actions.ElementAtOrDefault( 1 ).Sql.Should().Be( $"{statement}{Environment.NewLine}" );
                actions.ElementAtOrDefault( 1 ).OnCommandSetup.Should().NotBeNull();
                actions.ElementAtOrDefault( 1 ).Timeout.Should().Be( timeout );
            }
        }

        [Fact]
        public void AddParameterizedStatement_Generic_ShouldDoNothing_WhenBuilderIsDetached()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( false );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddParameterizedStatement_Generic_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetMode( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.Should().BeEmpty();
        }

        [Fact]
        public void AddParameterizedStatement_ShouldThrowSqlCompilerException_WhenParametersAreInvalid()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var action = Lambda.Of(
                () => sut.AddParameterizedStatement(
                    SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter<string>( "a" ) ),
                    new Source { A = 1 } ) );

            action.Should().ThrowExactly<SqlCompilerException>();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForCreatedActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Unchanged );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForRemovedActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            target.Remove();
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Unchanged );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForAlteredActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            target.SetName( "U" );
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Unchanged );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnCreated_ForCreatedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Created );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnRemoved_ForRemovedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

            sut.CompletePendingChanges();
            target.Remove();
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Removed );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForAlteredObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

            sut.CompletePendingChanges();
            target.SetName( "U" );
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Unchanged );
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForRemovedCreatedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            sut.CompletePendingChanges();
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            target.Remove();
            var result = ((ISqlDatabaseChangeTracker)sut).GetExistenceState( target );

            result.Should().Be( SqlObjectExistenceState.Unchanged );
        }

        [Fact]
        public void ContainsChange_ShouldReturnFalse_ForActiveObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            var result = ((ISqlDatabaseChangeTracker)sut).ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved );

            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForActiveObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            target.SetName( "U" );

            var result = ((ISqlDatabaseChangeTracker)sut).ContainsChange( target, SqlObjectChangeDescriptor.Name );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );

            var result = ((ISqlDatabaseChangeTracker)sut).ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = ((ISqlDatabaseChangeTracker)sut).ContainsChange( target, SqlObjectChangeDescriptor.IsNullable );

            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsChange_ShouldReturnFalse_ForObjectNonExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = ((ISqlDatabaseChangeTracker)sut).ContainsChange( target, SqlObjectChangeDescriptor.Name );

            result.Should().BeFalse();
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnEmpty_ForActiveObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsRemoved );

            result.Should().Be( SqlObjectOriginalValue<bool>.CreateEmpty() );
        }

        [Fact]
        public void ContainsChange_ShouldReturnCorrectResult_ForActiveObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            target.SetName( "U" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.Name );

            result.Should().Be( SqlObjectOriginalValue<string>.Create( "T" ) );
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnCorrectResult_ForObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsRemoved );

            result.Should().Be( SqlObjectOriginalValue<bool>.Create( true ) );
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnCorrectResult_ForObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsNullable );

            result.Should().Be( SqlObjectOriginalValue<bool>.Create( false ) );
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnEmpty_ForObjectNonExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.Name );

            result.Should().Be( SqlObjectOriginalValue<string>.CreateEmpty() );
        }

        [Fact]
        public void RemovalOfCreatedActiveObject_ShouldResetActiveObjectWithoutAnyChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            target.Remove();
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                actions.Should().BeEmpty();
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
            }
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void Attach_ShouldUpdateIsAttached(bool enabled)
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( ! enabled );

            var result = ((ISqlDatabaseChangeTracker)sut).Attach( enabled );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.IsAttached.Should().Be( enabled );
            }
        }

        [Fact]
        public void Attach_ShouldDoNothing_WhenBuilderIsAlreadyAttached()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = ((ISqlDatabaseChangeTracker)sut).Attach();

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.IsAttached.Should().BeTrue();
            }
        }

        [Fact]
        public void Attach_WithFalse_ShouldCompletePendingChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = sut.Attach( false );
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                sut.ActiveObject.Should().BeNull();
                sut.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
                activeObject.Should().BeNull();
                actions.Should().HaveCount( 1 );
                actions.ElementAtOrDefault( 0 ).Sql.Should().Be( "CREATE [Table] common.T;" );
            }
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void Detach_ShouldInvokeAttachWithNegatedValue(bool enabled)
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = sut.Detach( enabled );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                result.IsAttached.Should().Be( ! enabled );
            }
        }

        [Theory]
        [InlineData( null )]
        [InlineData( 100L )]
        public void SetActionTimeout_ShouldUpdateActionTimeout(long? seconds)
        {
            var timeout = seconds is null ? (TimeSpan?)null : TimeSpan.FromSeconds( seconds.Value );
            ISqlDatabaseChangeTracker sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = sut.SetActionTimeout( timeout );

            using ( new AssertionScope() )
            {
                result.Should().BeSameAs( sut );
                result.ActionTimeout.Should().Be( timeout );
            }
        }

        public sealed class Source
        {
            public int A { get; init; }
        }
    }
}
