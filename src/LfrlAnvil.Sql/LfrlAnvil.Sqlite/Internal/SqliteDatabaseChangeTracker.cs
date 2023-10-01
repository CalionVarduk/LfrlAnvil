using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseChangeTracker
{
    private const byte IsDetachedBit = 1 << 0;
    private const byte ModeMask = (1 << 8) - 2;

    private readonly SqliteDatabaseBuilder _database;
    private readonly List<string> _pendingStatements;
    private readonly List<SqliteDatabasePropertyChange> _ongoing;
    private readonly StringBuilder _ongoingStatement;
    private readonly Dictionary<ulong, SqliteTableBuilder> _modifiedTables;
    private SqliteAlterTableBuffer? _alterTableBuffer;
    private byte _mode;

    internal SqliteDatabaseChangeTracker(SqliteDatabaseBuilder database)
    {
        _database = database;
        _pendingStatements = new List<string>();
        _ongoing = new List<SqliteDatabasePropertyChange>();
        _ongoingStatement = new StringBuilder();
        _modifiedTables = new Dictionary<ulong, SqliteTableBuilder>();
        _alterTableBuffer = null;
        CurrentObject = null;
        _mode = (byte)SqlDatabaseCreateMode.DryRun << 1;
    }

    internal SqliteObjectBuilder? CurrentObject { get; private set; }
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
        CurrentObject = null;
        _modifiedTables.Clear();
        _ongoing.Clear();
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
        FullNameUpdated( (SqliteObjectBuilder)table, obj, oldValue );
    }

    internal void FullNameUpdated(SqliteViewBuilder view, string oldValue)
    {
        FullNameUpdated( view, view, oldValue );
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

    private void FullNameUpdated(SqliteObjectBuilder owner, SqliteObjectBuilder obj, string oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new SqliteDatabasePropertyChange(
            obj,
            SqliteObjectChangeDescriptor.Name,
            SqliteObjectStatus.Modified,
            oldValue,
            obj.FullName );

        AddChange( owner, change );
    }

    private void AddChange(SqliteObjectBuilder obj, SqliteDatabasePropertyChange change)
    {
        Assume.Equals( IsPreparingStatements, true, nameof( IsPreparingStatements ) );

        if ( ! ReferenceEquals( CurrentObject, obj ) )
        {
            if ( CurrentObject is not null )
                CompletePendingStatement();

            CurrentObject = obj;
        }

        _ongoing.Add( change );
    }

    private void CompletePendingStatement()
    {
        Assume.IsNotNull( CurrentObject, nameof( CurrentObject ) );
        Assume.ContainsAtLeast( _ongoing, 1, nameof( _ongoing ) );

        var changes = CollectionsMarshal.AsSpan( _ongoing );
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

        _ongoing.Clear();
        if ( _ongoingStatement.Length > 0 )
        {
            if ( CurrentObject.Type == SqlObjectType.Table )
            {
                if ( CurrentObject.IsRemoved )
                    _modifiedTables.Remove( CurrentObject.Id );
                else
                    _modifiedTables.TryAdd( CurrentObject.Id, ReinterpretCast.To<SqliteTableBuilder>( CurrentObject ) );
            }

            _pendingStatements.Add( _ongoingStatement.AppendLine().ToString() );
            _ongoingStatement.Clear();
        }

        CurrentObject = null;
    }

    private void CompletePendingCreateObjectStatement()
    {
        Assume.IsNotNull( CurrentObject, nameof( CurrentObject ) );

        if ( CurrentObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<SqliteTableBuilder>( CurrentObject );
            ValidateTable( currentTable );
            AppendCreateTable( _ongoingStatement, currentTable );
            AppendCreateIndexCollection( _ongoingStatement, currentTable.Indexes );
        }
        else
        {
            var currentView = ReinterpretCast.To<SqliteViewBuilder>( CurrentObject );
            AppendCreateView( _ongoingStatement, currentView, _database.NodeInterpreterFactory );
        }
    }

    private void CompletePendingDropObjectStatement()
    {
        Assume.IsNotNull( CurrentObject, nameof( CurrentObject ) );

        var oldFullName = TryFindOldFullNameForCurrentObject();

        if ( CurrentObject.Type == SqlObjectType.Table )
            _ongoingStatement.AppendDropTable( oldFullName ?? CurrentObject.FullName );
        else
            _ongoingStatement.AppendDropView( oldFullName ?? CurrentObject.FullName );
    }

    private void CompletePendingAlterObjectStatement(ReadOnlySpan<SqliteDatabasePropertyChange> changes)
    {
        Assume.IsNotNull( CurrentObject, nameof( CurrentObject ) );

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
                    var oldName = _alterTableBuffer.TryGetOldName( CurrentObject.Id );
                    Assume.IsNotNull( oldName, nameof( oldName ) );
                    _ongoingStatement.AppendRenameTable( oldName, CurrentObject.FullName );
                }

                if ( requiresReconstruction )
                    AppendReconstructTable( _ongoingStatement, currentTable, _alterTableBuffer );
                else
                    AppendAlterTableWithoutReconstruction( _ongoingStatement, currentTable, _alterTableBuffer );
            }

            _alterTableBuffer.Clear();
        }
        else
        {
            var currentView = ReinterpretCast.To<SqliteViewBuilder>( CurrentObject );
            var oldFullViewName = TryFindOldFullNameForCurrentObject();

            if ( oldFullViewName is null || oldFullViewName.Equals( currentView.FullName ) )
                return;

            _ongoingStatement.AppendDropView( oldFullViewName ).AppendLine().AppendLine();
            AppendCreateView( _ongoingStatement, currentView, _database.NodeInterpreterFactory );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(SqliteTableBuilder table)
    {
        if ( table.PrimaryKey is null )
            ExceptionThrower.Throw( new SqliteObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }

    private void AppendCreateTable(StringBuilder builder, SqliteTableBuilder table, string? temporaryName = null)
    {
        Assume.IsNotNull( table.PrimaryKey, nameof( table.PrimaryKey ) );

        var fullTableName = temporaryName ?? table.FullName;
        builder.AppendCreateTableBegin( fullTableName );

        foreach ( var column in table.Columns )
        {
            builder
                .AppendIndentation()
                .AppendColumnDefinition(
                    column.Name,
                    column.TypeDefinition,
                    column.IsNullable,
                    column.DefaultValue,
                    _database.NodeInterpreterFactory )
                .AppendElementSeparator()
                .AppendLine();
        }

        builder
            .AppendIndentation()
            .AppendPrimaryKeyDefinition( table.PrimaryKey.FullName )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        foreach ( var c in table.PrimaryKey.Index.Columns.Span )
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

            foreach ( var c in foreignKey.Index.Columns.Span )
                builder.AppendNamedElement( c.Column.Name );

            builder
                .AppendElementsEnd( trimCount: 2 )
                .AppendTokenSeparator()
                .AppendForeignKeyReferenceDefinition( foreignKey.ReferencedIndex.Table.FullName )
                .AppendTokenSeparator()
                .AppendElementsBegin();

            foreach ( var c in foreignKey.ReferencedIndex.Columns.Span )
                builder.AppendNamedElement( c.Column.Name );

            builder
                .AppendElementsEnd( trimCount: 2 )
                .AppendTokenSeparator()
                .AppendForeignKeyBehaviors( foreignKey.OnDeleteBehavior, foreignKey.OnUpdateBehavior );
        }

        builder.AppendCreateTableEnd();
    }

    private void AppendReconstructTable(StringBuilder builder, SqliteTableBuilder table, SqliteAlterTableBuffer buffer)
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
                builder.AppendDefaultValue( column.DefaultValue, _database.NodeInterpreterFactory );
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
                        .AppendDefaultValue( column.DefaultValue ?? column.TypeDefinition.DefaultValue, _database.NodeInterpreterFactory )
                        .AppendElementsEnd();
                }
                else if ( oldDataType == column.TypeDefinition.DbType )
                    builder.AppendName( oldName );
                else
                    builder.AppendCastAs( oldName, column.TypeDefinition.DbType.Name );
            }

            builder.AppendElementSeparator().AppendTokenSeparator();
        }

        builder.ShrinkBy( 2 ).AppendLine().AppendFrom( table.FullName ).AppendCommandEnd();
        builder.AppendLine().AppendLine().AppendDropTable( table.FullName );
        builder.AppendLine().AppendLine().AppendRenameTable( temporaryTableName, table.FullName );
        AppendCreateIndexCollection( builder, table.Indexes );
    }

    private void AppendAlterTableWithoutReconstruction(
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

    private StringBuilder AppendCreateIndex(StringBuilder builder, SqliteIndexBuilder index)
    {
        Assume.IsNull( index.PrimaryKey, nameof( index.PrimaryKey ) );

        builder
            .AppendCreateIndexDefinition( index.FullName, index.Table.FullName, index.IsUnique )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        foreach ( var c in index.Columns.Span )
            builder.AppendIndexedColumn( c.Column.Name, c.Ordering );

        builder.AppendElementsEnd( trimCount: 2 );
        if ( index.Filter is not null )
            builder.AppendIndexFilter( index.Filter, _database.NodeInterpreterFactory );

        return builder.AppendCommandEnd();
    }

    private void AppendCreateIndexCollection(StringBuilder builder, SqliteIndexBuilderCollection indexes)
    {
        if ( indexes.Count > 1 )
            builder.AppendLine();

        foreach ( var index in indexes )
        {
            if ( index.PrimaryKey is null )
                AppendCreateIndex( builder.AppendLine(), index );
        }
    }

    private static void AppendCreateView(StringBuilder builder, SqliteViewBuilder view, SqliteNodeInterpreterFactory nodeInterpreterFactory)
    {
        builder.AppendCreateViewBegin( view.FullName );

        var interpreter = nodeInterpreterFactory.Create( SqlNodeInterpreterContext.Create( builder ) );
        interpreter.Visit( view.Source );
        builder.AppendCommandEnd();
    }

    [Pure]
    private string? TryFindOldFullNameForCurrentObject()
    {
        foreach ( var change in CollectionsMarshal.AsSpan( _ongoing ) )
        {
            if ( ReferenceEquals( CurrentObject, change.Object ) && change.Descriptor == SqliteObjectChangeDescriptor.Name )
                return ReinterpretCast.To<string>( change.OldValue );
        }

        return null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string CreateTemporaryName(string name)
    {
        return $"__{name}__{Guid.NewGuid():N}__";
    }
}
