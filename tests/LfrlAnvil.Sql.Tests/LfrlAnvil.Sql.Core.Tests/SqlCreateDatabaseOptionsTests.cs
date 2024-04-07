using LfrlAnvil.Sql.Events;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests;

public class SqlCreateDatabaseOptionsTests : TestsBase
{
    [Fact]
    public void Default_ShouldReturnCorrectResult()
    {
        var sut = SqlCreateDatabaseOptions.Default;

        using ( new AssertionScope() )
        {
            sut.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            sut.VersionHistoryName.Should().BeNull();
            sut.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            sut.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            sut.CommandTimeout.Should().BeNull();
            sut.GetStatementListeners().ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void SetMode_ShouldUpdateModeCorrectly(SqlDatabaseCreateMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( mode );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Fact]
    public void SetVersionHistoryName_ShouldUpdateNameCorrectly()
    {
        var name = SqlSchemaObjectName.Create( "foo", "bar" );
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryName( name );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().Be( name );
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Fact]
    public void SetVersionHistoryName_ShouldResetNameCorrectly()
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryName( null );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void SetVersionHistoryPersistenceMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryPersistenceMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( mode );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void SetVersionHistoryQueryMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryQueryMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( mode );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Theory]
    [InlineData( null )]
    [InlineData( 100L )]
    public void SetCommandTimeout_ShouldUpdateTimeoutCorrectly(long? seconds)
    {
        var value = seconds is null ? ( TimeSpan? )null : TimeSpan.FromSeconds( seconds.Value );
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetCommandTimeout( value );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().Be( value );
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Fact]
    public void AddStatementListener_ShouldAddStatementListener_WhenOriginalOptionsDoNotHaveAnyListeners()
    {
        var listener = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var sut = SqlCreateDatabaseOptions.Default;

        var result = sut.AddStatementListener( listener );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( listener );
        }
    }

    [Fact]
    public void AddStatementListener_ShouldAddStatementListener_WhenOriginalOptionsHaveListeners()
    {
        var listener1 = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var listener2 = Substitute.For<ISqlDatabaseFactoryStatementListener>();
        var sut = SqlCreateDatabaseOptions.Default.AddStatementListener( listener1 );

        var result = sut.AddStatementListener( listener2 );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.VersionHistoryQueryMode.Should().Be( SqlDatabaseVersionHistoryMode.AllRecords );
            result.CommandTimeout.Should().BeNull();
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( listener1, listener2 );
            sut.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( listener1 );
        }
    }
}
