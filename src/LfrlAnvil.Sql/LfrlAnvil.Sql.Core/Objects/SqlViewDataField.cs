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
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlViewDataField" />
public abstract class SqlViewDataField : SqlObject, ISqlViewDataField
{
    private SqlViewDataFieldNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlViewDataField"/> instance.
    /// </summary>
    /// <param name="view">View that this data field belongs to.</param>
    /// <param name="name">Data field's name.</param>
    protected SqlViewDataField(SqlView view, string name)
        : base( view.Database, SqlObjectType.ViewDataField, name )
    {
        View = view;
        _node = null;
    }

    /// <inheritdoc cref="ISqlViewDataField.View" />
    public SqlView View { get; }

    /// <inheritdoc />
    public SqlViewDataFieldNode Node => _node ??= View.Node[Name];

    ISqlView ISqlViewDataField.View => View;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlViewDataField"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
