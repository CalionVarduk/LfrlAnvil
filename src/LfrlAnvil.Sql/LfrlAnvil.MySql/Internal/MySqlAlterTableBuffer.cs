using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql.Internal;

internal sealed class MySqlAlterTableBuffer
{
    internal readonly Dictionary<ulong, ObjectData> Objects;
    internal readonly Dictionary<ChangeKey, ChangeValue> PropertyChanges;
    internal readonly HashSet<string> DroppedIndexNames;
    internal readonly Dictionary<string, string> RenamedIndexes;
    internal readonly Dictionary<ulong, MySqlIndexBuilder> ModifiedIndexes;
    internal readonly Dictionary<ulong, MySqlIndexBuilder> CreatedIndexes;
    internal readonly HashSet<string> DroppedForeignKeyNames;
    internal readonly Dictionary<ulong, MySqlForeignKeyBuilder> CreatedForeignKeys;
    internal readonly HashSet<string> DroppedCheckNames;
    internal readonly Dictionary<ulong, MySqlCheckBuilder> CreatedChecks;
    internal readonly HashSet<string> DroppedColumnNames;
    internal readonly Dictionary<ulong, MySqlColumnBuilder> ModifiedColumns;
    internal readonly Dictionary<ulong, MySqlColumnBuilder> CreatedColumns;

    internal MySqlAlterTableBuffer()
    {
        Objects = new Dictionary<ulong, ObjectData>();
        PropertyChanges = new Dictionary<ChangeKey, ChangeValue>();
        DroppedIndexNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
        RenamedIndexes = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
        ModifiedIndexes = new Dictionary<ulong, MySqlIndexBuilder>();
        CreatedIndexes = new Dictionary<ulong, MySqlIndexBuilder>();
        DroppedForeignKeyNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
        CreatedForeignKeys = new Dictionary<ulong, MySqlForeignKeyBuilder>();
        DroppedCheckNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
        CreatedChecks = new Dictionary<ulong, MySqlCheckBuilder>();
        DroppedColumnNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
        ModifiedColumns = new Dictionary<ulong, MySqlColumnBuilder>();
        CreatedColumns = new Dictionary<ulong, MySqlColumnBuilder>();
    }

    internal ParseResult ParseChanges(MySqlTableBuilder table, ReadOnlySpan<MySqlDatabasePropertyChange> changes)
    {
        PopulateObjectsAndPropertyChanges( changes );
        var result = PopulateDetailedObjectChanges();

        foreach ( var column in table.Columns )
        {
            if ( ModifiedColumns.ContainsKey( column.Id ) ||
                ! CreatedColumns.ContainsKey( column.Id ) ||
                ! DroppedColumnNames.Remove( column.Name ) )
                continue;

            CreatedColumns.Remove( column.Id );
            ModifiedColumns.Add( column.Id, column );

            PropertyChanges.Remove( new ChangeKey( column.Id, MySqlObjectChangeDescriptor.Name ) );
            // TODO: "copy" changes from old column to new one
        }

        foreach ( var ix in ModifiedIndexes.Values )
        {
            foreach ( var fk in ix.OriginatingForeignKeys )
            {
                DroppedForeignKeyNames.Add( TryGetOldName( fk.Id ) ?? fk.Name );
                CreatedForeignKeys.TryAdd( fk.Id, fk );
            }
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? TryGetOldName(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, MySqlObjectChangeDescriptor.Name );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReinterpretCast.To<string>( change.OldValue )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal string? TryGetOldSchemaName(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, MySqlObjectChangeDescriptor.SchemaName );
        return PropertyChanges.TryGetValue( changeKey, out var change )
            ? ReinterpretCast.To<string>( change.OldValue )
            : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool ContainsPrimaryKeyChange(ulong objectId)
    {
        var changeKey = new ChangeKey( objectId, MySqlObjectChangeDescriptor.PrimaryKey );
        return PropertyChanges.ContainsKey( changeKey );
    }

    internal void Clear()
    {
        Objects.Clear();
        PropertyChanges.Clear();
        DroppedIndexNames.Clear();
        RenamedIndexes.Clear();
        ModifiedIndexes.Clear();
        CreatedIndexes.Clear();
        DroppedForeignKeyNames.Clear();
        CreatedForeignKeys.Clear();
        DroppedCheckNames.Clear();
        CreatedChecks.Clear();
        DroppedColumnNames.Clear();
        ModifiedColumns.Clear();
        CreatedColumns.Clear();
    }

    internal readonly record struct ParseResult(bool HasChanged, bool HasPrimaryKeyChanged, bool IsTableRenamed);

    internal readonly record struct ChangeKey(ulong ObjectId, MySqlObjectChangeDescriptor Descriptor);

    internal readonly record struct ChangeValue(object? OldValue, object? NewValue);

    internal readonly record struct ObjectData(MySqlObjectStatus Status, MySqlObjectBuilder Object);

    private void PopulateObjectsAndPropertyChanges(ReadOnlySpan<MySqlDatabasePropertyChange> changes)
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
        var hasPrimaryKeyChanged = false;
        var isTableRenamed = false;

        foreach ( var (key, value) in PropertyChanges )
        {
            var data = Objects[key.ObjectId];
            if ( (data.Status & MySqlObjectStatus.Unused) == MySqlObjectStatus.Unused )
                continue;

            if ( (data.Status & MySqlObjectStatus.Removed) != MySqlObjectStatus.None )
            {
                if ( key.Descriptor != MySqlObjectChangeDescriptor.Exists )
                {
                    var existsChangeKey = new ChangeKey( key.ObjectId, MySqlObjectChangeDescriptor.Exists );
                    Assume.True( PropertyChanges.ContainsKey( existsChangeKey ) );
                    continue;
                }

                hasChanged = true;

                if ( data.Object.Type == SqlObjectType.Index )
                {
                    if ( ContainsPrimaryKeyChange( key.ObjectId ) )
                        continue;

                    var oldName = TryGetOldName( key.ObjectId );
                    DroppedIndexNames.Add( oldName ?? data.Object.Name );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.ForeignKey )
                {
                    var oldName = TryGetOldName( key.ObjectId );
                    DroppedForeignKeyNames.Add( oldName ?? data.Object.Name );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Check )
                {
                    var oldName = TryGetOldName( key.ObjectId );
                    DroppedCheckNames.Add( oldName ?? data.Object.Name );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Column )
                {
                    var name = TryGetOldName( key.ObjectId ) ?? data.Object.Name;
                    DroppedColumnNames.Add( name );
                }

                continue;
            }

            if ( (data.Status & MySqlObjectStatus.Created) != MySqlObjectStatus.None )
            {
                if ( key.Descriptor != MySqlObjectChangeDescriptor.Exists )
                {
                    var existsChangeKey = new ChangeKey( key.ObjectId, MySqlObjectChangeDescriptor.Exists );
                    Assume.True( PropertyChanges.ContainsKey( existsChangeKey ) );
                    continue;
                }

                hasChanged = true;

                if ( data.Object.Type == SqlObjectType.Index )
                {
                    var ix = ReinterpretCast.To<MySqlIndexBuilder>( data.Object );
                    if ( ix.PrimaryKey is null )
                        CreatedIndexes.Add( key.ObjectId, ix );

                    continue;
                }

                if ( data.Object.Type == SqlObjectType.PrimaryKey )
                {
                    hasPrimaryKeyChanged = true;
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.ForeignKey )
                {
                    CreatedForeignKeys.Add( key.ObjectId, ReinterpretCast.To<MySqlForeignKeyBuilder>( data.Object ) );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Check )
                {
                    CreatedChecks.Add( key.ObjectId, ReinterpretCast.To<MySqlCheckBuilder>( data.Object ) );
                    continue;
                }

                if ( data.Object.Type == SqlObjectType.Column )
                    CreatedColumns.Add( key.ObjectId, ReinterpretCast.To<MySqlColumnBuilder>( data.Object ) );

                continue;
            }

            Assume.Equals( data.Status, MySqlObjectStatus.Modified );
            if ( Equals( value.OldValue, value.NewValue ) )
                continue;

            hasChanged = true;

            if ( data.Object.Type == SqlObjectType.Index )
            {
                var ix = ReinterpretCast.To<MySqlIndexBuilder>( data.Object );
                var oldName = key.Descriptor == MySqlObjectChangeDescriptor.Name
                    ? ReinterpretCast.To<string>( value.OldValue )
                    : TryGetOldName( key.ObjectId );

                if ( ix.PrimaryKey is not null )
                {
                    if ( ContainsPrimaryKeyChange( key.ObjectId ) )
                    {
                        DroppedIndexNames.Add( oldName ?? ix.Name );
                        ModifiedIndexes.TryAdd( key.ObjectId, ix );
                    }

                    continue;
                }

                if ( key.Descriptor == MySqlObjectChangeDescriptor.Name )
                {
                    if ( ! ModifiedIndexes.ContainsKey( key.ObjectId ) )
                        RenamedIndexes.Add( oldName ?? ix.Name, ix.Name );

                    continue;
                }

                RenamedIndexes.Remove( oldName ?? ix.Name );
                DroppedIndexNames.Add( oldName ?? ix.Name );
                CreatedIndexes.TryAdd( key.ObjectId, ix );
                ModifiedIndexes.TryAdd( key.ObjectId, ix );
                continue;
            }

            if ( data.Object.Type == SqlObjectType.PrimaryKey )
            {
                hasPrimaryKeyChanged = true;
                continue;
            }

            if ( data.Object.Type == SqlObjectType.ForeignKey )
            {
                var oldName = key.Descriptor == MySqlObjectChangeDescriptor.Name
                    ? ReinterpretCast.To<string>( value.OldValue )
                    : TryGetOldName( key.ObjectId );

                DroppedForeignKeyNames.Add( oldName ?? data.Object.Name );
                CreatedForeignKeys.TryAdd( key.ObjectId, ReinterpretCast.To<MySqlForeignKeyBuilder>( data.Object ) );
                continue;
            }

            if ( data.Object.Type == SqlObjectType.Check )
            {
                var oldName = key.Descriptor == MySqlObjectChangeDescriptor.Name
                    ? ReinterpretCast.To<string>( value.OldValue )
                    : TryGetOldName( key.ObjectId );

                DroppedCheckNames.Add( oldName ?? data.Object.Name );
                CreatedChecks.TryAdd( key.ObjectId, ReinterpretCast.To<MySqlCheckBuilder>( data.Object ) );
                continue;
            }

            if ( data.Object.Type == SqlObjectType.Column )
            {
                ModifiedColumns.TryAdd( key.ObjectId, ReinterpretCast.To<MySqlColumnBuilder>( data.Object ) );
                continue;
            }

            if ( data.Object.Type == SqlObjectType.Table )
            {
                if ( key.Descriptor == MySqlObjectChangeDescriptor.PrimaryKey )
                    hasPrimaryKeyChanged = true;
                else if ( key.Descriptor is MySqlObjectChangeDescriptor.Name or MySqlObjectChangeDescriptor.SchemaName )
                    isTableRenamed = true;
            }
        }

        return new ParseResult( hasChanged, hasPrimaryKeyChanged, isTableRenamed );
    }
}
