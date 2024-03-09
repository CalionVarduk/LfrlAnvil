using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Internal;

internal sealed class MySqlDatabaseChangeAggregator : SqlDatabaseChangeAggregator
{
    internal MySqlDatabaseChangeAggregator(MySqlDatabaseChangeTracker changes)
        : base( changes )
    {
        CreatedColumns = SqlDatabaseObjectsSet<MySqlColumnBuilder>.Create();
        ModifiedColumns = SqlColumnModificationSourcesSet<MySqlColumnBuilder>.Create();
        RemovedColumns = SqlDatabaseNamedObjectsSet<MySqlColumnBuilder>.Create();
        CreatedIndexes = SqlDatabaseObjectsSet<MySqlIndexBuilder>.Create();
        ModifiedIndexes = SqlDatabaseObjectsSet<MySqlIndexBuilder>.Create();
        RenamedIndexes = SqlDatabaseNamedObjectsSet<MySqlIndexBuilder>.Create();
        RemovedIndexes = SqlDatabaseNamedObjectsSet<MySqlIndexBuilder>.Create();
        CreatedForeignKeys = SqlDatabaseObjectsSet<MySqlForeignKeyBuilder>.Create();
        RemovedForeignKeys = SqlDatabaseNamedObjectsSet<MySqlForeignKeyBuilder>.Create();
        CreatedChecks = SqlDatabaseObjectsSet<MySqlCheckBuilder>.Create();
        RemovedChecks = SqlDatabaseNamedObjectsSet<MySqlCheckBuilder>.Create();
    }

    public SqlDatabaseObjectsSet<MySqlColumnBuilder> CreatedColumns { get; }
    public SqlColumnModificationSourcesSet<MySqlColumnBuilder> ModifiedColumns { get; }
    public SqlDatabaseNamedObjectsSet<MySqlColumnBuilder> RemovedColumns { get; }
    public SqlDatabaseObjectsSet<MySqlIndexBuilder> CreatedIndexes { get; }
    public SqlDatabaseObjectsSet<MySqlIndexBuilder> ModifiedIndexes { get; }
    public SqlDatabaseNamedObjectsSet<MySqlIndexBuilder> RenamedIndexes { get; }
    public SqlDatabaseNamedObjectsSet<MySqlIndexBuilder> RemovedIndexes { get; }
    public SqlDatabaseObjectsSet<MySqlForeignKeyBuilder> CreatedForeignKeys { get; }
    public SqlDatabaseNamedObjectsSet<MySqlForeignKeyBuilder> RemovedForeignKeys { get; }
    public SqlDatabaseObjectsSet<MySqlCheckBuilder> CreatedChecks { get; }
    public SqlDatabaseNamedObjectsSet<MySqlCheckBuilder> RemovedChecks { get; }
    public SqlObjectOriginalValue<string> OriginalName { get; private set; }
    public bool IsPrimaryKeyChanged { get; private set; }
    public bool IsRenamed => OriginalName.Exists;

    public bool RequiresNonForeignKeyAlteration =>
        IsPrimaryKeyChanged ||
        CreatedColumns.Count > 0 ||
        ModifiedColumns.Count > 0 ||
        RemovedColumns.Count > 0 ||
        RenamedIndexes.Count > 0 ||
        CreatedChecks.Count > 0 ||
        RemovedChecks.Count > 0;

    public bool HasChanged =>
        IsRenamed ||
        RequiresNonForeignKeyAlteration ||
        CreatedIndexes.Count > 0 ||
        RemovedIndexes.Count > 0 ||
        CreatedForeignKeys.Count > 0 ||
        RemovedForeignKeys.Count > 0;

    public override void Clear()
    {
        CreatedColumns.Clear();
        ModifiedColumns.Clear();
        RemovedColumns.Clear();
        CreatedIndexes.Clear();
        ModifiedIndexes.Clear();
        RenamedIndexes.Clear();
        RemovedIndexes.Clear();
        CreatedForeignKeys.Clear();
        RemovedForeignKeys.Clear();
        CreatedChecks.Clear();
        RemovedChecks.Clear();
        OriginalName = SqlObjectOriginalValue<string>.CreateEmpty();
        IsPrimaryKeyChanged = false;
    }

    protected override void HandleCreation(SqlObjectBuilder obj)
    {
        Assume.IsNotNull( Changes.ActiveObject );
        Assume.Equals( Changes.ActiveObject.Type, SqlObjectType.Table );

        switch ( obj.Type )
        {
            case SqlObjectType.Column:
            {
                CreatedColumns.Add( ReinterpretCast.To<MySqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<MySqlIndexBuilder>( obj );
                if ( ! index.IsVirtual )
                    CreatedIndexes.Add( index );

                break;
            }
            case SqlObjectType.ForeignKey:
            {
                CreatedForeignKeys.Add( ReinterpretCast.To<MySqlForeignKeyBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Check:
            {
                CreatedChecks.Add( ReinterpretCast.To<MySqlCheckBuilder>( obj ) );
                break;
            }
            default:
            {
                Assume.Equals( obj.Type, SqlObjectType.PrimaryKey );
                IsPrimaryKeyChanged = true;
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
                RemovedColumns.Add( name, ReinterpretCast.To<MySqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<MySqlIndexBuilder>( obj );
                if ( index.IsVirtual && ! Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                    break;

                var name = Changes.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );
                RemovedIndexes.Add( name, index );
                break;
            }
            case SqlObjectType.ForeignKey:
            {
                var name = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedForeignKeys.Add( name, ReinterpretCast.To<MySqlForeignKeyBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Check:
            {
                var name = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedChecks.Add( name, ReinterpretCast.To<MySqlCheckBuilder>( obj ) );
                break;
            }
            default:
            {
                Assume.Equals( obj.Type, SqlObjectType.PrimaryKey );
                IsPrimaryKeyChanged = true;
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
                ModifiedColumns.Add( ReinterpretCast.To<MySqlColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<MySqlIndexBuilder>( obj );
                var originalName = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );

                if ( index.IsVirtual )
                {
                    if ( Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                    {
                        RemovedIndexes.Add( originalName, index );
                        ModifiedIndexes.Add( index );
                    }

                    break;
                }

                if ( Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                {
                    CreatedIndexes.Add( index );
                    ModifiedIndexes.Add( index );
                    break;
                }

                if ( descriptor == SqlObjectChangeDescriptor.Name )
                {
                    if ( ! ModifiedIndexes.Contains( index ) )
                        RenamedIndexes.Add( originalName, index );

                    break;
                }

                RenamedIndexes.Remove( originalName );
                RemovedIndexes.Add( originalName, index );
                CreatedIndexes.Add( index );
                ModifiedIndexes.Add( index );
                break;
            }
            case SqlObjectType.ForeignKey:
            {
                var foreignKey = ReinterpretCast.To<MySqlForeignKeyBuilder>( obj );
                var originalName = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedForeignKeys.Add( originalName, foreignKey );
                CreatedForeignKeys.Add( foreignKey );
                break;
            }
            case SqlObjectType.Check:
            {
                var check = ReinterpretCast.To<MySqlCheckBuilder>( obj );
                var originalName = Changes.GetOriginalValue( obj, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( obj.Name );
                RemovedChecks.Add( originalName, check );
                CreatedChecks.Add( check );
                break;
            }
            default:
                Assume.Equals( obj.Type, SqlObjectType.PrimaryKey );
                IsPrimaryKeyChanged = true;
                break;
        }
    }

    internal void PrepareColumnsForAlteration(MySqlTableBuilder table)
    {
        Assume.Equals( table, Changes.ActiveObject );
        if ( CreatedColumns.Count == 0 || RemovedColumns.Count == 0 )
            return;

        foreach ( var column in table.Columns )
        {
            if ( ! CreatedColumns.Contains( column ) )
                continue;

            var removed = RemovedColumns.Remove( column.Name );
            if ( removed is null )
                continue;

            CreatedColumns.Remove( column );
            ModifiedColumns.Add( new SqlColumnModificationSource<MySqlColumnBuilder>( column, removed ) );
        }
    }

    internal void PrepareForeignKeysForAlteration()
    {
        foreach ( var index in ModifiedIndexes )
        {
            foreach ( var reference in index.ReferencingObjects )
            {
                if ( reference.Source.Object.Type != SqlObjectType.ForeignKey || reference.Source.Property is not null )
                    continue;

                var foreignKey = ReinterpretCast.To<MySqlForeignKeyBuilder>( reference.Source.Object );
                if ( ! ReferenceEquals( index, foreignKey.OriginIndex ) )
                    continue;

                var originalName = Changes.GetOriginalValue( foreignKey, SqlObjectChangeDescriptor.Name )
                    .GetValueOrDefault( foreignKey.Name );

                RemovedForeignKeys.Add( originalName, foreignKey );
                CreatedForeignKeys.Add( foreignKey );
            }
        }
    }
}
