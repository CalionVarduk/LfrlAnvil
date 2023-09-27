using System.Collections.Generic;
using System.IO;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.TestExtensions.FluentAssertions;

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
            sut.Dialect.Should().BeSameAs( SqliteDialect.Instance );
            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Should().BeEmpty();
            result.Exception.Should().BeNull();
            result.CommittedVersions.Length.Should().Be( 0 );
            result.PendingVersions.Length.Should().Be( 0 );
            result.OldVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( SqlDatabaseVersionHistory.InitialVersion );
            result.Database.Version.Should().Be( result.NewVersion );
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
    }
}
