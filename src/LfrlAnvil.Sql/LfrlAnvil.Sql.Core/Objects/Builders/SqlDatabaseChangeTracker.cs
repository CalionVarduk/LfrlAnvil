﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlDatabaseChangeTracker : ISqlDatabaseChangeTracker
{
    protected internal const byte IsDetachedBit = 1 << 0;
    protected internal const byte ModeMask = (1 << 8) - 2;

    private List<SqlDatabaseBuilderCommandAction>? _pendingActions;
    private Dictionary<ChangeKey, ChangePayload>? _activeChanges;
    private SqlDatabaseBuilder? _database;
    private SqlNodeInterpreterContext? _interpreterContext;
    private SqlDatabaseChangeAggregator? _changeAggregator;
    private byte _mode;

    protected SqlDatabaseChangeTracker()
    {
        ActiveObjectExistenceState = SqlObjectExistenceState.Unchanged;
        ActiveObject = null;
        ActionTimeout = null;
        _pendingActions = null;
        _activeChanges = null;
        _database = null;
        _interpreterContext = null;
        _changeAggregator = null;
        _mode = 0;
    }

    public SqlDatabaseBuilder Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    public SqlObjectBuilder? ActiveObject { get; private set; }
    public SqlObjectExistenceState ActiveObjectExistenceState { get; private set; }
    public TimeSpan? ActionTimeout { get; private set; }
    public SqlDatabaseCreateMode Mode => (SqlDatabaseCreateMode)((_mode & ModeMask) >> 1);
    public bool IsAttached => (_mode & IsDetachedBit) == 0;
    public bool IsActive => _mode > 0 && IsAttached;

    ISqlObjectBuilder? ISqlDatabaseChangeTracker.ActiveObject => ActiveObject;
    ISqlDatabaseBuilder ISqlDatabaseChangeTracker.Database => Database;

    public ReadOnlySpan<SqlDatabaseBuilderCommandAction> GetPendingActions()
    {
        CompletePendingChanges();
        return CollectionsMarshal.AsSpan( _pendingActions );
    }

    [Pure]
    public SqlObjectExistenceState GetExistenceState(SqlObjectBuilder target)
    {
        var changeKey = new ChangeKey( target.Id, SqlObjectChangeDescriptor.IsRemoved );
        if ( _activeChanges is null || ! _activeChanges.TryGetValue( changeKey, out var entry ) )
            return SqlObjectExistenceState.Unchanged;

        Assume.Equals( target, entry.Target );
        return Equals( entry.OriginalValue, Boxed.True ) ? SqlObjectExistenceState.Created : SqlObjectExistenceState.Removed;
    }

    [Pure]
    public bool ContainsChange(SqlObjectBuilder target, SqlObjectChangeDescriptor descriptor)
    {
        var changeKey = new ChangeKey( target.Id, descriptor );
        return _activeChanges is not null && _activeChanges.ContainsKey( changeKey );
    }

    public bool TryGetOriginalValue(SqlObjectBuilder target, SqlObjectChangeDescriptor descriptor, out object? result)
    {
        var changeKey = new ChangeKey( target.Id, descriptor );
        if ( _activeChanges is null || ! _activeChanges.TryGetValue( changeKey, out var entry ) )
        {
            result = null;
            return false;
        }

        Assume.Equals( target, entry.Target );
        result = entry.OriginalValue;
        return true;
    }

    public SqlDatabaseChangeTracker AddAction(Action<IDbCommand> action, Action<IDbCommand>? setup = null)
    {
        CompletePendingChanges();
        if ( IsActive )
            AddAction( SqlDatabaseBuilderCommandAction.CreateCustom( action, setup, ActionTimeout ) );

        return this;
    }

    public SqlDatabaseChangeTracker AddStatement(ISqlStatementNode statement)
    {
        CompletePendingChanges();
        if ( ! IsActive )
            return this;

        var interpreter = CreateNodeInterpreter();
        try
        {
            interpreter.Visit( statement.Node );
            if ( interpreter.Context.Parameters.Count > 0 )
                throw SqlHelpers.CreateObjectBuilderException(
                    Database,
                    ExceptionResources.StatementIsParameterized( statement, interpreter.Context ) );

            AddAction( SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString(), ActionTimeout ) );
            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public SqlDatabaseChangeTracker AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options = null)
    {
        CompletePendingChanges();
        if ( ! IsActive )
            return this;

        var interpreter = CreateNodeInterpreter();
        try
        {
            interpreter.Visit( statement.Node );
            var opt = options ?? SqlParameterBinderCreationOptions.Default;
            var executor = Database.ParameterBinders.Create( opt.SetContext( interpreter.Context ) ).Bind( parameters );
            AddAction(
                SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString(), executor, ActionTimeout ) );

            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public SqlDatabaseChangeTracker AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        CompletePendingChanges();
        if ( ! IsActive )
            return this;

        var interpreter = CreateNodeInterpreter();
        try
        {
            interpreter.Visit( statement.Node );
            var opt = options ?? SqlParameterBinderCreationOptions.Default;
            var executor = Database.ParameterBinders.Create<TSource>( opt.SetContext( interpreter.Context ) ).Bind( parameters );
            AddAction(
                SqlDatabaseBuilderCommandAction.CreateSql( interpreter.Context.Sql.AppendLine().ToString(), executor, ActionTimeout ) );

            interpreter.Context.Clear();
        }
        catch
        {
            interpreter.Context.Clear();
            throw;
        }

        return this;
    }

    public SqlDatabaseChangeTracker Attach(bool enabled = true)
    {
        if ( IsAttached == enabled )
            return this;

        if ( enabled )
        {
            _mode &= ModeMask;
            return this;
        }

        CompletePendingChanges();
        _mode |= IsDetachedBit;
        return this;
    }

    public SqlDatabaseChangeTracker CompletePendingChanges()
    {
        if ( ActiveObject is null )
            return this;

        switch ( ActiveObjectExistenceState )
        {
            case SqlObjectExistenceState.Created:
                Assume.True( _activeChanges is null || _activeChanges.Count == 0 );
                CompletePendingCreateObjectChanges( ActiveObject );
                break;

            case SqlObjectExistenceState.Removed:
                CompletePendingRemoveObjectChanges( ActiveObject );
                break;

            default:
                if ( _activeChanges is not null && _activeChanges.Count > 0 )
                    CompletePendingAlterObjectChanges( ActiveObject );

                break;
        }

        ClearActiveObjectState();
        return this;
    }

    public SqlDatabaseChangeTracker SetActionTimeout(TimeSpan? value)
    {
        ActionTimeout = value;
        return this;
    }

    protected virtual void AddIsRemovedChange(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        AddChange( activeObject, target, SqlObjectChangeDescriptor.IsRemoved, ! target.IsRemoved, target.IsRemoved );
    }

    protected virtual void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        AddChange( activeObject, target, SqlObjectChangeDescriptor.Name, originalValue, target.Name );
    }

    protected virtual void AddIsNullableChange(SqlColumnBuilder target)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.IsNullable, ! target.IsNullable, target.IsNullable );
    }

    protected virtual void AddDataTypeChange(SqlColumnBuilder target, ISqlDataType originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.DataType, originalValue, target.TypeDefinition.DataType );
    }

    protected virtual void AddDefaultValueChange(SqlColumnBuilder target, SqlExpressionNode? originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.DefaultValue, originalValue, target.DefaultValue );
    }

    protected virtual void AddComputationChange(SqlColumnBuilder target, SqlColumnComputation? originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.Computation, originalValue, target.Computation );
    }

    protected virtual void AddIsUniqueChange(SqlIndexBuilder target)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.IsUnique, ! target.IsUnique, target.IsUnique );
    }

    protected virtual void AddIsVirtualChange(SqlIndexBuilder target)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.IsVirtual, ! target.IsVirtual, target.IsVirtual );
    }

    protected virtual void AddFilterChange(SqlIndexBuilder target, SqlConditionNode? originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.Filter, originalValue, target.Filter );
    }

    protected virtual void AddPrimaryKeyChange(SqlIndexBuilder target, SqlPrimaryKeyBuilder? originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.PrimaryKey, originalValue, target.PrimaryKey );
    }

    protected virtual void AddOnDeleteBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.OnDeleteBehavior, originalValue, target.OnDeleteBehavior );
    }

    protected virtual void AddOnUpdateBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        AddChange( target.Table, target, SqlObjectChangeDescriptor.OnUpdateBehavior, originalValue, target.OnUpdateBehavior );
    }

    protected void AddChange<T>(
        SqlObjectBuilder activeObject,
        SqlObjectBuilder target,
        SqlObjectChangeDescriptor<T> descriptor,
        T originalValue,
        T newValue)
    {
        Assume.True( IsActive );
        Assume.False( Equals( originalValue, newValue ) );

        if ( ! ReferenceEquals( ActiveObject, activeObject ) )
        {
            if ( ActiveObject is not null )
                CompletePendingChanges();

            ActiveObject = activeObject;
            ActiveObjectExistenceState = SqlObjectExistenceState.Unchanged;
        }

        if ( descriptor.Equals( SqlObjectChangeDescriptor.IsRemoved ) )
        {
            Assume.IsNotNull( newValue );
            HandleIsRemovedChange( target, (bool)(object)newValue );
            return;
        }

        if ( ActiveObjectExistenceState != SqlObjectExistenceState.Unchanged )
            return;

        if ( ! ReferenceEquals( ActiveObject, target ) && GetExistenceState( target ) != SqlObjectExistenceState.Unchanged )
            return;

        var changeKey = new ChangeKey( target.Id, descriptor );
        _activeChanges ??= new Dictionary<ChangeKey, ChangePayload>();
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault( _activeChanges, changeKey, out var exists );
        if ( ! exists )
        {
            entry = new ChangePayload( target, GetChangePayloadValue( originalValue ) );
            return;
        }

        if ( Equals( entry.OriginalValue, GetChangePayloadValue( newValue ) ) )
            _activeChanges.Remove( changeKey );
    }

    protected abstract void CompletePendingCreateObjectChanges(SqlObjectBuilder obj);
    protected abstract void CompletePendingRemoveObjectChanges(SqlObjectBuilder obj);
    protected abstract void CompletePendingAlterObjectChanges(SqlObjectBuilder obj, SqlDatabaseChangeAggregator changeAggregator);

    [Pure]
    protected abstract SqlDatabaseChangeAggregator CreateAlterObjectChangeAggregator();

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected SqlDatabaseChangeAggregator GetOrCreateAlterObjectChangeAggregator()
    {
        return _changeAggregator ??= CreateAlterObjectChangeAggregator();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected SqlNodeInterpreter CreateNodeInterpreter()
    {
        return Database.NodeInterpreters.Create( GetNodeInterpreterContext() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected SqlNodeInterpreterContext GetNodeInterpreterContext()
    {
        return _interpreterContext ??= SqlNodeInterpreterContext.Create( capacity: 2048 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void AddSqlAction(string sql)
    {
        AddAction( SqlDatabaseBuilderCommandAction.CreateSql( sql, ActionTimeout ) );
    }

    protected void AddAction(SqlDatabaseBuilderCommandAction action)
    {
        Assume.True( IsActive );
        _pendingActions ??= new List<SqlDatabaseBuilderCommandAction>();
        _pendingActions.Add( action );
    }

    protected void CompletePendingAlterObjectChanges(SqlObjectBuilder obj)
    {
        Assume.IsNotNull( _activeChanges );

        var changeAggregator = GetOrCreateAlterObjectChangeAggregator();
        foreach ( var (key, payload) in _activeChanges )
            changeAggregator.Add( payload.Target, key.Descriptor, payload.OriginalValue );

        CompletePendingAlterObjectChanges( obj, changeAggregator );
        changeAggregator.Clear();
    }

    internal void Created(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        Assume.False( target.IsRemoved );
        if ( IsActive )
            AddIsRemovedChange( activeObject, target );
    }

    internal void Removed(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        Assume.True( target.IsRemoved );
        if ( IsActive )
            AddIsRemovedChange( activeObject, target );
    }

    internal void NameChanged(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        Assume.NotEquals( target.Name, originalValue );
        if ( IsActive )
            AddNameChange( activeObject, target, originalValue );
    }

    internal void IsNullableChanged(SqlColumnBuilder target, bool originalValue)
    {
        Assume.NotEquals( target.IsNullable, originalValue );
        if ( IsActive )
            AddIsNullableChange( target );
    }

    internal void TypeDefinitionChanged(SqlColumnBuilder target, SqlColumnTypeDefinition originalValue)
    {
        Assume.NotEquals( target.TypeDefinition, originalValue );
        if ( IsActive && ! Equals( originalValue.DataType, target.TypeDefinition.DataType ) )
            AddDataTypeChange( target, originalValue.DataType );
    }

    internal void DefaultValueChanged(SqlColumnBuilder target, SqlExpressionNode? originalValue)
    {
        Assume.Conditional(
            target.DefaultValue is null,
            () => Assume.IsNotNull( originalValue ),
            () => Assume.NotEquals( target.DefaultValue!, originalValue ) );

        if ( IsActive )
            AddDefaultValueChange( target, originalValue );
    }

    internal void ComputationChanged(SqlColumnBuilder target, SqlColumnComputation? originalValue)
    {
        Assume.NotEquals( target.Computation ?? default, originalValue ?? default );
        if ( IsActive )
            AddComputationChange( target, originalValue );
    }

    internal void IsUniqueChanged(SqlIndexBuilder target, bool originalValue)
    {
        Assume.NotEquals( target.IsUnique, originalValue );
        if ( IsActive )
            AddIsUniqueChange( target );
    }

    internal void IsVirtualChanged(SqlIndexBuilder target, bool originalValue)
    {
        Assume.NotEquals( target.IsVirtual, originalValue );
        if ( IsActive )
            AddIsVirtualChange( target );
    }

    internal void FilterChanged(SqlIndexBuilder target, SqlConditionNode? originalValue)
    {
        Assume.Conditional(
            target.Filter is null,
            () => Assume.IsNotNull( originalValue ),
            () => Assume.NotEquals( target.Filter!, originalValue ) );

        if ( IsActive )
            AddFilterChange( target, originalValue );
    }

    internal void PrimaryKeyChanged(SqlIndexBuilder target, SqlPrimaryKeyBuilder? originalValue)
    {
        Assume.Conditional(
            target.PrimaryKey is null,
            () => Assume.IsNotNull( originalValue ),
            () => Assume.NotEquals( target.PrimaryKey!, originalValue ) );

        if ( IsActive )
            AddPrimaryKeyChange( target, originalValue );
    }

    internal void OnDeleteBehaviorChanged(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        Assume.NotEquals( target.OnDeleteBehavior, originalValue );
        if ( IsActive )
            AddOnDeleteBehaviorChange( target, originalValue );
    }

    internal void OnUpdateBehaviorChanged(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        Assume.NotEquals( target.OnUpdateBehavior, originalValue );
        if ( IsActive )
            AddOnUpdateBehaviorChange( target, originalValue );
    }

    internal void SetDatabase(SqlDatabaseBuilder database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Changes, this );
        _database = database;
    }

    protected internal void SetModeAndAttach(SqlDatabaseCreateMode mode)
    {
        Assume.IsDefined( mode );
        Assume.IsNull( ActiveObject );
        _mode = (byte)((int)mode << 1);
    }

    internal void ClearPendingActions()
    {
        _pendingActions?.Clear();
    }

    private void HandleIsRemovedChange(SqlObjectBuilder target, bool isRemoved)
    {
        if ( ReferenceEquals( ActiveObject, target ) )
        {
            if ( ! isRemoved )
                ActiveObjectExistenceState = SqlObjectExistenceState.Created;
            else if ( ActiveObjectExistenceState == SqlObjectExistenceState.Unchanged )
            {
                ActiveObjectExistenceState = SqlObjectExistenceState.Removed;
                CompletePendingChanges();
            }
            else
            {
                Assume.Equals( ActiveObjectExistenceState, SqlObjectExistenceState.Created );
                ClearActiveObjectState();
            }

            return;
        }

        if ( ActiveObjectExistenceState != SqlObjectExistenceState.Unchanged )
            return;

        var changeKey = new ChangeKey( target.Id, SqlObjectChangeDescriptor.IsRemoved );
        if ( ! isRemoved )
            AddActiveChangeEntry( changeKey, new ChangePayload( target, Boxed.True ) );
        else
        {
            var existenceState = GetExistenceState( target );
            if ( existenceState == SqlObjectExistenceState.Unchanged )
                AddActiveChangeEntry( changeKey, new ChangePayload( target, Boxed.False ) );
            else
            {
                Assume.Equals( existenceState, SqlObjectExistenceState.Created );
                Assume.IsNotNull( _activeChanges );
                _activeChanges.Remove( changeKey );
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static object? GetChangePayloadValue<T>(T value)
    {
        return typeof( T ) == typeof( bool ) ? Boxed.GetBool( (bool)(object)value! ) : value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ClearActiveObjectState()
    {
        ActiveObject = null;
        ActiveObjectExistenceState = default;
        _activeChanges?.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddActiveChangeEntry(ChangeKey key, ChangePayload payload)
    {
        _activeChanges ??= new Dictionary<ChangeKey, ChangePayload>();
        _activeChanges.Add( key, payload );
    }

    private readonly record struct ChangeKey(ulong ObjectId, SqlObjectChangeDescriptor Descriptor);

    private readonly record struct ChangePayload(SqlObjectBuilder Target, object? OriginalValue);

    [Pure]
    SqlObjectExistenceState ISqlDatabaseChangeTracker.GetExistenceState(ISqlObjectBuilder target)
    {
        return GetExistenceState( SqlHelpers.CastOrThrow<SqlObjectBuilder>( Database, target ) );
    }

    [Pure]
    bool ISqlDatabaseChangeTracker.ContainsChange(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor)
    {
        return ContainsChange( SqlHelpers.CastOrThrow<SqlObjectBuilder>( Database, target ), descriptor );
    }

    bool ISqlDatabaseChangeTracker.TryGetOriginalValue(
        ISqlObjectBuilder target,
        SqlObjectChangeDescriptor descriptor,
        out object? result)
    {
        return TryGetOriginalValue( SqlHelpers.CastOrThrow<SqlObjectBuilder>( Database, target ), descriptor, out result );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.AddAction(Action<IDbCommand> action, Action<IDbCommand>? setup)
    {
        return AddAction( action, setup );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.AddStatement(ISqlStatementNode statement)
    {
        return AddStatement( statement );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<KeyValuePair<string, object?>> parameters,
        SqlParameterBinderCreationOptions? options)

    {
        return AddParameterizedStatement( statement, parameters, options );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options)
    {
        return AddParameterizedStatement( statement, parameters, options );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.Attach(bool enabled)
    {
        return Attach( enabled );
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.CompletePendingChanges()
    {
        return CompletePendingChanges();
    }

    ISqlDatabaseChangeTracker ISqlDatabaseChangeTracker.SetActionTimeout(TimeSpan? value)
    {
        return SetActionTimeout( value );
    }
}
