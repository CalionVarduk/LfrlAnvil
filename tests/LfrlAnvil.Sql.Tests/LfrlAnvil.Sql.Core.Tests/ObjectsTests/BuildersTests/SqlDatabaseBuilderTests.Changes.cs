using System.Collections.Generic;
using System.Data;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.TestExtensions.Sql.Mocks;

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
            var result = (( ISqlDatabaseChangeTracker )sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    activeObject.TestNull(),
                    actions.Select( a => a.Sql ).TestSequence( [ "CREATE [Table] common.T;" ] ) )
                .Go();
        }

        [Fact]
        public void CompletePendingChanges_ShouldMaterializePendingRemoveChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();

            var actionCount = sut.Database.GetPendingActionCount();
            table.Remove();
            var result = (( ISqlDatabaseChangeTracker )sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    activeObject.TestNull(),
                    actions.Select( a => a.Sql ).TestSequence( [ "REMOVE [Table] common.T;" ] ) )
                .Go();
        }

        [Fact]
        public void CompletePendingChanges_ShouldMaterializePendingAlterChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();

            var actionCount = sut.Database.GetPendingActionCount();
            table.SetName( "U" );
            var result = (( ISqlDatabaseChangeTracker )sut).CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    activeObject.TestNull(),
                    actions.Select( a => a.Sql )
                        .TestSequence(
                        [
                            """
                            ALTER [Table] common.U
                              ALTER [Table] common.U ([1] : 'Name' (System.String) FROM T);
                            """
                        ] ) )
                .Go();
        }

        [Fact]
        public void CompletePendingChanges_ShouldDoNothing_WhenThereAreNoPendingChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            var result = sut.CompletePendingChanges();
            var activeObject = sut.ActiveObject;
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    activeObject.TestNull(),
                    actions.TestEmpty() )
                .Go();
        }

        [Fact]
        public void GetPendingActions_ShouldMaterializePendingCreateChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var previous = sut.GetPendingActions().ToArray();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = sut.GetPendingActions().ToArray();

            Assertion.All(
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    result.Skip( previous.Length ).Select( a => a.Sql ).TestSequence( [ "CREATE [Table] common.T;" ] ) )
                .Go();
        }

        [Fact]
        public void GetPendingActions_ShouldReturnPreviousChanges_WhenThereAreNoPendingChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var expected = sut.GetPendingActions().ToArray();

            var result = sut.GetPendingActions().ToArray();

            result.TestSequence( expected ).Go();
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
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetModeAndAttach( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = (( ISqlDatabaseChangeTracker )sut).AddAction( actionCallback, setupCallback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.Length.TestEquals( 1 ),
                    actions.TestAll(
                        (a, _) => Assertion.All(
                            a.Sql.TestNull(),
                            a.OnCommandSetup.TestRefEquals( setupCallback ),
                            a.Timeout.TestEquals( timeout ) ) ) )
                .Go();
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

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.TestSequence(
                    [
                        (a, _) => a.Sql.TestEquals( "CREATE [Table] common.T;" ),
                        (a, _) => Assertion.All(
                            a.Sql.TestNull(),
                            a.OnCommandSetup.TestRefEquals( setupCallback ),
                            a.Timeout.TestEquals( timeout ) )
                    ] ) )
                .Go();
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

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddAction_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var callback = Substitute.For<Action<IDbCommand>>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetModeAndAttach( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddAction( callback );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.TestEmpty().Go();
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddStatement_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetModeAndAttach( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = (( ISqlDatabaseChangeTracker )sut).AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.Length.TestEquals( 1 ),
                    actions.TestAll(
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.Timeout.TestEquals( timeout ) ) ) )
                .Go();
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

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.TestSequence(
                    [
                        (a, _) => a.Sql.TestEquals( "CREATE [Table] common.T;" ),
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.Timeout.TestEquals( timeout ) )
                    ] ) )
                .Go();
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

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddStatement_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetModeAndAttach( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddStatement( statement );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddStatement_ShouldThrowSqlObjectBuilderException_WhenStatementContainsParameters()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var action = Lambda.Of( () => sut.AddStatement( SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter( "a" ) ) ) );

            action.Test(
                    exc => exc.TestType()
                        .Exact<SqlObjectBuilderException>(
                            e => Assertion.All( e.Dialect.TestEquals( SqlDialectMock.Instance ), e.Errors.Count.TestEquals( 1 ) ) ) )
                .Go();
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetModeAndAttach( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = (( ISqlDatabaseChangeTracker )sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { SqlParameter.Named( "a", 1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.Length.TestEquals( 1 ),
                    actions.TestAll(
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.OnCommandSetup.TestNotNull(),
                            a.Timeout.TestEquals( timeout ) ) ) )
                .Go();
        }

        [Fact]
        public void AddParameterizedStatement_TypeErased_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes.SetActionTimeout( timeout );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = (( ISqlDatabaseChangeTracker )sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { SqlParameter.Named( "a", 1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.TestSequence(
                    [
                        (a, _) => a.Sql.TestEquals( "CREATE [Table] common.T;" ),
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.OnCommandSetup.TestNotNull(),
                            a.Timeout.TestEquals( timeout ) )
                    ] ) )
                .Go();
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
                new[] { KeyValuePair.Create( "a", ( object? )1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddParameterizedStatement_TypeErased_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetModeAndAttach( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter( "a" ) ),
                new[] { KeyValuePair.Create( "a", ( object? )1 ) }.AsEnumerable() );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.TestEmpty().Go();
        }

        [Theory]
        [InlineData( SqlDatabaseCreateMode.Commit )]
        [InlineData( SqlDatabaseCreateMode.DryRun )]
        public void AddParameterizedStatement_Generic_ShouldAddNewStatement(SqlDatabaseCreateMode mode)
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetModeAndAttach( mode );

            var actionCount = sut.Database.GetPendingActionCount();
            var result = (( ISqlDatabaseChangeTracker )sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ),
                new Source { A = 1 } );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.Length.TestEquals( 1 ),
                    actions.TestAll(
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.OnCommandSetup.TestNotNull(),
                            a.Timeout.TestEquals( timeout ) ) ) )
                .Go();
        }

        [Fact]
        public void AddParameterizedStatement_Generic_ShouldAddNewStatement_WhenThereArePendingChanges()
        {
            var timeout = Fixture.Create<TimeSpan>();
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetActionTimeout( timeout ).SetModeAndAttach( SqlDatabaseCreateMode.Commit );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = (( ISqlDatabaseChangeTracker )sut).AddParameterizedStatement(
                SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ),
                new Source { A = 1 } );

            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    actions.TestSequence(
                    [
                        (a, _) => a.Sql.TestEquals( "CREATE [Table] common.T;" ),
                        (a, _) => Assertion.All(
                            a.Sql.TestEquals( $"{statement}{Environment.NewLine}" ),
                            a.OnCommandSetup.TestNotNull(),
                            a.Timeout.TestEquals( timeout ) )
                    ] ) )
                .Go();
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

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddParameterizedStatement_Generic_ShouldDoNothing_WhenBuilderIsInNoChangesMode()
        {
            var statement = Fixture.Create<string>();
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.CompletePendingChanges().SetModeAndAttach( SqlDatabaseCreateMode.NoChanges );

            var actionCount = sut.Database.GetPendingActionCount();
            sut.AddParameterizedStatement( SqlNode.RawStatement( statement, SqlNode.Parameter<int>( "a" ) ), new Source { A = 1 } );
            var actions = sut.Database.GetLastPendingActions( actionCount );

            actions.TestEmpty().Go();
        }

        [Fact]
        public void AddParameterizedStatement_ShouldThrowSqlCompilerException_WhenParametersAreInvalid()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var action = Lambda.Of(
                () => sut.AddParameterizedStatement(
                    SqlNode.RawStatement( Fixture.Create<string>(), SqlNode.Parameter<string>( "a" ) ),
                    new Source { A = 1 } ) );

            action.Test( exc => exc.TestType().Exact<SqlCompilerException>() ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForCreatedActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Unchanged ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForRemovedActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            target.Remove();
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Unchanged ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForAlteredActiveObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            target.SetName( "U" );
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Unchanged ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnCreated_ForCreatedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Created ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnRemoved_ForRemovedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

            sut.CompletePendingChanges();
            target.Remove();
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Removed ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForAlteredObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

            sut.CompletePendingChanges();
            target.SetName( "U" );
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Unchanged ).Go();
        }

        [Fact]
        public void GetExistenceState_ShouldReturnUnchanged_ForRemovedCreatedObject()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            sut.CompletePendingChanges();
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            target.Remove();
            var result = (( ISqlDatabaseChangeTracker )sut).GetExistenceState( target );

            result.TestEquals( SqlObjectExistenceState.Unchanged ).Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnFalse_ForActiveObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            var result = (( ISqlDatabaseChangeTracker )sut).ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved );

            result.TestFalse().Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForActiveObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            target.SetName( "U" );

            var result = (( ISqlDatabaseChangeTracker )sut).ContainsChange( target, SqlObjectChangeDescriptor.Name );

            result.TestTrue().Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );

            var result = (( ISqlDatabaseChangeTracker )sut).ContainsChange( target, SqlObjectChangeDescriptor.IsRemoved );

            result.TestTrue().Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnTrue_ForObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = (( ISqlDatabaseChangeTracker )sut).ContainsChange( target, SqlObjectChangeDescriptor.IsNullable );

            result.TestTrue().Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnFalse_ForObjectNonExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = (( ISqlDatabaseChangeTracker )sut).ContainsChange( target, SqlObjectChangeDescriptor.Name );

            result.TestFalse().Go();
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnEmpty_ForActiveObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsRemoved );

            result.TestEquals( SqlObjectOriginalValue<bool>.CreateEmpty() ).Go();
        }

        [Fact]
        public void ContainsChange_ShouldReturnCorrectResult_ForActiveObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            target.SetName( "U" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.Name );

            result.TestEquals( SqlObjectOriginalValue<string>.Create( "T" ) ).Go();
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnCorrectResult_ForObjectIsRemovedExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var table = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            sut.CompletePendingChanges();
            var target = table.Columns.Create( "C" );

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsRemoved );

            result.TestEquals( SqlObjectOriginalValue<bool>.Create( true ) ).Go();
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnCorrectResult_ForObjectExistingChangeOtherThanIsRemoved()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.IsNullable );

            result.TestEquals( SqlObjectOriginalValue<bool>.Create( false ) ).Go();
        }

        [Fact]
        public void GetOriginalValue_ShouldReturnEmpty_ForObjectNonExistingChange()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );
            sut.CompletePendingChanges();
            target.MarkAsNullable();

            var result = sut.GetOriginalValue( target, SqlObjectChangeDescriptor.Name );

            result.TestEquals( SqlObjectOriginalValue<string>.CreateEmpty() ).Go();
        }

        [Fact]
        public void RemovalOfCreatedActiveObject_ShouldResetActiveObjectWithoutAnyChanges()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var actionCount = sut.Database.GetPendingActionCount();
            var target = sut.Database.Schemas.Default.Objects.CreateTable( "T" );
            target.Remove();
            var actions = sut.Database.GetLastPendingActions( actionCount );

            Assertion.All(
                    actions.TestEmpty(),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ) )
                .Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void Attach_ShouldUpdateIsAttached(bool enabled)
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;
            sut.Attach( ! enabled );

            var result = (( ISqlDatabaseChangeTracker )sut).Attach( enabled );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.IsAttached.TestEquals( enabled ) )
                .Go();
        }

        [Fact]
        public void Attach_ShouldDoNothing_WhenBuilderIsAlreadyAttached()
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = (( ISqlDatabaseChangeTracker )sut).Attach();

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.IsAttached.TestTrue() )
                .Go();
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

            Assertion.All(
                    result.TestRefEquals( sut ),
                    sut.ActiveObject.TestNull(),
                    sut.ActiveObjectExistenceState.TestEquals( default ),
                    activeObject.TestNull(),
                    actions.Select( a => a.Sql ).TestSequence( [ "CREATE [Table] common.T;" ] ) )
                .Go();
        }

        [Theory]
        [InlineData( true )]
        [InlineData( false )]
        public void Detach_ShouldInvokeAttachWithNegatedValue(bool enabled)
        {
            var sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = sut.Detach( enabled );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    result.IsAttached.TestEquals( ! enabled ) )
                .Go();
        }

        [Theory]
        [InlineData( null )]
        [InlineData( 100L )]
        public void SetActionTimeout_ShouldUpdateActionTimeout(long? seconds)
        {
            var timeout = seconds is null ? ( TimeSpan? )null : TimeSpan.FromSeconds( seconds.Value );
            ISqlDatabaseChangeTracker sut = SqlDatabaseBuilderMock.Create().Changes;

            var result = sut.SetActionTimeout( timeout );

            Assertion.All(
                    result.TestRefEquals( sut ),
                    result.ActionTimeout.TestEquals( timeout ) )
                .Go();
        }

        public sealed class Source
        {
            public int A { get; init; }
        }
    }
}
