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
            sut.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
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
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
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
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
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
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( sut.GetStatementListeners().ToArray() );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryPersistenceMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryPersistenceMode.LastRecordOnly )]
    public void SetVersionHistoryPersistenceMode_ShouldUpdateModeCorrectly(SqlDatabaseVersionHistoryPersistenceMode mode)
    {
        var sut = SqlCreateDatabaseOptions.Default;
        var result = sut.SetVersionHistoryPersistenceMode( mode );

        using ( new AssertionScope() )
        {
            result.Mode.Should().Be( SqlDatabaseCreateMode.NoChanges );
            result.VersionHistoryName.Should().BeNull();
            result.VersionHistoryPersistenceMode.Should().Be( mode );
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
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
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
            result.VersionHistoryPersistenceMode.Should().Be( SqlDatabaseVersionHistoryPersistenceMode.AllRecords );
            result.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( listener1, listener2 );
            sut.GetStatementListeners().ToArray().Should().BeSequentiallyEqualTo( listener1 );
        }
    }
}
