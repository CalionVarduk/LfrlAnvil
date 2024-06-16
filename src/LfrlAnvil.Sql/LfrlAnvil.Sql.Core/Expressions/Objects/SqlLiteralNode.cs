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
using LfrlAnvil.Sql.Extensions;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a type-erased literal constant.
/// </summary>
public abstract class SqlLiteralNode : SqlExpressionNode
{
    internal SqlLiteralNode(TypeNullability type)
        : base( SqlNodeType.Literal )
    {
        Type = type;
    }

    /// <summary>
    /// Runtime type of this literal.
    /// </summary>
    public TypeNullability Type { get; }

    /// <summary>
    /// Returns an underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    [Pure]
    public abstract object GetValue();

    /// <summary>
    /// Converts the underlying value to an inline DB representation.
    /// </summary>
    /// <param name="typeDefinitionProvider"><see cref="ISqlColumnTypeDefinitionProvider"/> instance to use.</param>
    /// <returns>Inline DB representation of the underlying value.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When the type associated with this literal does not exist in the provided <paramref name="typeDefinitionProvider"/>.
    /// </exception>
    [Pure]
    public abstract string GetSql(ISqlColumnTypeDefinitionProvider typeDefinitionProvider);
}

/// <summary>
/// Represents an SQL syntax tree expression node that defines a generic literal constant.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public sealed class SqlLiteralNode<T> : SqlLiteralNode
    where T : notnull
{
    internal SqlLiteralNode(T value)
        : base( TypeNullability.Create<T>() )
    {
        Value = value;
    }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public T Value { get; }

    /// <inheritdoc />
    [Pure]
    public override object GetValue()
    {
        return Value;
    }

    /// <inheritdoc />
    [Pure]
    public override string GetSql(ISqlColumnTypeDefinitionProvider typeDefinitionProvider)
    {
        var definition = typeDefinitionProvider.GetByType<T>();
        var result = definition.ToDbLiteral( Value );
        return result;
    }
}
