using System;
using System.Collections.Generic;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Internal;

internal sealed class PostgreSqlDatabaseChangeAggregator : SqlDatabaseChangeAggregator
{
    private List<SqlColumnModificationSource<PostgreSqlColumnBuilder>>? _columnsToRecreate;

    internal PostgreSqlDatabaseChangeAggregator(PostgreSqlDatabaseChangeTracker changes)
        : base( changes )
    {
        CreatedColumns = SqlDatabaseObjectsSet<PostgreSqlColumnBuilder>.Create();
        ModifiedColumns = SqlColumnModificationSourcesSet<PostgreSqlColumnBuilder>.Create();
        ColumnRenames = new Dictionary<ulong, PostgreSqlObjectRename>();
        RemovedColumns = SqlDatabaseNamedObjectsSet<PostgreSqlColumnBuilder>.Create();
        CreatedIndexes = SqlDatabaseObjectsSet<PostgreSqlIndexBuilder>.Create();
        RemovedIndexes = SqlDatabaseNamedObjectsSet<PostgreSqlIndexBuilder>.Create();
        CreatedForeignKeys = SqlDatabaseObjectsSet<PostgreSqlForeignKeyBuilder>.Create();
        RemovedForeignKeys = SqlDatabaseNamedObjectsSet<PostgreSqlForeignKeyBuilder>.Create();
        CreatedChecks = SqlDatabaseObjectsSet<PostgreSqlCheckBuilder>.Create();
        RemovedChecks = SqlDatabaseNamedObjectsSet<PostgreSqlCheckBuilder>.Create();
        ConstraintRenames = new Dictionary<ulong, PostgreSqlObjectRename>();
        ObjectsByOriginalName = SqlDatabaseNamedObjectsSet<SqlObjectBuilder>.Create();
        _columnsToRecreate = null;
    }

    public SqlDatabaseObjectsSet<PostgreSqlColumnBuilder> CreatedColumns { get; }
    public SqlColumnModificationSourcesSet<PostgreSqlColumnBuilder> ModifiedColumns { get; }
    public Dictionary<ulong, PostgreSqlObjectRename> ColumnRenames { get; }
    public SqlDatabaseNamedObjectsSet<PostgreSqlColumnBuilder> RemovedColumns { get; }
    public SqlDatabaseObjectsSet<PostgreSqlIndexBuilder> CreatedIndexes { get; }
    public SqlDatabaseNamedObjectsSet<PostgreSqlIndexBuilder> RemovedIndexes { get; }
    public SqlDatabaseObjectsSet<PostgreSqlForeignKeyBuilder> CreatedForeignKeys { get; }
    public SqlDatabaseNamedObjectsSet<PostgreSqlForeignKeyBuilder> RemovedForeignKeys { get; }
    public SqlDatabaseObjectsSet<PostgreSqlCheckBuilder> CreatedChecks { get; }
    public SqlDatabaseNamedObjectsSet<PostgreSqlCheckBuilder> RemovedChecks { get; }
    public Dictionary<ulong, PostgreSqlObjectRename> ConstraintRenames { get; }
    public SqlObjectOriginalValue<string> OriginalName { get; private set; }
    public SqlDatabaseNamedObjectsSet<SqlObjectBuilder> ObjectsByOriginalName { get; }
    public PostgreSqlPrimaryKeyBuilder? DroppedPrimaryKey { get; private set; }
    public PostgreSqlPrimaryKeyBuilder? CreatedPrimaryKey { get; private set; }
    public bool IsRenamed => OriginalName.Exists;

    public bool HasChanged =>
        IsRenamed ||
        DroppedPrimaryKey is not null ||
        CreatedPrimaryKey is not null ||
        CreatedColumns.Count > 0 ||
        ModifiedColumns.Count > 0 ||
        RemovedColumns.Count > 0 ||
        CreatedChecks.Count > 0 ||
        RemovedChecks.Count > 0 ||
        CreatedIndexes.Count > 0 ||
        RemovedIndexes.Count > 0 ||
        CreatedForeignKeys.Count > 0 ||
        RemovedForeignKeys.Count > 0 ||
        ConstraintRenames.Count > 0;

    public override void Clear()
    {
        _columnsToRecreate?.Clear();
        CreatedColumns.Clear();
        ModifiedColumns.Clear();
        ColumnRenames.Clear();
        RemovedColumns.Clear();
        CreatedIndexes.Clear();
        RemovedIndexes.Clear();
        CreatedForeignKeys.Clear();
        RemovedForeignKeys.Clear();
        CreatedChecks.Clear();
        RemovedChecks.Clear();
        ConstraintRenames.Clear();
        ObjectsByOriginalName.Clear();
        OriginalName = SqlObjectOriginalValue<string>.CreateEmpty();
        DroppedPrimaryKey = null;
        CreatedPrimaryKey = null;
    }

    protected override void HandleCreation(SqlObjectBuilder obj)
    {
        Assume.IsNotNull( Changes.ActiveObject );
        Assume.Equals( Changes.ActiveObject.Type, SqlObjectType.Table );

        switch ( obj.Type )
        {
            case SqlObjectType.Column:
            {
                CreatedColumns.Add( ReinterpretCast.To<PostgreSqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<PostgreSqlIndexBuilder>( obj );
                if ( ! index.IsVirtual )
                    CreatedIndexes.Add( index );

                break;
            }
            case SqlObjectType.ForeignKey:
            {
                CreatedForeignKeys.Add( ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Check:
            {
                CreatedChecks.Add( ReinterpretCast.To<PostgreSqlCheckBuilder>( obj ) );
                break;
            }
            default:
            {
                Assume.Equals( obj.Type, SqlObjectType.PrimaryKey );
                CreatedPrimaryKey = ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( obj );
                break;
            }
        }
    }

    protected override void HandleRemoval(SqlObjectBuilder obj)
    {
        Assume.IsNotNull( Changes.ActiveObject );
        Assume.Equals( Changes.ActiveObject.Type, SqlObjectType.Table );

        switch ( obj.Type )
        {
            case SqlObjectType.Column:
            {
                var name = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedColumns.Add( name, ReinterpretCast.To<PostgreSqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<PostgreSqlIndexBuilder>( obj );
                if ( index.IsVirtual && ! Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                    break;

                var name = Changes.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );
                RemovedIndexes.Add( name, index );
                break;
            }
            case SqlObjectType.ForeignKey:
            {
                var name = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedForeignKeys.Add( name, ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Check:
            {
                var name = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedChecks.Add( name, ReinterpretCast.To<PostgreSqlCheckBuilder>( obj ) );
                break;
            }
            default:
            {
                Assume.Equals( obj.Type, SqlObjectType.PrimaryKey );
                DroppedPrimaryKey = ReinterpretCast.To<PostgreSqlPrimaryKeyBuilder>( obj );
                break;
            }
        }
    }

    protected override void HandleModification(SqlObjectBuilder obj, SqlObjectChangeDescriptor descriptor, object? originalValue)
    {
        Assume.IsNotNull( Changes.ActiveObject );

        if ( ReferenceEquals( Changes.ActiveObject, obj ) )
        {
            Assume.True( Changes.ActiveObject.Type is SqlObjectType.Table or SqlObjectType.View );
            Assume.Equals( descriptor, SqlObjectChangeDescriptor.Name );
            Assume.IsNotNull( originalValue );
            OriginalName = SqlObjectOriginalValue<string>.Create( ReinterpretCast.To<string>( originalValue ) );
            return;
        }

        Assume.Equals( Changes.ActiveObject.Type, SqlObjectType.Table );

        switch ( obj.Type )
        {
            case SqlObjectType.Column:
            {
                ModifiedColumns.Add( ReinterpretCast.To<PostgreSqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<PostgreSqlIndexBuilder>( obj );
                var originalName = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );

                if ( index.IsVirtual )
                {
                    if ( Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                        RemovedIndexes.Add( originalName, index );

                    break;
                }

                if ( Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                {
                    CreatedIndexes.Add( index );
                    break;
                }

                if ( descriptor == SqlObjectChangeDescriptor.Name )
                {
                    if ( ! CreatedIndexes.Contains( index ) )
                        ConstraintRenames.Add( index.Id, PostgreSqlObjectRename.Create( index, originalName ) );

                    break;
                }

                ConstraintRenames.Remove( index.Id );
                RemovedIndexes.Add( originalName, index );
                CreatedIndexes.Add( index );
                break;
            }
            case SqlObjectType.ForeignKey:
            {
                var foreignKey = ReinterpretCast.To<PostgreSqlForeignKeyBuilder>( obj );
                var originalName = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );

                if ( descriptor == SqlObjectChangeDescriptor.Name )
                {
                    if ( ! CreatedForeignKeys.Contains( foreignKey ) )
                        ConstraintRenames.Add( foreignKey.Id, PostgreSqlObjectRename.Create( foreignKey, originalName ) );

                    break;
                }

                ConstraintRenames.Remove( foreignKey.Id );
                RemovedForeignKeys.Add( originalName, foreignKey );
                CreatedForeignKeys.Add( foreignKey );
                break;
            }
            default:
                Assume.True( obj.Type is SqlObjectType.Check or SqlObjectType.PrimaryKey );
                Assume.Equals( descriptor, SqlObjectChangeDescriptor.Name );
                Assume.IsNotNull( originalValue );
                ConstraintRenames.Add( obj.Id, PostgreSqlObjectRename.Create( obj, ReinterpretCast.To<string>( originalValue ) ) );
                break;
        }
    }

    internal void PrepareColumnsForAlteration(PostgreSqlTableBuilder table)
    {
        Assume.Equals( table, Changes.ActiveObject );
        if ( CreatedColumns.Count > 0 && RemovedColumns.Count > 0 )
        {
            foreach ( var column in table.Columns )
            {
                if ( ! CreatedColumns.Contains( column ) )
                    continue;

                var removed = RemovedColumns.Remove( column.Name );
                if ( removed is null )
                    continue;

                CreatedColumns.Remove( column );
                ModifiedColumns.Add( new SqlColumnModificationSource<PostgreSqlColumnBuilder>( column, removed ) );
            }
        }

        foreach ( var modification in ModifiedColumns )
        {
            var originalComputation = Changes.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Computation )
                .GetValueOrDefault( modification.Source.Computation );

            if ( modification.Column.Computation is null || originalComputation == modification.Column.Computation.Value )
            {
                var originalName = Changes.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Name )
                    .GetValueOrDefault( modification.Source.Name );

                if ( ! originalName.Equals( modification.Column.Name, StringComparison.OrdinalIgnoreCase ) )
                    ColumnRenames.Add( modification.Column.Id, PostgreSqlObjectRename.Create( modification.Column, originalName ) );

                continue;
            }

            _columnsToRecreate ??= new List<SqlColumnModificationSource<PostgreSqlColumnBuilder>>();
            _columnsToRecreate.Add( modification );
        }

        if ( _columnsToRecreate is null || _columnsToRecreate.Count == 0 )
            return;

        foreach ( var modification in _columnsToRecreate )
        {
            var originalName = Changes.GetOriginalValue( modification.Source, SqlObjectChangeDescriptor.Name )
                .GetValueOrDefault( modification.Source.Name );

            ModifiedColumns.Remove( modification.Column );
            RemovedColumns.Add( originalName, modification.Source );
            CreatedColumns.Add( modification.Column );
        }
    }

    internal void PopulateColumnsByOriginalName()
    {
        ObjectsByOriginalName.Clear();
        foreach ( var rename in ColumnRenames.Values )
            ObjectsByOriginalName.Add( rename.OriginalName, rename.Object );
    }

    internal void PopulateConstraintsByOriginalName()
    {
        ObjectsByOriginalName.Clear();
        foreach ( var rename in ConstraintRenames.Values )
            ObjectsByOriginalName.Add( rename.OriginalName, rename.Object );
    }
}
