// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseChangeAggregator : SqlDatabaseChangeAggregator
{
    internal SqliteDatabaseChangeAggregator(SqliteDatabaseChangeTracker changes)
        : base( changes )
    {
        CreatedColumns = SqlDatabaseObjectsSet<SqliteColumnBuilder>.Create();
        ModifiedColumns = SqlColumnModificationSourcesSet<SqliteColumnBuilder>.Create();
        ColumnRenames = new Dictionary<ulong, SqliteColumnRename>();
        RemovedColumns = SqlDatabaseNamedObjectsSet<SqliteColumnBuilder>.Create();
        CreatedIndexes = SqlDatabaseObjectsSet<SqliteIndexBuilder>.Create();
        RemovedIndexes = SqlDatabaseNamedObjectsSet<SqliteIndexBuilder>.Create();
        ColumnsByOriginalName = SqlDatabaseNamedObjectsSet<SqliteColumnBuilder>.Create();
    }

    public SqlDatabaseObjectsSet<SqliteColumnBuilder> CreatedColumns { get; }
    public SqlColumnModificationSourcesSet<SqliteColumnBuilder> ModifiedColumns { get; }
    public Dictionary<ulong, SqliteColumnRename> ColumnRenames { get; }
    public SqlDatabaseNamedObjectsSet<SqliteColumnBuilder> RemovedColumns { get; }
    public SqlDatabaseObjectsSet<SqliteIndexBuilder> CreatedIndexes { get; }
    public SqlDatabaseNamedObjectsSet<SqliteIndexBuilder> RemovedIndexes { get; }
    public SqlObjectOriginalValue<string> OriginalName { get; private set; }
    public SqlDatabaseNamedObjectsSet<SqliteColumnBuilder> ColumnsByOriginalName { get; }
    public bool RequiresReconstruction { get; private set; }
    public bool IsRenamed => OriginalName.Exists;

    public bool HasChanged =>
        IsRenamed
        || RequiresReconstruction
        || CreatedColumns.Count > 0
        || ModifiedColumns.Count > 0
        || RemovedColumns.Count > 0
        || CreatedIndexes.Count > 0
        || RemovedIndexes.Count > 0;

    public override void Clear()
    {
        CreatedColumns.Clear();
        ModifiedColumns.Clear();
        ColumnRenames.Clear();
        RemovedColumns.Clear();
        CreatedIndexes.Clear();
        RemovedIndexes.Clear();
        ColumnsByOriginalName.Clear();
        OriginalName = SqlObjectOriginalValue<string>.CreateEmpty();
        RequiresReconstruction = false;
    }

    protected override void HandleCreation(SqlObjectBuilder obj)
    {
        Assume.IsNotNull( Changes.ActiveObject );
        Assume.Equals( Changes.ActiveObject.Type, SqlObjectType.Table );

        switch ( obj.Type )
        {
            case SqlObjectType.Column:
                CreatedColumns.Add( ReinterpretCast.To<SqliteColumnBuilder>( obj ) );
                RequiresReconstruction = true;
                break;

            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                if ( ! index.IsVirtual )
                    CreatedIndexes.Add( index );

                break;
            }

            default:
                Assume.True( obj.Type is SqlObjectType.PrimaryKey or SqlObjectType.ForeignKey or SqlObjectType.Check );
                RequiresReconstruction = true;
                break;
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
                RemovedColumns.Add( name, ReinterpretCast.To<SqliteColumnBuilder>( obj ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                if ( index.IsVirtual && ! Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                    break;

                var name = Changes.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );
                RemovedIndexes.Add( name, index );
                break;
            }
            default:
                Assume.True( obj.Type is SqlObjectType.PrimaryKey or SqlObjectType.ForeignKey or SqlObjectType.Check );
                RequiresReconstruction = true;
                break;
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
                var column = ReinterpretCast.To<SqliteColumnBuilder>( obj );
                ModifiedColumns.Add( column );

                if ( descriptor != SqlObjectChangeDescriptor.Name )
                {
                    RequiresReconstruction = true;
                    break;
                }

                Assume.IsNotNull( originalValue );
                ColumnRenames.Add( column.Id, SqliteColumnRename.Create( column, ReinterpretCast.To<string>( originalValue ) ) );
                break;
            }
            case SqlObjectType.Index:
            {
                var index = ReinterpretCast.To<SqliteIndexBuilder>( obj );
                var originalName = Changes.GetOriginalValue( index, SqlObjectChangeDescriptor.Name ).GetValueOrDefault( index.Name );

                if ( index.IsVirtual )
                {
                    if ( Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                        RemovedIndexes.Add( originalName, index );

                    break;
                }

                if ( ! Changes.ContainsChange( index, SqlObjectChangeDescriptor.IsVirtual ) )
                    RemovedIndexes.Add( originalName, index );

                CreatedIndexes.Add( index );
                break;
            }
            default:
                Assume.True( obj.Type is SqlObjectType.PrimaryKey or SqlObjectType.ForeignKey or SqlObjectType.Check );
                RequiresReconstruction = true;
                break;
        }
    }

    internal void UpdateReconstructionRequirement(SqliteTableBuilder table)
    {
        Assume.Equals( table, Changes.ActiveObject );

        if ( RequiresReconstruction || ! IsRenamed )
            return;

        foreach ( var constraint in table.Constraints )
        {
            if ( constraint.Type != SqlObjectType.ForeignKey )
                continue;

            var foreignKey = ReinterpretCast.To<SqliteForeignKeyBuilder>( constraint );
            if ( foreignKey.IsSelfReference() )
            {
                RequiresReconstruction = true;
                return;
            }
        }
    }

    internal void PrepareColumnsForReconstruction(SqliteTableBuilder table)
    {
        Assume.True( RequiresReconstruction );
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
            ModifiedColumns.Add( new SqlColumnModificationSource<SqliteColumnBuilder>( column, removed ) );
        }
    }

    internal void PopulateColumnsByOriginalName()
    {
        Assume.False( RequiresReconstruction );
        foreach ( var rename in ColumnRenames.Values )
            ColumnsByOriginalName.Add( rename.OriginalName, rename.Column );
    }
}
