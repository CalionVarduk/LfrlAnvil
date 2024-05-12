using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object used for managing application of versions to the database.
/// </summary>
public class SqlDatabaseCommitVersionsContext : IDisposable
{
    private readonly Dictionary<string, SqlColumnTypeDefinition> _versionHistoryColumnTypes;
    private DbCommand? _insertVersionHistoryRecordCommand;
    private DbCommand? _updateVersionHistoryRecordCommand;
    private DbCommand? _deleteAllVersionHistoryRecordsCommand;

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseCommitVersionsContext"/> instance.
    /// </summary>
    protected internal SqlDatabaseCommitVersionsContext()
    {
        _versionHistoryColumnTypes = new Dictionary<string, SqlColumnTypeDefinition>( comparer: SqlHelpers.NameComparer );
        _insertVersionHistoryRecordCommand = null;
        _updateVersionHistoryRecordCommand = null;
        _deleteAllVersionHistoryRecordsCommand = null;
    }

    internal bool PersistLastVersionHistoryRecordOnly => _deleteAllVersionHistoryRecordsCommand is not null;

    /// <inhertidoc />
    public virtual void Dispose()
    {
        _insertVersionHistoryRecordCommand?.Dispose();
        _updateVersionHistoryRecordCommand?.Dispose();
        _deleteAllVersionHistoryRecordsCommand?.Dispose();
    }

    /// <summary>
    /// Callback invoked just before the processing of all versions to apply starts.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    protected internal virtual void OnBeforeVersionRangeApplication(
        SqlDatabaseBuilder builder,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor) { }

    /// <summary>
    /// Callback invoked just after the processing of all versions to apply has finished.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    protected internal virtual void OnAfterVersionRangeApplication(
        SqlDatabaseBuilder builder,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor) { }

    /// <summary>
    /// Callback invoked just before a DB transaction for a single applied version is created.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="key">
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last SQL statement of the applied version.
    /// </param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    /// <returns>
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last ran SQL statement of the applied version.
    /// </returns>
    protected internal virtual SqlDatabaseFactoryStatementKey OnBeforeVersionTransaction(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        return key;
    }

    /// <summary>
    /// Callback invoked just after a DB transaction for a single applied version is committed.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="key">
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last SQL statement of the applied version.
    /// </param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    protected internal virtual void OnAfterVersionTransaction(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor) { }

    /// <summary>
    /// Callback invoked just before a range of SQL statements prepared for a single applied version is executed.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="key">
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last SQL statement of the applied version.
    /// </param>
    /// <param name="command">DB command that will execute all statements.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    /// <returns>
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last ran SQL statement of the applied version.
    /// </returns>
    protected internal virtual SqlDatabaseFactoryStatementKey OnBeforeVersionActionRangeExecution(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbCommand command,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        return key;
    }

    /// <summary>
    /// Callback invoked just after a range of SQL statements prepared for a single applied version has been executed.
    /// </summary>
    /// <param name="builder">SQL database builder.</param>
    /// <param name="key">
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last SQL statement of the applied version.
    /// </param>
    /// <param name="command">DB command that executed all statements.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    /// <returns>
    /// <see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies the last ran SQL statement of the applied version.
    /// </returns>
    protected internal virtual SqlDatabaseFactoryStatementKey OnAfterVersionActionRangeExecution(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbCommand command,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        return key;
    }

    /// <summary>
    /// Prepares a DB command responsible for inserting a single record into the version history table.
    /// </summary>
    /// <param name="command">DB command to prepare.</param>
    /// <param name="table">SQL table builder for the version history table.</param>
    /// <param name="nodeInterpreter">SQL node interpreter instance.</param>
    protected virtual void PrepareInsertVersionHistoryRecordCommand(
        DbCommand command,
        SqlTableBuilder table,
        SqlNodeInterpreter nodeInterpreter)
    {
        var columns = table.Columns;
        var cOrdinal = columns.Get( SqlHelpers.VersionHistoryOrdinalName );
        var cVersionMajor = columns.Get( SqlHelpers.VersionHistoryVersionMajorName );
        var cVersionMinor = columns.Get( SqlHelpers.VersionHistoryVersionMinorName );
        var cVersionBuild = columns.Get( SqlHelpers.VersionHistoryVersionBuildName );
        var cVersionRevision = columns.Get( SqlHelpers.VersionHistoryVersionRevisionName );
        var cDescription = columns.Get( SqlHelpers.VersionHistoryDescriptionName );
        var cCommitDateUtc = columns.Get( SqlHelpers.VersionHistoryCommitDateUtcName );
        var cCommitDurationInTicks = columns.Get( SqlHelpers.VersionHistoryCommitDurationInTicksName );

        var pOrdinal = SqlNode.Parameter( cOrdinal.Name, cOrdinal.Node.Type );
        var pVersionMajor = SqlNode.Parameter( cVersionMajor.Name, cVersionMajor.Node.Type );
        var pVersionMinor = SqlNode.Parameter( cVersionMinor.Name, cVersionMinor.Node.Type );
        var pVersionBuild = SqlNode.Parameter( cVersionBuild.Name, cVersionBuild.Node.Type );
        var pVersionRevision = SqlNode.Parameter( cVersionRevision.Name, cVersionRevision.Node.Type );
        var pDescription = SqlNode.Parameter( cDescription.Name, cDescription.Node.Type );
        var pCommitDateUtc = SqlNode.Parameter( cCommitDateUtc.Name, cCommitDateUtc.Node.Type );

        var insertInto = SqlNode.Values(
                pOrdinal,
                pVersionMajor,
                pVersionMinor,
                pVersionBuild,
                pVersionRevision,
                pDescription,
                pCommitDateUtc,
                SqlNode.Literal( 0 ) )
            .ToInsertInto(
                table.Node,
                cOrdinal.Node,
                cVersionMajor.Node,
                cVersionMinor.Node,
                cVersionBuild.Node,
                cVersionRevision.Node,
                cDescription.Node,
                cCommitDateUtc.Node,
                cCommitDurationInTicks.Node );

        nodeInterpreter.VisitInsertInto( insertInto );

        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        AddCommandParameter( command, cOrdinal );
        AddCommandParameter( command, cVersionMajor );
        AddCommandParameter( command, cVersionMinor );
        AddCommandParameter( command, cVersionBuild );
        AddCommandParameter( command, cVersionRevision );
        AddCommandParameter( command, cDescription );
        AddCommandParameter( command, cCommitDateUtc );
    }

    /// <summary>
    /// Prepares a DB command responsible for removing all records from the version history table.
    /// </summary>
    /// <param name="command">DB command to prepare.</param>
    /// <param name="table">SQL table builder for the version history table.</param>
    /// <param name="nodeInterpreter">SQL node interpreter instance.</param>
    protected virtual void PrepareDeleteAllVersionHistoryRecordsCommand(
        DbCommand command,
        SqlTableBuilder table,
        SqlNodeInterpreter nodeInterpreter)
    {
        var deleteFrom = table.Node.ToDataSource().ToDeleteFrom();
        nodeInterpreter.VisitDeleteFrom( deleteFrom );
        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
    }

    /// <summary>
    /// Prepares a DB command responsible for updating a single record in the version history table.
    /// </summary>
    /// <param name="command">DB command to prepare.</param>
    /// <param name="table">SQL table builder for the version history table.</param>
    /// <param name="nodeInterpreter">SQL node interpreter instance.</param>
    protected virtual void PrepareUpdateVersionHistoryRecordCommand(
        DbCommand command,
        SqlTableBuilder table,
        SqlNodeInterpreter nodeInterpreter)
    {
        var columns = table.Columns;
        var cOrdinal = columns.Get( SqlHelpers.VersionHistoryOrdinalName );
        var cCommitDurationInTicks = columns.Get( SqlHelpers.VersionHistoryCommitDurationInTicksName );

        var pOrdinal = SqlNode.Parameter( cOrdinal.Name, cOrdinal.Node.Type );
        var pCommitDurationInTicks = SqlNode.Parameter( cCommitDurationInTicks.Name, cCommitDurationInTicks.Node.Type );

        var update = table.Node
            .ToDataSource()
            .AndWhere( cOrdinal.Node == pOrdinal )
            .ToUpdate( cCommitDurationInTicks.Node.Assign( pCommitDurationInTicks ) );

        nodeInterpreter.VisitUpdate( update );

        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        AddCommandParameter( command, cOrdinal );
        AddCommandParameter( command, cCommitDurationInTicks );
    }

    /// <summary>
    /// Prepares values of DB parameters of a command responsible for updating a single record in the version history table.
    /// </summary>
    /// <param name="parameters">Collection of DB parameters.</param>
    /// <param name="ordinal"><see cref="SqlDatabaseVersionRecord.Ordinal"/> of the record to update.</param>
    /// <param name="elapsedTime">Specifies the time it took to fully apply the version to the database.</param>
    protected virtual void SetUpdateVersionHistoryRecordCommandParameters(
        DbParameterCollection parameters,
        int ordinal,
        TimeSpan elapsedTime)
    {
        Assume.Equals( parameters.Count, 2 );
        var pOrdinal = parameters[0];
        var pCommitDurationInTicks = parameters[1];

        var pOrdinalType = GetVersionHistoryColumnType( pOrdinal.ParameterName );
        var pCommitDurationInTicksType = GetVersionHistoryColumnType( pCommitDurationInTicks.ParameterName );

        pOrdinal.Value = pOrdinalType.TryToParameterValue( ordinal );
        pCommitDurationInTicks.Value = pCommitDurationInTicksType.TryToParameterValue( elapsedTime.Ticks );
    }

    /// <summary>
    /// Prepares values of DB parameters of a command responsible for inserting a single record into the version history table.
    /// </summary>
    /// <param name="parameters">Collection of DB parameters.</param>
    /// <param name="ordinal"><see cref="SqlDatabaseVersionRecord.Ordinal"/> of the record to insert.</param>
    /// <param name="version">Identifier of the applied version.</param>
    /// <param name="description">Description of the applied version.</param>
    protected virtual void SetInsertVersionHistoryRecordCommandParameters(
        DbParameterCollection parameters,
        int ordinal,
        Version version,
        string description)
    {
        Assume.Equals( parameters.Count, 7 );
        var pOrdinal = parameters[0];
        var pVersionMajor = parameters[1];
        var pVersionMinor = parameters[2];
        var pVersionBuild = parameters[3];
        var pVersionRevision = parameters[4];
        var pDescription = parameters[5];
        var pCommitDateUtc = parameters[6];

        var pOrdinalType = GetVersionHistoryColumnType( pOrdinal.ParameterName );
        var pVersionMajorType = GetVersionHistoryColumnType( pVersionMajor.ParameterName );
        var pVersionMinorType = GetVersionHistoryColumnType( pVersionMinor.ParameterName );
        var pVersionBuildType = GetVersionHistoryColumnType( pVersionBuild.ParameterName );
        var pVersionRevisionType = GetVersionHistoryColumnType( pVersionRevision.ParameterName );
        var pDescriptionType = GetVersionHistoryColumnType( pDescription.ParameterName );
        var pCommitDateUtcType = GetVersionHistoryColumnType( pCommitDateUtc.ParameterName );

        pOrdinal.Value = pOrdinalType.TryToParameterValue( ordinal );
        pVersionMajor.Value = pVersionMajorType.TryToParameterValue( version.Major );
        pVersionMinor.Value = pVersionMinorType.TryToParameterValue( version.Minor );
        pVersionBuild.Value = version.Build >= 0 ? pVersionBuildType.TryToParameterValue( version.Build ) : DBNull.Value;
        pVersionRevision.Value = version.Revision >= 0 ? pVersionRevisionType.TryToParameterValue( version.Revision ) : DBNull.Value;
        pDescription.Value = pDescriptionType.TryToParameterValue( description );
        pCommitDateUtc.Value = pCommitDateUtcType.TryToParameterValue( DateTime.UtcNow );
    }

    /// <summary>
    /// Returns an <see cref="SqlColumnTypeDefinition"/> of a version history table's column with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the column.</param>
    /// <returns>
    /// <see cref="SqlColumnTypeDefinition"/> of a version history table's column with the provided <paramref name="name"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">When version history table column with the provided name does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected SqlColumnTypeDefinition GetVersionHistoryColumnType(string name)
    {
        return _versionHistoryColumnTypes[name];
    }

    /// <summary>
    /// Adds a DB parameter to the provided <paramref name="command"/> based on the <paramref name="column"/> definition.
    /// </summary>
    /// <param name="command">DB command to add a DB parameter to.</param>
    /// <param name="column">SQL column builder to use for initializing the DB parameter.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddCommandParameter(DbCommand command, SqlColumnBuilder column)
    {
        var parameter = command.CreateParameter();
        column.TypeDefinition.SetParameterInfo( parameter, column.IsNullable );
        parameter.ParameterName = column.Name;
        command.Parameters.Add( parameter );
    }

    internal void InitializeVersionHistoryCommands(
        DbConnection connection,
        SqlTableBuilder table,
        SqlNodeInterpreter nodeInterpreter,
        SqlDatabaseVersionHistoryMode persistenceMode)
    {
        Assume.IsNull( _insertVersionHistoryRecordCommand );
        Assume.IsNull( _updateVersionHistoryRecordCommand );
        Assume.IsNull( _deleteAllVersionHistoryRecordsCommand );
        Assume.IsEmpty( _versionHistoryColumnTypes );

        _versionHistoryColumnTypes.EnsureCapacity( table.Columns.Count );
        foreach ( var column in table.Columns )
            _versionHistoryColumnTypes.Add( column.Name, column.TypeDefinition );

        var command = connection.CreateCommand();
        try
        {
            PrepareInsertVersionHistoryRecordCommand( command, table, nodeInterpreter );
            _insertVersionHistoryRecordCommand = command;
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }
        finally
        {
            nodeInterpreter.Context.Clear();
        }

        command = connection.CreateCommand();
        try
        {
            PrepareUpdateVersionHistoryRecordCommand( command, table, nodeInterpreter );
            _updateVersionHistoryRecordCommand = command;
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }
        finally
        {
            nodeInterpreter.Context.Clear();
        }

        if ( persistenceMode == SqlDatabaseVersionHistoryMode.LastRecordOnly )
        {
            command = connection.CreateCommand();
            try
            {
                PrepareDeleteAllVersionHistoryRecordsCommand( command, table, nodeInterpreter );
                _deleteAllVersionHistoryRecordsCommand = command;
                command.Prepare();
            }
            catch
            {
                command.Dispose();
                throw;
            }
            finally
            {
                nodeInterpreter.Context.Clear();
            }
        }
    }

    internal void InsertVersionHistoryRecord(
        DbTransaction transaction,
        int ordinal,
        Version version,
        string description,
        SqlDatabaseFactoryStatementKey key,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        Assume.IsNotNull( _insertVersionHistoryRecordCommand );
        Assume.IsNull( _insertVersionHistoryRecordCommand.Transaction );

        try
        {
            _insertVersionHistoryRecordCommand.Transaction = transaction;
            SetInsertVersionHistoryRecordCommandParameters( _insertVersionHistoryRecordCommand.Parameters, ordinal, version, description );
            executor.Execute(
                _insertVersionHistoryRecordCommand,
                key,
                SqlDatabaseFactoryStatementType.VersionHistory,
                SqlHelpers.ExecuteNonQueryDelegate );
        }
        finally
        {
            _insertVersionHistoryRecordCommand.Transaction = null;
        }
    }

    internal void DeleteAllVersionHistoryRecords(
        DbTransaction transaction,
        SqlDatabaseFactoryStatementKey key,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        Assume.IsNotNull( _deleteAllVersionHistoryRecordsCommand );
        Assume.IsNull( _deleteAllVersionHistoryRecordsCommand.Transaction );

        try
        {
            _deleteAllVersionHistoryRecordsCommand.Transaction = transaction;
            executor.Execute(
                _deleteAllVersionHistoryRecordsCommand,
                key,
                SqlDatabaseFactoryStatementType.VersionHistory,
                SqlHelpers.ExecuteNonQueryDelegate );
        }
        finally
        {
            _deleteAllVersionHistoryRecordsCommand.Transaction = null;
        }
    }

    internal void UpdateVersionHistoryRecords(
        ReadOnlySpan<SqlDatabaseVersionElapsedTime> elapsedTimes,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        if ( elapsedTimes.Length == 0 )
            return;

        Assume.IsNotNull( _updateVersionHistoryRecordCommand?.Connection );
        Assume.IsNull( _updateVersionHistoryRecordCommand.Transaction );

        try
        {
            using var transaction = _updateVersionHistoryRecordCommand.Connection.BeginTransaction( IsolationLevel.Serializable );
            _updateVersionHistoryRecordCommand.Transaction = transaction;

            foreach ( var (ordinal, elapsedTime) in elapsedTimes )
            {
                SetUpdateVersionHistoryRecordCommandParameters( _updateVersionHistoryRecordCommand.Parameters, ordinal, elapsedTime );
                executor.ExecuteForVersionHistory( _updateVersionHistoryRecordCommand, SqlHelpers.ExecuteNonQueryDelegate );
            }

            transaction.Commit();
        }
        finally
        {
            _updateVersionHistoryRecordCommand.Transaction = null;
        }
    }
}
