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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a table column.
/// </summary>
public class SqlColumnDefinitionNode : SqlNodeBase
{
    internal SqlColumnDefinitionNode(
        string name,
        TypeNullability type,
        SqlExpressionNode? defaultValue,
        SqlColumnComputation? computation)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
        TypeDefinition = null;
        DefaultValue = defaultValue;
        Computation = computation;
    }

    internal SqlColumnDefinitionNode(
        string name,
        ISqlColumnTypeDefinition typeDefinition,
        bool isNullable,
        SqlExpressionNode? defaultValue,
        SqlColumnComputation? computation)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = TypeNullability.Create( typeDefinition.RuntimeType, isNullable );
        TypeDefinition = typeDefinition;
        DefaultValue = defaultValue;
        Computation = computation;
    }

    /// <summary>
    /// Column's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Column's runtime type.
    /// </summary>
    public TypeNullability Type { get; }

    /// <summary>
    /// Optional <see cref="ISqlColumnTypeDefinition"/> instance that defines this column's type.
    /// </summary>
    public ISqlColumnTypeDefinition? TypeDefinition { get; }

    /// <summary>
    /// Column's optional default value.
    /// </summary>
    public SqlExpressionNode? DefaultValue { get; }

    /// <summary>
    /// Column's optional computation.
    /// </summary>
    public SqlColumnComputation? Computation { get; }
}
