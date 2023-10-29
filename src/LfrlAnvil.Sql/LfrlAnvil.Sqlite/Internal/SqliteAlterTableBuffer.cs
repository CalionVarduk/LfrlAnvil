using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteAlterTableBuffer
{
    internal readonly Dictionary<ulong, ObjectData> Objects;
    internal readonly Dictionary<ChangeKey, ChangeValue> PropertyChanges;
    internal readonly HashSet<SqlSchemaObjectName> DroppedIndexNames;
    internal readonly Dictionary<ulong, SqliteIndexBuilder> CreatedIndexes;
    internal readonly Dictionary<string, SqliteColumnBuilder> DroppedColumnsByName;
    internal readonly Dictionary<ulong, SqliteColumnBuilder> CreatedColumns;
    internal readonly HashSet<ulong> ModifiedColumns;
    internal readonly Dictionary<ulong, ColumnRename> ColumnRenames;
    internal readonly Dictionary<string, ulong> ColumnIdsByCurrentName;

    internal SqliteAlterTableBuffer()
    {
        Objects = new Dictionary<ulong, ObjectData>();
        PropertyChanges = new Dictionary<ChangeKey, ChangeValue>();
        DroppedIndexNames = new HashSet<SqlSchemaObjectName>();
        CreatedIndexes = new Dictionary<ulong, SqliteIndexBuilder>();
        DroppedColumnsByName = new Dictionary<string, SqliteColumnBuilder>();
        CreatedColumns = new Dictionary<ulong, SqliteColumnBuilder>();
        ModifiedColumns = new HashSet<ulong>();
        ColumnRenames = new Dictionary<ulong, ColumnRename>();
        ColumnIdsByCurrentName = new Dictionary<string, ulong>();
    }

    internal ParseResult ParseChanges(SqliteTableBuilder table, ReadOnlySpan<SqliteDatabasePropertyChange> changes)
    {
        PopulateObjectsAndPropertyChanges( changes );
        var result = PopulateDetailedObjectChanges();

        if ( result.RequiresReconstruction )
            PrepareColumnChangesForReconstruction( table );
        else
            PrepareForColumnRenaming();

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? TryGetOldName(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, SqliteObjectChangeDescriptor.Name );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReinterpretCast.To<string>( change.OldValue )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? TryGetOldSchemaName(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, SqliteObjectChangeDescriptor.SchemaName );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReinterpretCast.To<string>( change.OldValue )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool? TryGetOldIsNullable(ulong columnId)
    {
        var changeKey = new ChangeKey( columnId, SqliteObjectChangeDescriptor.IsNullable );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReferenceEquals( change.OldValue, Boxed.True )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal SqliteDataType? TryGetOldDataType(ulong columnId)
    {
        var changeKey = new ChangeKey( columnId, SqliteObjectChangeDescriptor.DataType );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReinterpretCast.To<SqliteDataType>( change.OldValue )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsPrimaryKeyChange(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, SqliteObjectChangeDescriptor.PrimaryKey );
        return PropertyChanges.ContainsKey( changeKey );
    }

    internal void Clear()
    {
        Objects.Clear();
        PropertyChanges.Clear();
        DroppedIndexNames.Clear();
        CreatedIndexes.Clear();
        DroppedColumnsByName.Clear();
        CreatedColumns.Clear();
        ModifiedColumns.Clear();
        ColumnRenames.Clear();
        ColumnIdsByCurrentName.Clear();
    }

    internal readonly record struct ParseResult(bool HasChanged, bool RequiresReconstruction, bool IsTableRenamed);

    internal readonly record struct ChangeKey(ulong ObjectId, SqliteObjectChangeDescriptor Descriptor);

    internal readonly record struct ChangeValue(object? OldValue, object? NewValue);

    internal readonly record struct ObjectData(SqliteObjectStatus Status, SqliteObjectBuilder Object);

    internal readonly record struct ColumnRename(string OldName, string NewName, bool IsPending);

    private void PopulateObjectsAndPropertyChanges(ReadOnlySpan<SqliteDatabasePropertyChange> changes)
    {
        foreach ( var change in changes )
        {
            ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault( Objects, change.Object.Id, out var exists );
            data = new ObjectData( data.Status | change.Status, change.Object );

            var key = new ChangeKey( change.Object.Id, change.Descriptor );
            ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault( PropertyChanges, key, out exists );
            entry = new ChangeValue( exists ? entry.OldValue : change.OldValue, change.NewValue );
        }
    }

    private ParseResult PopulateDetailedObjectChanges()
    {
        var hasChanged = false;
        var requiresReconstruction = false;
        var isTableRenamed = false;

        foreach ( var (key, value) in PropertyChanges )
        {
            var data = Objects[key.ObjectId];
            if ( (data.Status & SqliteObjectStatus.Unused) == SqliteObjectStatus.Unused )
                continue;

            if ( (data.Status & SqliteObjectStatus.Removed) != SqliteObjectStatus.None )
            {
                if ( key.Descriptor != SqliteObjectChangeDescriptor.Exists )
                {
                    var existsChangeKey = new ChangeKey( key.ObjectId, SqliteObjectChangeDescriptor.Exists );
                    Assume.True( PropertyChanges.ContainsKey( existsChangeKey ) );
                    continue;
                }

                hasChanged = true;

                if ( data.Object.Type == SqlObjectType.Index )
                {
                    if ( ContainsPrimaryKeyChange( key.ObjectId ) )
                        continue;

                    var ix = ReinterpretCast.To<SqliteIndexBuilder>( data.Object );
                    var oldName = TryGetOldName( key.ObjectId );
                    var oldSchemaName = TryGetOldSchemaName( key.ObjectId );
                    DroppedIndexNames.Add( SqlSchemaObjectName.Create( oldSchemaName ?? ix.Table.Schema.Name, oldName ?? ix.Name ) );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Column )
                {
                    var name = TryGetOldName( key.ObjectId ) ?? data.Object.Name;
                    DroppedColumnsByName.Add( name, ReinterpretCast.To<SqliteColumnBuilder>( data.Object ) );
                    continue;
                }

                requiresReconstruction = true;
                continue;
            }

            if ( (data.Status & SqliteObjectStatus.Created) != SqliteObjectStatus.None )
            {
                if ( key.Descriptor != SqliteObjectChangeDescriptor.Exists )
                {
                    var existsChangeKey = new ChangeKey( key.ObjectId, SqliteObjectChangeDescriptor.Exists );
                    Assume.True( PropertyChanges.ContainsKey( existsChangeKey ) );
                    continue;
                }

                hasChanged = true;

                if ( data.Object.Type == SqlObjectType.Index )
                {
                    var ix = ReinterpretCast.To<SqliteIndexBuilder>( data.Object );
                    if ( ix.PrimaryKey is null )
                        CreatedIndexes.Add( key.ObjectId, ix );

                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Column )
                    CreatedColumns.Add( key.ObjectId, ReinterpretCast.To<SqliteColumnBuilder>( data.Object ) );

                requiresReconstruction = true;
                continue;
            }

            Assume.Equals( data.Status, SqliteObjectStatus.Modified );
            if ( Equals( value.OldValue, value.NewValue ) )
                continue;

            hasChanged = true;

            if ( data.Object.Type == SqlObjectType.Index )
            {
                var ix = ReinterpretCast.To<SqliteIndexBuilder>( data.Object );

                var oldName = key.Descriptor == SqliteObjectChangeDescriptor.Name
                    ? ReinterpretCast.To<string>( value.OldValue )
                    : TryGetOldName( key.ObjectId );

                var oldSchemaName = key.Descriptor == SqliteObjectChangeDescriptor.SchemaName
                    ? ReinterpretCast.To<string>( value.OldValue )
                    : TryGetOldSchemaName( key.ObjectId );

                if ( ix.PrimaryKey is not null )
                {
                    if ( ContainsPrimaryKeyChange( key.ObjectId ) )
                        DroppedIndexNames.Add( SqlSchemaObjectName.Create( oldSchemaName ?? ix.Table.Schema.Name, oldName ?? ix.Name ) );

                    continue;
                }

                DroppedIndexNames.Add( SqlSchemaObjectName.Create( oldSchemaName ?? ix.Table.Schema.Name, oldName ?? ix.Name ) );
                CreatedIndexes.TryAdd( key.ObjectId, ix );
                continue;
            }

            if ( data.Object.Type == SqlObjectType.Column )
            {
                ModifiedColumns.Add( key.ObjectId );

                if ( key.Descriptor == SqliteObjectChangeDescriptor.Name )
                {
                    Assume.IsNotNull( value.OldValue );
                    Assume.IsNotNull( value.NewValue );
                    var rename = new ColumnRename(
                        ReinterpretCast.To<string>( value.OldValue ),
                        ReinterpretCast.To<string>( value.NewValue ),
                        IsPending: true );

                    ColumnRenames.Add( key.ObjectId, rename );
                    continue;
                }

                requiresReconstruction = true;
                continue;
            }

            if ( data.Object.Type == SqlObjectType.Table &&
                key.Descriptor is SqliteObjectChangeDescriptor.Name or SqliteObjectChangeDescriptor.SchemaName )
            {
                isTableRenamed = true;
                continue;
            }

            requiresReconstruction = true;
        }

        return new ParseResult( hasChanged, requiresReconstruction, isTableRenamed );
    }

    private void PrepareColumnChangesForReconstruction(SqliteTableBuilder table)
    {
        foreach ( var column in table.Columns )
        {
            if ( ModifiedColumns.Contains( column.Id ) ||
                ! CreatedColumns.ContainsKey( column.Id ) ||
                ! DroppedColumnsByName.Remove( column.Name, out var removed ) )
                continue;

            CreatedColumns.Remove( column.Id );
            ModifiedColumns.Add( column.Id );

            PropertyChanges.Remove( new ChangeKey( column.Id, SqliteObjectChangeDescriptor.Name ) );

            var oldKey = new ChangeKey( removed.Id, SqliteObjectChangeDescriptor.IsNullable );
            var oldValue = PropertyChanges.TryGetValue( oldKey, out var oldChange )
                ? oldChange.OldValue
                : Boxed.GetBool( removed.IsNullable );

            var key = new ChangeKey( column.Id, SqliteObjectChangeDescriptor.IsNullable );
            PropertyChanges[key] = new ChangeValue( oldValue, Boxed.GetBool( column.IsNullable ) );

            oldKey = new ChangeKey( removed.Id, SqliteObjectChangeDescriptor.DataType );
            oldValue = PropertyChanges.TryGetValue( oldKey, out oldChange ) ? oldChange.OldValue : removed.TypeDefinition.DbType;
            key = new ChangeKey( column.Id, SqliteObjectChangeDescriptor.DataType );
            PropertyChanges[key] = new ChangeValue( oldValue, column.TypeDefinition.DbType );
        }
    }

    private void PrepareForColumnRenaming()
    {
        foreach ( var (id, rename) in ColumnRenames )
            ColumnIdsByCurrentName.Add( rename.OldName, id );
    }
}
