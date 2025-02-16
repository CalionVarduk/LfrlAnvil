using LfrlAnvil.Sql.Events;

namespace LfrlAnvil.Sql.Tests;

public class SqlCreateDatabaseOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = SqlCreateDatabaseOptions.Default;

        Assertion.All(
                sut.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                sut.VersionHistoryName.TestNull(),
                sut.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                sut.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                sut.CommandTimeout.TestNull(),
                sut.GetStatementListeners().ToArray().TestEmpty() )
            .Go();
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void SetMode_ShouldUpdateModeCorrectly(SqlDatabaseCreateMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetMode( mode );

        Assertion.All(
                result.Mode.TestEquals( mode ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Fact]
    public void SetVersionHistoryName_ShouldUpdateNameCorrectly()
    {
        var name = SqlSchemaObjectName.Create( "foo", "bar" );
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryName( name );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestEquals( name ),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Fact]
    public void SetVersionHistoryName_ShouldResetNameCorrectly()
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryName( null );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void SetVersionHistoryPersistenceMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryPersistenceMode( mode );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( mode ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void SetVersionHistoryQueryMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryQueryMode( mode );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( mode ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( 100L )]
    public void SetCommandTimeout_ShouldUpdateTimeoutCorrectly(long? seconds)
    {
        var value = seconds is null ? ( TimeSpan? )null : TimeSpan.FromSeconds( seconds.Value );
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetCommandTimeout( value );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestEquals( value ),
                result.GetStatementListeners().TestSequence( sut.GetStatementListeners().ToArray() ) )
            .Go();
    }

    [Fact]
    public void AddStatementListener_ShouldAddStatementListener_WhenOriginalOptionsDoNotHaveAnyListeners()
    {
        var listener = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var sut = SqlCreateDatabaseOptions.Default;

        var result = sut.AddStatementListener( listener );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().ToArray().TestSequence( [ listener ] ) )
            .Go();
    }

    [Fact]
    public void AddStatementListener_ShouldAddStatementListener_WhenOriginalOptionsHaveListeners()
    {
        var listener1 = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var listener2 = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var sut = SqlCreateDatabaseOptions.Default.AddStatementListener( listener1 );

        var result = sut.AddStatementListener( listener2 );

        Assertion.All(
                result.Mode.TestEquals( SqlDatabaseCreateMode.NoChanges ),
                result.VersionHistoryName.TestNull(),
                result.VersionHistoryPersistenceMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.VersionHistoryQueryMode.TestEquals( SqlDatabaseVersionHistoryMode.AllRecords ),
                result.CommandTimeout.TestNull(),
                result.GetStatementListeners().ToArray().TestSequence( [ listener1, listener2 ] ),
                sut.GetStatementListeners().ToArray().TestSequence( [ listener1 ] ) )
            .Go();
    }
}
