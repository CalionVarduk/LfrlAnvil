using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Internal.Expressions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Internal;

public sealed class MySqlDatabaseChangeTracker : ISqlDatabaseChangeTracker
{
    private const byte IsDetachedBit = 1 << 0;
    private const byte ModeMask = (1 << 8) - 2;

    private readonly MySqlDatabaseBuilder _database;
    private readonly List<SqlDatabaseBuilderCommandAction> _pendingActions;
    private readonly List<ISqlStatementNode> _ongoingStatements;
    private readonly List<MySqlDatabasePropertyChange> _ongoingPropertyChanges;
    private MySqlAlterTableBuffer? _alterTableBuffer;
    private SqlNodeInterpreterContext? _interpreterContext;
    private byte _mode;

    internal MySqlDatabaseChangeTracker(MySqlDatabaseBuilder database)
    {
        _database = database;
        _pendingActions = new List<SqlDatabaseBuilderCommandAction>();
        _ongoingStatements = new List<ISqlStatementNode>();
        _ongoingPropertyChanges = new List<MySqlDatabasePropertyChange>();
        _alterTableBuffer = null;
        _interpreterContext = null;
        ActiveObject = null;
        ActiveObjectExistenceState = SqlObjectExistenceState.Unchanged;
        _mode = (byte)SqlDatabaseCreateMode.DryRun << 1;
    }

    public MySqlObjectBuilder? ActiveObject { get; private set; }
    public SqlObjectExistenceState ActiveObjectExistenceState { get; private set; }
    public TimeSpan? ActionTimeout { get; }
    public SqlDatabaseCreateMode Mode => (SqlDatabaseCreateMode)((_mode & ModeMask) >> 1);
    public bool IsAttached => (_mode & IsDetachedBit) == 0;
    internal bool IsPreparingStatements => _mode > 0 && IsAttached;

    ISqlObjectBuilder? ISqlDatabaseChangeTracker.ActiveObject => ActiveObject;
    ISqlDatabaseBuilder ISqlDatabaseChangeTracker.Database => _database;

    public ReadOnlySpan<SqlDatabaseBuilderCommandAction> GetPendingActions()
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        return CollectionsMarshal.AsSpan( _pendingActions );
    }

    public ISqlDatabaseChangeTracker AddAction(Action<IDbCommand> action, Action<IDbCommand>? setup = null)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( IsPreparingStatements )
            _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateCustom( action, setup ) );

        return this;
    }

    public ISqlDatabaseChangeTracker AddStatement(ISqlStatementNode statement)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( ! IsPreparingStatements )
            return this;

        _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
        var interpreter = _database.NodeInterpreters.Create( _interpreterContext );
        try
        {
            interpreter.Visit( statement.Node );
            if ( interpreter.Context.Parameters.Count > 0 )
                throw new MySqlObjectBuilderException( ExceptionResources.StatementIsParameterized( statement, interpreter.Context ) );

            _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public ISqlDatabaseChangeTracker AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( ! IsPreparingStatements )
            return this;

        _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
        var interpreter = _database.NodeInterpreters.Create( _interpreterContext );
        try
        {
            interpreter.Visit( statement.Node );
            var opt = options ?? SqlParameterBinderCreationOptions.Default;
            var executor = _database.ParameterBinders.Create( opt.SetContext( interpreter.Context ) ).Bind( parameters );
            _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString(), executor ) );
            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public ISqlDatabaseChangeTracker AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( ! IsPreparingStatements )
            return this;

        _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
        var interpreter = _database.NodeInterpreters.Create( _interpreterContext );
        try
        {
            interpreter.Visit( statement.Node );
            var opt = options ?? SqlParameterBinderCreationOptions.Default;
            var executor = _database.ParameterBinders.Create<TSource>( opt.SetContext( interpreter.Context ) ).Bind( parameters );
            _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString(), executor ) );
            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public ISqlDatabaseChangeTracker Attach(bool enabled = true)
    {
        if ( IsAttached == enabled )
            return this;

        if ( enabled )
        {
            _mode &= ModeMask;
            return this;
        }

        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        _mode |= IsDetachedBit;
        return this;
    }

    public ISqlDatabaseChangeTracker SetDetachedMode(bool enabled = true)
    {
        return Attach( ! enabled );
    }

    internal void SetMode(SqlDatabaseCreateMode mode)
    {
        Assume.IsDefined( mode );
        _mode = (byte)((int)mode << 1);
    }

    internal void ClearStatements()
    {
        ActiveObject = null;
        _ongoingPropertyChanges.Clear();
        _pendingActions.Clear();
    }

    internal void ObjectCreated(MySqlTableBuilder table, MySqlObjectBuilder obj)
    {
        ObjectCreated( (MySqlObjectBuilder)table, obj );
    }

    internal void ObjectCreated(MySqlViewBuilder view)
    {
        ObjectCreated( view, view );
    }

    internal void ObjectRemoved(MySqlTableBuilder table, MySqlObjectBuilder obj)
    {
        ObjectRemoved( (MySqlObjectBuilder)table, obj );
    }

    internal void ObjectRemoved(MySqlViewBuilder view)
    {
        ObjectRemoved( view, view );
    }

    internal void NameUpdated(MySqlTableBuilder table, MySqlObjectBuilder obj, string oldValue)
    {
        NameUpdated( table, obj, MySqlObjectChangeDescriptor.Name, oldValue, obj.Name );
    }

    internal void NameUpdated(MySqlViewBuilder view, string oldValue)
    {
        NameUpdated( view, view, MySqlObjectChangeDescriptor.Name, oldValue, view.Name );
    }

    internal void SchemaNameUpdated(MySqlTableBuilder table, MySqlObjectBuilder obj, string oldValue)
    {
        NameUpdated( table, obj, MySqlObjectChangeDescriptor.SchemaName, oldValue, table.Schema.Name );
    }

    internal void TypeDefinitionUpdated(MySqlColumnBuilder column, MySqlColumnTypeDefinition oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            column,
            MySqlObjectChangeDescriptor.DataType,
            MySqlObjectStatus.Modified,
            oldValue.DataType,
            column.TypeDefinition.DataType );

        AddChange( column.Table, change );
    }

    internal void IsNullableUpdated(MySqlColumnBuilder column)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            column,
            MySqlObjectChangeDescriptor.IsNullable,
            MySqlObjectStatus.Modified,
            Boxed.GetBool( ! column.IsNullable ),
            Boxed.GetBool( column.IsNullable ) );

        AddChange( column.Table, change );
    }

    internal void DefaultValueUpdated(MySqlColumnBuilder column, object? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            column,
            MySqlObjectChangeDescriptor.DefaultValue,
            MySqlObjectStatus.Modified,
            oldValue,
            column.DefaultValue );

        AddChange( column.Table, change );
    }

    internal void OnDeleteBehaviorUpdated(MySqlForeignKeyBuilder foreignKey, ReferenceBehavior oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            foreignKey,
            MySqlObjectChangeDescriptor.OnDeleteBehavior,
            MySqlObjectStatus.Modified,
            oldValue,
            foreignKey.OnDeleteBehavior );

        AddChange( foreignKey.OriginIndex.Table, change );
    }

    internal void OnUpdateBehaviorUpdated(MySqlForeignKeyBuilder foreignKey, ReferenceBehavior oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            foreignKey,
            MySqlObjectChangeDescriptor.OnUpdateBehavior,
            MySqlObjectStatus.Modified,
            oldValue,
            foreignKey.OnUpdateBehavior );

        AddChange( foreignKey.OriginIndex.Table, change );
    }

    internal void IsUniqueUpdated(MySqlIndexBuilder index)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            index,
            MySqlObjectChangeDescriptor.IsUnique,
            MySqlObjectStatus.Modified,
            Boxed.GetBool( ! index.IsUnique ),
            Boxed.GetBool( index.IsUnique ) );

        AddChange( index.Table, change );
    }

    internal void IsFilterUpdated(MySqlIndexBuilder index, SqlConditionNode? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            index,
            MySqlObjectChangeDescriptor.Filter,
            MySqlObjectStatus.Modified,
            oldValue,
            index.Filter );

        AddChange( index.Table, change );
    }

    internal void PrimaryKeyUpdated(MySqlIndexBuilder index, MySqlPrimaryKeyBuilder? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            index,
            MySqlObjectChangeDescriptor.PrimaryKey,
            MySqlObjectStatus.Modified,
            oldValue,
            index.PrimaryKey );

        AddChange( index.Table, change );
    }

    internal void PrimaryKeyUpdated(MySqlTableBuilder table, MySqlPrimaryKeyBuilder? oldValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            table,
            MySqlObjectChangeDescriptor.PrimaryKey,
            MySqlObjectStatus.Modified,
            oldValue,
            table.Constraints.TryGetPrimaryKey() );

        AddChange( table, change );
    }

    internal void SchemaCreated(string name)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( ! IsPreparingStatements )
            return;

        var interpreter = _database.NodeInterpreters.Create( GetNodeInterpreterContext() );
        MySqlHelpers.AppendCreateSchemaStatement( interpreter, name );
        _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
        interpreter.Context.Sql.Clear();
    }

    internal void SchemaDropped(string name)
    {
        if ( _ongoingPropertyChanges.Count > 0 )
            CompletePendingChanges();

        if ( ! IsPreparingStatements )
            return;

        var interpreter = _database.NodeInterpreters.Create( GetNodeInterpreterContext() );
        MySqlHelpers.AppendDropSchemaStatement( interpreter, name );
        _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
        interpreter.Context.Sql.Clear();
    }

    internal void CreateGuidFunction()
    {
        Assume.IsEmpty( _ongoingPropertyChanges );
        Assume.True( IsPreparingStatements );
        var interpreter = _database.NodeInterpreters.Create( GetNodeInterpreterContext() );
        MySqlHelpers.AppendCreateGuidFunctionStatement( interpreter, _database.CommonSchemaName );
        _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
        interpreter.Context.Sql.Clear();
    }

    internal void CreateDropIndexIfExistsProcedure()
    {
        Assume.IsEmpty( _ongoingPropertyChanges );
        Assume.True( IsPreparingStatements );
        var interpreter = _database.NodeInterpreters.Create( GetNodeInterpreterContext() );
        MySqlHelpers.AppendDropIndexIfExistsProcedureStatement( interpreter, _database.CommonSchemaName );
        _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
        interpreter.Context.Sql.Clear();
    }

    private void ObjectCreated(MySqlObjectBuilder owner, MySqlObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            obj,
            MySqlObjectChangeDescriptor.Exists,
            MySqlObjectStatus.Created,
            Boxed.False,
            Boxed.True );

        AddChange( owner, change );
    }

    private void ObjectRemoved(MySqlObjectBuilder owner, MySqlObjectBuilder obj)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            obj,
            MySqlObjectChangeDescriptor.Exists,
            MySqlObjectStatus.Removed,
            Boxed.True,
            Boxed.False );

        AddChange( owner, change );
    }

    private void NameUpdated(
        MySqlObjectBuilder owner,
        MySqlObjectBuilder obj,
        MySqlObjectChangeDescriptor descriptor,
        string oldValue,
        string newValue)
    {
        if ( ! IsPreparingStatements )
            return;

        var change = new MySqlDatabasePropertyChange(
            obj,
            descriptor,
            MySqlObjectStatus.Modified,
            oldValue,
            newValue );

        AddChange( owner, change );
    }

    private void AddChange(MySqlObjectBuilder obj, MySqlDatabasePropertyChange change)
    {
        Assume.True( IsPreparingStatements );

        if ( ! ReferenceEquals( ActiveObject, obj ) )
        {
            if ( ActiveObject is not null )
                CompletePendingChanges();

            ActiveObject = obj;
        }

        _ongoingPropertyChanges.Add( change );
    }

    public ISqlDatabaseChangeTracker CompletePendingChanges()
    {
        Assume.IsNotNull( ActiveObject );
        Assume.ContainsAtLeast( _ongoingPropertyChanges, 1 );

        var changes = CollectionsMarshal.AsSpan( _ongoingPropertyChanges );
        var firstChange = changes[0];
        var lastChange = changes[^1];

        if ( firstChange.Status == MySqlObjectStatus.Created && ReferenceEquals( firstChange.Object, ActiveObject ) )
        {
            if ( lastChange.Status != MySqlObjectStatus.Removed || ! ReferenceEquals( lastChange.Object, ActiveObject ) )
                CompletePendingCreateObjectStatement();
        }
        else if ( lastChange.Status == MySqlObjectStatus.Removed && ReferenceEquals( lastChange.Object, ActiveObject ) )
            CompletePendingDropObjectStatement();
        else
            CompletePendingAlterObjectStatement( changes );

        _ongoingPropertyChanges.Clear();
        var changeStatements = CollectionsMarshal.AsSpan( _ongoingStatements );
        if ( changeStatements.Length > 0 )
        {
            var interpreter = _database.NodeInterpreters.Create( GetNodeInterpreterContext() );
            var batch = SqlNode.Batch( changeStatements.ToArray() );
            interpreter.VisitStatementBatch( batch );

            _pendingActions.Add( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString() ) );
            interpreter.Context.Sql.Clear();
            _ongoingStatements.Clear();
        }

        ActiveObject = null;
        return this;
    }

    private void CompletePendingCreateObjectStatement()
    {
        Assume.IsNotNull( ActiveObject );

        if ( ActiveObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<MySqlTableBuilder>( ActiveObject );
            ValidateTable( currentTable );

            var createTable = currentTable.ToCreateNode( includeForeignKeys: false );
            _ongoingStatements.Add( createTable );

            List<MySqlForeignKeyBuilder>? foreignKeys = null;
            foreach ( var constraint in currentTable.Constraints )
            {
                if ( constraint.Type == SqlObjectType.ForeignKey )
                {
                    foreignKeys ??= new List<MySqlForeignKeyBuilder>();
                    foreignKeys.Add( ReinterpretCast.To<MySqlForeignKeyBuilder>( constraint ) );
                    continue;
                }

                if ( constraint.Type != SqlObjectType.Index )
                    continue;

                var ix = ReinterpretCast.To<MySqlIndexBuilder>( constraint );
                if ( ix.PrimaryKey is null )
                    _ongoingStatements.Add( ix.ToCreateNode() );
            }

            if ( foreignKeys is not null )
            {
                var foreignKeyDefinitions = foreignKeys.ToDefinitionRange( createTable.RecordSet );
                _ongoingStatements.Add( MySqlAlterTableNode.CreateAddForeignKeys( currentTable.Info, foreignKeyDefinitions ) );
            }
        }
        else
        {
            var currentView = ReinterpretCast.To<MySqlViewBuilder>( ActiveObject );
            _ongoingStatements.Add( currentView.ToCreateNode() );
        }
    }

    private void CompletePendingDropObjectStatement()
    {
        Assume.IsNotNull( ActiveObject );

        if ( ActiveObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<MySqlTableBuilder>( ActiveObject );
            var tableInfo = SqlRecordSetInfo.Create( FindOldFullNameForCurrentObject( currentTable.Info.Name ) );
            _ongoingStatements.Add( SqlNode.DropTable( tableInfo ) );
        }
        else
        {
            var currentView = ReinterpretCast.To<MySqlViewBuilder>( ActiveObject );
            var viewInfo = SqlRecordSetInfo.Create( FindOldFullNameForCurrentObject( currentView.Info.Name ) );
            _ongoingStatements.Add( SqlNode.DropView( viewInfo ) );
        }
    }

    private void CompletePendingAlterObjectStatement(ReadOnlySpan<MySqlDatabasePropertyChange> changes)
    {
        Assume.IsNotNull( ActiveObject );

        if ( ActiveObject.Type == SqlObjectType.Table )
        {
            var currentTable = ReinterpretCast.To<MySqlTableBuilder>( ActiveObject );
            _alterTableBuffer ??= new MySqlAlterTableBuffer();
            var (hasChanged, hasPrimaryKeyChanged, isTableRenamed) = _alterTableBuffer.ParseChanges( currentTable, changes );

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

                AddAlterTableStatementsWithoutReconstruction( currentTable, hasPrimaryKeyChanged, _alterTableBuffer );
            }

            _alterTableBuffer.Clear();
        }
        else
        {
            var currentView = ReinterpretCast.To<MySqlViewBuilder>( ActiveObject );
            var oldViewName = FindOldFullNameForCurrentObject( currentView.Info.Name );
            if ( oldViewName.Equals( currentView.Info.Name ) )
                return;

            _ongoingStatements.Add( SqlNode.DropView( SqlRecordSetInfo.Create( oldViewName ) ) );
            _ongoingStatements.Add( currentView.ToCreateNode() );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ValidateTable(MySqlTableBuilder table)
    {
        if ( table.Constraints.TryGetPrimaryKey() is null )
            ExceptionThrower.Throw( new MySqlObjectBuilderException( ExceptionResources.PrimaryKeyIsMissing( table ) ) );
    }

    private void AddAlterTableStatementsWithoutReconstruction(
        MySqlTableBuilder table,
        bool hasPrimaryKeyChanged,
        MySqlAlterTableBuffer buffer)
    {
        foreach ( var column in buffer.CreatedColumns.Values )
        {
            if ( column.DefaultValue is null && ! column.IsNullable )
                column.UpdateDefaultValueBasedOnDataType();
        }

        if ( buffer.DroppedForeignKeyNames.Count > 0 )
            _ongoingStatements.Add( MySqlAlterTableNode.CreateDropForeignKeys( table.Info, buffer.DroppedForeignKeyNames.ToArray() ) );

        foreach ( var ix in buffer.DroppedIndexNames )
            _ongoingStatements.Add( SqlNode.DropIndex( table.Info, SqlSchemaObjectName.Create( table.Info.Name.Schema, ix ) ) );

        if ( buffer.DroppedColumnNames.Count > 0 ||
            buffer.DroppedCheckNames.Count > 0 ||
            buffer.RenamedIndexes.Count > 0 ||
            buffer.ModifiedColumns.Count > 0 ||
            buffer.CreatedColumns.Count > 0 ||
            buffer.CreatedChecks.Count > 0 ||
            hasPrimaryKeyChanged )
        {
            var changedColumns = Array.Empty<KeyValuePair<string, SqlColumnDefinitionNode>>();
            if ( buffer.ModifiedColumns.Count > 0 )
            {
                var i = 0;
                changedColumns = new KeyValuePair<string, SqlColumnDefinitionNode>[buffer.ModifiedColumns.Count];
                foreach ( var c in buffer.ModifiedColumns.Values )
                    changedColumns[i++] = KeyValuePair.Create( buffer.TryGetOldName( c.Id ) ?? c.Name, c.ToDefinitionNode() );
            }

            _ongoingStatements.Add(
                new MySqlAlterTableNode(
                    info: table.Info,
                    oldColumns: buffer.DroppedColumnNames.ToArray(),
                    oldForeignKeys: Array.Empty<string>(),
                    oldChecks: buffer.DroppedCheckNames.ToArray(),
                    renamedIndexes: buffer.RenamedIndexes.ToArray(),
                    changedColumns: changedColumns,
                    newColumns: buffer.CreatedColumns.Values.ToDefinitionRange(),
                    newPrimaryKey: hasPrimaryKeyChanged ? table.Constraints.TryGetPrimaryKey()?.ToDefinitionNode( table.Node ) : null,
                    newForeignKeys: Array.Empty<SqlForeignKeyDefinitionNode>(),
                    newChecks: buffer.CreatedChecks.Values.ToDefinitionRange() ) );
        }

        foreach ( var ix in buffer.CreatedIndexes.Values )
            _ongoingStatements.Add( ix.ToCreateNode() );

        if ( buffer.CreatedForeignKeys.Count > 0 )
        {
            var foreignKeys = buffer.CreatedForeignKeys.Values.ToDefinitionRange( table.Node );
            _ongoingStatements.Add( MySqlAlterTableNode.CreateAddForeignKeys( table.Info, foreignKeys ) );
        }
    }

    [Pure]
    private SqlSchemaObjectName FindOldFullNameForCurrentObject(SqlSchemaObjectName currentName)
    {
        string? oldSchemaName = null;
        string? oldName = null;

        foreach ( var change in CollectionsMarshal.AsSpan( _ongoingPropertyChanges ) )
        {
            if ( ! ReferenceEquals( ActiveObject, change.Object ) )
                continue;

            if ( change.Descriptor == MySqlObjectChangeDescriptor.Name )
                oldName ??= ReinterpretCast.To<string>( change.OldValue );
            else if ( change.Descriptor == MySqlObjectChangeDescriptor.SchemaName )
                oldSchemaName ??= ReinterpretCast.To<string>( change.OldValue );
        }

        return SqlSchemaObjectName.Create( oldSchemaName ?? currentName.Schema, oldName ?? currentName.Object );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private SqlNodeInterpreterContext GetNodeInterpreterContext()
    {
        return _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
    }

    SqlObjectExistenceState ISqlDatabaseChangeTracker.GetExistenceState(ISqlObjectBuilder target)
    {
        throw new NotImplementedException();
    }

    bool ISqlDatabaseChangeTracker.ContainsChange(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor)
    {
        throw new NotImplementedException();
    }

    bool ISqlDatabaseChangeTracker.TryGetOriginalValue(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor, out object? result)
    {
        throw new NotImplementedException();
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.SetActionTimeout(TimeSpan? value)
    {
        throw new NotImplementedException();
    }
}
