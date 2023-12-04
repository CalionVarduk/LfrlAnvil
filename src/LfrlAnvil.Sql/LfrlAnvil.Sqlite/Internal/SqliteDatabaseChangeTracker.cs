using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseChangeTracker
{
    private const byte IsDetachedBit = 1 << 0;
    private const byte ModeMask = (1 << 8) - 2;

    private readonly SqliteDatabaseBuilder _database;
    private readonly List<SqlDatabaseBuilderStatement> _pendingStatements;
    private readonly List<ISqlStatementNode> _ongoingStatements;
    private readonly List<SqliteDatabasePropertyChange> _ongoingPropertyChanges;
    private readonly Dictionary<ulong, SqliteTableBuilder> _modifiedTables;
    private SqliteAlterTableBuffer? _alterTableBuffer;
    private SqlNodeInterpreterContext? _interpreterContext;
    private byte _mode;

    internal SqliteDatabaseChangeTracker(SqliteDatabaseBuilder database)
    {
        _database = database;
        _pendingStatements = new List<SqlDatabaseBuilderStatement>();
        _ongoingStatements = new List<ISqlStatementNode>();
        _ongoingPropertyChanges = new List<SqliteDatabasePropertyChange>();
        _modifiedTables = new Dictionary<ulong, SqliteTableBuilder>();
        _alterTableBuffer = null;
        _interpreterContext = null;
        CurrentObject = null;
        _mode = (byte)SqlDatabaseCreateMode.DryRun << 1;
    }

    internal SqliteObjectBuilder? CurrentObject { get; private set; }
    internal SqlDatabaseCreateMode Mode => (SqlDatabaseCreateMode)((_mode & ModeMask) >> 1);
    internal bool IsAttached => (_mode & IsDetachedBit) == 0;
    internal bool IsPreparingStatements => _mode > 0 && IsAttached;
    internal IEnumerable<string> ModifiedTableNames => _modifiedTables.Values.Select( t => t.FullName );

    internal ReadOnlySpan<SqlDatabaseBuilderStatement> GetPendingStatements()
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingStatement();

        return CollectionsMarshal.AsSpan( _pendingStatements );
    }

    internal void AddStatement(SqlDatabaseBuilderStatement statement)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
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

        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingStatement();

        _mode |= IsDetachedBit;
    }

    internal void SetMode(SqlDatabaseCreateMode mode)
    {
        Assume.IsDefined( mode );
        _mode = (byte)((int)mode << 1);
    }

    internal void ClearStatements()
    {
        CurrentObject = null;
        _modifiedTables.Clear();
        _ongoingPropertyChanges.Clear();
        _pendingStatements.Clear();
    }

    internal void ObjectCreated(SqliteTableBuilder table, SqliteObjectBuilder obj)
    {
        ObjectCreated( (SqliteObjectBuilder)table, obj );
    }

    internal void ObjectCreated(SqliteViewBuilder view)
    {
        ObjectCreated( view, view );
    }

    internal void ObjectRemoved(SqliteTableBuilder table, SqliteObjectBuilder obj)
    {
        ObjectRemoved( (SqliteObjectBuilder)table, obj );
    }

    internal void ObjectRemoved(SqliteViewBuilder view)
    {
        ObjectRemoved( view, view );
    }

    internal void NameUpdated(SqliteTableBuilder table, SqliteObjectBuilder obj, string oldValue)
    {
        NameUpdated( table, obj, SqliteObjectChangeDescriptor.Name, oldValue, obj.Name );
    }

    internal void NameUpdated(SqliteViewBuilder view, string oldValue)
    {
        NameUpdated( view, view, SqliteObjectChangeDescriptor.Name, oldValue, view.Name );
    }

    internal void SchemaNameUpdated(SqliteTableBuilder table, SqliteObjectBuilder obj, string oldValue)
    {
        NameUpdated( table, obj, SqliteObjectChangeDescriptor.SchemaName, oldValue, table.Schema.Name );
    }

    internal void SchemaNameUpdated(SqliteViewBuilder view, string oldValue)
    {
        NameUpdated( view, view, SqliteObjectChangeDescriptor.SchemaName, oldValue, view.Schema.Name );
    }

    internal void TypeDefinitionUpdated(SqliteColumnBuilder column, SqliteColumnTypeDefinition oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            column,
            SqliteObjectChangeDescriptor.DataType,
            SqliteObjectStatus.Modified,
            oldValue.DataType,
            column.TypeDefinition.DataType );

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

        AddChange( foreignKey.OriginIndex.Table, change );
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

        AddChange( foreignKey.OriginIndex.Table, change );
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

    internal void IsFilterUpdated(SqliteIndexBuilder index, SqlConditionNode? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            index,
            SqliteObjectChangeDescriptor.Filter,
            SqliteObjectStatus.Modified,
            oldValue,
            index.Filter );

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

    private void ObjectCreated(SqliteObjectBuilder owner, SqliteObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Exists,
            SqliteObjectStatus.Created,
            Boxed.False,
            Boxed.True );

        AddChange( owner, change );
    }

    private void ObjectRemoved(SqliteObjectBuilder owner, SqliteObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Exists,
            SqliteObjectStatus.Removed,
            Boxed.True,
            Boxed.False );

        AddChange( owner, change );
    }

    private void NameUpdated(
        SqliteObjectBuilder owner,
        SqliteObjectBuilder obj,
        SqliteObjectChangeDescriptor descriptor,
        string oldValue,
        string newValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            descriptor,
            SqliteObjectStatus.Modified,
            oldValue,
            newValue );

        AddChange( owner, change );
    }

    private void AddChange(SqliteObjectBuilder obj, SqliteDatabasePropertyChange change)
    {
        Assume.Equals( IsPreparingStatements, true );

        if ( ! ReferenceEquals( CurrentObject, obj ) )
        {
            if ( CurrentObject is not null )
                CompletePendingStatement();

            CurrentObject = obj;
        }

        _ongoingPropertyChanges.Add( change );
    }

    private void CompletePendingStatement()
    {
        Assume.IsNotNull( CurrentObject );
        Assume.ContainsAtLeast( _ongoingPropertyChanges, 1 );

        var changes = CollectionsMarshal.AsSpan( _ongoingPropertyChanges );
        var firstChange = changes[0];
        var lastChange = changes[^1];

        if ( firstChange.Status == SqliteObjectStatus.Created && ReferenceEquals( firstChange.Object, CurrentObject ) )
        {
            if ( lastChange.Status != SqliteObjectStatus.Removed || ! ReferenceEquals( lastChange.Object, CurrentObject ) )
                CompletePendingCreateObjectStatement();
        }
        else if ( lastChange.Status == SqliteObjectStatus.Removed && ReferenceEquals( lastChange.Object, CurrentObject ) )
            CompletePendingDropObjectStatement();
        else
            CompletePendingAlterObjectStatement( changes );

        _ongoingPropertyChanges.Clear();
        var changeStatements = CollectionsMarshal.AsSpan( _ongoingStatements );
        if ( changeStatements.Length > 0 )
        {
            if ( CurrentObject.Type == SqlObjectType.Table )
            {
                if ( CurrentObject.IsRemoved )
                    _modifiedTables.Remove( CurrentObject.Id );
                else
                    _modifiedTables.TryAdd( CurrentObject.Id, ReinterpretCast.To<SqliteTableBuilder>( CurrentObject ) );
            }

            _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
            var interpreter = _database.NodeInterpreters.Create( _interpreterContext );
            var batch = SqlNode.Batch( changeStatements.ToArray() );
            interpreter.VisitStatementBatch( batch );

            _pendingStatements.Add( SqlDatabaseBuilderStatement.Create( _interpreterContext ) );
            _interpreterContext.Sql.Clear();
            _ongoingStatements.Clear();
        }

        CurrentObject = null;
    }

    private void CompletePendingCreateObjectStatement()
    {
        Assume.IsNotNull( CurrentObject );

        if ( CurrentObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<SqliteTableBuilder>( CurrentObject );
            ValidateTable( currentTable );

            _ongoingStatements.Add( currentTable.ToCreateNode( useFullConstraintNames: true ) );
            foreach ( var ix in currentTable.Indexes )
            {
                if ( ix.PrimaryKey is null )
                    _ongoingStatements.Add( ix.ToCreateNode() );
            }
        }
        else
        {
            var currentView = ReinterpretCast.To<SqliteViewBuilder>( CurrentObject );
            _ongoingStatements.Add( currentView.ToCreateNode() );
        }
    }

    private void CompletePendingDropObjectStatement()
    {
        Assume.IsNotNull( CurrentObject );

        if ( CurrentObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<SqliteTableBuilder>( CurrentObject );
            var tableInfo = SqlRecordSetInfo.Create( FindOldFullNameForCurrentObject( currentTable.Info.Name ) );
            _ongoingStatements.Add( SqlNode.DropTable( tableInfo ) );
        }
        else
        {
            var currentView = ReinterpretCast.To<SqliteViewBuilder>( CurrentObject );
            var viewInfo = SqlRecordSetInfo.Create( FindOldFullNameForCurrentObject( currentView.Info.Name ) );
            _ongoingStatements.Add( SqlNode.DropView( viewInfo ) );
        }
    }

    private void CompletePendingAlterObjectStatement(ReadOnlySpan<SqliteDatabasePropertyChange> changes)
    {
        Assume.IsNotNull( CurrentObject );

        if ( CurrentObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<SqliteTableBuilder>( CurrentObject );
            _alterTableBuffer ??= new SqliteAlterTableBuffer();
            var (hasChanged, requiresReconstruction, isTableRenamed) = _alterTableBuffer.ParseChanges( currentTable, changes );

            if ( hasChanged )
            {
                ValidateTable( currentTable );

                if ( isTableRenamed )
                {
                    var oldSchemaName = _alterTableBuffer.TryGetOldSchemaName( currentTable.Id ) ?? currentTable.Schema.Name;
                    var oldName = _alterTableBuffer.TryGetOldName( currentTable.Id ) ?? currentTable.Name;
                    var name = SqlSchemaObjectName.Create( oldSchemaName, oldName );
                    Assume.NotEquals( name, currentTable.Info.Name );
                    _ongoingStatements.Add( SqlNode.RenameTable( SqlRecordSetInfo.Create( name ), currentTable.Info.Name ) );
                }

                if ( requiresReconstruction )
                    AddAlterTableReconstructionStatements( currentTable, _alterTableBuffer );
                else
                    AddAlterTableStatementsWithoutReconstruction( currentTable, _alterTableBuffer );
            }

            _alterTableBuffer.Clear();
        }
        else
        {
            var currentView = ReinterpretCast.To<SqliteViewBuilder>( CurrentObject );
            var oldViewName = FindOldFullNameForCurrentObject( currentView.Info.Name );
            if ( oldViewName.Equals( currentView.Info.Name ) )
                return;

            _ongoingStatements.Add( SqlNode.DropView( SqlRecordSetInfo.Create( oldViewName ) ) );
            _ongoingStatements.Add( currentView.ToCreateNode() );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(SqliteTableBuilder table)
    {
        if ( table.PrimaryKey is null )
            ExceptionThrower.Throw( new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }

    private void AddAlterTableReconstructionStatements(SqliteTableBuilder table, SqliteAlterTableBuffer buffer)
    {
        foreach ( var ix in buffer.DroppedIndexNames )
            _ongoingStatements.Add( SqlNode.DropIndex( ix ) );

        foreach ( var ix in table.Indexes )
        {
            if ( ix.PrimaryKey is not null || buffer.CreatedIndexes.ContainsKey( ix.Id ) )
                continue;

            var ixSchemaName = buffer.TryGetOldSchemaName( ix.Id ) ?? ix.Table.Schema.Name;
            var ixName = buffer.TryGetOldName( ix.Id ) ?? ix.Name;
            _ongoingStatements.Add( SqlNode.DropIndex( SqlSchemaObjectName.Create( ixSchemaName, ixName ) ) );
        }

        foreach ( var column in buffer.CreatedColumns.Values )
        {
            if ( column.DefaultValue is null && ! column.IsNullable )
                column.UpdateDefaultValueBasedOnDataType();
        }

        var temporaryTableName = SqlRecordSetInfo.Create( CreateTemporaryName( table.FullName ) );
        var createTemporaryTable = table.ToCreateNode( customInfo: temporaryTableName, useFullConstraintNames: true );
        _ongoingStatements.Add( createTemporaryTable );

        var i = 0;
        var selections = new SqlSelectNode[table.Columns.Count];
        var tableColumns = new SqlDataFieldNode[table.Columns.Count];

        foreach ( var column in table.Columns )
        {
            tableColumns[i] = createTemporaryTable.RecordSet[column.Name];

            if ( buffer.CreatedColumns.ContainsKey( column.Id ) )
            {
                selections[i++] = (column.DefaultValue ?? SqlNode.Null()).As( column.Name );
                continue;
            }

            if ( ! buffer.Objects.ContainsKey( column.Id ) )
            {
                selections[i++] = column.Node.AsSelf();
                continue;
            }

            var oldName = buffer.TryGetOldName( column.Id ) ?? column.Name;
            var oldIsNullable = buffer.TryGetOldIsNullable( column.Id ) ?? column.IsNullable;
            var oldDataType = buffer.TryGetOldDataType( column.Id ) ?? column.TypeDefinition.DataType;

            SqlExpressionNode oldDataField = oldDataType == column.TypeDefinition.DataType
                ? column.Node
                : table.RecordSet.GetRawField(
                        oldName,
                        TypeNullability.Create(
                            _database.TypeDefinitions.GetDefaultForDataType( oldDataType ).RuntimeType,
                            oldIsNullable ) )
                    .CastTo( column.TypeDefinition.RuntimeType );

            if ( oldIsNullable && ! column.IsNullable )
            {
                selections[i++] = oldDataField.Coalesce( column.DefaultValue ?? column.TypeDefinition.DefaultValue ).As( column.Name );
                continue;
            }

            selections[i++] = oldDataField.As( column.Name );
        }

        var insertInto = table.RecordSet
            .ToDataSource()
            .Select( selections )
            .ToInsertInto( createTemporaryTable.RecordSet, tableColumns );

        _ongoingStatements.Add( insertInto );
        _ongoingStatements.Add( SqlNode.DropTable( table.Info ) );
        _ongoingStatements.Add( SqlNode.RenameTable( temporaryTableName, table.Info.Name ) );

        foreach ( var ix in table.Indexes )
        {
            if ( ix.PrimaryKey is null )
                _ongoingStatements.Add( ix.ToCreateNode() );
        }
    }

    private void AddAlterTableStatementsWithoutReconstruction(SqliteTableBuilder table, SqliteAlterTableBuffer buffer)
    {
        foreach ( var ix in buffer.DroppedIndexNames )
            _ongoingStatements.Add( SqlNode.DropIndex( ix ) );

        foreach ( var column in buffer.DroppedColumnsByName.Keys )
            _ongoingStatements.Add( SqlNode.DropColumn( table.Info, column ) );

        foreach ( var (id, rename) in buffer.ColumnRenames )
        {
            if ( ! rename.IsPending )
                continue;

            ref var renameRef = ref CollectionsMarshal.GetValueRefOrNullRef( buffer.ColumnRenames, id );
            Assume.False( Unsafe.IsNullRef( ref renameRef ) );

            renameRef = new SqliteAlterTableBuffer.ColumnRename( rename.OldName, rename.NewName, IsPending: false );
            HandleColumnRename( _ongoingStatements, table, buffer, id, ref renameRef );
        }

        foreach ( var ix in buffer.CreatedIndexes.Values )
            _ongoingStatements.Add( ix.ToCreateNode() );

        static void HandleColumnRename(
            List<ISqlStatementNode> changeStatements,
            SqliteTableBuilder table,
            SqliteAlterTableBuffer buffer,
            ulong id,
            ref SqliteAlterTableBuffer.ColumnRename rename)
        {
            Assume.Equals( rename.IsPending, false );

            if ( buffer.ColumnIdsByCurrentName.TryGetValue( rename.NewName, out var idByName ) )
            {
                ref var conflictingRename = ref CollectionsMarshal.GetValueRefOrNullRef( buffer.ColumnRenames, idByName );
                Assume.False( Unsafe.IsNullRef( ref conflictingRename ) );

                if ( conflictingRename.IsPending )
                {
                    conflictingRename = new SqliteAlterTableBuffer.ColumnRename(
                        conflictingRename.OldName,
                        conflictingRename.NewName,
                        IsPending: false );

                    HandleColumnRename( changeStatements, table, buffer, idByName, ref conflictingRename );
                }
                else
                {
                    var tempName = CreateTemporaryName( conflictingRename.OldName );
                    conflictingRename = new SqliteAlterTableBuffer.ColumnRename( tempName, conflictingRename.NewName, IsPending: false );

                    changeStatements.Add( SqlNode.RenameColumn( table.Info, rename.NewName, tempName ) );
                    buffer.ColumnIdsByCurrentName.Remove( rename.NewName );
                    buffer.ColumnIdsByCurrentName.Add( tempName, idByName );
                }
            }

            changeStatements.Add( SqlNode.RenameColumn( table.Info, rename.OldName, rename.NewName ) );
            buffer.ColumnIdsByCurrentName.Remove( rename.OldName );
            buffer.ColumnIdsByCurrentName.Add( rename.NewName, id );
        }
    }

    [Pure]
    private SqlSchemaObjectName FindOldFullNameForCurrentObject(SqlSchemaObjectName currentName)
    {
        string? oldSchemaName = null;
        string? oldName = null;

        foreach ( var change in CollectionsMarshal.AsSpan( _ongoingPropertyChanges ) )
        {
            if ( ! ReferenceEquals( CurrentObject, change.Object ) )
                continue;

            if ( change.Descriptor == SqliteObjectChangeDescriptor.Name )
                oldName ??= ReinterpretCast.To<string>( change.OldValue );
            else if ( change.Descriptor == SqliteObjectChangeDescriptor.SchemaName )
                oldSchemaName ??= ReinterpretCast.To<string>( change.OldValue );
        }

        return SqlSchemaObjectName.Create( oldSchemaName ?? currentName.Schema, oldName ?? currentName.Object );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string CreateTemporaryName(string name)
    {
        return $"__{name}__{Guid.NewGuid():N}__";
    }
}
