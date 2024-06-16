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
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <inheritdoc cref="ISqlConstraintBuilder" />
public abstract class SqlConstraintBuilder : SqlObjectBuilder, ISqlConstraintBuilder
{
    internal SqlConstraintBuilder(SqlTableBuilder table, SqlObjectType type, string name)
        : base( table.Database, type, name )
    {
        Table = table;
    }

    /// <summary>
    /// Creates a new <see cref="SqlConstraintBuilder"/> instance.
    /// </summary>
    /// <param name="table">Table that this constraint is attached to.</param>
    /// <param name="name">Object's name.</param>
    protected SqlConstraintBuilder(SqlTableBuilder table, string name)
        : base( table.Database, name )
    {
        Table = table;
    }

    /// <inheritdoc cref="ISqlConstraintBuilder.Table" />
    public SqlTableBuilder Table { get; }

    ISqlTableBuilder ISqlConstraintBuilder.Table => Table;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlConstraintBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    /// <inheritdoc cref="ISqlConstraintBuilder.SetName(string)" />
    public new SqlConstraintBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="ISqlConstraintBuilder.SetDefaultName()" />
    public SqlConstraintBuilder SetDefaultName()
    {
        return SetName( GetDefaultName() );
    }

    /// <summary>
    /// Creates a default name for this constraint.
    /// </summary>
    /// <returns>Default name for this constraint.</returns>
    [Pure]
    protected abstract string GetDefaultName();

    /// <inheritdoc />
    protected override SqlPropertyChange<string> BeforeNameChange(string newValue)
    {
        var change = base.BeforeNameChange( newValue );
        if ( change.IsCancelled )
            return change;

        ChangeNameInCollection( Table.Constraints, this, newValue );
        return change;
    }

    /// <inheritdoc />
    protected override void AfterNameChange(string originalValue)
    {
        AddNameChange( Table, this, originalValue );
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        base.BeforeRemove();
        RemoveFromCollection( Table.Constraints, this );
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        AddRemoval( Table, this );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetName(string name)
    {
        return SetName( name );
    }

    ISqlConstraintBuilder ISqlConstraintBuilder.SetDefaultName()
    {
        return SetDefaultName();
    }
}
