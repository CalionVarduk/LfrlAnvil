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
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlCheckBuilder" />
public abstract class SqlCheckBuilder : SqlConstraintBuilder, ISqlCheckBuilder
{
    private ReadOnlyArray<SqlColumnBuilder> _referencedColumns;

    /// <summary>
    /// Creates a new <see cref="SqlCheckBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this constraint is attached to.</param>
    /// <param name="name">Object's name.</param>
    /// <param name="condition">Underlying condition of this check constraint.</param>
    /// <param name="referencedColumns">Collection of columns referenced by this check constraint.</param>
    protected SqlCheckBuilder(
        SqlTableBuilder table,
        string name,
        SqlConditionNode condition,
        ReadOnlyArray<SqlColumnBuilder> referencedColumns)
        : base( table, SqlObjectType.Check, name )
    {
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
        Condition = condition;
        SetReferencedColumns( referencedColumns );
    }

    /// <inheritdoc />
    public SqlConditionNode Condition { get; }

    /// <inheritdoc cref="ISqlCheckBuilder.ReferencedColumns" />
    public SqlObjectBuilderArray<SqlColumnBuilder> ReferencedColumns => SqlObjectBuilderArray<SqlColumnBuilder>.From( _referencedColumns );

    IReadOnlyCollection<ISqlColumnBuilder> ISqlCheckBuilder.ReferencedColumns => _referencedColumns.GetUnderlyingArray();

    /// <inheritdoc cref="SqlConstraintBuilder.SetName(string)" />
    public new SqlCheckBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlConstraintBuilder.SetDefaultName()" />
    public new SqlCheckBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc />
    [Pure]
    protected sealed override string GetDefaultName()
    {
        return Database.DefaultNames.GetForCheck( Table );
    }

    /// <summary>
    /// Adds a collection of <paramref name="columns"/> to <see cref="ReferencedColumns"/>
    /// and adds this check constraint to their reference sources.
    /// </summary>
    /// <param name="columns">Collection of columns to add.</param>
    protected void SetReferencedColumns(ReadOnlyArray<SqlColumnBuilder> columns)
    {
        _referencedColumns = columns;
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            AddReference( column, refSource );
    }

    /// <summary>
    /// Removes all columns from <see cref="ReferencedColumns"/>
    /// and removes this check constraint from their reference sources.
    /// </summary>
    protected void ClearReferencedColumns()
    {
        var refSource = SqlObjectBuilderReferenceSource.Create( this );
        foreach ( var column in _referencedColumns )
            RemoveReference( column, refSource );

        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        ClearReferencedColumns();
    }

    /// <inheritdoc />
    protected override void QuickRemoveCore()
    {
        base.QuickRemoveCore();
        _referencedColumns = ReadOnlyArray<SqlColumnBuilder>.Empty;
    }

    ISqlCheckBuilder ISqlCheckBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlCheckBuilder ISqlCheckBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
