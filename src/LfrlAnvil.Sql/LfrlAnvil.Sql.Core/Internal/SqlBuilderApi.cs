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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a single building block of an SQL database.
/// </summary>
public abstract class SqlBuilderApi
{
    /// <summary>
    /// Registers a reference in the provided object's <see cref="SqlObjectBuilder.ReferencingObjects"/> collection.
    /// </summary>
    /// <param name="obj">Object to register a reference in.</param>
    /// <param name="source">Reference to register.</param>
    /// <param name="target">Optional target (sub-object) of the reference. Equal to null by default.</param>
    /// <returns><b>true</b> when reference was registered, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static bool AddReference(
        SqlObjectBuilder obj,
        SqlObjectBuilderReferenceSource<SqlObjectBuilder> source,
        SqlObjectBuilder? target = null)
    {
        obj.ReferencedTargets ??= new Dictionary<SqlObjectBuilderReferenceSource<SqlObjectBuilder>, SqlObjectBuilder>();
        return obj.ReferencedTargets.TryAdd( source, target ?? obj );
    }

    /// <summary>
    /// Removes a reference from the provided object's <see cref="SqlObjectBuilder.ReferencingObjects"/> collection.
    /// </summary>
    /// <param name="obj">Object to remove a reference from.</param>
    /// <param name="source">Reference to remove.</param>
    /// <returns><b>true</b> when reference was removed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static bool RemoveReference(SqlObjectBuilder obj, SqlObjectBuilderReferenceSource<SqlObjectBuilder> source)
    {
        return obj.ReferencedTargets?.Remove( source ) ?? false;
    }

    /// <summary>
    /// Removes all references from the provided object's <see cref="SqlObjectBuilder.ReferencingObjects"/> collection.
    /// </summary>
    /// <param name="obj">Object to remove all references from.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearReferences(SqlObjectBuilder obj)
    {
        obj.ReferencedTargets?.Clear();
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/> to the objects <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to add an object to.</param>
    /// <param name="obj">Object to add.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddToCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj)
    {
        collection.Add( obj );
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/> to the constraints <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to add an object to.</param>
    /// <param name="obj">Object to add.</param>
    /// <remarks>This method automatically adds the <paramref name="obj"/> to the objects collection of the related schema.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddToCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj)
    {
        AddToCollection( obj.Table.Schema.Objects, obj );
        collection.Add( obj );
    }

    /// <summary>
    /// Changes the name of the provided <paramref name="obj"/> in the columns <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to change the name in.</param>
    /// <param name="obj">Column whose name will be changed.</param>
    /// <param name="newName">New name of the column.</param>
    /// <exception cref="SqlObjectBuilderException">
    /// When new name already exists in the <paramref name="collection"/> or the name is not valid.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlColumnBuilderCollection collection, SqlColumnBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    /// <summary>
    /// Changes the name of the provided <paramref name="obj"/> in the objects <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to change the name in.</param>
    /// <param name="obj">Object whose name will be changed.</param>
    /// <param name="newName">New name of the object.</param>
    /// <exception cref="SqlObjectBuilderException">
    /// When new name already exists in the <paramref name="collection"/> or the name is not valid.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    /// <summary>
    /// Changes the name of the provided <paramref name="obj"/> in the constraints <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to change the name in.</param>
    /// <param name="obj">Constraint whose name will be changed.</param>
    /// <param name="newName">New name of the constraint.</param>
    /// <exception cref="SqlObjectBuilderException">
    /// When new name already exists in the <paramref name="collection"/> or the name is not valid.
    /// </exception>
    /// <remarks>This method automatically changes the name in the objects collection of the related schema.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj, string newName)
    {
        ChangeNameInCollection( obj.Table.Schema.Objects, obj, newName );
        collection.ChangeName( obj, newName );
    }

    /// <summary>
    /// Changes the name of the provided <paramref name="obj"/> in the schemas <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to change the name in.</param>
    /// <param name="obj">Schema whose name will be changed.</param>
    /// <param name="newName">New name of the schema.</param>
    /// <exception cref="SqlObjectBuilderException">
    /// When new name already exists in the <paramref name="collection"/> or the name is not valid.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ChangeNameInCollection(SqlSchemaBuilderCollection collection, SqlSchemaBuilder obj, string newName)
    {
        collection.ChangeName( obj, newName );
    }

    /// <summary>
    /// Removes the provided <paramref name="obj"/> from the columns <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to remove a column from.</param>
    /// <param name="obj">Column to remove.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlColumnBuilderCollection collection, SqlColumnBuilder obj)
    {
        collection.Remove( obj );
    }

    /// <summary>
    /// Removes the provided <paramref name="obj"/> from the objects <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to remove an object from.</param>
    /// <param name="obj">Object to remove.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlObjectBuilderCollection collection, SqlObjectBuilder obj)
    {
        collection.Remove( obj );
    }

    /// <summary>
    /// Removes the provided <paramref name="obj"/> from the constraints <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to remove a constraint from.</param>
    /// <param name="obj">Constraint to remove.</param>
    /// <remarks>This method automatically removed the <paramref name="obj"/> from the objects collection of the related schema.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlConstraintBuilderCollection collection, SqlConstraintBuilder obj)
    {
        RemoveFromCollection( obj.Table.Schema.Objects, obj );
        collection.Remove( obj );
    }

    /// <summary>
    /// Removes the provided <paramref name="obj"/> from the schemas <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to remove a schema from.</param>
    /// <param name="obj">Schema to remove.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void RemoveFromCollection(SqlSchemaBuilderCollection collection, SqlSchemaBuilder obj)
    {
        collection.Remove( obj );
    }

    /// <summary>
    /// Removes all columns from the columns <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to clear.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlColumnBuilderCollection collection)
    {
        collection.Clear();
    }

    /// <summary>
    /// Removes all objects from the objects <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to clear.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlObjectBuilderCollection collection)
    {
        collection.Clear();
    }

    /// <summary>
    /// Removes all constraints from the constraints <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection to clear.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ClearCollection(SqlConstraintBuilderCollection collection)
    {
        collection.Clear();
    }

    /// <summary>
    /// Performs a quick removal of the provided <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object to remove.</param>
    /// <remarks>
    /// Quick removal is faster than normal removal but it is less safe and it should only be used
    /// when the object's parent is being removed.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void QuickRemove(SqlObjectBuilder obj)
    {
        obj.QuickRemove();
    }

    /// <summary>
    /// Returns the underlying object pool.
    /// </summary>
    /// <param name="database">Source of the object pool.</param>
    /// <returns>Database's underlying object pool.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static MemorySequencePool<SqlObjectBuilder> GetObjectPool(SqlDatabaseBuilder database)
    {
        return database.ObjectPool;
    }

    /// <summary>
    /// Registers an object creation event in the related database's change tracker.
    /// </summary>
    /// <param name="activeObject">Object for which the change should be registered.</param>
    /// <param name="target">Created object.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddCreation(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        target.Database.Changes.Created( activeObject, target );
    }

    /// <summary>
    /// Registers an object removal event in the related database's change tracker.
    /// </summary>
    /// <param name="activeObject">Object for which the change should be registered.</param>
    /// <param name="target">Removed object.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddRemoval(SqlObjectBuilder activeObject, SqlObjectBuilder target)
    {
        target.Database.Changes.Removed( activeObject, target );
    }

    /// <summary>
    /// Registers an object <see cref="SqlObjectBuilder.Name"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="activeObject">Object for which the change should be registered.</param>
    /// <param name="target">Renamed object.</param>
    /// <param name="originalValue">Object's <see cref="SqlObjectBuilder.Name"/> before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddNameChange(SqlObjectBuilder activeObject, SqlObjectBuilder target, string originalValue)
    {
        target.Database.Changes.NameChanged( activeObject, target, originalValue );
    }

    /// <summary>
    /// Registers a column's <see cref="SqlColumnBuilder.IsNullable"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified column.</param>
    /// <param name="originalValue">Column's <see cref="SqlColumnBuilder.IsNullable"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddIsNullableChange(SqlColumnBuilder target, bool originalValue)
    {
        target.Database.Changes.IsNullableChanged( target, originalValue );
    }

    /// <summary>
    /// Registers a column's <see cref="SqlColumnBuilder.TypeDefinition"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified column.</param>
    /// <param name="originalValue">Column's <see cref="SqlColumnBuilder.TypeDefinition"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddTypeDefinitionChange(SqlColumnBuilder target, SqlColumnTypeDefinition originalValue)
    {
        target.Database.Changes.TypeDefinitionChanged( target, originalValue );
    }

    /// <summary>
    /// Registers a column's <see cref="SqlColumnBuilder.DefaultValue"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified column.</param>
    /// <param name="originalValue">Column's <see cref="SqlColumnBuilder.DefaultValue"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddDefaultValueChange(SqlColumnBuilder target, SqlExpressionNode? originalValue)
    {
        target.Database.Changes.DefaultValueChanged( target, originalValue );
    }

    /// <summary>
    /// Registers a column's <see cref="SqlColumnBuilder.Computation"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified column.</param>
    /// <param name="originalValue">Column's <see cref="SqlColumnBuilder.Computation"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddComputationChange(SqlColumnBuilder target, SqlColumnComputation? originalValue)
    {
        target.Database.Changes.ComputationChanged( target, originalValue );
    }

    /// <summary>
    /// Registers an index's <see cref="SqlIndexBuilder.IsUnique"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified index.</param>
    /// <param name="originalValue">Index's <see cref="SqlIndexBuilder.IsUnique"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddIsUniqueChange(SqlIndexBuilder target, bool originalValue)
    {
        target.Database.Changes.IsUniqueChanged( target, originalValue );
    }

    /// <summary>
    /// Registers an index's <see cref="SqlIndexBuilder.IsVirtual"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified index.</param>
    /// <param name="originalValue">Index's <see cref="SqlIndexBuilder.IsVirtual"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddIsVirtualChange(SqlIndexBuilder target, bool originalValue)
    {
        target.Database.Changes.IsVirtualChanged( target, originalValue );
    }

    /// <summary>
    /// Registers an index's <see cref="SqlIndexBuilder.Filter"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified index.</param>
    /// <param name="originalValue">Index's <see cref="SqlIndexBuilder.Filter"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddFilterChange(SqlIndexBuilder target, SqlConditionNode? originalValue)
    {
        target.Database.Changes.FilterChanged( target, originalValue );
    }

    /// <summary>
    /// Registers an index's <see cref="SqlIndexBuilder.PrimaryKey"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified index.</param>
    /// <param name="originalValue">Index's <see cref="SqlIndexBuilder.PrimaryKey"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddPrimaryKeyChange(SqlIndexBuilder target, SqlPrimaryKeyBuilder? originalValue)
    {
        target.Database.Changes.PrimaryKeyChanged( target, originalValue );
    }

    /// <summary>
    /// Registers a foreign key's <see cref="SqlForeignKeyBuilder.OnDeleteBehavior"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified foreign key.</param>
    /// <param name="originalValue">Foreign key's <see cref="SqlForeignKeyBuilder.OnDeleteBehavior"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddOnDeleteBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        target.Database.Changes.OnDeleteBehaviorChanged( target, originalValue );
    }

    /// <summary>
    /// Registers a foreign key's <see cref="SqlForeignKeyBuilder.OnUpdateBehavior"/> change event in the related database's change tracker.
    /// </summary>
    /// <param name="target">Modified foreign key.</param>
    /// <param name="originalValue">Foreign key's <see cref="SqlForeignKeyBuilder.OnUpdateBehavior"/> value before the change.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void AddOnUpdateBehaviorChange(SqlForeignKeyBuilder target, ReferenceBehavior originalValue)
    {
        target.Database.Changes.OnUpdateBehaviorChanged( target, originalValue );
    }

    /// <summary>
    /// Resets the underlying <see cref="SqlTableBuilder.Info"/> cache.
    /// </summary>
    /// <param name="target">Table to reset the cache for.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ResetInfo(SqlTableBuilder target)
    {
        target.ResetInfoCache();
    }

    /// <summary>
    /// Resets the underlying <see cref="SqlViewBuilder.Info"/> cache.
    /// </summary>
    /// <param name="target">View to reset the cache for.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static void ResetInfo(SqlViewBuilder target)
    {
        target.ResetInfoCache();
    }
}
