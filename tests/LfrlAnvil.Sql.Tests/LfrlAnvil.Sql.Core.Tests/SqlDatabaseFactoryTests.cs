using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.Sql.Tests;

public class SqlDatabaseFactoryTests : TestsBase
{
    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit, true )]
    [InlineData( SqlDatabaseCreateMode.Commit, false )]
    [InlineData( SqlDatabaseCreateMode.DryRun, true )]
    [InlineData( SqlDatabaseCreateMode.DryRun, false )]
    [InlineData( SqlDatabaseCreateMode.NoChanges, true )]
    [InlineData( SqlDatabaseCreateMode.NoChanges, false )]
    public void Create_WithoutVersions_ShouldCreateCorrectDatabase(SqlDatabaseCreateMode mode, bool createVersionHistoryTable)
    {
        ISqlDatabaseFactory sut = new SqlDatabaseFactoryMock( createVersionHistoryTable );
        var history = new SqlDatabaseVersionHistory();

        var result = sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        using ( new AssertionScope() )
        {
            result.Database.Dialect.Should().BeSameAs( sut.Dialect );
            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Name.Should().Be( "common" );
            result.Database.Schemas.Default.Objects.Should().BeEmpty();
            result.Database.Version.Should().Be( result.NewVersion );
            result.Database.ServerVersion.Should().Be( "0.0.0" );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    public void Create_ShouldCreateVersionHistoryTable_WhenVersionHistoryTableDoesNotExist(SqlDatabaseCreateMode mode)
    {
        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: true );
        var history = new SqlDatabaseVersionHistory();

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "CreateDbCommand(DbTransaction[0]:DbCommand[0])",
                    "DbTransaction[0]:DbCommand[0].ExecuteNonQuery(CREATE [Table] common.__VersionHistory;)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0]:DbCommand[0].Dispose(True)",
                    "DbTransaction[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    @"DbCommand[1].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[1].DbDataReader[0].Close",
                    "DbCommand[1].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    public void Create_ShouldCreateVersionHistoryTable_WhenVersionHistoryTableDoesNotExist_WithCustomName(SqlDatabaseCreateMode mode)
    {
        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: true );
        var history = new SqlDatabaseVersionHistory();

        sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( mode )
                .SetVersionHistoryName( SqlSchemaObjectName.Create( "foo", "bar" ) ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "CreateDbCommand(DbTransaction[0]:DbCommand[0])",
                    "DbTransaction[0]:DbCommand[0].ExecuteNonQuery(CREATE [Table] foo.bar;)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0]:DbCommand[0].Dispose(True)",
                    "DbTransaction[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    @"DbCommand[1].ExecuteReader(DbDataReader[0] => FROM [foo].[bar]
ORDER BY ([foo].[bar].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[1].DbDataReader[0].Close",
                    "DbCommand[1].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    public void Create_ShouldDoNothing_WhenVersionHistoryTableExists(SqlDatabaseCreateMode mode)
    {
        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: false );
        var history = new SqlDatabaseVersionHistory();

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    public void Create_ShouldDoNothing_WhenVersionHistoryTableExists_WithLastRecordOnlyQueryMode(SqlDatabaseCreateMode mode)
    {
        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: false );
        var history = new SqlDatabaseVersionHistory();

        sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( mode )
                .SetVersionHistoryQueryMode( SqlDatabaseVersionHistoryMode.LastRecordOnly ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
LIMIT (""1"" : System.Int32)
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) DESC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    public void Create_ShouldThrow_WhenVersionThrows(SqlDatabaseCreateMode mode)
    {
        var exception = new Exception();
        var sut = new SqlDatabaseFactoryMock();

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                _ => throw exception ) );

        var action = Lambda.Of( () => sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) ) );

        using ( new AssertionScope() )
        {
            action.Should().ThrowExactly<Exception>().And.Should().BeSameAs( exception );
            sut.Connection.State.Should().Be( ConnectionState.Closed );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges, SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseCreateMode.NoChanges, SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    [InlineData( SqlDatabaseCreateMode.DryRun, SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseCreateMode.DryRun, SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    [InlineData( SqlDatabaseCreateMode.Commit, SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseCreateMode.Commit, SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void Create_ShouldReturnCorrectResult_WhenAllVersionsHaveAlreadyBeenCommitted(
        SqlDatabaseCreateMode mode,
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        var secondVersionRecord = new SqlDatabaseVersionRecord(
            Ordinal: 2,
            Version: new Version( "0.2" ),
            Description: string.Empty,
            CommitDateUtc: DateTime.UtcNow,
            CommitDuration: TimeSpan.FromMilliseconds( 12 ) );

        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                persistenceMode == SqlDatabaseVersionHistoryMode.LastRecordOnly
                    ? CreateVersionResultSet( secondVersionRecord )
                    : CreateVersionResultSet(
                        new SqlDatabaseVersionRecord(
                            Ordinal: 1,
                            Version: new Version( "0.1" ),
                            Description: string.Empty,
                            CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                            CommitDuration: TimeSpan.FromMilliseconds( 10 ) ),
                        secondVersionRecord )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( mode )
                .SetVersionHistoryPersistenceMode( persistenceMode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().Be( new Version( "0.2" ) );
            result.NewVersion.Should().Be( new Version( "0.2" ) );
            result.OriginalVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void Create_ShouldReturnCorrectResult_WhenNoVersionsHaveBeenCommittedYet_NoChangesMode(
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.NoChanges )
                .SetVersionHistoryPersistenceMode( persistenceMode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Should().BeEmpty();
            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void Create_ShouldReturnCorrectResult_WhenNoVersionsHaveBeenCommittedYet_DryRunMode(
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.DryRun )
                .SetVersionHistoryPersistenceMode( persistenceMode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult_WhenNoVersionsHaveBeenCommittedYet_CommitMode_PersistAllRecords()
    {
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryMode.AllRecords ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    "DbCommand[1].CreateParameter(DbParameter[0])",
                    "DbCommand[1].DbParameter[0].DbType = Int32",
                    "DbCommand[1].DbParameter[0].IsNullable = False",
                    "DbCommand[1].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[1].CreateParameter(DbParameter[1])",
                    "DbCommand[1].DbParameter[1].DbType = Int32",
                    "DbCommand[1].DbParameter[1].IsNullable = False",
                    "DbCommand[1].DbParameter[1].ParameterName = VersionMajor",
                    "DbCommand[1].CreateParameter(DbParameter[2])",
                    "DbCommand[1].DbParameter[2].DbType = Int32",
                    "DbCommand[1].DbParameter[2].IsNullable = False",
                    "DbCommand[1].DbParameter[2].ParameterName = VersionMinor",
                    "DbCommand[1].CreateParameter(DbParameter[3])",
                    "DbCommand[1].DbParameter[3].DbType = Int32",
                    "DbCommand[1].DbParameter[3].IsNullable = True",
                    "DbCommand[1].DbParameter[3].ParameterName = VersionBuild",
                    "DbCommand[1].CreateParameter(DbParameter[4])",
                    "DbCommand[1].DbParameter[4].DbType = Int32",
                    "DbCommand[1].DbParameter[4].IsNullable = True",
                    "DbCommand[1].DbParameter[4].ParameterName = VersionRevision",
                    "DbCommand[1].CreateParameter(DbParameter[5])",
                    "DbCommand[1].DbParameter[5].DbType = String",
                    "DbCommand[1].DbParameter[5].IsNullable = False",
                    "DbCommand[1].DbParameter[5].ParameterName = Description",
                    "DbCommand[1].CreateParameter(DbParameter[6])",
                    "DbCommand[1].DbParameter[6].DbType = String",
                    "DbCommand[1].DbParameter[6].IsNullable = False",
                    "DbCommand[1].DbParameter[6].ParameterName = CommitDateUtc",
                    "DbCommand[1].Prepare",
                    "CreateDbCommand(DbCommand[2])",
                    "DbCommand[2].CreateParameter(DbParameter[0])",
                    "DbCommand[2].DbParameter[0].DbType = Int32",
                    "DbCommand[2].DbParameter[0].IsNullable = False",
                    "DbCommand[2].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[2].CreateParameter(DbParameter[1])",
                    "DbCommand[2].DbParameter[1].DbType = Int32",
                    "DbCommand[2].DbParameter[1].IsNullable = False",
                    "DbCommand[2].DbParameter[1].ParameterName = CommitDurationInTicks",
                    "DbCommand[2].Prepare",
                    "CreateDbCommand(DbCommand[3])",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "DbTransaction[0]:DbCommand[3].ExecuteNonQuery(CREATE [Table] common.T1;)",
                    @"DbTransaction[0]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[1].Serializable)",
                    "DbTransaction[1]:DbCommand[3].ExecuteNonQuery(CREATE [Table] common.T2;)",
                    @"DbTransaction[1]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[1].Commit",
                    "DbTransaction[1].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[2].Serializable)",
                    @"DbTransaction[2]:DbCommand[2].ExecuteNonQuery(UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);)",
                    @"DbTransaction[2]:DbCommand[2].ExecuteNonQuery(UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);)",
                    "DbTransaction[2].Commit",
                    "DbTransaction[2].Dispose(True)",
                    "DbCommand[3].Dispose(True)",
                    "DbCommand[1].Dispose(True)",
                    "DbCommand[2].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( new Version( "0.2" ) );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult_WhenNoVersionsHaveBeenCommittedYet_CommitMode_PersistLastRecordOnly()
    {
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryMode.LastRecordOnly ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    "DbCommand[1].CreateParameter(DbParameter[0])",
                    "DbCommand[1].DbParameter[0].DbType = Int32",
                    "DbCommand[1].DbParameter[0].IsNullable = False",
                    "DbCommand[1].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[1].CreateParameter(DbParameter[1])",
                    "DbCommand[1].DbParameter[1].DbType = Int32",
                    "DbCommand[1].DbParameter[1].IsNullable = False",
                    "DbCommand[1].DbParameter[1].ParameterName = VersionMajor",
                    "DbCommand[1].CreateParameter(DbParameter[2])",
                    "DbCommand[1].DbParameter[2].DbType = Int32",
                    "DbCommand[1].DbParameter[2].IsNullable = False",
                    "DbCommand[1].DbParameter[2].ParameterName = VersionMinor",
                    "DbCommand[1].CreateParameter(DbParameter[3])",
                    "DbCommand[1].DbParameter[3].DbType = Int32",
                    "DbCommand[1].DbParameter[3].IsNullable = True",
                    "DbCommand[1].DbParameter[3].ParameterName = VersionBuild",
                    "DbCommand[1].CreateParameter(DbParameter[4])",
                    "DbCommand[1].DbParameter[4].DbType = Int32",
                    "DbCommand[1].DbParameter[4].IsNullable = True",
                    "DbCommand[1].DbParameter[4].ParameterName = VersionRevision",
                    "DbCommand[1].CreateParameter(DbParameter[5])",
                    "DbCommand[1].DbParameter[5].DbType = String",
                    "DbCommand[1].DbParameter[5].IsNullable = False",
                    "DbCommand[1].DbParameter[5].ParameterName = Description",
                    "DbCommand[1].CreateParameter(DbParameter[6])",
                    "DbCommand[1].DbParameter[6].DbType = String",
                    "DbCommand[1].DbParameter[6].IsNullable = False",
                    "DbCommand[1].DbParameter[6].ParameterName = CommitDateUtc",
                    "DbCommand[1].Prepare",
                    "CreateDbCommand(DbCommand[2])",
                    "DbCommand[2].CreateParameter(DbParameter[0])",
                    "DbCommand[2].DbParameter[0].DbType = Int32",
                    "DbCommand[2].DbParameter[0].IsNullable = False",
                    "DbCommand[2].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[2].CreateParameter(DbParameter[1])",
                    "DbCommand[2].DbParameter[1].DbType = Int32",
                    "DbCommand[2].DbParameter[1].IsNullable = False",
                    "DbCommand[2].DbParameter[1].ParameterName = CommitDurationInTicks",
                    "DbCommand[2].Prepare",
                    "CreateDbCommand(DbCommand[3])",
                    "DbCommand[3].Prepare",
                    "CreateDbCommand(DbCommand[4])",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "DbTransaction[0]:DbCommand[4].ExecuteNonQuery(CREATE [Table] common.T1;)",
                    "DbTransaction[0]:DbCommand[3].ExecuteNonQuery(DELETE FROM [common].[__VersionHistory];)",
                    @"DbTransaction[0]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[1].Serializable)",
                    "DbTransaction[1]:DbCommand[4].ExecuteNonQuery(CREATE [Table] common.T2;)",
                    "DbTransaction[1]:DbCommand[3].ExecuteNonQuery(DELETE FROM [common].[__VersionHistory];)",
                    @"DbTransaction[1]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[1].Commit",
                    "DbTransaction[1].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[2].Serializable)",
                    @"DbTransaction[2]:DbCommand[2].ExecuteNonQuery(UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);)",
                    "DbTransaction[2].Commit",
                    "DbTransaction[2].Dispose(True)",
                    "DbCommand[4].Dispose(True)",
                    "DbCommand[1].Dispose(True)",
                    "DbCommand[2].Dispose(True)",
                    "DbCommand[3].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( new Version( "0.2" ) );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void Create_ShouldReturnCorrectResult_WhenSomeVersionsHaveNotBeenCommittedYet_NoChangesMode(
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.NoChanges )
                .SetVersionHistoryPersistenceMode( persistenceMode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo( "[Table] common.T1", "[Index] common.UIX_T1_C1A", "[PrimaryKey] common.PK_T1" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().Be( new Version( "0.1" ) );
            result.NewVersion.Should().BeSameAs( result.OldVersion );
            result.OriginalVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 0, 1 ).ToArray() );
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 1 ).ToArray() );
        }
    }

    [Theory]
    [InlineData( SqlDatabaseVersionHistoryMode.AllRecords )]
    [InlineData( SqlDatabaseVersionHistoryMode.LastRecordOnly )]
    public void Create_ShouldReturnCorrectResult_WhenSomeVersionsHaveNotBeenCommittedYet_DryRunMode(
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.DryRun )
                .SetVersionHistoryPersistenceMode( persistenceMode ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().Be( new Version( "0.1" ) );
            result.NewVersion.Should().BeSameAs( result.OldVersion );
            result.OriginalVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 0, 1 ).ToArray() );
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 1 ).ToArray() );
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult_WhenSomeVersionsHaveNotBeenCommittedYet_CommitMode_PersistAllRecords()
    {
        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryMode.AllRecords ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    "DbCommand[1].CreateParameter(DbParameter[0])",
                    "DbCommand[1].DbParameter[0].DbType = Int32",
                    "DbCommand[1].DbParameter[0].IsNullable = False",
                    "DbCommand[1].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[1].CreateParameter(DbParameter[1])",
                    "DbCommand[1].DbParameter[1].DbType = Int32",
                    "DbCommand[1].DbParameter[1].IsNullable = False",
                    "DbCommand[1].DbParameter[1].ParameterName = VersionMajor",
                    "DbCommand[1].CreateParameter(DbParameter[2])",
                    "DbCommand[1].DbParameter[2].DbType = Int32",
                    "DbCommand[1].DbParameter[2].IsNullable = False",
                    "DbCommand[1].DbParameter[2].ParameterName = VersionMinor",
                    "DbCommand[1].CreateParameter(DbParameter[3])",
                    "DbCommand[1].DbParameter[3].DbType = Int32",
                    "DbCommand[1].DbParameter[3].IsNullable = True",
                    "DbCommand[1].DbParameter[3].ParameterName = VersionBuild",
                    "DbCommand[1].CreateParameter(DbParameter[4])",
                    "DbCommand[1].DbParameter[4].DbType = Int32",
                    "DbCommand[1].DbParameter[4].IsNullable = True",
                    "DbCommand[1].DbParameter[4].ParameterName = VersionRevision",
                    "DbCommand[1].CreateParameter(DbParameter[5])",
                    "DbCommand[1].DbParameter[5].DbType = String",
                    "DbCommand[1].DbParameter[5].IsNullable = False",
                    "DbCommand[1].DbParameter[5].ParameterName = Description",
                    "DbCommand[1].CreateParameter(DbParameter[6])",
                    "DbCommand[1].DbParameter[6].DbType = String",
                    "DbCommand[1].DbParameter[6].IsNullable = False",
                    "DbCommand[1].DbParameter[6].ParameterName = CommitDateUtc",
                    "DbCommand[1].Prepare",
                    "CreateDbCommand(DbCommand[2])",
                    "DbCommand[2].CreateParameter(DbParameter[0])",
                    "DbCommand[2].DbParameter[0].DbType = Int32",
                    "DbCommand[2].DbParameter[0].IsNullable = False",
                    "DbCommand[2].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[2].CreateParameter(DbParameter[1])",
                    "DbCommand[2].DbParameter[1].DbType = Int32",
                    "DbCommand[2].DbParameter[1].IsNullable = False",
                    "DbCommand[2].DbParameter[1].ParameterName = CommitDurationInTicks",
                    "DbCommand[2].Prepare",
                    "CreateDbCommand(DbCommand[3])",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "DbTransaction[0]:DbCommand[3].ExecuteNonQuery(CREATE [Table] common.T2;)",
                    @"DbTransaction[0]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[1].Serializable)",
                    @"DbTransaction[1]:DbCommand[2].ExecuteNonQuery(UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);)",
                    "DbTransaction[1].Commit",
                    "DbTransaction[1].Dispose(True)",
                    "DbCommand[3].Dispose(True)",
                    "DbCommand[1].Dispose(True)",
                    "DbCommand[2].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().Be( new Version( "0.1" ) );
            result.NewVersion.Should().Be( new Version( "0.2" ) );
            result.OriginalVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 0, 1 ).ToArray() );
            result.CommittedVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 1 ).ToArray() );
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult_WhenSomeVersionsHaveNotBeenCommittedYet_CommitMode_PersistLastRecordOnly()
    {
        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .SetVersionHistoryPersistenceMode( SqlDatabaseVersionHistoryMode.LastRecordOnly ) );

        using ( new AssertionScope() )
        {
            sut.Connection.State.Should().Be( ConnectionState.Closed );
            sut.Connection.Audit.Should()
                .BeSequentiallyEqualTo(
                    "ChangeState(Closed => Open)",
                    "CreateDbCommand(DbCommand[0])",
                    @"DbCommand[0].ExecuteReader(DbDataReader[0] => FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;)",
                    "DbCommand[0].DbDataReader[0].Close",
                    "DbCommand[0].Dispose(True)",
                    "CreateDbCommand(DbCommand[1])",
                    "DbCommand[1].CreateParameter(DbParameter[0])",
                    "DbCommand[1].DbParameter[0].DbType = Int32",
                    "DbCommand[1].DbParameter[0].IsNullable = False",
                    "DbCommand[1].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[1].CreateParameter(DbParameter[1])",
                    "DbCommand[1].DbParameter[1].DbType = Int32",
                    "DbCommand[1].DbParameter[1].IsNullable = False",
                    "DbCommand[1].DbParameter[1].ParameterName = VersionMajor",
                    "DbCommand[1].CreateParameter(DbParameter[2])",
                    "DbCommand[1].DbParameter[2].DbType = Int32",
                    "DbCommand[1].DbParameter[2].IsNullable = False",
                    "DbCommand[1].DbParameter[2].ParameterName = VersionMinor",
                    "DbCommand[1].CreateParameter(DbParameter[3])",
                    "DbCommand[1].DbParameter[3].DbType = Int32",
                    "DbCommand[1].DbParameter[3].IsNullable = True",
                    "DbCommand[1].DbParameter[3].ParameterName = VersionBuild",
                    "DbCommand[1].CreateParameter(DbParameter[4])",
                    "DbCommand[1].DbParameter[4].DbType = Int32",
                    "DbCommand[1].DbParameter[4].IsNullable = True",
                    "DbCommand[1].DbParameter[4].ParameterName = VersionRevision",
                    "DbCommand[1].CreateParameter(DbParameter[5])",
                    "DbCommand[1].DbParameter[5].DbType = String",
                    "DbCommand[1].DbParameter[5].IsNullable = False",
                    "DbCommand[1].DbParameter[5].ParameterName = Description",
                    "DbCommand[1].CreateParameter(DbParameter[6])",
                    "DbCommand[1].DbParameter[6].DbType = String",
                    "DbCommand[1].DbParameter[6].IsNullable = False",
                    "DbCommand[1].DbParameter[6].ParameterName = CommitDateUtc",
                    "DbCommand[1].Prepare",
                    "CreateDbCommand(DbCommand[2])",
                    "DbCommand[2].CreateParameter(DbParameter[0])",
                    "DbCommand[2].DbParameter[0].DbType = Int32",
                    "DbCommand[2].DbParameter[0].IsNullable = False",
                    "DbCommand[2].DbParameter[0].ParameterName = Ordinal",
                    "DbCommand[2].CreateParameter(DbParameter[1])",
                    "DbCommand[2].DbParameter[1].DbType = Int32",
                    "DbCommand[2].DbParameter[1].IsNullable = False",
                    "DbCommand[2].DbParameter[1].ParameterName = CommitDurationInTicks",
                    "DbCommand[2].Prepare",
                    "CreateDbCommand(DbCommand[3])",
                    "DbCommand[3].Prepare",
                    "CreateDbCommand(DbCommand[4])",
                    "BeginDbTransaction(DbTransaction[0].Serializable)",
                    "DbTransaction[0]:DbCommand[4].ExecuteNonQuery(CREATE [Table] common.T2;)",
                    "DbTransaction[0]:DbCommand[3].ExecuteNonQuery(DELETE FROM [common].[__VersionHistory];)",
                    @"DbTransaction[0]:DbCommand[1].ExecuteNonQuery(INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));)",
                    "DbTransaction[0].Commit",
                    "DbTransaction[0].Dispose(True)",
                    "BeginDbTransaction(DbTransaction[1].Serializable)",
                    @"DbTransaction[1]:DbCommand[2].ExecuteNonQuery(UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);)",
                    "DbTransaction[1].Commit",
                    "DbTransaction[1].Dispose(True)",
                    "DbCommand[4].Dispose(True)",
                    "DbCommand[1].Dispose(True)",
                    "DbCommand[2].Dispose(True)",
                    "DbCommand[3].Dispose(True)",
                    "ChangeState(Open => Closed)",
                    "Dispose(True)" );

            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T1",
                    "[Index] common.UIX_T1_C1A",
                    "[PrimaryKey] common.PK_T1",
                    "[Table] common.T2",
                    "[Index] common.UIX_T2_C2A",
                    "[PrimaryKey] common.PK_T2" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeNull();
            result.OldVersion.Should().Be( new Version( "0.1" ) );
            result.NewVersion.Should().Be( new Version( "0.2" ) );
            result.OriginalVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 0, 1 ).ToArray() );
            result.CommittedVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.Slice( 1 ).ToArray() );
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Create_ShouldResetChangeTrackerAttachmentBeforeEachVersion(SqlDatabaseCreateMode mode)
    {
        var isAttached = new[] { true, true, true, true };

        var sut = new SqlDatabaseFactoryMock();
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ),
                    new SqlDatabaseVersionRecord(
                        Ordinal: 2,
                        Version: new Version( "0.2" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow,
                        CommitDuration: TimeSpan.FromMilliseconds( 12 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    isAttached[0] = db.Changes.IsAttached;
                    db.Changes.Detach();
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    isAttached[1] = db.Changes.IsAttached;
                    db.Changes.Detach();
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.3" ),
                db =>
                {
                    isAttached[2] = db.Changes.IsAttached;
                    db.Changes.Detach();
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.4" ),
                db =>
                {
                    isAttached[3] = db.Changes.IsAttached;
                    db.Changes.Detach();
                } ) );

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        isAttached.Should().BeSequentiallyEqualTo( true, true, true, true );
    }

    [Fact]
    public void Create_ShouldReturnResultWithException_WhenExceptionIsThrownDuringActiveDbTransactionForVersionInCommitMode()
    {
        var exception = new Exception();
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .AddStatementListener(
                    SqlDatabaseFactoryStatementListener.Create(
                        onBefore: e =>
                        {
                            if ( e.Sql == "CREATE [Table] common.T;" )
                                throw exception;
                        },
                        onAfter: null ) ) );

        using ( new AssertionScope() )
        {
            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T",
                    "[Index] common.UIX_T_CA",
                    "[PrimaryKey] common.PK_T" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeSameAs( exception );
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeEmpty();
            result.PendingVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
        }
    }

    [Fact]
    public void
        Create_ShouldReturnResultWithException_WhenExceptionIsThrownDuringActiveDbTransactionForVersionHistoryRecordsUpdateInCommitMode()
    {
        var exception = new Exception();
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
                } ) );

        var result = sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .AddStatementListener(
                    SqlDatabaseFactoryStatementListener.Create(
                        onBefore: e =>
                        {
                            if ( e.Sql.StartsWith( "UPDATE FROM [common].[__VersionHistory]" ) )
                                throw exception;
                        },
                        onAfter: null ) ) );

        using ( new AssertionScope() )
        {
            result.Database.Schemas.Should().BeSequentiallyEqualTo( result.Database.Schemas.Default );
            result.Database.Schemas.Default.Objects.Select( o => o.ToString() )
                .Should()
                .BeEquivalentTo(
                    "[Table] common.T",
                    "[Index] common.UIX_T_CA",
                    "[PrimaryKey] common.PK_T" );

            result.Database.Version.Should().Be( result.NewVersion );
            result.Exception.Should().BeSameAs( exception );
            result.OldVersion.Should().BeSameAs( SqlDatabaseVersionHistory.InitialVersion );
            result.NewVersion.Should().Be( new Version( "0.1" ) );
            result.OriginalVersions.ToArray().Should().BeEmpty();
            result.CommittedVersions.ToArray().Should().BeSequentiallyEqualTo( history.Versions.ToArray() );
            result.PendingVersions.ToArray().Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    public void Create_ShouldNotInvokeVersionStatements_WhenNotInCommitMode(SqlDatabaseCreateMode mode)
    {
        var caughtEvents = new List<string>();

        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: true );
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( mode )
                .AddStatementListener(
                    SqlDatabaseFactoryStatementListener.Create(
                        onBefore: e =>
                            caughtEvents.Add( $"[Before] [{e.Key.Version}, {e.Key.Ordinal}] [{e.Type}]{Environment.NewLine}{e.Sql}" ),
                        onAfter: (e, _, _) =>
                            caughtEvents.Add( $"[After] [{e.Key.Version}, {e.Key.Ordinal}] [{e.Type}]{Environment.NewLine}{e.Sql}" ) ) ) );

        caughtEvents.Should()
            .BeSequentiallyEqualTo(
                @"[Before] [0.0, 1] [VersionHistory]
CREATE [Table] common.__VersionHistory;",
                @"[After] [0.0, 1] [VersionHistory]
CREATE [Table] common.__VersionHistory;",
                @"[Before] [0.0, 2] [VersionHistory]
FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;",
                @"[After] [0.0, 2] [VersionHistory]
FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;" );
    }

    [Fact]
    public void Create_ShouldInvokeVersionStatements_WhenInCommitMode()
    {
        var caughtEvents = new List<string>();

        var sut = new SqlDatabaseFactoryMock( createVersionHistoryTable: true );
        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T1" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
                } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db =>
                {
                    var table = db.Schemas.Default.Objects.CreateTable( "T2" );
                    table.Constraints.SetPrimaryKey( table.Columns.Create( "C2" ).Asc() );
                } ) );

        sut.Create(
            "DataSource=testing",
            history,
            SqlCreateDatabaseOptions.Default
                .SetMode( SqlDatabaseCreateMode.Commit )
                .AddStatementListener(
                    SqlDatabaseFactoryStatementListener.Create(
                        onBefore: e =>
                            caughtEvents.Add( $"[Before] [{e.Key.Version}, {e.Key.Ordinal}] [{e.Type}]{Environment.NewLine}{e.Sql}" ),
                        onAfter: (e, _, _) =>
                            caughtEvents.Add( $"[After] [{e.Key.Version}, {e.Key.Ordinal}] [{e.Type}]{Environment.NewLine}{e.Sql}" ) ) ) );

        caughtEvents.Should()
            .BeSequentiallyEqualTo(
                @"[Before] [0.0, 1] [VersionHistory]
CREATE [Table] common.__VersionHistory;",
                @"[After] [0.0, 1] [VersionHistory]
CREATE [Table] common.__VersionHistory;",
                @"[Before] [0.0, 2] [VersionHistory]
FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;",
                @"[After] [0.0, 2] [VersionHistory]
FROM [common].[__VersionHistory]
ORDER BY ([common].[__VersionHistory].[Ordinal] : System.Int32) ASC
SELECT
  *;",
                @"[Before] [0.2, 1] [Change]
CREATE [Table] common.T2;",
                @"[After] [0.2, 1] [Change]
CREATE [Table] common.T2;",
                @"[Before] [0.2, 2] [VersionHistory]
INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));",
                @"[After] [0.2, 2] [VersionHistory]
INSERT INTO [common].[__VersionHistory] ([common].[__VersionHistory].[Ordinal] : System.Int32, [common].[__VersionHistory].[VersionMajor] : System.Int32, [common].[__VersionHistory].[VersionMinor] : System.Int32, [common].[__VersionHistory].[VersionBuild] : Nullable<System.Int32>, [common].[__VersionHistory].[VersionRevision] : Nullable<System.Int32>, [common].[__VersionHistory].[Description] : System.String, [common].[__VersionHistory].[CommitDateUtc] : System.DateTime, [common].[__VersionHistory].[CommitDurationInTicks] : System.Int64)
VALUES
((@Ordinal : System.Int32), (@VersionMajor : System.Int32), (@VersionMinor : System.Int32), (@VersionBuild : Nullable<System.Int32>), (@VersionRevision : Nullable<System.Int32>), (@Description : System.String), (@CommitDateUtc : System.DateTime), (""0"" : System.Int32));",
                @"[Before] [0.0, 3] [VersionHistory]
UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);",
                @"[After] [0.0, 3] [VersionHistory]
UPDATE FROM [common].[__VersionHistory]
AND WHERE ([common].[__VersionHistory].[Ordinal] : System.Int32) == (@Ordinal : System.Int32)
SET
  ([common].[__VersionHistory].[CommitDurationInTicks] : System.Int64) = (@CommitDurationInTicks : System.Int64);" );
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    [InlineData( SqlDatabaseCreateMode.Commit )]
    public void Create_ShouldInvokeInitialAndCommittedVersionsConnectionChangeCallbacks(SqlDatabaseCreateMode mode)
    {
        var caughtEvents = new List<string>();

        var sut = new SqlDatabaseFactoryMock();
        sut.ConnectionChangeCallbacks.Add(
            e => caughtEvents.Add( $"[Initial] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );

        sut.Connection.EnqueueResultSets(
            new[]
            {
                CreateVersionResultSet(
                    new SqlDatabaseVersionRecord(
                        Ordinal: 1,
                        Version: new Version( "0.1" ),
                        Description: string.Empty,
                        CommitDateUtc: DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
                        CommitDuration: TimeSpan.FromMilliseconds( 10 ) ) )
            } );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    db.AddConnectionChangeCallback(
                        e => caughtEvents.Add( $"[0.1] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );
                } ) );

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        caughtEvents.Should()
            .BeSequentiallyEqualTo(
                "[Initial] Closed => Open",
                "[0.1] Closed => Open",
                "[Initial] Open => Closed",
                "[0.1] Open => Closed" );
    }

    [Theory]
    [InlineData( SqlDatabaseCreateMode.NoChanges )]
    [InlineData( SqlDatabaseCreateMode.DryRun )]
    public void Create_ShouldNotInvokeUncommittedVersionsConnectionChangeCallbacks_WhenNotInCommitMode(SqlDatabaseCreateMode mode)
    {
        var caughtEvents = new List<string>();

        var sut = new SqlDatabaseFactoryMock();
        sut.ConnectionChangeCallbacks.Add(
            e => caughtEvents.Add( $"[Initial] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    db.AddConnectionChangeCallback(
                        e => caughtEvents.Add( $"[0.1] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );
                } ) );

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( mode ) );

        caughtEvents.Should().BeSequentiallyEqualTo( "[Initial] Closed => Open", "[Initial] Open => Closed" );
    }

    [Fact]
    public void Create_ShouldInvokeUncommittedVersionsConnectionChangeCallbacks_WhenInCommitMode()
    {
        var caughtEvents = new List<string>();

        var sut = new SqlDatabaseFactoryMock();
        sut.ConnectionChangeCallbacks.Add(
            e => caughtEvents.Add( $"[Initial] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );

        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db =>
                {
                    db.AddConnectionChangeCallback(
                        e => caughtEvents.Add( $"[0.1] {e.StateChange.OriginalState} => {e.StateChange.CurrentState}" ) );
                } ) );

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

        caughtEvents.Should()
            .BeSequentiallyEqualTo(
                "[Initial] Closed => Open",
                "[0.1] Closed => Open",
                "[Initial] Open => Closed",
                "[0.1] Open => Closed" );
    }

    [Fact]
    public void DatabaseBuilder_UserData_ShouldBePropagatedBetweenVersions()
    {
        var data = new object();
        object? caughtData = null;
        var sut = new SqlDatabaseFactoryMock();
        var history = new SqlDatabaseVersionHistory(
            SqlDatabaseVersion.Create(
                new Version( "0.1" ),
                db => { db.UserData = data; } ),
            SqlDatabaseVersion.Create(
                new Version( "0.2" ),
                db => { caughtData = db.UserData; } ) );

        sut.Create( "DataSource=testing", history, SqlCreateDatabaseOptions.Default.SetMode( SqlDatabaseCreateMode.Commit ) );

        caughtData.Should().BeSameAs( data );
    }

    [Pure]
    private static ResultSet CreateVersionResultSet(params SqlDatabaseVersionRecord[] records)
    {
        var fields = new[]
        {
            "Ordinal",
            "VersionMajor",
            "VersionMinor",
            "VersionBuild",
            "VersionRevision",
            "Description",
            "CommitDateUtc",
            "CommitDurationInTicks"
        };

        var rows = new object?[records.Length][];
        for ( var i = 0; i < rows.Length; ++i )
        {
            var r = records[i];
            var row = new object?[fields.Length];
            row[0] = r.Ordinal;
            row[1] = r.Version.Major;
            row[2] = r.Version.Minor;
            row[3] = r.Version.Build >= 0 ? r.Version.Build : null;
            row[4] = r.Version.Revision >= 0 ? r.Version.Revision : null;
            row[5] = r.Description;
            row[6] = r.CommitDateUtc;
            row[7] = r.CommitDuration.Ticks;
            rows[i] = row;
        }

        return new ResultSet( fields, rows );
    }
}
