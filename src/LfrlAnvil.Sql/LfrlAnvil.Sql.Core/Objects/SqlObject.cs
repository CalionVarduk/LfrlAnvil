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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlObject" />
public abstract class SqlObject : ISqlObject
{
    /// <summary>
    /// Creates a new <see cref="SqlObject"/> instance.
    /// </summary>
    /// <param name="database">Database that this object belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlObject(SqlDatabase database, SqlObjectBuilder builder)
        : this( database, builder.Type, builder.Name ) { }

    /// <summary>
    /// Creates a new <see cref="SqlObject"/> instance.
    /// </summary>
    /// <param name="database">Database that this object belongs to.</param>
    /// <param name="type">Object's type.</param>
    /// <param name="name">Object's name.</param>
    protected SqlObject(SqlDatabase database, SqlObjectType type, string name)
    {
        Assume.IsDefined( type );
        Database = database;
        Type = type;
        Name = name;
    }

    /// <inheritdoc cref="ISqlObject.Database" />
    public SqlDatabase Database { get; }

    /// <inheritdoc />
    public SqlObjectType Type { get; }

    /// <inheritdoc />
    public string Name { get; }

    ISqlDatabase ISqlObject.Database => Database;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObject"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {Name}";
    }
}
