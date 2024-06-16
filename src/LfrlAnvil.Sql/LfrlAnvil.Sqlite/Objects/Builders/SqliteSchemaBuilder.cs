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

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteSchemaBuilder : SqlSchemaBuilder
{
    internal SqliteSchemaBuilder(SqliteDatabaseBuilder database, string name)
        : base( database, name, new SqliteObjectBuilderCollection() ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilder.Objects" />
    public new SqliteObjectBuilderCollection Objects => ReinterpretCast.To<SqliteObjectBuilderCollection>( base.Objects );

    /// <inheritdoc cref="SqlSchemaBuilder.SetName(string)" />
    public new SqliteSchemaBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc />
    protected override void BeforeRemove()
    {
        ThrowIfDefault();
        ThrowIfReferenced();
        RemoveFromCollection( Database.Schemas, this );
    }

    /// <inheritdoc />
    protected override void AfterRemove()
    {
        base.AfterRemove();
        QuickRemoveObjects();
    }
}
