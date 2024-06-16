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

using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlColumnBuilder : SqlColumnBuilder
{
    internal MySqlColumnBuilder(MySqlTableBuilder table, string name, SqlColumnTypeDefinition typeDefinition)
        : base( table, name, typeDefinition ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlColumnBuilder.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilder.SetName(string)" />
    public new MySqlColumnBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetType(SqlColumnTypeDefinition)" />
    public new MySqlColumnBuilder SetType(SqlColumnTypeDefinition definition)
    {
        base.SetType( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.MarkAsNullable(bool)" />
    public new MySqlColumnBuilder MarkAsNullable(bool enabled = true)
    {
        base.MarkAsNullable( enabled );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetDefaultValue(SqlExpressionNode)" />
    public new MySqlColumnBuilder SetDefaultValue(SqlExpressionNode? value)
    {
        base.SetDefaultValue( value );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilder.SetComputation(SqlColumnComputation?)" />
    public new MySqlColumnBuilder SetComputation(SqlColumnComputation? computation)
    {
        base.SetComputation( computation );
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void UpdateDefaultValueBasedOnDataType()
    {
        SetDefaultValueBasedOnDataType();
    }
}
