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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlForeignKeyBuilder" />
public abstract class SqlForeignKeyBuilder : SqlConstraintBuilder, ISqlForeignKeyBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlForeignKeyBuilder"/> instance.
    /// </summary>
    /// <param name="originIndex">SQL index that this foreign key originates from.</param>
    /// <param name="referencedIndex">SQL index referenced by this foreign key.</param>
    /// <param name="name">Object's name.</param>
    protected SqlForeignKeyBuilder(SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex, string name)
        : base( originIndex.Table, SqlObjectType.ForeignKey, name )
    {
        OriginIndex = originIndex;
        ReferencedIndex = referencedIndex;
        OnDeleteBehavior = ReferenceBehavior.Restrict;
        OnUpdateBehavior = ReferenceBehavior.Restrict;
        AddIndexReferences();
    }

    /// <inheritdoc cref="ISqlForeignKeyBuilder.OriginIndex" />
    public SqlIndexBuilder OriginIndex { get; }

    /// <inheritdoc cref="ISqlForeignKeyBuilder.ReferencedIndex" />
    public SqlIndexBuilder ReferencedIndex { get; }

    /// <inheritdoc />
    public ReferenceBehavior OnDeleteBehavior { get; private set; }

    /// <inheritdoc />
    public ReferenceBehavior OnUpdateBehavior { get; private set; }

    ISqlIndexBuilder ISqlForeignKeyBuilder.OriginIndex => OriginIndex;
    ISqlIndexBuilder ISqlForeignKeyBuilder.ReferencedIndex => ReferencedIndex;

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="ISqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior)" />
    public SqlForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        ThrowIfRemoved();
        var change = BeforeOnDeleteBehaviorChange( behavior );
        if ( change.IsCancelled )
            return this;

        var originalValue = OnDeleteBehavior;
        OnDeleteBehavior = change.NewValue;
        AfterOnDeleteBehaviorChange( originalValue );
        return this;
    }

    /// <inheritdoc cref="ISqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior)" />
    public SqlForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        ThrowIfRemoved();
        var change = BeforeOnUpdateBehaviorChange( behavior );
        if ( change.IsCancelled )
            return this;

        var originalValue = OnUpdateBehavior;
        OnUpdateBehavior = change.NewValue;
        AfterOnUpdateBehaviorChange( originalValue );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForForeignKey( OriginIndex, ReferencedIndex );
    }

    /// <summary>
    /// Callback invoked just before <see cref="OnDeleteBehavior"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="OnDeleteBehavior"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="OnDeleteBehavior"/> of this foreign key cannot be changed.</exception>
    protected virtual SqlPropertyChange<ReferenceBehavior> BeforeOnDeleteBehaviorChange(ReferenceBehavior newValue)
    {
        if ( OnDeleteBehavior == newValue )
            return SqlPropertyChange.Cancel<ReferenceBehavior>();

        if ( newValue == ReferenceBehavior.SetNull )
            ThrowIfCannotUseSetNullReferenceBehavior();

        return newValue;
    }

    /// <summary>
    /// Callback invoked just after <see cref="OnDeleteBehavior"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterOnDeleteBehaviorChange(ReferenceBehavior originalValue)
    {
        AddOnDeleteBehaviorChange( this, originalValue );
    }

    /// <summary>
    /// Callback invoked just before <see cref="OnUpdateBehavior"/> change is processed.
    /// </summary>
    /// <param name="newValue">Value to set.</param>
    /// <returns><see cref="SqlPropertyChange{T}"/> instance associated with <see cref="OnUpdateBehavior"/> change attempt.</returns>
    /// <exception cref="SqlObjectBuilderException">When <see cref="OnUpdateBehavior"/> of this foreign key cannot be changed.</exception>
    protected virtual SqlPropertyChange<ReferenceBehavior> BeforeOnUpdateBehaviorChange(ReferenceBehavior newValue)
    {
        if ( OnUpdateBehavior == newValue )
            return SqlPropertyChange.Cancel<ReferenceBehavior>();

        if ( newValue == ReferenceBehavior.SetNull )
            ThrowIfCannotUseSetNullReferenceBehavior();

        return newValue;
    }

    /// <summary>
    /// Callback invoked just after <see cref="OnUpdateBehavior"/> change has been processed.
    /// </summary>
    /// <param name="originalValue">Original value.</param>
    protected virtual void AfterOnUpdateBehaviorChange(ReferenceBehavior originalValue)
    {
        AddOnUpdateBehaviorChange( this, originalValue );
    }

    /// <summary>
    /// Throws an exception when <see cref="ReferenceBehavior.SetNull"/> behavior cannot be used.
    /// </summary>
    /// <exception cref="SqlObjectBuilderException">When <see cref="ReferenceBehavior.SetNull"/> behavior cannot be used.</exception>
    /// <remarks>
    /// All <see cref="SqlIndexBuilder.Columns"/> of <see cref="OriginIndex"/> must be single columns
    /// with <see cref="SqlColumnBuilder.IsNullable"/> equal to <b>true</b>.
    /// </remarks>
    protected void ThrowIfCannotUseSetNullReferenceBehavior()
    {
        foreach ( var column in OriginIndex.Columns )
        {
            if ( column is null || ! column.IsNullable )
                throw SqlHelpers.CreateObjectBuilderException(
                    Database,
                    ExceptionResources.IndexContainsNonNullableColumns( OriginIndex ) );
        }
    }

    /// <summary>
    /// Adds this foreign key to <see cref="OriginIndex"/> and <see cref="ReferencedIndex"/> reference sources.
    /// This foreign key may optionally be added as a reference source to table and schema of <see cref="ReferencedIndex"/>.
    /// </summary>
    protected void AddIndexReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        AddReference( OriginIndex, refSource );
        AddReference( ReferencedIndex, refSource );
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        AddReference( ReferencedIndex.Table, refSource, ReferencedIndex );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            AddReference( ReferencedIndex.Table.Schema, refSource, ReferencedIndex );
    }

    /// <summary>
    /// Removes this foreign key from <see cref="OriginIndex"/> and <see cref="ReferencedIndex"/> reference sources.
    /// </summary>
    protected void RemoveIndexReferences()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        RemoveReference( OriginIndex, refSource );
        RemoveReference( ReferencedIndex, refSource );
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        RemoveReference( ReferencedIndex.Table, refSource );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            RemoveReference( ReferencedIndex.Table.Schema, refSource );
    }

    /// <summary>
    /// Removes this foreign key from <see cref="OriginIndex"/> and <see cref="ReferencedIndex"/> reference sources.
    /// This is a version for the <see cref="QuickRemoveCore()"/> method.
    /// </summary>
    protected void QuickRemoveReferencedIndexReferences()
    {
        if ( ReferenceEquals( OriginIndex.Table, ReferencedIndex.Table ) )
            return;

        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        RemoveReference( ReferencedIndex, refSource );
        RemoveReference( ReferencedIndex.Table, refSource );
        if ( ! ReferenceEquals( OriginIndex.Table.Schema, ReferencedIndex.Table.Schema ) )
            RemoveReference( ReferencedIndex.Table.Schema, refSource );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveIndexReferences();
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        QuickRemoveReferencedIndexReferences();
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        return SetOnDeleteBehavior( behavior );
    }

    ISqlForeignKeyBuilder ISqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        return SetOnUpdateBehavior( behavior );
    }
}
