using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDatabaseFactoryTests : TestsBase
{
    [Fact]
    public void Create_WithoutVersions_ShouldCreateCorrectDatabase()
    {
        ISqlDatabaseFactory sut = new SqliteDatabaseFactory();

        var result = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() );
        var versions = result.Database.GetRegisteredVersions();

        using ( new AssertionScope() )
        {
            var db = ReinterpretCast.To<SqliteDatabase>( result.Database );
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Should().BeEmpty();
            result.Exception.Should().BeNull();
            result.CommittedVersions.Length.Should().Be( 0 );
            result.PendingVersions.Length.Should().Be( 0 );
            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.Database.Version.Should().Be( result.NewVersion );
            result.Database.ServerVersion.Should().Be( db.Connect().ServerVersion );
            versions.Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_WithChangedVersionHistoryTarget_ShouldCreateCorrectDatabase()
    {
        var versionApplied = false;

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                _ => { versionApplied = true; } ) );

        var result = sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.NoChanges )
                .SetVersionHistorySchemaName( "vs" )
                .SetVersionHistoryTableName( "history" ) );

        var versions = result.Database.GetRegisteredVersions();

        using ( new AssertionScope() )
        {
            versionApplied.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.CommittedVersions.Length.Should().Be( 0 );
            result.PendingVersions.ToArray().Select( v => v.Value ).Should().BeSequentiallyEqualTo( Version.Parse( "0.1" ) );
            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            versions.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Create_ShouldCorrectlyResetBuilderStateBetweenVersions(SqlDatabaseCreateMode mode)
    {
        bool? isAttached = null;
        string[]? pendingStatements = null;

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                b =>
                {
                    var t = b.Schemas.Default.Objects.CreateTable( "T" );
                    t.SetPrimaryKey( t.Columns.Create( "C" ).Asc() );
                    b.SetDetachedMode();
                } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.2" ),
                b =>
                {
                    isAttached = b.IsAttached;
                    pendingStatements = b.GetPendingStatements().ToArray();
                } ) );

        sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        using ( new AssertionScope() )
        {
            isAttached.Should().BeTrue();
            pendingStatements.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Create_ShouldRethrowException_WhenVersionThrows(SqlDatabaseCreateMode mode)
    {
        var exception = new Exception();

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                _ => throw exception ) );

        var action = Lambda.Of( () => sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) ) );

        action.Should().Throw<Exception>().And.Should().BeSameAs( exception );
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Create_ShouldRethrowException_WhenValidationOutsideOfVersionThrows(SqlDatabaseCreateMode mode)
    {
        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                b => { b.Schemas.Default.Objects.CreateTable( "T" ); } ) );

        var action = Lambda.Of( () => sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) ) );

        action.Should().ThrowExactly<SqliteObjectBuilderException>();
    }

    [Fact]
    public void Create_ShouldStoreAllAppliedVersions_WhenPersistenceModeIsAllRecords()
    {
        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                "1st version",
                _ => { } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.2" ),
                "2nd version",
                _ => { } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.3" ),
                "3rd version",
                _ => { } ) );

        var start = DateTime.UtcNow;

        var result = sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryPersistenceMode.AllRecords ) );

        var end = DateTime.UtcNow;
        var versions = result.Database.GetRegisteredVersions();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            result.CommittedVersions.ToArray()
                .Select( v => v.Value )
                .Should()
                .BeSequentiallyEqualTo( Version.Parse( "0.1" ), Version.Parse( "0.2" ), Version.Parse( "0.3" ) );

            result.PendingVersions.Length.Should().Be( 0 );
            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( Version.Parse( "0.3" ) );

            versions.Should().HaveCount( 3 );

            var firstVersion = versions.ElementAtOrDefault( 0 );
            (firstVersion?.Ordinal).Should().Be( 1 );
            (firstVersion?.Version).Should().Be( Version.Parse( "0.1" ) );
            (firstVersion?.Description).Should().Be( "1st version" );
            (firstVersion?.CommitDateUtc).Should().BeOnOrAfter( start ).And.BeOnOrBefore( end );
            (firstVersion?.CommitDuration).Should().BeGreaterThan( TimeSpan.Zero );

            var secondVersion = versions.ElementAtOrDefault( 1 );
            (secondVersion?.Ordinal).Should().Be( 2 );
            (secondVersion?.Version).Should().Be( Version.Parse( "0.2" ) );
            (secondVersion?.Description).Should().Be( "2nd version" );
            (secondVersion?.CommitDateUtc).Should().BeOnOrAfter( firstVersion?.CommitDateUtc ?? DateTime.Now ).And.BeOnOrBefore( end );
            (secondVersion?.CommitDuration).Should().BeGreaterThan( TimeSpan.Zero );

            var thirdVersion = versions.ElementAtOrDefault( 2 );
            (thirdVersion?.Ordinal).Should().Be( 3 );
            (thirdVersion?.Version).Should().Be( Version.Parse( "0.3" ) );
            (thirdVersion?.Description).Should().Be( "3rd version" );
            (thirdVersion?.CommitDateUtc).Should().BeOnOrAfter( secondVersion?.CommitDateUtc ?? DateTime.Now ).And.BeOnOrBefore( end );
            (thirdVersion?.CommitDuration).Should().BeGreaterThan( TimeSpan.Zero );
        }
    }

    [Fact]
    public void Create_ShouldStoreLastAppliedVersionOnly_WhenPersistenceModeIsLastRecordOnly()
    {
        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                "1st version",
                _ => { } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.2" ),
                "2nd version",
                _ => { } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.3" ),
                "3rd version",
                _ => { } ) );

        var start = DateTime.UtcNow;

        var result = sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryPersistenceMode.LastRecordOnly ) );

        var end = DateTime.UtcNow;
        var versions = result.Database.GetRegisteredVersions();

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            result.CommittedVersions.ToArray()
                .Select( v => v.Value )
                .Should()
                .BeSequentiallyEqualTo( Version.Parse( "0.1" ), Version.Parse( "0.2" ), Version.Parse( "0.3" ) );

            result.PendingVersions.Length.Should().Be( 0 );
            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( Version.Parse( "0.3" ) );

            versions.Should().HaveCount( 1 );

            var lastVersion = versions.ElementAtOrDefault( 0 );
            (lastVersion?.Ordinal).Should().Be( 3 );
            (lastVersion?.Version).Should().Be( Version.Parse( "0.3" ) );
            (lastVersion?.Description).Should().Be( "3rd version" );
            (lastVersion?.CommitDateUtc).Should().BeOnOrAfter( start ).And.BeOnOrBefore( end );
            (lastVersion?.CommitDuration).Should().BeGreaterThan( TimeSpan.Zero );
        }
    }

    [Fact]
    public void Create_ShouldReturnResultWithException_WhenVersionCausesForeignKeyConflicts()
    {
        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                "1st version",
                b =>
                {
                    var t = b.Schemas.Default.Objects.CreateTable( "T" );
                    t.SetPrimaryKey( t.Columns.Create( "C1" ).SetType<int>().Asc() );
                    t.Columns.Create( "C2" ).SetType<int>().MarkAsNullable();
                    b.AddRawStatement( "INSERT INTO T (C1, C2) VALUES (1, NULL), (2, 1), (3, 5), (4, 6);" );
                } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.2" ),
                "2nd version",
                b =>
                {
                    b.Schemas.Default.SetName( "foo" );
                    var t = b.Schemas.Default.Objects.GetTable( "T" );
                    var ix = t.Indexes.Create( t.Columns.Get( "C2" ).Asc() );
                    t.ForeignKeys.Create( ix, t.PrimaryKey!.Index );
                } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.3" ),
                "3rd version",
                _ => { } ) );

        var result = sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );
        var versions = result.Database.GetRegisteredVersions();

        using ( new AssertionScope() )
        {
            var exception =
                (SqliteForeignKeyCheckException?)result.Exception.Should().BeOfType<SqliteForeignKeyCheckException>().And.Subject;

            (exception?.Version).Should().Be( Version.Parse( "0.2" ) );
            (exception?.FailedTableNames).Should().BeSequentiallyEqualTo( "foo_T" );

            result.CommittedVersions.ToArray().Select( v => v.Value ).Should().BeSequentiallyEqualTo( Version.Parse( "0.1" ) );
            result.PendingVersions.ToArray()
                .Select( v => v.Value )
                .Should()
                .BeSequentiallyEqualTo( Version.Parse( "0.2" ), Version.Parse( "0.3" ) );

            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( Version.Parse( "0.1" ) );

            versions.Should().HaveCount( 1 );
            (versions.ElementAtOrDefault( 0 )?.Version).Should().Be( Version.Parse( "0.1" ) );
        }
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomGetCurrentDateFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_CURRENT_DATE()";

        var min = DateOnly.FromDateTime( DateTime.Now );
        using var reader = command.ExecuteReader();
        reader.Read();
        var result = DateOnly.Parse( reader.GetString( 0 ) );
        var max = DateOnly.FromDateTime( DateTime.Now );

        using ( new AssertionScope() )
        {
            result.Should().BeGreaterOrEqualTo( min );
            result.Should().BeLessOrEqualTo( max );
        }
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomGetCurrentTimeFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_CURRENT_TIME()";

        var min = TimeOnly.FromDateTime( DateTime.Now );
        using var reader = command.ExecuteReader();
        reader.Read();
        var result = TimeOnly.Parse( reader.GetString( 0 ) );
        var max = TimeOnly.FromDateTime( DateTime.Now );

        using ( new AssertionScope() )
        {
            result.Should().BeGreaterOrEqualTo( min );
            result.Should().BeLessOrEqualTo( max );
        }
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomGetCurrentDateTimeFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_CURRENT_DATETIME()";

        var min = DateTime.Now;
        using var reader = command.ExecuteReader();
        reader.Read();
        var result = DateTime.Parse( reader.GetString( 0 ) );
        var max = DateTime.Now;

        using ( new AssertionScope() )
        {
            result.Should().BeOnOrAfter( min );
            result.Should().BeOnOrBefore( max );
        }
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomGetCurrentTimestampFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT GET_CURRENT_TIMESTAMP()";

        var min = DateTime.UtcNow.Ticks;
        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetInt64( 0 );
        var max = DateTime.UtcNow.Ticks;

        using ( new AssertionScope() )
        {
            result.Should().BeGreaterOrEqualTo( min );
            result.Should().BeLessOrEqualTo( max );
        }
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomNewGuidFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT NEW_GUID()";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetValue( 0 ) as byte[];

        var action = Lambda.Of( () => new Guid( result! ) );

        action.Should().NotThrow();
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomToLowerFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TO_LOWER('FooBarQUX')";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetString( 0 );

        result.Should().Be( "foobarqux" );
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomToLowerFunction_ThatReturnsNullWhenParameterIsNull()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TO_LOWER(NULL)";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetValue( 0 );

        result.Should().BeOfType<DBNull>();
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomToUpperFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TO_UPPER('fOObARqux')";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetString( 0 );

        result.Should().Be( "FOOBARQUX" );
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomToUpperFunction_ThatReturnsNullWhenParameterIsNull()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT TO_UPPER(NULL)";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetValue( 0 );

        result.Should().BeOfType<DBNull>();
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomInstrLastFunction()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT INSTR_LAST('foo.bar.qux', '.')";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetInt64( 0 );

        result.Should().Be( 8 );
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomInstrLastFunction_ThatReturnsNullWhenFirstParameterIsNull()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT INSTR_LAST(NULL, '.')";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetValue( 0 );

        result.Should().BeOfType<DBNull>();
    }

    [Fact]
    public void Create_ShouldReturnDatabaseWithCustomInstrLastFunction_ThatReturnsNullWhenSecondParameterIsNull()
    {
        var sut = new SqliteDatabaseFactory();
        var db = sut.Create( "DataSource=:memory:", new SqlDatabaseVersionHistory() ).Database;

        using var connection = db.Connect();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT INSTR_LAST('foo', NULL)";

        using var reader = command.ExecuteReader();
        reader.Read();
        var result = reader.GetValue( 0 );

        result.Should().BeOfType<DBNull>();
    }

    [Fact]
    public void Create_ShouldInvokeStatementListenersWithCorrectEvents_WhenModeIsCommit()
    {
        var onBefore = Substitute.For<Action<SqlDatabaseFactoryStatementEvent>>();
        var onAfter = Substitute.For<Action<SqlDatabaseFactoryStatementEvent, TimeSpan, Exception?>>();
        var listener = SqlDatabaseFactoryStatementListener.Create( onBefore, onAfter );

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.0.1" ),
                b =>
                {
                    var table = b.Schemas.Default.Objects.CreateTable( "T" );
                    table.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.0.2" ),
                b =>
                {
                    b.Schemas.Default.Objects.GetTable( "T" ).Columns.Create( "B" );
                    var table = b.Schemas.Default.Objects.CreateTable( "U" );
                    table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
                } ) );

        sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ).AddStatementListener( listener ) );

        using ( new AssertionScope() )
        {
            onBefore.Verify().CallCount.Should().Be( 18 );
            onAfter.Verify().CallCount.Should().Be( 18 );
        }
    }

    [Fact]
    public void Create_ShouldInvokeStatementListenersWithCorrectEvents_WhenModeIsCommitAndStatementThrowsAnException()
    {
        var onBefore = Substitute.For<Action<SqlDatabaseFactoryStatementEvent>>();
        var onAfter = Substitute.For<Action<SqlDatabaseFactoryStatementEvent, TimeSpan, Exception?>>();
        var listener = SqlDatabaseFactoryStatementListener.Create( onBefore, onAfter );

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.0.1" ),
                b =>
                {
                    var table = b.Schemas.Default.Objects.CreateTable( "T" );
                    table.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                Version.Parse( "0.0.2" ),
                b => b.AddRawStatement( "INSERT INTO T (A, B) VALUES (1, 1);" ) ) );

        var min = DateTime.UtcNow;

        sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ).AddStatementListener( listener ) );

        var max = DateTime.UtcNow;

        using ( new AssertionScope() )
        {
            var invalidArgs = onAfter.Verify().CallAt( 10 ).Exists().And.Arguments;
            var @event = (SqlDatabaseFactoryStatementEvent)invalidArgs[0]!;
            var elapsedTime = (TimeSpan)invalidArgs[1]!;
            var exception = (Exception)invalidArgs[2]!;

            @event.Key.Version.Should().BeEquivalentTo( Version.Parse( "0.0.2" ) );
            @event.Key.Ordinal.Should().Be( 2 );
            @event.Sql.Should().Be( "INSERT INTO T (A, B) VALUES (1, 1);" );
            @event.Parameters.Should().BeEmpty();
            @event.Type.Should().Be( SqlDatabaseFactoryStatementType.Change );
            @event.UtcStartDate.Should().BeOnOrAfter( min );
            @event.UtcStartDate.Should().BeOnOrBefore( max );
            elapsedTime.Should().BeGreaterThan( TimeSpan.Zero );
            exception.Should().BeOfType<SqliteException>();
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldRegisterCallbackAndInvokeItDuringDatabaseCreation()
    {
        var firstCallback = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();
        var secondCallback = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create( Version.Parse( "0.0.1" ), b => b.AddConnectionChangeCallback( firstCallback ) ),
            SqlDatabaseVersion.Create( Version.Parse( "0.0.2" ), b => b.AddConnectionChangeCallback( secondCallback ) ) );

        var result = sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.DryRun ) );

        using ( new AssertionScope() )
        {
            firstCallback.Verify().CallCount.Should().Be( 1 );
            var firstEvent = (SqlDatabaseConnectionChangeEvent?)firstCallback.Verify().CallAt( 0 ).Arguments.ElementAtOrDefault( 0 );
            (firstEvent?.Connection).Should().BeSameAs( result.Database.Connect() );
            (firstEvent?.StateChange).Should().BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );

            secondCallback.Verify().CallCount.Should().Be( 1 );
            var secondEvent = (SqlDatabaseConnectionChangeEvent?)secondCallback.Verify().CallAt( 0 ).Arguments.ElementAtOrDefault( 0 );
            (secondEvent?.Connection).Should().BeSameAs( result.Database.Connect() );
            (secondEvent?.StateChange).Should().BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldRegisterCallbackAndInvokeItDuringDatabaseDisposal()
    {
        var callback = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create( Version.Parse( "0.0.1" ), b => b.AddConnectionChangeCallback( callback ) ) );

        var result = sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.DryRun ) );
        result.Database.Dispose();

        using ( new AssertionScope() )
        {
            callback.Verify().CallCount.Should().Be( 2 );
            var @event = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 1 ).Arguments.ElementAtOrDefault( 0 );
            (@event?.Connection).Should().BeSameAs( result.Database.Connect() );
            (@event?.StateChange).Should().BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Open, ConnectionState.Closed ) );
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldRegisterCallbackAndInvokeItOnConnectionClose_WhenBuilderThrowsAnException()
    {
        var callback = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create( Version.Parse( "0.0.1" ), b => b.AddConnectionChangeCallback( callback ) ),
            SqlDatabaseVersion.Create( Version.Parse( "0.0.2" ), _ => throw new Exception() ) );

        try
        {
            sut.Create( "DataSource=:memory:", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );
        }
        catch { }

        using ( new AssertionScope() )
        {
            callback.Verify().CallCount.Should().Be( 2 );
            var @event = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 1 ).Arguments.ElementAtOrDefault( 0 );
            (@event?.StateChange).Should().BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Open, ConnectionState.Closed ) );
        }
    }

    [Fact]
    public void RegisterSqlite_ShouldAddSqliteFactory()
    {
        var sut = new SqlDatabaseFactoryProvider();
        var result = sut.RegisterSqlite();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().BeSequentiallyEqualTo( SqliteDialect.Instance );
            result.GetFor( SqliteDialect.Instance ).Should().BeOfType<SqliteDatabaseFactory>();
        }
    }

    //[Fact]
    [Fact( Skip = "x" )]
    public void X()
    {
        var sut = new SqliteDatabaseFactory();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                "1st version",
                db =>
                {
                    var s = db.Schemas.Default;
                    var t = s.Objects.CreateTable( "T" );
                    var a = t.Columns.Create( "A" );
                    var b = t.Columns.Create( "B" ).MarkAsNullable();
                    var c = t.Columns.Create( "C" );

                    t.SetPrimaryKey( a.Asc(), c.Asc() );

                    t.Indexes.Create( b.Asc() ).SetFilter( x => x["B"] != null );

                    t = s.Objects.CreateTable( "X" );
                    a = t.Columns.Create( "A" );
                    b = t.Columns.Create( "B" ).MarkAsNullable();

                    t.SetPrimaryKey( a.Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=:memory:",
            history,
            SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

        var interpreter = result.Database.NodeInterpreterFactory.Create();
        var table = result.Database.Schemas.Default.Objects.GetTable( "T" ).ToRecordSet();
        var xTable = result.Database.Schemas.Default.Objects.GetTable( "X" ).ToRecordSet();
        var aliasedTable = table.As( "U" );

        var insertInto = SqlNode.Batch(
            SqlNode.Values(
                    new[,]
                    {
                        { SqlNode.Literal( 1 ), SqlNode.Literal( 101 ), SqlNode.Literal( 1 ) },
                        { SqlNode.Literal( 2 ), SqlNode.Null(), SqlNode.Literal( 1 ) }
                    } )
                .ToInsertInto( table, table["A"], table["B"], table["C"] ),
            SqlNode.Values( SqlNode.Literal( 1 ), SqlNode.Literal( 11 ) )
                .ToInsertInto( xTable, xTable["A"], xTable["B"] ) );

        var update2 = aliasedTable.Join( xTable.InnerOn( xTable["A"] == aliasedTable["A"] ) )
            .ToUpdate( table["B"].Assign( table["B"] + xTable["B"] + SqlNode.Literal( 1 ) ) );

        var cte = aliasedTable.Join( xTable.InnerOn( xTable["A"] == aliasedTable["A"] ) )
            .Select( aliasedTable["A"].AsSelf(), xTable["B"].AsSelf() )
            .ToCte( "_X" );

        var cteUpdate = aliasedTable.Join( xTable.InnerOn( xTable["A"] == aliasedTable["A"] ) )
            .With( cte )
            .ToUpdate(
                table["B"]
                    .Assign(
                        table["B"] +
                        cte.RecordSet.ToDataSource().AndWhere( cte.RecordSet["A"] == table["A"] ).Select( cte.RecordSet["B"].AsSelf() ) +
                        SqlNode.Literal( 1 ) ) );

        var update = aliasedTable.ToDataSource()
            .AndWhere( aliasedTable["B"] != null )
            .OrderBy( aliasedTable["A"].Asc() )
            .ToUpdate( table["B"].Assign( table["B"] + SqlNode.Literal( 1 ) ) );

        var query = table.ToDataSource().Select( table.GetAll() ).OrderBy( table["A"].Asc() );

        using var connection = result.Database.Connect();
        using var cmd = connection.CreateCommand();

        var createTable = SqlNode.CreateTable(
            SqlRecordSetInfo.Create( "test" ),
            new[]
            {
                SqlNode.Column<int>( "x" ),
                SqlNode.Column<DateTime>( "y", defaultValue: SqlNode.Literal("foo").Concat(SqlNode.Literal("bar")) )
            },
            constraintsProvider: t => SqlCreateTableConstraints.Empty
                .WithPrimaryKey( SqlNode.PrimaryKey( "PK_test", t["x"].Asc() ) )
                .WithChecks( SqlNode.Check( "CHK_test", t["x"] > SqlNode.Literal( 0 ) ) ) );

        interpreter.Visit( createTable );
        //interpreter.Visit( insertInto );
        cmd.CommandText = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();

        var dt = DateTime.Now;
        cmd.ExecuteNonQuery();

        cmd.CommandText = "INSERT INTO test (x) VALUES (1);";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT * FROM test;";
        using ( var reader = cmd.ExecuteReader() )
        {
            var x = new List<Dictionary<string, object?>>();
            while ( reader.Read() )
            {
                var dict = new Dictionary<string, object?>();
                for ( var i = 0; i < reader.FieldCount; ++i )
                    dict[reader.GetName( i )] = reader.GetValue( i );

                x.Add( dict );
            }
        }

        interpreter.Visit( update2 );
        cmd.CommandText = interpreter.Context.Sql.ToString();
        interpreter.Context.Clear();

        cmd.ExecuteNonQuery();

        interpreter.Visit( query );
        cmd.CommandText = interpreter.Context.Sql.ToString();

        var values = new List<Dictionary<string, object?>>();
        using ( var reader = cmd.ExecuteReader() )
        {
            while ( reader.Read() )
            {
                var row = new Dictionary<string, object?>();
                row["A"] = reader.GetValue( reader.GetOrdinal( "A" ) );
                row["B"] = reader.GetValue( reader.GetOrdinal( "B" ) );
                values.Add( row );
            }
        }

        cmd.CommandText = "SELECT NEW_GUID() AS A, GET_CURRENT_DATETIME() AS B, GET_CURRENT_TIMESTAMP() AS C";

        using ( var reader = cmd.ExecuteReader() )
        {
            while ( reader.Read() )
            {
                var row = new Dictionary<string, object?>();
                row["A"] = reader.GetGuid( reader.GetOrdinal( "A" ) );
                row["B"] = DateTime.Parse( reader.GetString( reader.GetOrdinal( "B" ) ) );
                row["C"] = reader.GetInt64( reader.GetOrdinal( "C" ) );
                values.Add( row );
            }
        }
    }

    public class Persistent : TestsBase
    {
        [Fact]
        public void Create_ShouldCreateCorrectDatabase_WhenTheSameVersionsAreAppliedSecondTime()
        {
            const string dbName = ".test_0.db";
            const string connectionString = $"DataSource=./{dbName};Pooling=false";

            var sut = new SqliteDatabaseFactory();
            var history = new SqlDatabaseVersionHistory( SqlDatabaseVersion.Create( Version.Parse( "0.1" ), _ => { } ) );

            try
            {
                var result1 = sut.Create(
                    connectionString,
                    history,
                    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

                var result2 = sut.Create(
                    connectionString,
                    history,
                    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

                var versions = result2.Database.GetRegisteredVersions();

                using ( new AssertionScope() )
                {
                    result1.Exception.Should().BeNull();
                    result1.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
                    result1.NewVersion.Should().Be( Version.Parse( "0.1" ) );
                    result1.CommittedVersions.ToArray().Select( v => v.Value ).Should().BeSequentiallyEqualTo( Version.Parse( "0.1" ) );
                    result1.PendingVersions.Length.Should().Be( 0 );

                    result2.Exception.Should().BeNull();
                    result2.OldVersion.Should().Be( Version.Parse( "0.1" ) );
                    result2.NewVersion.Should().Be( Version.Parse( "0.1" ) );
                    result2.CommittedVersions.Length.Should().Be( 0 );
                    result2.PendingVersions.Length.Should().Be( 0 );

                    versions.Should().HaveCount( 1 );
                    versions.ElementAtOrDefault( 0 )?.Version.Should().Be( Version.Parse( "0.1" ) );
                }
            }
            finally
            {
                var dbPath = Path.Combine( Environment.CurrentDirectory, dbName );
                File.Delete( dbPath );
            }
        }

        [Fact]
        public void Create_ShouldCreateCorrectDatabase_WhenNewVersionIsAppliedLater()
        {
            const string dbName = ".test_1.db";
            const string connectionString = $"DataSource=./{dbName};Pooling=false";

            var version1Modes = new List<(SqlDatabaseCreateMode Mode, bool IsAttached)>();
            var version2Modes = new List<(SqlDatabaseCreateMode Mode, bool IsAttached)>();
            var version3Modes = new List<(SqlDatabaseCreateMode Mode, bool IsAttached)>();

            var sut = new SqliteDatabaseFactory();
            var version1 = SqlDatabaseVersion.Create(
                Version.Parse( "0.1" ),
                b =>
                {
                    version1Modes.Add( (b.Mode, b.IsAttached) );
                    b.SetDetachedMode();
                } );

            var version2 = SqlDatabaseVersion.Create(
                Version.Parse( "0.2" ),
                b =>
                {
                    version2Modes.Add( (b.Mode, b.IsAttached) );
                    b.SetDetachedMode();
                } );

            var version3 = SqlDatabaseVersion.Create(
                Version.Parse( "0.3" ),
                b => { version3Modes.Add( (b.Mode, b.IsAttached) ); } );

            try
            {
                var result1 = sut.Create(
                    connectionString,
                    new SqlDatabaseVersionHistory( version1, version2 ),
                    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

                var result2 = sut.Create(
                    connectionString,
                    new SqlDatabaseVersionHistory( version1, version2, version3 ),
                    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

                var versions = result2.Database.GetRegisteredVersions();

                using ( new AssertionScope() )
                {
                    version1Modes.Should()
                        .BeSequentiallyEqualTo( (SqlDatabaseCreateMode.Commit, true), (SqlDatabaseCreateMode.NoChanges, true) );

                    version2Modes.Should()
                        .BeSequentiallyEqualTo( (SqlDatabaseCreateMode.Commit, true), (SqlDatabaseCreateMode.NoChanges, true) );

                    version3Modes.Should().BeSequentiallyEqualTo( (SqlDatabaseCreateMode.Commit, true) );

                    result1.Exception.Should().BeNull();
                    result1.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
                    result1.NewVersion.Should().Be( Version.Parse( "0.2" ) );
                    result1.CommittedVersions.ToArray()
                        .Select( v => v.Value )
                        .Should()
                        .BeSequentiallyEqualTo( Version.Parse( "0.1" ), Version.Parse( "0.2" ) );

                    result1.PendingVersions.Length.Should().Be( 0 );

                    result2.Exception.Should().BeNull();
                    result2.OldVersion.Should().Be( Version.Parse( "0.2" ) );
                    result2.NewVersion.Should().Be( Version.Parse( "0.3" ) );
                    result2.CommittedVersions.ToArray().Select( v => v.Value ).Should().BeSequentiallyEqualTo( Version.Parse( "0.3" ) );
                    result2.PendingVersions.Length.Should().Be( 0 );

                    versions.Select( v => v.Version )
                        .Should()
                        .BeSequentiallyEqualTo( Version.Parse( "0.1" ), Version.Parse( "0.2" ), Version.Parse( "0.3" ) );
                }
            }
            finally
            {
                var dbPath = Path.Combine( Environment.CurrentDirectory, dbName );
                File.Delete( dbPath );
            }
        }

        [Fact]
        public void AddConnectionChangeCallback_ShouldRegisterCallbackAndInvokeItOnEachConnectionOpenAndClose()
        {
            const string dbName = ".test_2.db";
            const string connectionString = $"DataSource=./{dbName};Pooling=false";

            var callback = Substitute.For<Action<SqlDatabaseConnectionChangeEvent>>();

            var sut = new SqliteDatabaseFactory();
            var history = new SqlDatabaseVersionHistory(
                SqlDatabaseVersion.Create( Version.Parse( "0.1" ), b => b.AddConnectionChangeCallback( callback ) ) );

            try
            {
                var result = sut.Create(
                    connectionString,
                    history,
                    SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

                var connection = result.Database.Connect();
                connection.Dispose();

                using ( new AssertionScope() )
                {
                    callback.Verify().CallCount.Should().Be( 4 );

                    var firstEvent = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 0 ).Arguments.ElementAtOrDefault( 0 );
                    (firstEvent?.StateChange).Should()
                        .BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );

                    var secondEvent = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 1 ).Arguments.ElementAtOrDefault( 0 );
                    (secondEvent?.StateChange).Should()
                        .BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Open, ConnectionState.Closed ) );

                    var thirdEvent = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 2 ).Arguments.ElementAtOrDefault( 0 );
                    (thirdEvent?.Connection).Should().BeSameAs( connection );
                    (thirdEvent?.StateChange).Should()
                        .BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );

                    var fourthEvent = (SqlDatabaseConnectionChangeEvent?)callback.Verify().CallAt( 3 ).Arguments.ElementAtOrDefault( 0 );
                    (fourthEvent?.Connection).Should().BeSameAs( connection );
                    (fourthEvent?.StateChange).Should()
                        .BeEquivalentTo( new StateChangeEventArgs( ConnectionState.Open, ConnectionState.Closed ) );
                }
            }
            finally
            {
                var dbPath = Path.Combine( Environment.CurrentDirectory, dbName );
                File.Delete( dbPath );
            }
        }
    }
}
