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
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a type-erased description of an SQL object builder property change.
/// </summary>
public abstract class SqlObjectChangeDescriptor : IEquatable<SqlObjectChangeDescriptor>
{
    /// <summary>
    /// Represents a change in object's <see cref="ISqlObjectBuilder.IsRemoved"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<bool> IsRemoved =
        new SqlObjectChangeDescriptor<bool>( nameof( IsRemoved ), 0 );

    /// <summary>
    /// Represents a change in object's <see cref="ISqlObjectBuilder.Name"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<string> Name =
        new SqlObjectChangeDescriptor<string>( nameof( Name ), 1 );

    /// <summary>
    /// Represents a change in column's <see cref="ISqlColumnBuilder.IsNullable"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<bool> IsNullable =
        new SqlObjectChangeDescriptor<bool>( nameof( IsNullable ), 2 );

    /// <summary>
    /// Represents a change in column's <see cref="ISqlDataType"/> from its <see cref="ISqlColumnBuilder.TypeDefinition"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<ISqlDataType> DataType =
        new SqlObjectChangeDescriptor<ISqlDataType>( nameof( DataType ), 3 );

    /// <summary>
    /// Represents a change in column's <see cref="ISqlColumnBuilder.DefaultValue"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<SqlExpressionNode?> DefaultValue =
        new SqlObjectChangeDescriptor<SqlExpressionNode?>( nameof( DefaultValue ), 4 );

    /// <summary>
    /// Represents a change in column's <see cref="ISqlColumnBuilder.Computation"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<SqlColumnComputation?> Computation =
        new SqlObjectChangeDescriptor<SqlColumnComputation?>( nameof( Computation ), 5 );

    /// <summary>
    /// Represents a change in index's <see cref="ISqlIndexBuilder.IsUnique"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<bool> IsUnique =
        new SqlObjectChangeDescriptor<bool>( nameof( IsUnique ), 6 );

    /// <summary>
    /// Represents a change in index's <see cref="ISqlIndexBuilder.IsVirtual"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<bool> IsVirtual =
        new SqlObjectChangeDescriptor<bool>( nameof( IsVirtual ), 7 );

    /// <summary>
    /// Represents a change in index's <see cref="ISqlIndexBuilder.Filter"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<SqlConditionNode?> Filter =
        new SqlObjectChangeDescriptor<SqlConditionNode?>( nameof( Filter ), 8 );

    /// <summary>
    /// Represents a change in index's <see cref="ISqlIndexBuilder.PrimaryKey"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<SqlPrimaryKeyBuilder?> PrimaryKey =
        new SqlObjectChangeDescriptor<SqlPrimaryKeyBuilder?>( nameof( PrimaryKey ), 9 );

    /// <summary>
    /// Represents a change in foreign key's <see cref="ISqlForeignKeyBuilder.OnDeleteBehavior"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<ReferenceBehavior> OnDeleteBehavior =
        new SqlObjectChangeDescriptor<ReferenceBehavior>( nameof( OnDeleteBehavior ), 10 );

    /// <summary>
    /// Represents a change in foreign key's <see cref="ISqlForeignKeyBuilder.OnUpdateBehavior"/> property.
    /// </summary>
    public static readonly SqlObjectChangeDescriptor<ReferenceBehavior> OnUpdateBehavior =
        new SqlObjectChangeDescriptor<ReferenceBehavior>( nameof( OnUpdateBehavior ), 11 );

    internal SqlObjectChangeDescriptor(string description, int key)
    {
        Description = description;
        Key = key;
    }

    /// <summary>
    /// Description of the change.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Identifier of the change.
    /// </summary>
    public int Key { get; }

    /// <summary>
    /// Type of the property associated with this change.
    /// </summary>
    public abstract Type Type { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObjectChangeDescriptor"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Key}] : '{Description}' ({Type.GetDebugString()})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlObjectChangeDescriptor t && Equals( t );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlObjectChangeDescriptor? other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        return other is not null && Key == other.Key && Type == other.Type;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlObjectChangeDescriptor? a, SqlObjectChangeDescriptor? b)
    {
        return a?.Equals( b ) ?? b is null;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(SqlObjectChangeDescriptor? a, SqlObjectChangeDescriptor? b)
    {
        return ! a?.Equals( b ) ?? b is not null;
    }
}

/// <summary>
/// Represents a generic description of an SQL object builder property change.
/// </summary>
/// <typeparam name="T">Type of the property associated with the change.</typeparam>
public sealed class SqlObjectChangeDescriptor<T> : SqlObjectChangeDescriptor
{
    internal SqlObjectChangeDescriptor(string description, int key)
        : base( description, key ) { }

    /// <inheritdoc />
    public override Type Type => typeof( T );

    /// <summary>
    /// Creates a new <see cref="SqlObjectChangeDescriptor{T}"/> instance.
    /// </summary>
    /// <param name="description">Description of the change.</param>
    /// <param name="key">Identifier of the change.</param>
    /// <returns>New <see cref="SqlObjectChangeDescriptor{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="key"/> is reserved. See <see cref="SqlObjectChangeDescriptor"/> and its static members for more information.
    /// </exception>
    [Pure]
    public static SqlObjectChangeDescriptor<T> Create(string description, int key)
    {
        Ensure.IsNotInRange( key, IsRemoved.Key, OnUpdateBehavior.Key );
        return new SqlObjectChangeDescriptor<T>( description, key );
    }
}
