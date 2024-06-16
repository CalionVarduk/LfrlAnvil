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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlConstraint" />
public abstract class SqlConstraint : SqlObject, ISqlConstraint
{
    /// <summary>
    /// Creates a new <see cref="SqlConstraint"/> instance.
    /// </summary>
    /// <param name="table">Table that this constraint belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlConstraint(SqlTable table, SqlConstraintBuilder builder)
        : base( table.Database, builder )
    {
        Table = table;
    }

    /// <inheritdoc cref="ISqlConstraint.Table" />
    public SqlTable Table { get; }

    ISqlTable ISqlConstraint.Table => Table;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlConstraint"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
