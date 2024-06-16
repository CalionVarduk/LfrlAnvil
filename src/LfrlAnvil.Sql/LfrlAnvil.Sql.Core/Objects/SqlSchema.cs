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

using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlSchema" />
public abstract class SqlSchema : SqlObject, ISqlSchema
{
    /// <summary>
    /// Creates a new <see cref="SqlSchema"/> instance.
    /// </summary>
    /// <param name="database">Database that this schema belongs to.</param>
    /// <param name="builder">Source builder.</param>
    /// <param name="objects">Collection of objects that belong to this schema.</param>
    protected SqlSchema(SqlDatabase database, SqlSchemaBuilder builder, SqlObjectCollection objects)
        : base( database, builder )
    {
        Objects = objects;
        Objects.SetSchema( this );
    }

    /// <inheritdoc cref="ISqlSchema.Objects" />
    public SqlObjectCollection Objects { get; }

    ISqlObjectCollection ISqlSchema.Objects => Objects;
}
