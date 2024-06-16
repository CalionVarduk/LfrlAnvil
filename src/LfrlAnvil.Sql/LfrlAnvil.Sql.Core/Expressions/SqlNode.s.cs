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

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Expressions.Arithmetic;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Persistence;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Creates instances of <see cref="SqlNodeBase"/> type.
/// </summary>
public static partial class SqlNode
{
    private static SqlNullNode? _null;
    private static SqlTrueNode? _true;
    private static SqlFalseNode? _false;
    private static SqlDistinctTraitNode? _distinct;
    private static SqlDummyDataSourceNode? _dummyDataSource;
    private static SqlCommitTransactionNode? _commitTransaction;
    private static SqlRollbackTransactionNode? _rollbackTransaction;

    /// <summary>
    /// Creates a new <see cref="SqlLiteralNode{T}"/> instance or <see cref="SqlNullNode"/> instance when <paramref name="value"/> is null.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlLiteralNode{T}"/> instance or <see cref="SqlNullNode"/> instance.</returns>
    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : notnull
    {
        return Generic<T>.IsNull( value ) ? Null() : new SqlLiteralNode<T>( value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLiteralNode{T}"/> instance or <see cref="SqlNullNode"/> instance when <paramref name="value"/> is null.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlLiteralNode{T}"/> instance or <see cref="SqlNullNode"/> instance.</returns>
    [Pure]
    public static SqlExpressionNode Literal<T>(T? value)
        where T : struct
    {
        return value is null ? Null() : new SqlLiteralNode<T>( value.Value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNullNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlNullNode"/> instance.</returns>
    [Pure]
    public static SqlNullNode Null()
    {
        return _null ??= new SqlNullNode();
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterNode"/> instance.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="isNullable">Specifies whether or not this parameter should be nullable. Equal to <b>false</b> by default.</param>
    /// <param name="index">
    /// Optional 0-based position of this parameter.
    /// Non-null values mean that the parameter may be interpreted as a positional parameter. Equal to null by default.
    /// </param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlParameterNode"/> instance.</returns>
    [Pure]
    public static SqlParameterNode Parameter<T>(string name, bool isNullable = false, int? index = null)
    {
        return Parameter( name, TypeNullability.Create( typeof( T ), isNullable ), index );
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterNode"/> instance.
    /// </summary>
    /// <param name="name">Parameter's name.</param>
    /// <param name="type">Optional runtime type of this parameter. Equal to null by default.</param>
    /// <param name="index">
    /// Optional 0-based position of this parameter.
    /// Non-null values mean that the parameter may be interpreted as a positional parameter. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlParameterNode"/> instance.</returns>
    [Pure]
    public static SqlParameterNode Parameter(string name, TypeNullability? type = null, int? index = null)
    {
        return new SqlParameterNode( name, type, index );
    }

    /// <summary>
    /// Creates a new collection of <see cref="SqlParameterNode"/> instances.
    /// </summary>
    /// <param name="name">Base name of all parameters.</param>
    /// <param name="count">Number of parameters.</param>
    /// <param name="isNullable">Specifies whether or not all parameters should be nullable. Equal to <b>false</b> by default.</param>
    /// <param name="firstIndex">
    /// Optional 0-based position of the first parameter.
    /// Non-null values mean that the parameter may be interpreted as a positional parameter. Equal to null by default.
    /// </param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New collection of <see cref="SqlParameterNode"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    public static SqlParameterNode[] ParameterRange<T>(string name, int count, bool isNullable = false, int? firstIndex = null)
    {
        return ParameterRange( name, count, TypeNullability.Create( typeof( T ), isNullable ), firstIndex );
    }

    /// <summary>
    /// Creates a new collection of <see cref="SqlParameterNode"/> instances.
    /// </summary>
    /// <param name="name">Base name of all parameters.</param>
    /// <param name="count">Number of parameters.</param>
    /// <param name="type">Optional runtime type of all parameters. Equal to null by default.</param>
    /// <param name="firstIndex">
    /// Optional 0-based position of the first parameter.
    /// Non-null values mean that the parameter may be interpreted as a positional parameter. Equal to null by default.
    /// </param>
    /// <returns>New collection of <see cref="SqlParameterNode"/> instances.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    public static SqlParameterNode[] ParameterRange(string name, int count, TypeNullability? type = null, int? firstIndex = null)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0 );
        if ( count == 0 )
            return Array.Empty<SqlParameterNode>();

        var result = new SqlParameterNode[count];
        if ( firstIndex is null )
        {
            for ( var i = 0; i < count; ++i )
                result[i] = Parameter( $"{name}{i + 1}", type );
        }
        else
        {
            var index = firstIndex.Value;
            for ( var i = 0; i < count; ++i )
                result[i] = Parameter( $"{name}{i + 1}", type, index++ );
        }

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="SqlTypeCastExpressionNode"/> instance.
    /// </summary>
    /// <param name="expression">Underlying value to cast to a different type.</param>
    /// <param name="type">Target runtime type.</param>
    /// <returns>New <see cref="SqlTypeCastExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlTypeCastExpressionNode TypeCast(SqlExpressionNode expression, Type type)
    {
        return new SqlTypeCastExpressionNode( expression, type );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTypeCastExpressionNode"/> instance.
    /// </summary>
    /// <param name="expression">Underlying value to cast to a different type.</param>
    /// <param name="typeDefinition"><see cref="ISqlColumnTypeDefinition"/> instance that defines the target type.</param>
    /// <returns>New <see cref="SqlTypeCastExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlTypeCastExpressionNode TypeCast(SqlExpressionNode expression, ISqlColumnTypeDefinition typeDefinition)
    {
        return new SqlTypeCastExpressionNode( expression, typeDefinition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNegateExpressionNode"/> instance.
    /// </summary>
    /// <param name="value">Operand.</param>
    /// <returns>New <see cref="SqlNegateExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlNegateExpressionNode Negate(SqlExpressionNode value)
    {
        return new SqlNegateExpressionNode( value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseNotExpressionNode"/> instance.
    /// </summary>
    /// <param name="value">Operand.</param>
    /// <returns>New <see cref="SqlBitwiseNotExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseNotExpressionNode BitwiseNot(SqlExpressionNode value)
    {
        return new SqlBitwiseNotExpressionNode( value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAddExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlAddExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlAddExpressionNode Add(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlAddExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlConcatExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlConcatExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlConcatExpressionNode Concat(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlConcatExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSubtractExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlSubtractExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlSubtractExpressionNode Subtract(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlSubtractExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiplyExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlMultiplyExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlMultiplyExpressionNode Multiply(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlMultiplyExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDivideExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlDivideExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlDivideExpressionNode Divide(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlDivideExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlModuloExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlModuloExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlModuloExpressionNode Modulo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlModuloExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseAndExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseAndExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseAndExpressionNode BitwiseAnd(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseAndExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseOrExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseOrExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseOrExpressionNode BitwiseOr(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseOrExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseXorExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseXorExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseXorExpressionNode BitwiseXor(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseXorExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseLeftShiftExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseLeftShiftExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseLeftShiftExpressionNode BitwiseLeftShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseLeftShiftExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBitwiseRightShiftExpressionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlBitwiseRightShiftExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlBitwiseRightShiftExpressionNode BitwiseRightShift(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlBitwiseRightShiftExpressionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawExpressionNode"/> instance.
    /// </summary>
    /// <param name="sql">Raw SQL expression.</param>
    /// <param name="parameters">Collection of parameter nodes.</param>
    /// <returns>New <see cref="SqlRawExpressionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlRawExpressionNode RawExpression(string sql, params SqlParameterNode[] parameters)
    {
        return RawExpression( sql, type: null, parameters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawExpressionNode"/> instance.
    /// </summary>
    /// <param name="sql">Raw SQL expression.</param>
    /// <param name="type">Optional runtime type of the result of this expression.</param>
    /// <param name="parameters">Collection of parameter nodes.</param>
    /// <returns>New <see cref="SqlRawExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlRawExpressionNode RawExpression(string sql, TypeNullability? type, params SqlParameterNode[] parameters)
    {
        return new SqlRawExpressionNode( sql, type, parameters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlEqualToConditionNode EqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlEqualToConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNotEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlNotEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlNotEqualToConditionNode NotEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlNotEqualToConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlGreaterThanConditionNode"/> instance.</returns>
    [Pure]
    public static SqlGreaterThanConditionNode GreaterThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlLessThanConditionNode"/> instance.</returns>
    [Pure]
    public static SqlLessThanConditionNode LessThan(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlGreaterThanOrEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlGreaterThanOrEqualToConditionNode GreaterThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlGreaterThanOrEqualToConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLessThanOrEqualToConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlLessThanOrEqualToConditionNode"/> instance.</returns>
    [Pure]
    public static SqlLessThanOrEqualToConditionNode LessThanOrEqualTo(SqlExpressionNode left, SqlExpressionNode right)
    {
        return new SqlLessThanOrEqualToConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBetweenConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="min">Minimum acceptable value.</param>
    /// <param name="max">Maximum acceptable value.</param>
    /// <returns>New <see cref="SqlBetweenConditionNode"/> instance.</returns>
    [Pure]
    public static SqlBetweenConditionNode Between(SqlExpressionNode value, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( value, min, max, isNegated: false );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlBetweenConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="min">Minimum acceptable value.</param>
    /// <param name="max">Maximum acceptable value.</param>
    /// <returns>New negated <see cref="SqlBetweenConditionNode"/> instance.</returns>
    [Pure]
    public static SqlBetweenConditionNode NotBetween(SqlExpressionNode value, SqlExpressionNode min, SqlExpressionNode max)
    {
        return new SqlBetweenConditionNode( value, min, max, isNegated: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLikeConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="pattern">String pattern to check the value against.</param>
    /// <param name="escape">Optional escape character for the pattern. Equal to null by default.</param>
    /// <returns>New <see cref="SqlLikeConditionNode"/> instance.</returns>
    [Pure]
    public static SqlLikeConditionNode Like(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: false );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlLikeConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="pattern">String pattern to check the value against.</param>
    /// <param name="escape">Optional escape character for the pattern. Equal to null by default.</param>
    /// <returns>New negated <see cref="SqlLikeConditionNode"/> instance.</returns>
    [Pure]
    public static SqlLikeConditionNode NotLike(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape = null)
    {
        return new SqlLikeConditionNode( value, pattern, escape, isNegated: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="query">Sub-query to check.</param>
    /// <returns>New <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    public static SqlExistsConditionNode Exists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: false );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlExistsConditionNode"/> instance.
    /// </summary>
    /// <param name="query">Sub-query to check.</param>
    /// <returns>New negated <see cref="SqlExistsConditionNode"/> instance.</returns>
    [Pure]
    public static SqlExistsConditionNode NotExists(SqlQueryExpressionNode query)
    {
        return new SqlExistsConditionNode( query, isNegated: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New <see cref="SqlInConditionNode"/> instance or <see cref="SqlFalseNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode In(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? False() : new SqlInConditionNode( value, expressions, isNegated: false );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/>
    /// when <paramref name="expressions"/> are empty.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="expressions">Collection of values that the value is compared against.</param>
    /// <returns>New negated <see cref="SqlInConditionNode"/> instance or <see cref="SqlTrueNode"/> instance.</returns>
    [Pure]
    public static SqlConditionNode NotIn(SqlExpressionNode value, params SqlExpressionNode[] expressions)
    {
        return expressions.Length == 0 ? True() : new SqlInConditionNode( value, expressions, isNegated: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInQueryConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="query">Sub-query that the value is compared against.</param>
    /// <returns>New <see cref="SqlInQueryConditionNode"/> instance.</returns>
    [Pure]
    public static SqlInQueryConditionNode InQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: false );
    }

    /// <summary>
    /// Creates a new negated <see cref="SqlInQueryConditionNode"/> instance.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <param name="query">Sub-query that the value is compared against.</param>
    /// <returns>New negated <see cref="SqlInQueryConditionNode"/> instance.</returns>
    [Pure]
    public static SqlInQueryConditionNode NotInQuery(SqlExpressionNode value, SqlQueryExpressionNode query)
    {
        return new SqlInQueryConditionNode( value, query, isNegated: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTrueNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlTrueNode"/> instance.</returns>
    [Pure]
    public static SqlTrueNode True()
    {
        return _true ??= new SqlTrueNode();
    }

    /// <summary>
    /// Creates a new <see cref="SqlFalseNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlFalseNode"/> instance.</returns>
    [Pure]
    public static SqlFalseNode False()
    {
        return _false ??= new SqlFalseNode();
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawConditionNode"/> instance.
    /// </summary>
    /// <param name="sql">Raw SQL condition.</param>
    /// <param name="parameters">Collection of parameter nodes.</param>
    /// <returns>New <see cref="SqlRawConditionNode"/> instance.</returns>
    [Pure]
    public static SqlRawConditionNode RawCondition(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawConditionNode( sql, parameters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlConditionValueNode"/> instance.
    /// </summary>
    /// <param name="condition">Underlying condition.</param>
    /// <returns>New <see cref="SqlConditionValueNode"/> instance.</returns>
    [Pure]
    public static SqlConditionValueNode Value(SqlConditionNode condition)
    {
        return new SqlConditionValueNode( condition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAndConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlAndConditionNode"/> instance.</returns>
    [Pure]
    public static SqlAndConditionNode And(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlAndConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrConditionNode"/> instance.
    /// </summary>
    /// <param name="left">First operand.</param>
    /// <param name="right">Second operand.</param>
    /// <returns>New <see cref="SqlOrConditionNode"/> instance.</returns>
    [Pure]
    public static SqlOrConditionNode Or(SqlConditionNode left, SqlConditionNode right)
    {
        return new SqlOrConditionNode( left, right );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying <see cref="ISqlTable"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTableNode"/> instance.</returns>
    [Pure]
    public static SqlTableNode Table(ISqlTable value, string? alias = null)
    {
        return new SqlTableNode( value, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTableBuilderNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying <see cref="ISqlTableBuilder"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlTableBuilderNode"/> instance.</returns>
    [Pure]
    public static SqlTableBuilderNode Table(ISqlTableBuilder value, string? alias = null)
    {
        return new SqlTableBuilderNode( value, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying <see cref="ISqlView"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlViewNode"/> instance.</returns>
    [Pure]
    public static SqlViewNode View(ISqlView value, string? alias = null)
    {
        return new SqlViewNode( value, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlViewBuilderNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying <see cref="ISqlViewBuilder"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlViewBuilderNode"/> instance.</returns>
    [Pure]
    public static SqlViewBuilderNode View(ISqlViewBuilder value, string? alias = null)
    {
        return new SqlViewBuilderNode( value, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance marked as raw.
    /// </summary>
    /// <param name="name">Raw name of this record set.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlRawRecordSetNode"/> instance.</returns>
    [Pure]
    public static SqlRawRecordSetNode RawRecordSet(string name, string? alias = null)
    {
        return new SqlRawRecordSetNode( name, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance.
    /// </summary>
    /// <param name="info"><see cref="SqlRecordSetInfo"/> associated with this record set.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlRawRecordSetNode"/> instance.</returns>
    [Pure]
    public static SqlRawRecordSetNode RawRecordSet(SqlRecordSetInfo info, string? alias = null)
    {
        return new SqlRawRecordSetNode( info, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawDataFieldNode"/> instance.
    /// </summary>
    /// <param name="recordSet"><see cref="SqlRecordSetNode"/> that this data field belongs to.</param>
    /// <param name="name">Name of this data field.</param>
    /// <param name="type">Optional runtime type of this data field. Equal to null by default.</param>
    /// <returns>New <see cref="SqlRawDataFieldNode"/> instance.</returns>
    [Pure]
    public static SqlRawDataFieldNode RawDataField(SqlRecordSetNode recordSet, string name, TypeNullability? type = null)
    {
        return new SqlRawDataFieldNode( recordSet, name, type );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawQueryExpressionNode"/> instance.
    /// </summary>
    /// <param name="sql">Raw SQL query expression.</param>
    /// <param name="parameters">Collection of parameter nodes.</param>
    /// <returns>New <see cref="SqlRawQueryExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlRawQueryExpressionNode RawQuery(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawQueryExpressionNode( sql, parameters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlFilterTraitNode"/> instance.
    /// </summary>
    /// <param name="filter">Underlying predicate.</param>
    /// <param name="isConjunction">
    /// Specifies whether or not this trait should be merged with other <see cref="SqlFilterTraitNode"/> instances through
    /// an <see cref="SqlAndConditionNode"/> rather than an <see cref="SqlOrConditionNode"/>.
    /// </param>
    /// <returns>New <see cref="SqlFilterTraitNode"/> instance.</returns>
    [Pure]
    public static SqlFilterTraitNode FilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlFilterTraitNode( filter, isConjunction );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAggregationTraitNode"/> instance.
    /// </summary>
    /// <param name="expressions">Collection of expressions to aggregate by.</param>
    /// <returns>New <see cref="SqlAggregationTraitNode"/> instance.</returns>
    [Pure]
    public static SqlAggregationTraitNode AggregationTrait(params SqlExpressionNode[] expressions)
    {
        return new SqlAggregationTraitNode( expressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAggregationFilterTraitNode"/> instance.
    /// </summary>
    /// <param name="filter">Underlying predicate.</param>
    /// <param name="isConjunction">
    /// Specifies whether or not this trait should be merged with other <see cref="SqlAggregationFilterTraitNode"/> instances through
    /// an <see cref="SqlAndConditionNode"/> rather than an <see cref="SqlOrConditionNode"/>.
    /// </param>
    /// <returns>New <see cref="SqlAggregationFilterTraitNode"/> instance.</returns>
    [Pure]
    public static SqlAggregationFilterTraitNode AggregationFilterTrait(SqlConditionNode filter, bool isConjunction)
    {
        return new SqlAggregationFilterTraitNode( filter, isConjunction );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDistinctTraitNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlDistinctTraitNode"/> instance.</returns>
    [Pure]
    public static SqlDistinctTraitNode DistinctTrait()
    {
        return _distinct ??= new SqlDistinctTraitNode();
    }

    /// <summary>
    /// Creates a new <see cref="SqlSortTraitNode"/> instance.
    /// </summary>
    /// <param name="ordering">Collection of ordering definitions.</param>
    /// <returns>New <see cref="SqlSortTraitNode"/> instance.</returns>
    [Pure]
    public static SqlSortTraitNode SortTrait(params SqlOrderByNode[] ordering)
    {
        return new SqlSortTraitNode( ordering );
    }

    /// <summary>
    /// Creates a new <see cref="SqlLimitTraitNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <returns>New <see cref="SqlLimitTraitNode"/> instance.</returns>
    [Pure]
    public static SqlLimitTraitNode LimitTrait(SqlExpressionNode value)
    {
        return new SqlLimitTraitNode( value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOffsetTraitNode"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <returns>New <see cref="SqlOffsetTraitNode"/> instance.</returns>
    [Pure]
    public static SqlOffsetTraitNode OffsetTrait(SqlExpressionNode value)
    {
        return new SqlOffsetTraitNode( value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCommonTableExpressionTraitNode"/> instance.
    /// </summary>
    /// <param name="commonTableExpressions">Collection of common table expressions.</param>
    /// <returns>New <see cref="SqlCommonTableExpressionTraitNode"/> instance.</returns>
    [Pure]
    public static SqlCommonTableExpressionTraitNode CommonTableExpressionTrait(
        params SqlCommonTableExpressionNode[] commonTableExpressions)
    {
        return new SqlCommonTableExpressionTraitNode( commonTableExpressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowDefinitionTraitNode"/> instance.
    /// </summary>
    /// <param name="windows">Collection of window definitions.</param>
    /// <returns>New <see cref="SqlWindowDefinitionTraitNode"/> instance.</returns>
    [Pure]
    public static SqlWindowDefinitionTraitNode WindowDefinitionTrait(params SqlWindowDefinitionNode[] windows)
    {
        return new SqlWindowDefinitionTraitNode( windows );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowTraitNode"/> instance.
    /// </summary>
    /// <param name="definition">Underlying window definition.</param>
    /// <returns>New <see cref="SqlWindowTraitNode"/> instance.</returns>
    [Pure]
    public static SqlWindowTraitNode WindowTrait(SqlWindowDefinitionNode definition)
    {
        return new SqlWindowTraitNode( definition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Window's name.</param>
    /// <param name="ordering">Collection of ordering expressions used by this window.</param>
    /// <param name="frame">
    /// Optional <see cref="SqlWindowFrameNode"/> instance that defines the frame of this window. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlWindowDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlWindowDefinitionNode WindowDefinition(string name, SqlOrderByNode[] ordering, SqlWindowFrameNode? frame = null)
    {
        return WindowDefinition( name, Array.Empty<SqlExpressionNode>(), ordering, frame );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Window's name.</param>
    /// <param name="partitioning">Collection of expressions by which this window partitions the result set.</param>
    /// <param name="ordering">Collection of ordering expressions used by this window.</param>
    /// <param name="frame">
    /// Optional <see cref="SqlWindowFrameNode"/> instance that defines the frame of this window. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlWindowDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlWindowDefinitionNode WindowDefinition(
        string name,
        SqlExpressionNode[] partitioning,
        SqlOrderByNode[] ordering,
        SqlWindowFrameNode? frame = null)
    {
        return new SqlWindowDefinitionNode( name, partitioning, ordering, frame );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameNode"/> instance with <see cref="SqlWindowFrameType.Rows"/> type.
    /// </summary>
    /// <param name="start">Beginning <see cref="SqlWindowFrameBoundary"/> of this frame.</param>
    /// <param name="end">Ending <see cref="SqlWindowFrameBoundary"/> of this frame.</param>
    /// <returns>New <see cref="SqlWindowFrameNode"/> instance.</returns>
    [Pure]
    public static SqlWindowFrameNode RowsWindowFrame(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
    {
        return new SqlWindowFrameNode( SqlWindowFrameType.Rows, start, end );
    }

    /// <summary>
    /// Creates a new <see cref="SqlWindowFrameNode"/> instance with <see cref="SqlWindowFrameType.Range"/> type.
    /// </summary>
    /// <param name="start">Beginning <see cref="SqlWindowFrameBoundary"/> of this frame.</param>
    /// <param name="end">Ending <see cref="SqlWindowFrameBoundary"/> of this frame.</param>
    /// <returns>New <see cref="SqlWindowFrameNode"/> instance.</returns>
    [Pure]
    public static SqlWindowFrameNode RangeWindowFrame(SqlWindowFrameBoundary start, SqlWindowFrameBoundary end)
    {
        return new SqlWindowFrameNode( SqlWindowFrameType.Range, start, end );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectFieldNode"/> instance.
    /// </summary>
    /// <param name="expression">Selected expression.</param>
    /// <param name="alias">Alias of the selected expression.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    public static SqlSelectFieldNode Select(SqlExpressionNode expression, string alias)
    {
        return new SqlSelectFieldNode( expression, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectFieldNode"/> instance.
    /// </summary>
    /// <param name="dataField">Selected data field.</param>
    /// <param name="alias">Optional alias of the selected expression. Equal to null by default.</param>
    /// <returns>New <see cref="SqlSelectFieldNode"/> instance.</returns>
    [Pure]
    public static SqlSelectFieldNode Select(SqlDataFieldNode dataField, string? alias = null)
    {
        return new SqlSelectFieldNode( dataField, alias );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectRecordSetNode"/> instance.
    /// </summary>
    /// <param name="recordSet">Single record set to select all data fields from.</param>
    /// <returns>New <see cref="SqlSelectRecordSetNode"/> instance.</returns>
    [Pure]
    public static SqlSelectRecordSetNode SelectAll(SqlRecordSetNode recordSet)
    {
        return new SqlSelectRecordSetNode( recordSet );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectAllNode"/> instance.
    /// </summary>
    /// <param name="dataSource">Data source to select all data fields from.</param>
    /// <returns>New <see cref="SqlSelectAllNode"/> instance.</returns>
    [Pure]
    public static SqlSelectAllNode SelectAll(SqlDataSourceNode dataSource)
    {
        return new SqlSelectAllNode( dataSource );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSelectExpressionNode"/> instance.
    /// </summary>
    /// <param name="selectNode">Underlying selection.</param>
    /// <returns>New <see cref="SqlSelectExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlSelectExpressionNode SelectExpression(SqlSelectNode selectNode)
    {
        return new SqlSelectExpressionNode( selectNode );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.
    /// </summary>
    /// <param name="dataSource">Underlying data source.</param>
    /// <param name="selection">Collection of expressions to include in this query's selection.</param>
    /// <typeparam name="TDataSourceNode">SQL data source node type.</typeparam>
    /// <returns>New <see cref="SqlDataSourceQueryExpressionNode{TDataSourceNode}"/> instance.</returns>
    [Pure]
    public static SqlDataSourceQueryExpressionNode<TDataSourceNode> Query<TDataSourceNode>(
        TDataSourceNode dataSource,
        params SqlSelectNode[] selection)
        where TDataSourceNode : SqlDataSourceNode
    {
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( dataSource, selection );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryExpressionNode"/> instance.
    /// </summary>
    /// <param name="firstQuery">First underlying query.</param>
    /// <param name="followingQueries">Collection of queries that sequentially follow after the first query.</param>
    /// <returns>New <see cref="SqlCompoundQueryExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryExpressionNode CompoundQuery(
        SqlQueryExpressionNode firstQuery,
        params SqlCompoundQueryComponentNode[] followingQueries)
    {
        return new SqlCompoundQueryExpressionNode( firstQuery, followingQueries );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNamedFunctionRecordSetNode"/> instance.
    /// </summary>
    /// <param name="function">Underlying <see cref="SqlNamedFunctionExpressionNode"/> instance.</param>
    /// <param name="alias">Alias of this record set.</param>
    /// <returns>New <see cref="SqlNamedFunctionRecordSetNode"/> instance.</returns>
    [Pure]
    public static SqlNamedFunctionRecordSetNode NamedFunctionRecordSet(SqlNamedFunctionExpressionNode function, string alias)
    {
        return new SqlNamedFunctionRecordSetNode( function, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNewTableNode"/> instance.
    /// </summary>
    /// <param name="creationNode">Underlying <see cref="SqlCreateTableNode"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlNewTableNode"/> instance.</returns>
    [Pure]
    public static SqlNewTableNode NewTableRecordSet(SqlCreateTableNode creationNode, string? alias = null)
    {
        return new SqlNewTableNode( creationNode, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlNewViewNode"/> instance.
    /// </summary>
    /// <param name="creationNode">Underlying <see cref="SqlCreateViewNode"/> instance.</param>
    /// <param name="alias">Optional alias of this record set. Equal to null by default.</param>
    /// <returns>New <see cref="SqlNewViewNode"/> instance.</returns>
    [Pure]
    public static SqlNewViewNode NewViewRecordSet(SqlCreateViewNode creationNode, string? alias = null)
    {
        return new SqlNewViewNode( creationNode, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlQueryRecordSetNode"/> instance.
    /// </summary>
    /// <param name="query">Underlying <see cref="SqlQueryExpressionNode"/> instance.</param>
    /// <param name="alias">Alias of this record set.</param>
    /// <returns>New <see cref="SqlQueryRecordSetNode"/> instance.</returns>
    [Pure]
    public static SqlQueryRecordSetNode QueryRecordSet(SqlQueryExpressionNode query, string alias)
    {
        return new SqlQueryRecordSetNode( query, alias, isOptional: false );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrdinalCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="query">Underlying query that defines this common table expression.</param>
    /// <param name="name">Name of this common table expression.</param>
    /// <returns>New <see cref="SqlOrdinalCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlOrdinalCommonTableExpressionNode OrdinalCommonTableExpression(SqlQueryExpressionNode query, string name)
    {
        return new SqlOrdinalCommonTableExpressionNode( query, name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.
    /// </summary>
    /// <param name="query">Underlying query that defines this common table expression.</param>
    /// <param name="name">Name of this common table expression.</param>
    /// <returns>New <see cref="SqlRecursiveCommonTableExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlRecursiveCommonTableExpressionNode RecursiveCommonTableExpression(SqlCompoundQueryExpressionNode query, string name)
    {
        return new SqlRecursiveCommonTableExpressionNode( query, name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSingleDataSourceNode{TRecordSetNode}"/> instance.
    /// </summary>
    /// <param name="from"><see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <typeparam name="TRecordSetNode">SQL record set node type.</typeparam>
    /// <returns>New <see cref="SqlSingleDataSourceNode{TRecordSetNode}"/> instance.</returns>
    [Pure]
    public static SqlSingleDataSourceNode<TRecordSetNode> SingleDataSource<TRecordSetNode>(TRecordSetNode from)
        where TRecordSetNode : SqlRecordSetNode
    {
        return new SqlSingleDataSourceNode<TRecordSetNode>( from );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDummyDataSourceNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlDummyDataSourceNode"/> instance.</returns>
    [Pure]
    public static SqlDummyDataSourceNode DummyDataSource()
    {
        return _dummyDataSource ??= new SqlDummyDataSourceNode( Chain<SqlTraitNode>.Empty );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="from">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="from">First <see cref="SqlRecordSetNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    public static SqlMultiDataSourceNode Join(SqlRecordSetNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="from"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="joins">
    /// Sequential collection of all <see cref="SqlDataSourceJoinOnNode"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlDataSourceJoinOnNode[] joins)
    {
        return new SqlMultiDataSourceNode( from, joins );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataSourceNode"/> instance.
    /// </summary>
    /// <param name="from"><see cref="SqlDataSourceNode"/> instance from which this data source's definition begins.</param>
    /// <param name="definitions">
    /// Sequential collection of all <see cref="SqlJoinDefinition"/> instances that define this data source.
    /// </param>
    /// <returns>New <see cref="SqlMultiDataSourceNode"/> instance.</returns>
    [Pure]
    public static SqlMultiDataSourceNode Join(SqlDataSourceNode from, params SqlJoinDefinition[] definitions)
    {
        return new SqlMultiDataSourceNode( from, definitions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Asc"/> ordering.
    /// </summary>
    /// <param name="expression">Underlying expression.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByAsc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Asc );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance with <see cref="Sql.OrderBy.Desc"/> ordering.
    /// </summary>
    /// <param name="expression">Underlying expression.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlOrderByNode OrderByDesc(SqlExpressionNode expression)
    {
        return OrderBy( expression, Sql.OrderBy.Desc );
    }

    /// <summary>
    /// Creates a new <see cref="SqlOrderByNode"/> instance.
    /// </summary>
    /// <param name="expression">Underlying expression.</param>
    /// <param name="ordering">Ordering used by this definition.</param>
    /// <returns>New <see cref="SqlOrderByNode"/> instance.</returns>
    [Pure]
    public static SqlOrderByNode OrderBy(SqlExpressionNode expression, OrderBy ordering)
    {
        return new SqlOrderByNode( expression, ordering );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Inner"/> type.
    /// </summary>
    /// <param name="innerRecordSet">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    public static SqlDataSourceJoinOnNode InnerJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Inner, innerRecordSet, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Left"/> type.
    /// </summary>
    /// <param name="innerRecordSet">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    public static SqlDataSourceJoinOnNode LeftJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Left, innerRecordSet, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Right"/> type.
    /// </summary>
    /// <param name="innerRecordSet">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    public static SqlDataSourceJoinOnNode RightJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Right, innerRecordSet, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Full"/> type.
    /// </summary>
    /// <param name="innerRecordSet">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <param name="onExpression">Condition of this join operation.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    public static SqlDataSourceJoinOnNode FullJoinOn(SqlRecordSetNode innerRecordSet, SqlConditionNode onExpression)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Full, innerRecordSet, onExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDataSourceJoinOnNode"/> instance with <see cref="SqlJoinType.Cross"/> type.
    /// </summary>
    /// <param name="innerRecordSet">Inner <see cref="SqlRecordSetNode"/> instance.</param>
    /// <returns>New <see cref="SqlDataSourceJoinOnNode"/> instance.</returns>
    [Pure]
    public static SqlDataSourceJoinOnNode CrossJoin(SqlRecordSetNode innerRecordSet)
    {
        return new SqlDataSourceJoinOnNode( SqlJoinType.Cross, innerRecordSet, True() );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Union"/> operator.
    /// </summary>
    /// <param name="query">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryComponentNode UnionWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Union );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.UnionAll"/> operator.
    /// </summary>
    /// <param name="query">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryComponentNode UnionAllWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.UnionAll );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Intersect"/> operator.
    /// </summary>
    /// <param name="query">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryComponentNode IntersectWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Intersect );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance with <see cref="SqlCompoundQueryOperator.Except"/> operator.
    /// </summary>
    /// <param name="query">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryComponentNode ExceptWith(SqlQueryExpressionNode query)
    {
        return new SqlCompoundQueryComponentNode( query, SqlCompoundQueryOperator.Except );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCompoundQueryComponentNode"/> instance.
    /// </summary>
    /// <param name="operator">Compound query operator with which this query should be included.</param>
    /// <param name="query">Underlying query.</param>
    /// <returns>New <see cref="SqlCompoundQueryComponentNode"/> instance.</returns>
    [Pure]
    public static SqlCompoundQueryComponentNode CompoundWith(SqlCompoundQueryOperator @operator, SqlQueryExpressionNode query)
    {
        Ensure.IsDefined( @operator );
        return new SqlCompoundQueryComponentNode( query, @operator );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSwitchExpressionNode"/> instance.
    /// </summary>
    /// <param name="condition">Underlying condition.</param>
    /// <param name="whenTrue">Expression to be returned when the condition returns <b>true</b>.</param>
    /// <param name="whenFalse">Expression to be returned when the condition returns <b>false</b>.</param>
    /// <returns>New <see cref="SqlSwitchExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlSwitchExpressionNode Iif(SqlConditionNode condition, SqlExpressionNode whenTrue, SqlExpressionNode whenFalse)
    {
        return Switch( new[] { SwitchCase( condition, whenTrue ) }, whenFalse );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSwitchExpressionNode"/> instance.
    /// </summary>
    /// <param name="cases">Collection of cases.</param>
    /// <param name="defaultExpression">Default expression.</param>
    /// <returns>New <see cref="SqlSwitchExpressionNode"/> instance.</returns>
    [Pure]
    public static SqlSwitchExpressionNode Switch(IEnumerable<SqlSwitchCaseNode> cases, SqlExpressionNode defaultExpression)
    {
        return new SqlSwitchExpressionNode( cases.ToArray(), defaultExpression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlSwitchCaseNode"/> instance.
    /// </summary>
    /// <param name="condition">Underlying condition.</param>
    /// <param name="expression">Underlying expression.</param>
    /// <returns>New <see cref="SqlSwitchCaseNode"/> instance.</returns>
    [Pure]
    public static SqlSwitchCaseNode SwitchCase(SqlConditionNode condition, SqlExpressionNode expression)
    {
        return new SqlSwitchCaseNode( condition, expression );
    }

    /// <summary>
    /// Creates a new <see cref="SqlValuesNode"/> instance from a 2-dimensional collection of values.
    /// </summary>
    /// <param name="expressions">2-dimensional collection of values.</param>
    /// <returns>New <see cref="SqlValuesNode"/> instance.</returns>
    [Pure]
    public static SqlValuesNode Values(SqlExpressionNode[,] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlValuesNode"/> instance from a 1-dimensional collection of values.
    /// </summary>
    /// <param name="expressions">1-dimensional collection of values.</param>
    /// <returns>New <see cref="SqlValuesNode"/> instance.</returns>
    [Pure]
    public static SqlValuesNode Values(params SqlExpressionNode[] expressions)
    {
        return new SqlValuesNode( expressions );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRawStatementNode"/> instance.
    /// </summary>
    /// <param name="sql">Raw SQL statement.</param>
    /// <param name="parameters">Collection of parameter nodes.</param>
    /// <returns>New <see cref="SqlRawStatementNode"/> instance.</returns>
    [Pure]
    public static SqlRawStatementNode RawStatement(string sql, params SqlParameterNode[] parameters)
    {
        return new SqlRawStatementNode( sql, parameters );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDeleteFromNode"/> instance.
    /// </summary>
    /// <param name="dataSource">Data source that defines records to be deleted.</param>
    /// <returns>New <see cref="SqlDeleteFromNode"/> instance.</returns>
    [Pure]
    public static SqlDeleteFromNode DeleteFrom(SqlDataSourceNode dataSource)
    {
        return new SqlDeleteFromNode( dataSource );
    }

    /// <summary>
    /// Creates a new <see cref="SqlTruncateNode"/> instance.
    /// </summary>
    /// <param name="table">Table to truncate.</param>
    /// <returns>New <see cref="SqlTruncateNode"/> instance.</returns>
    [Pure]
    public static SqlTruncateNode Truncate(SqlRecordSetNode table)
    {
        return new SqlTruncateNode( table );
    }

    /// <summary>
    /// Creates a new <see cref="SqlValueAssignmentNode"/> instance.
    /// </summary>
    /// <param name="dataField">Data field to assign value to.</param>
    /// <param name="value">Value to assign.</param>
    /// <returns>New <see cref="SqlValueAssignmentNode"/> instance.</returns>
    [Pure]
    public static SqlValueAssignmentNode ValueAssignment(SqlDataFieldNode dataField, SqlExpressionNode value)
    {
        return new SqlValueAssignmentNode( dataField, value );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpdateNode"/> instance.
    /// </summary>
    /// <param name="dataSource">Data source that defines records to be updated.</param>
    /// <param name="assignments">Collection of value assignments that this update refers to.</param>
    /// <returns>New <see cref="SqlUpdateNode"/> instance.</returns>
    [Pure]
    public static SqlUpdateNode Update(SqlDataSourceNode dataSource, params SqlValueAssignmentNode[] assignments)
    {
        return new SqlUpdateNode( dataSource, assignments );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="query"><see cref="SqlQueryExpressionNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    [Pure]
    public static SqlInsertIntoNode InsertInto(
        SqlQueryExpressionNode query,
        SqlRecordSetNode recordSet,
        params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( query, recordSet, dataFields );
    }

    /// <summary>
    /// Creates a new <see cref="SqlInsertIntoNode"/> instance.
    /// </summary>
    /// <param name="values"><see cref="SqlValuesNode"/> source of records to be inserted.</param>
    /// <param name="recordSet">Table to insert into.</param>
    /// <param name="dataFields">Collection of record set data fields that this insertion refers to.</param>
    /// <returns>New <see cref="SqlInsertIntoNode"/> instance.</returns>
    [Pure]
    public static SqlInsertIntoNode InsertInto(SqlValuesNode values, SqlRecordSetNode recordSet, params SqlDataFieldNode[] dataFields)
    {
        return new SqlInsertIntoNode( values, recordSet, dataFields );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="query"><see cref="SqlQueryExpressionNode"/> source of records to be inserted or updated.</param>
    /// <param name="recordSet">Table to upsert into.</param>
    /// <param name="insertDataFields">Collection of record set data fields that the insertion part of this upsert refers to.</param>
    /// <param name="updateAssignments">
    /// Provider of a collection of value assignments that the update part of this upsert refers to.
    /// The first parameter is the table to upsert into and the second parameter is the <see cref="SqlUpsertNode.UpdateSource"/>
    /// of the created upsert node.
    /// </param>
    /// <param name="conflictTarget">
    /// Optional collection of data fields from the table that define the insertion conflict target.
    /// Empty conflict target may cause the table's primary key to be used instead. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    [Pure]
    public static SqlUpsertNode Upsert(
        SqlQueryExpressionNode query,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        ReadOnlyArray<SqlDataFieldNode>? conflictTarget = null)
    {
        return new SqlUpsertNode(
            query,
            recordSet,
            insertDataFields,
            conflictTarget ?? ReadOnlyArray<SqlDataFieldNode>.Empty,
            updateAssignments );
    }

    /// <summary>
    /// Creates a new <see cref="SqlUpsertNode"/> instance.
    /// </summary>
    /// <param name="values"><see cref="SqlValuesNode"/> source of records to be inserted or updated.</param>
    /// <param name="recordSet">Table to upsert into.</param>
    /// <param name="insertDataFields">Collection of record set data fields that the insertion part of this upsert refers to.</param>
    /// <param name="updateAssignments">
    /// Provider of a collection of value assignments that the update part of this upsert refers to.
    /// The first parameter is the table to upsert into and the second parameter is the <see cref="SqlUpsertNode.UpdateSource"/>
    /// of the created upsert node.
    /// </param>
    /// <param name="conflictTarget">
    /// Optional collection of data fields from the table that define the insertion conflict target.
    /// Empty conflict target may cause the table's primary key to be used instead. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlUpsertNode"/> instance.</returns>
    [Pure]
    public static SqlUpsertNode Upsert(
        SqlValuesNode values,
        SqlRecordSetNode recordSet,
        ReadOnlyArray<SqlDataFieldNode> insertDataFields,
        Func<SqlRecordSetNode, SqlInternalRecordSetNode, IEnumerable<SqlValueAssignmentNode>> updateAssignments,
        ReadOnlyArray<SqlDataFieldNode>? conflictTarget = null)
    {
        return new SqlUpsertNode(
            values,
            recordSet,
            insertDataFields,
            conflictTarget ?? ReadOnlyArray<SqlDataFieldNode>.Empty,
            updateAssignments );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Column's name.</param>
    /// <param name="isNullable">Specifies whether or not this column should be nullable. Equal to <b>false</b> by default.</param>
    /// <param name="defaultValue">Column's optional default value. Equal to null by default.</param>
    /// <param name="computation">Column's optional computation. Equal to null by default.</param>
    /// <typeparam name="T">Column's runtime type.</typeparam>
    /// <returns>New <see cref="SqlColumnDefinitionNode"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnDefinitionNode Column<T>(
        string name,
        bool isNullable = false,
        SqlExpressionNode? defaultValue = null,
        SqlColumnComputation? computation = null)
        where T : notnull
    {
        return Column( name, TypeNullability.Create<T>( isNullable ), defaultValue, computation );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Column's name.</param>
    /// <param name="type">Column's runtime type.</param>
    /// <param name="defaultValue">Column's optional default value. Equal to null by default.</param>
    /// <param name="computation">Column's optional computation. Equal to null by default.</param>
    /// <returns>New <see cref="SqlColumnDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlColumnDefinitionNode Column(
        string name,
        TypeNullability type,
        SqlExpressionNode? defaultValue = null,
        SqlColumnComputation? computation = null)
    {
        return new SqlColumnDefinitionNode( name, type, defaultValue, computation );
    }

    /// <summary>
    /// Creates a new <see cref="SqlColumnDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Column's name.</param>
    /// <param name="typeDefinition"><see cref="ISqlColumnTypeDefinition"/> instance that defines this column's type.</param>
    /// <param name="isNullable">Specifies whether or not this column should be nullable. Equal to <b>false</b> by default.</param>
    /// <param name="defaultValue">Column's optional default value. Equal to null by default.</param>
    /// <param name="computation">Column's optional computation. Equal to null by default.</param>
    /// <returns>New <see cref="SqlColumnDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlColumnDefinitionNode Column(
        string name,
        ISqlColumnTypeDefinition typeDefinition,
        bool isNullable = false,
        SqlExpressionNode? defaultValue = null,
        SqlColumnComputation? computation = null)
    {
        return new SqlColumnDefinitionNode( name, typeDefinition, isNullable, defaultValue, computation );
    }

    /// <summary>
    /// Creates a new <see cref="SqlPrimaryKeyDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Primary key constraint's name.</param>
    /// <param name="columns">Collection of columns that define this primary key constraint.</param>
    /// <returns>New <see cref="SqlPrimaryKeyDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlPrimaryKeyDefinitionNode PrimaryKey(SqlSchemaObjectName name, ReadOnlyArray<SqlOrderByNode> columns)
    {
        return new SqlPrimaryKeyDefinitionNode( name, columns );
    }

    /// <summary>
    /// Creates a new <see cref="SqlForeignKeyDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Foreign key constraint's name.</param>
    /// <param name="columns">Collection of columns from source table that this foreign key originates from.</param>
    /// <param name="referencedTable">Table referenced by this foreign key constraint.</param>
    /// <param name="referencedColumns">Collection of columns from referenced table referenced by this foreign key constraint.</param>
    /// <param name="onDeleteBehavior">
    /// Specifies this foreign key constraint's on delete behavior. Equal to <see cref="ReferenceBehavior.Restrict"/> by default.
    /// </param>
    /// <param name="onUpdateBehavior">
    /// Specifies this foreign key constraint's on update behavior. Equal to <see cref="ReferenceBehavior.Restrict"/> by default.
    /// </param>
    /// <returns>New <see cref="SqlForeignKeyDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlForeignKeyDefinitionNode ForeignKey(
        SqlSchemaObjectName name,
        SqlDataFieldNode[] columns,
        SqlRecordSetNode referencedTable,
        SqlDataFieldNode[] referencedColumns,
        ReferenceBehavior? onDeleteBehavior = null,
        ReferenceBehavior? onUpdateBehavior = null)
    {
        return new SqlForeignKeyDefinitionNode(
            name,
            columns,
            referencedTable,
            referencedColumns,
            onDeleteBehavior ?? ReferenceBehavior.Restrict,
            onUpdateBehavior ?? ReferenceBehavior.Restrict );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCheckDefinitionNode"/> instance.
    /// </summary>
    /// <param name="name">Check constraint's name.</param>
    /// <param name="condition">Check constraint's condition.</param>
    /// <returns>New <see cref="SqlCheckDefinitionNode"/> instance.</returns>
    [Pure]
    public static SqlCheckDefinitionNode Check(SqlSchemaObjectName name, SqlConditionNode condition)
    {
        return new SqlCheckDefinitionNode( name, condition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateTableNode"/> instance.
    /// </summary>
    /// <param name="info">Table's name.</param>
    /// <param name="columns">Collection of columns.</param>
    /// <param name="ifNotExists">
    /// Specifies whether or not this table should only be created if it does not already exist in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <param name="constraintsProvider">Optional <see cref="SqlCreateTableConstraints"/> provider.</param>
    /// <returns>New <see cref="SqlCreateTableNode"/> instance.</returns>
    [Pure]
    public static SqlCreateTableNode CreateTable(
        SqlRecordSetInfo info,
        SqlColumnDefinitionNode[] columns,
        bool ifNotExists = false,
        Func<SqlNewTableNode, SqlCreateTableConstraints>? constraintsProvider = null)
    {
        return new SqlCreateTableNode( info, ifNotExists, columns, constraintsProvider );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateViewNode"/> instance.
    /// </summary>
    /// <param name="info">View's name.</param>
    /// <param name="source">Underlying source query expression that defines this view.</param>
    /// <param name="replaceIfExists">
    /// Specifies whether or not the view should be replaced if it already exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlCreateViewNode"/> instance.</returns>
    [Pure]
    public static SqlCreateViewNode CreateView(SqlRecordSetInfo info, SqlQueryExpressionNode source, bool replaceIfExists = false)
    {
        return new SqlCreateViewNode( info, replaceIfExists, source );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateIndexNode"/> instance.
    /// </summary>
    /// <param name="name">Index's name.</param>
    /// <param name="isUnique">Specifies whether or not this index is unique.</param>
    /// <param name="table">Table on which this index is created.</param>
    /// <param name="columns">Collection of expressions that define this index.</param>
    /// <param name="replaceIfExists">
    /// Specifies whether or not the index should be replaced if it already exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <param name="filter">Optional filter condition. Equal to null by default.</param>
    /// <returns>New <see cref="SqlCreateIndexNode"/> instance.</returns>
    [Pure]
    public static SqlCreateIndexNode CreateIndex(
        SqlSchemaObjectName name,
        bool isUnique,
        SqlRecordSetNode table,
        ReadOnlyArray<SqlOrderByNode> columns,
        bool replaceIfExists = false,
        SqlConditionNode? filter = null)
    {
        return new SqlCreateIndexNode( name, isUnique, replaceIfExists, table, columns, filter );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRenameTableNode"/> instance.
    /// </summary>
    /// <param name="info">Table's old name.</param>
    /// <param name="newName">Table's new name.</param>
    /// <returns>New <see cref="SqlRenameTableNode"/> instance.</returns>
    [Pure]
    public static SqlRenameTableNode RenameTable(SqlRecordSetInfo info, SqlSchemaObjectName newName)
    {
        return new SqlRenameTableNode( info, newName );
    }

    /// <summary>
    /// Creates a new <see cref="SqlRenameColumnNode"/> instance.
    /// </summary>
    /// <param name="table">Source table.</param>
    /// <param name="oldName">Column's new name.</param>
    /// <param name="newName">Column's new name.</param>
    /// <returns>New <see cref="SqlRenameColumnNode"/> instance.</returns>
    [Pure]
    public static SqlRenameColumnNode RenameColumn(SqlRecordSetInfo table, string oldName, string newName)
    {
        return new SqlRenameColumnNode( table, oldName, newName );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAddColumnNode"/> instance.
    /// </summary>
    /// <param name="table">Source table.</param>
    /// <param name="definition">Definition of the column to add.</param>
    /// <returns>New <see cref="SqlAddColumnNode"/> instance.</returns>
    [Pure]
    public static SqlAddColumnNode AddColumn(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
    {
        return new SqlAddColumnNode( table, definition );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropColumnNode"/> instance.
    /// </summary>
    /// <param name="table">Source table.</param>
    /// <param name="name">Column's name.</param>
    /// <returns>New <see cref="SqlDropColumnNode"/> instance.</returns>
    [Pure]
    public static SqlDropColumnNode DropColumn(SqlRecordSetInfo table, string name)
    {
        return new SqlDropColumnNode( table, name );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropTableNode"/> instance.
    /// </summary>
    /// <param name="table">Table's name.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this table exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropTableNode"/> instance.</returns>
    [Pure]
    public static SqlDropTableNode DropTable(SqlRecordSetInfo table, bool ifExists = false)
    {
        return new SqlDropTableNode( table, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropViewNode"/> instance.
    /// </summary>
    /// <param name="view">View's name.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this view exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropViewNode"/> instance.</returns>
    [Pure]
    public static SqlDropViewNode DropView(SqlRecordSetInfo view, bool ifExists = false)
    {
        return new SqlDropViewNode( view, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDropIndexNode"/> instance.
    /// </summary>
    /// <param name="table">Source table.</param>
    /// <param name="name">Index's name.</param>
    /// <param name="ifExists">
    /// Specifies whether or not the removal attempt should only be made if this index exists in DB. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="SqlDropIndexNode"/> instance.</returns>
    [Pure]
    public static SqlDropIndexNode DropIndex(SqlRecordSetInfo table, SqlSchemaObjectName name, bool ifExists = false)
    {
        return new SqlDropIndexNode( table, name, ifExists );
    }

    /// <summary>
    /// Creates a new <see cref="SqlStatementBatchNode"/> instance.
    /// </summary>
    /// <param name="statements">Collection of SQL statements.</param>
    /// <returns>New <see cref="SqlStatementBatchNode"/> instance.</returns>
    [Pure]
    public static SqlStatementBatchNode Batch(params ISqlStatementNode[] statements)
    {
        return new SqlStatementBatchNode( statements );
    }

    /// <summary>
    /// Creates a new <see cref="SqlBeginTransactionNode"/> instance.
    /// </summary>
    /// <param name="isolationLevel">Transaction's <see cref="System.Data.IsolationLevel"/>.</param>
    /// <returns>New <see cref="SqlBeginTransactionNode"/> instance.</returns>
    [Pure]
    public static SqlBeginTransactionNode BeginTransaction(IsolationLevel isolationLevel)
    {
        return new SqlBeginTransactionNode( isolationLevel );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCommitTransactionNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlCommitTransactionNode"/> instance.</returns>
    [Pure]
    public static SqlCommitTransactionNode CommitTransaction()
    {
        return _commitTransaction ??= new SqlCommitTransactionNode();
    }

    /// <summary>
    /// Creates a new <see cref="SqlRollbackTransactionNode"/> instance.
    /// </summary>
    /// <returns>New <see cref="SqlRollbackTransactionNode"/> instance.</returns>
    [Pure]
    public static SqlRollbackTransactionNode RollbackTransaction()
    {
        return _rollbackTransaction ??= new SqlRollbackTransactionNode();
    }
}
