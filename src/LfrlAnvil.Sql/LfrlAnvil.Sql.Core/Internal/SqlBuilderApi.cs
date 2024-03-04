using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public abstract class SqlBuilderApi
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static bool AddReference(
        SqlObjectBuilder obj,
        SqlObjectBuilderReferenceSource<SqlObjectBuilder> source,
        SqlObjectBuilder? target = null)
    {
        obj.ReferencedTargets ??= new Dictionary<SqlObjectBuilderReferenceSource<SqlObjectBuilder>, SqlObjectBuilder>();
        return obj.ReferencedTargets.TryAdd( source, target ?? obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static bool RemoveReference(SqlObjectBuilder obj, SqlObjectBuilderReferenceSource<SqlObjectBuilder> source)
    {
        return obj.ReferencedTargets?.Remove( source ) ?? false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearReferences(SqlObjectBuilder obj)
    {
        obj.ReferencedTargets?.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddToCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj)
    {
        collection.Add( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddToCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj)
    {
        AddToCollection( obj.Table.Schema.Objects, obj );
        collection.Add( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlColumnBuilderCollection collection, SqlColumnBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj, string newName)
    {
        ChangeNameInCollection( obj.Table.Schema.Objects, obj, newName );
        collection.ChangeName( obj, newName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlSchemaBuilderCollection collection, SqlSchemaBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlColumnBuilderCollection collection, SqlColumnBuilder obj)
    {
        collection.Remove( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj)
    {
        collection.Remove( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj)
    {
        RemoveFromCollection( obj.Table.Schema.Objects, obj );
        collection.Remove( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlSchemaBuilderCollection collection, SqlSchemaBuilder obj)
    {
        collection.Remove( obj );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlColumnBuilderCollection collection)
    {
        collection.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlObjectBuilderCollection collection)
    {
        collection.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlConstraintBuilderCollection collection)
    {
        collection.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void QuickRemove(SqlObjectBuilder obj)
    {
        obj.QuickRemove();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MemorySequencePool<SqlObjectBuilder> GetObjectPool(SqlDatabaseBuilder database)
    {
        return database.ObjectPool;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddCreation(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        target.Database.Changes.Created( activeObject, target );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddRemoval(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        target.Database.Changes.Removed( activeObject, target );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        target.Database.Changes.NameChanged( activeObject, target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddIsNullableChange(SqlColumnBuilder target, bool originalValue)
    {
        target.Database.Changes.IsNullableChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddTypeDefinitionChange(SqlColumnBuilder target, SqlColumnTypeDefinition originalValue)
    {
        target.Database.Changes.TypeDefinitionChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddDefaultValueChange(SqlColumnBuilder target, SqlExpressionNode? originalValue)
    {
        target.Database.Changes.DefaultValueChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddIsUniqueChange(SqlIndexBuilder target, bool originalValue)
    {
        target.Database.Changes.IsUniqueChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddFilterChange(SqlIndexBuilder target, SqlConditionNode? originalValue)
    {
        target.Database.Changes.FilterChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddPrimaryKeyChange(SqlIndexBuilder target, SqlPrimaryKeyBuilder? originalValue)
    {
        target.Database.Changes.PrimaryKeyChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddOnDeleteBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        target.Database.Changes.OnDeleteBehaviorChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddOnUpdateBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        target.Database.Changes.OnUpdateBehaviorChanged( target, originalValue );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ResetInfo(SqlTableBuilder target)
    {
        target.ResetInfoCache();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ResetInfo(SqlViewBuilder target)
    {
        target.ResetInfoCache();
    }
}
