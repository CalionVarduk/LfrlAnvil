using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseChangeTracker
{
    private const byte IsDetachedBit = 1 << 0;
    private const byte ModeMask = (1 << 8) - 2;

    private readonly List<string> _pendingStatements;
    private readonly List<SqliteDatabasePropertyChange> _ongoing;
    private readonly StringBuilder _ongoingStatement;
    private readonly Dictionary<ulong, SqliteTableBuilder> _modifiedTables;
    private SqliteAlterTableBuffer? _alterTableBuffer;
    private byte _mode;

    internal SqliteDatabaseChangeTracker()
    {
        _pendingStatements = new List<string>();
        _ongoing = new List<SqliteDatabasePropertyChange>();
        _ongoingStatement = new StringBuilder();
        _modifiedTables = new Dictionary<ulong, SqliteTableBuilder>();
        _alterTableBuffer = null;
        CurrentTable = null;
        _mode = (byte)SqlDatabaseCreateMode.DryRun << 1;
    }

    internal SqliteTableBuilder? CurrentTable { get; private set; }
    internal SqlDatabaseCreateMode Mode => (SqlDatabaseCreateMode)((_mode & ModeMask) >> 1);
    internal bool IsAttached => (_mode & IsDetachedBit) == 0;
    internal bool IsPreparingStatements => _mode > 0 && IsAttached;
    internal IEnumerable<string> ModifiedTableNames => _modifiedTables.Values.Select( t => t.FullName );

    internal ReadOnlySpan<string> GetPendingStatements()
    {
        if ( _ongoing.Count > 0 )
            CompletePendingStatement();

        return CollectionsMarshal.AsSpan( _pendingStatements );
    }

    internal void AddRawStatement(string statement)
    {
        if ( _ongoing.Count > 0 )
            CompletePendingStatement();

        if ( IsPreparingStatements )
            _pendingStatements.Add( statement );
    }

    internal void SetAttachedMode(bool enabled)
    {
        if ( IsAttached == enabled )
            return;

        if ( enabled )
        {
            _mode &= ModeMask;
            return;
        }

        if ( _ongoing.Count > 0 )
            CompletePendingStatement();

        _mode |= IsDetachedBit;
    }

    internal void SetMode(SqlDatabaseCreateMode mode)
    {
        Assume.IsDefined( mode, nameof( mode ) );
        _mode = (byte)((int)mode << 1);
    }

    internal void ClearStatements()
    {
        CurrentTable = null;
        _modifiedTables.Clear();
        _ongoing.Clear();
        _pendingStatements.Clear();
    }

    internal void ObjectCreated(SqliteTableBuilder table, SqliteObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Exists,
            SqliteObjectStatus.Created,
            Boxed.False,
            Boxed.True );

        AddChange( table, change );
    }

    internal void ObjectRemoved(SqliteTableBuilder table, SqliteObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Exists,
            SqliteObjectStatus.Removed,
            Boxed.True,
            Boxed.False );

        AddChange( table, change );
    }

    internal void NameUpdated(SqliteTableBuilder table, SqliteObjectBuilder obj, string oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Name,
            SqliteObjectStatus.Modified,
            oldValue,
            obj.Name );

        AddChange( table, change );
    }

    internal void FullNameUpdated(SqliteTableBuilder table, SqliteObjectBuilder obj, string oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Name,
            SqliteObjectStatus.Modified,
            oldValue,
            obj.FullName );

        AddChange( table, change );
    }

    internal void TypeDefinitionUpdated(SqliteColumnBuilder column, SqliteColumnTypeDefinition oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            column,
            SqliteObjectChangeDescriptor.DataType,
            SqliteObjectStatus.Modified,
            oldValue.DbType,
            column.TypeDefinition.DbType );

        AddChange( column.Table, change );
    }

    internal void IsNullableUpdated(SqliteColumnBuilder column)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            column,
            SqliteObjectChangeDescriptor.IsNullable,
            SqliteObjectStatus.Modified,
            Boxed.GetBool( ! column.IsNullable ),
            Boxed.GetBool( column.IsNullable ) );

        AddChange( column.Table, change );
    }

    internal void DefaultValueUpdated(SqliteColumnBuilder column, object? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            column,
            SqliteObjectChangeDescriptor.DefaultValue,
            SqliteObjectStatus.Modified,
            oldValue,
            column.DefaultValue );

        AddChange( column.Table, change );
    }

    internal void OnDeleteBehaviorUpdated(SqliteForeignKeyBuilder foreignKey, ReferenceBehavior oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            foreignKey,
            SqliteObjectChangeDescriptor.OnDeleteBehavior,
            SqliteObjectStatus.Modified,
            oldValue,
            foreignKey.OnDeleteBehavior );

        AddChange( foreignKey.Index.Table, change );
    }

    internal void OnUpdateBehaviorUpdated(SqliteForeignKeyBuilder foreignKey, ReferenceBehavior oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            foreignKey,
            SqliteObjectChangeDescriptor.OnUpdateBehavior,
            SqliteObjectStatus.Modified,
            oldValue,
            foreignKey.OnUpdateBehavior );

        AddChange( foreignKey.Index.Table, change );
    }

    internal void IsUniqueUpdated(SqliteIndexBuilder index)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            index,
            SqliteObjectChangeDescriptor.IsUnique,
            SqliteObjectStatus.Modified,
            Boxed.GetBool( ! index.IsUnique ),
            Boxed.GetBool( index.IsUnique ) );

        AddChange( index.Table, change );
    }

    internal void PrimaryKeyUpdated(SqliteIndexBuilder index, SqlitePrimaryKeyBuilder? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            index,
            SqliteObjectChangeDescriptor.PrimaryKey,
            SqliteObjectStatus.Modified,
            oldValue,
            index.PrimaryKey );

        AddChange( index.Table, change );
    }

    internal void PrimaryKeyUpdated(SqliteTableBuilder table, SqlitePrimaryKeyBuilder? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            table,
            SqliteObjectChangeDescriptor.PrimaryKey,
            SqliteObjectStatus.Modified,
            oldValue,
            table.PrimaryKey );

        AddChange( table, change );
    }

    internal void ReconstructionRequested(SqliteTableBuilder table)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            table,
            SqliteObjectChangeDescriptor.Reconstruct,
            SqliteObjectStatus.Modified,
            Boxed.False,
            Boxed.True );

        AddChange( table, change );
    }

    private void AddChange(SqliteTableBuilder table, SqliteDatabasePropertyChange change)
    {
        Assume.Equals( IsPreparingStatements, true, nameof( IsPreparingStatements ) );

        if ( ! ReferenceEquals( CurrentTable, table ) )
        {
            if ( CurrentTable is not null )
                CompletePendingStatement();

            CurrentTable = table;
        }

        _ongoing.Add( change );
    }

    private void CompletePendingStatement()
    {
        Assume.IsNotNull( CurrentTable, nameof( CurrentTable ) );
        Assume.ContainsAtLeast( _ongoing, 1, nameof( _ongoing ) );

        var changes = CollectionsMarshal.AsSpan( _ongoing );
        var firstChange = changes[0];
        var lastChange = changes[^1];

        if ( firstChange.Status == SqliteObjectStatus.Created && ReferenceEquals( firstChange.Object, CurrentTable ) )
        {
            if ( lastChange.Status != SqliteObjectStatus.Removed || ! ReferenceEquals( lastChange.Object, CurrentTable ) )
                CompletePendingCreateTableStatement();
        }
        else if ( lastChange.Status == SqliteObjectStatus.Removed && ReferenceEquals( lastChange.Object, CurrentTable ) )
            CompletePendingDropTableStatement();
        else
            CompletePendingAlterTableStatement( changes );

        _ongoing.Clear();
        if ( _ongoingStatement.Length > 0 )
        {
            if ( CurrentTable.IsRemoved )
                _modifiedTables.Remove( CurrentTable.Id );
            else
                _modifiedTables.TryAdd( CurrentTable.Id, CurrentTable );

            _pendingStatements.Add( _ongoingStatement.AppendLine().ToString() );
            _ongoingStatement.Clear();
        }

        CurrentTable = null;
    }

    private void CompletePendingCreateTableStatement()
    {
        Assume.IsNotNull( CurrentTable, nameof( CurrentTable ) );
        ValidateTable( CurrentTable );

        AppendCreateTable( _ongoingStatement, CurrentTable );
        AppendCreateIndexCollection( _ongoingStatement, CurrentTable.Indexes );
    }

    private void CompletePendingDropTableStatement()
    {
        Assume.IsNotNull( CurrentTable, nameof( CurrentTable ) );
        _ongoingStatement.AppendDropTable( CurrentTable.FullName );
    }

    private void CompletePendingAlterTableStatement(ReadOnlySpan<SqliteDatabasePropertyChange> changes)
    {
        Assume.IsNotNull( CurrentTable, nameof( CurrentTable ) );

        _alterTableBuffer ??= new SqliteAlterTableBuffer();
        var (hasChanged, requiresReconstruction, isTableRenamed) = _alterTableBuffer.ParseChanges( CurrentTable, changes );

        if ( hasChanged )
        {
            ValidateTable( CurrentTable );

            if ( isTableRenamed )
            {
                var oldName = _alterTableBuffer.TryGetOldName( CurrentTable.Id );
                Assume.IsNotNull( oldName, nameof( oldName ) );
                _ongoingStatement.AppendRenameTable( oldName, CurrentTable.FullName );
            }

            if ( requiresReconstruction )
                AppendReconstructTable( _ongoingStatement, CurrentTable, _alterTableBuffer );
            else
                AppendAlterTableWithoutReconstruction( _ongoingStatement, CurrentTable, _alterTableBuffer );
        }

        _alterTableBuffer.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(SqliteTableBuilder table)
    {
        if ( table.PrimaryKey is null )
            ExceptionThrower.Throw( new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }

    private static void AppendCreateTable(StringBuilder builder, SqliteTableBuilder table, string? temporaryName = null)
    {
        Assume.IsNotNull( table.PrimaryKey, nameof( table.PrimaryKey ) );

        var fullTableName = temporaryName ?? table.FullName;
        builder.AppendCreateTableBegin( fullTableName );

        foreach ( var column in table.Columns )
        {
            builder
                .AppendIndentation()
                .AppendColumnDefinition( column.Name, column.TypeDefinition, column.IsNullable, column.DefaultValue )
                .AppendElementSeparator()
                .AppendLine();
        }

        builder
            .AppendIndentation()
            .AppendPrimaryKeyDefinition( table.PrimaryKey.FullName )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        foreach ( var c in table.PrimaryKey.Index.Columns )
            builder.AppendIndexedColumn( c.Column.Name, c.Ordering );

        builder.AppendElementsEnd( trimCount: 2 );

        foreach ( var foreignKey in table.ForeignKeys )
        {
            builder
                .AppendElementSeparator()
                .AppendLine()
                .AppendIndentation()
                .AppendForeignKeyDefinition( foreignKey.FullName )
                .AppendTokenSeparator()
                .AppendElementsBegin();

            foreach ( var c in foreignKey.Index.Columns )
                builder.AppendNamedElement( c.Column.Name );

            builder
                .AppendElementsEnd( trimCount: 2 )
                .AppendTokenSeparator()
                .AppendForeignKeyReferenceDefinition( foreignKey.ReferencedIndex.Table.FullName )
                .AppendTokenSeparator()
                .AppendElementsBegin();

            foreach ( var c in foreignKey.ReferencedIndex.Columns )
                builder.AppendNamedElement( c.Column.Name );

            builder
                .AppendElementsEnd( trimCount: 2 )
                .AppendTokenSeparator()
                .AppendForeignKeyBehaviors( foreignKey.OnDeleteBehavior, foreignKey.OnUpdateBehavior );
        }

        builder.AppendCreateTableEnd();
    }

    private static void AppendReconstructTable(StringBuilder builder, SqliteTableBuilder table, SqliteAlterTableBuffer buffer)
    {
        if ( builder.Length > 0 )
            builder.AppendLine().AppendLine();

        var droppedIndexCount = buffer.DroppedIndexNames.Count;
        foreach ( var ix in buffer.DroppedIndexNames )
            builder.AppendDropIndex( ix ).AppendLine();

        foreach ( var ix in table.Indexes )
        {
            if ( ix.PrimaryKey is not null || buffer.CreatedIndexes.ContainsKey( ix.Id ) )
                continue;

            var ixName = buffer.TryGetOldName( ix.Id ) ?? ix.FullName;
            builder.AppendDropIndex( ixName ).AppendLine();
            ++droppedIndexCount;
        }

        if ( droppedIndexCount > 0 )
            builder.AppendLine();

        foreach ( var column in buffer.CreatedColumns.Values )
        {
            if ( column.DefaultValue is null && ! column.IsNullable )
                column.UpdateDefaultValueBasedOnDataType();
        }

        var temporaryTableName = CreateTemporaryName( table.FullName );
        AppendCreateTable( builder, table, temporaryTableName );

        builder.AppendLine().AppendLine().AppendInsertIntoBegin( temporaryTableName );
        foreach ( var column in table.Columns )
            builder.AppendNamedElement( column.Name );

        builder.AppendElementsEnd( trimCount: 2 ).AppendLine().AppendSelect().AppendTokenSeparator();
        foreach ( var column in table.Columns )
        {
            if ( buffer.CreatedColumns.ContainsKey( column.Id ) )
                builder.AppendDefaultValue( column.TypeDefinition, column.DefaultValue );
            else if ( ! buffer.Objects.ContainsKey( column.Id ) )
                builder.AppendName( column.Name );
            else
            {
                var oldName = buffer.TryGetOldName( column.Id ) ?? column.Name;
                var oldIsNullable = buffer.TryGetOldIsNullable( column.Id ) ?? column.IsNullable;
                var oldDataType = buffer.TryGetOldDataType( column.Id ) ?? column.TypeDefinition.DbType;

                if ( oldIsNullable && ! column.IsNullable )
                {
                    builder.AppendCoalesceBegin();

                    if ( oldDataType == column.TypeDefinition.DbType )
                        builder.AppendName( oldName );
                    else
                        builder.AppendCastAs( oldName, column.TypeDefinition.DbType.Name );

                    builder
                        .AppendElementSeparator()
                        .AppendTokenSeparator()
                        .AppendDefaultValue( column.TypeDefinition, column.DefaultValue ?? column.TypeDefinition.DefaultValue )
                        .AppendElementsEnd();
                }
                else if ( oldDataType == column.TypeDefinition.DbType )
                    builder.AppendName( oldName );
                else
                    builder.AppendCastAs( oldName, column.TypeDefinition.DbType.Name );
            }

            builder.AppendElementSeparator().AppendTokenSeparator();
        }

        builder.Length -= 2;
        builder.AppendLine().AppendFrom( table.FullName ).AppendCommandEnd();

        builder.AppendLine().AppendLine().AppendDropTable( table.FullName );
        builder.AppendLine().AppendLine().AppendRenameTable( temporaryTableName, table.FullName );
        AppendCreateIndexCollection( builder, table.Indexes );
    }

    private static void AppendAlterTableWithoutReconstruction(
        StringBuilder builder,
        SqliteTableBuilder table,
        SqliteAlterTableBuffer buffer)
    {
        if ( buffer.DroppedIndexNames.Count > 0 )
        {
            if ( builder.Length > 0 )
                builder.AppendLine().AppendLine();

            foreach ( var ix in buffer.DroppedIndexNames )
                builder.AppendDropIndex( ix ).AppendLine();

            builder.RemoveLastNewLine();
        }

        if ( buffer.DroppedColumnsByName.Count > 0 )
        {
            if ( builder.Length > 0 )
                builder.AppendLine().AppendLine();

            foreach ( var column in buffer.DroppedColumnsByName.Keys )
                builder.AppendDropColumn( table.FullName, column ).AppendLine();

            builder.RemoveLastNewLine();
        }

        if ( buffer.ColumnRenames.Count > 0 )
        {
            if ( builder.Length > 0 )
                builder.AppendLine().AppendLine();

            foreach ( var (id, rename) in buffer.ColumnRenames )
            {
                if ( ! rename.IsPending )
                    continue;

                ref var renameRef = ref CollectionsMarshal.GetValueRefOrNullRef( buffer.ColumnRenames, id );
                Assume.False( Unsafe.IsNullRef( ref renameRef ), nameof( renameRef ) + " cannot be a null ref." );

                renameRef = new SqliteAlterTableBuffer.ColumnRename( rename.OldName, rename.NewName, IsPending: false );
                HandleColumnRename( builder, table, buffer, id, ref renameRef );
            }

            builder.RemoveLastNewLine();
        }

        if ( buffer.CreatedIndexes.Count > 0 )
        {
            if ( builder.Length > 0 )
                builder.AppendLine().AppendLine();

            foreach ( var ix in buffer.CreatedIndexes.Values )
                AppendCreateIndex( builder, ix ).AppendLine();

            builder.RemoveLastNewLine();
        }

        static void HandleColumnRename(
            StringBuilder builder,
            SqliteTableBuilder table,
            SqliteAlterTableBuffer buffer,
            ulong id,
            ref SqliteAlterTableBuffer.ColumnRename rename)
        {
            Assume.Equals( rename.IsPending, false, nameof( rename.IsPending ) );

            if ( buffer.ColumnIdsByCurrentName.TryGetValue( rename.NewName, out var idByName ) )
            {
                ref var conflictingRename = ref CollectionsMarshal.GetValueRefOrNullRef( buffer.ColumnRenames, idByName );
                Assume.False( Unsafe.IsNullRef( ref conflictingRename ), nameof( conflictingRename ) + " cannot be a null ref." );

                if ( conflictingRename.IsPending )
                {
                    conflictingRename = new SqliteAlterTableBuffer.ColumnRename(
                        conflictingRename.OldName,
                        conflictingRename.NewName,
                        IsPending: false );

                    HandleColumnRename( builder, table, buffer, idByName, ref conflictingRename );
                }
                else
                {
                    var tempName = CreateTemporaryName( conflictingRename.OldName );
                    conflictingRename = new SqliteAlterTableBuffer.ColumnRename( tempName, conflictingRename.NewName, IsPending: false );

                    builder.AppendRenameColumn( table.FullName, rename.NewName, tempName ).AppendLine();
                    buffer.ColumnIdsByCurrentName.Remove( rename.NewName );
                    buffer.ColumnIdsByCurrentName.Add( tempName, idByName );
                }
            }

            builder.AppendRenameColumn( table.FullName, rename.OldName, rename.NewName ).AppendLine();
            buffer.ColumnIdsByCurrentName.Remove( rename.OldName );
            buffer.ColumnIdsByCurrentName.Add( rename.NewName, id );
        }
    }

    private static StringBuilder AppendCreateIndex(StringBuilder builder, SqliteIndexBuilder index)
    {
        Assume.IsNull( index.PrimaryKey, nameof( index.PrimaryKey ) );

        builder
            .AppendCreateIndexDefinition( index.FullName, index.Table.FullName, index.IsUnique )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        foreach ( var c in index.Columns )
            builder.AppendIndexedColumn( c.Column.Name, c.Ordering );

        return builder.AppendElementsEnd( trimCount: 2 ).AppendCommandEnd();
    }

    private static void AppendCreateIndexCollection(StringBuilder builder, SqliteIndexBuilderCollection indexes)
    {
        if ( indexes.Count > 1 )
            builder.AppendLine();

        foreach ( var index in indexes )
        {
            if ( index.PrimaryKey is null )
                AppendCreateIndex( builder.AppendLine(), index );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string CreateTemporaryName(string name)
    {
        return $"__{name}__{Guid.NewGuid():N}__";
    }
}
