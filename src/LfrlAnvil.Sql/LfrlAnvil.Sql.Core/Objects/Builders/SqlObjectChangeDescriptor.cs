using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Objects.Builders;

public abstract class SqlObjectChangeDescriptor : IEquatable<SqlObjectChangeDescriptor>
{
    public static readonly SqlObjectChangeDescriptor<bool> IsRemoved =
        new SqlObjectChangeDescriptor<bool>( nameof( IsRemoved ), 0 );

    public static readonly SqlObjectChangeDescriptor<string> Name =
        new SqlObjectChangeDescriptor<string>( nameof( Name ), 1 );

    public static readonly SqlObjectChangeDescriptor<string> SchemaName =
        new SqlObjectChangeDescriptor<string>( nameof( SchemaName ), 2 );

    public static readonly SqlObjectChangeDescriptor<bool> IsNullable =
        new SqlObjectChangeDescriptor<bool>( nameof( IsNullable ), 3 );

    public static readonly SqlObjectChangeDescriptor<ISqlDataType> DataType =
        new SqlObjectChangeDescriptor<ISqlDataType>( nameof( DataType ), 4 );

    public static readonly SqlObjectChangeDescriptor<SqlExpressionNode?> DefaultValue =
        new SqlObjectChangeDescriptor<SqlExpressionNode?>( nameof( DefaultValue ), 5 );

    public static readonly SqlObjectChangeDescriptor<bool> IsUnique =
        new SqlObjectChangeDescriptor<bool>( nameof( IsUnique ), 6 );

    public static readonly SqlObjectChangeDescriptor<SqlConditionNode?> Filter =
        new SqlObjectChangeDescriptor<SqlConditionNode?>( nameof( Filter ), 7 );

    public static readonly SqlObjectChangeDescriptor<SqlPrimaryKeyBuilder?> PrimaryKey =
        new SqlObjectChangeDescriptor<SqlPrimaryKeyBuilder?>( nameof( PrimaryKey ), 8 );

    public static readonly SqlObjectChangeDescriptor<ReferenceBehavior> OnDeleteBehavior =
        new SqlObjectChangeDescriptor<ReferenceBehavior>( nameof( OnDeleteBehavior ), 9 );

    public static readonly SqlObjectChangeDescriptor<ReferenceBehavior> OnUpdateBehavior =
        new SqlObjectChangeDescriptor<ReferenceBehavior>( nameof( OnUpdateBehavior ), 10 );

    internal SqlObjectChangeDescriptor(string description, int key)
    {
        Description = description;
        Key = key;
    }

    public string Description { get; }
    public int Key { get; }
    public abstract Type Type { get; }

    [Pure]
    public override string ToString()
    {
        return $"[{Key}] : '{Description}' ({Type.GetDebugString()})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlObjectChangeDescriptor t && Equals( t );
    }

    [Pure]
    public bool Equals(SqlObjectChangeDescriptor? other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        return other is not null && Key == other.Key && Type == other.Type;
    }

    [Pure]
    public static bool operator ==(SqlObjectChangeDescriptor? a, SqlObjectChangeDescriptor? b)
    {
        return a?.Equals( b ) ?? b is null;
    }

    [Pure]
    public static bool operator !=(SqlObjectChangeDescriptor? a, SqlObjectChangeDescriptor? b)
    {
        return ! a?.Equals( b ) ?? b is not null;
    }
}

public sealed class SqlObjectChangeDescriptor<T> : SqlObjectChangeDescriptor
{
    internal SqlObjectChangeDescriptor(string description, int key)
        : base( description, key ) { }

    public override Type Type => typeof( T );

    [Pure]
    public static SqlObjectChangeDescriptor<T> Create(string description, int key)
    {
        Ensure.IsNotInRange( key, IsRemoved.Key, OnUpdateBehavior.Key );
        return new SqlObjectChangeDescriptor<T>( description, key );
    }
}
