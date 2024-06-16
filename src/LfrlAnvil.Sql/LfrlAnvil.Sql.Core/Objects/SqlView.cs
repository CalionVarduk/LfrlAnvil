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
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlView" />
public abstract class SqlView : SqlObject, ISqlView
{
    private SqlViewNode? _node;
    private SqlRecordSetInfo? _info;

    /// <summary>
    /// Creates a new <see cref="SqlView"/> instance.
    /// </summary>
    /// <param name="schema">Schema that this view belongs to.</param>
    /// <param name="builder">Source builder.</param>
    /// <param name="dataFields">Collection of data fields that belong to this view.</param>
    protected SqlView(SqlSchema schema, SqlViewBuilder builder, SqlViewDataFieldCollection dataFields)
        : base( schema.Database, builder )
    {
        Schema = schema;
        _info = builder.GetCachedInfo();
        _node = null;
        DataFields = dataFields;
        DataFields.SetView( this, builder.Source );
    }

    /// <inheritdoc cref="ISqlView.Schema" />
    public SqlSchema Schema { get; }

    /// <inheritdoc cref="ISqlView.DataFields" />
    public SqlViewDataFieldCollection DataFields { get; }

    /// <inheritdoc />
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );

    /// <inheritdoc />
    public SqlViewNode Node => _node ??= SqlNode.View( this );

    ISqlSchema ISqlView.Schema => Schema;
    ISqlViewDataFieldCollection ISqlView.DataFields => DataFields;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlView"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }
}
