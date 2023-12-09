using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteColumnTypeDefinition : ISqlColumnTypeDefinition
{
    internal SqliteColumnTypeDefinition(SqliteDataType dataType, SqlLiteralNode defaultValue, LambdaExpression outputMapping)
    {
        DataType = dataType;
        DefaultValue = defaultValue;
        OutputMapping = outputMapping;
    }

    public SqliteDataType DataType { get; }
    public SqlLiteralNode DefaultValue { get; }
    public LambdaExpression OutputMapping { get; }
    public abstract Type RuntimeType { get; }
    ISqlDataType ISqlColumnTypeDefinition.DataType => DataType;

    [Pure]
    public sealed override string ToString()
    {
        return $"{RuntimeType.GetDebugString()} <=> {DataType}, {nameof( DefaultValue )}: [{DefaultValue}]";
    }

    [Pure]
    public abstract string? TryToDbLiteral(object value);

    [Pure]
    public abstract object? TryToParameterValue(object value);

    public virtual void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.SqliteType = DataType.Value;
        parameter.DbType = DataType.DbType;
    }

    void ISqlColumnTypeDefinition.SetParameterInfo(IDbDataParameter parameter, bool isNullable)
    {
        if ( parameter is SqliteParameter sqlite )
            SetParameterInfo( sqlite, isNullable );
        else
            parameter.DbType = DataType.DbType;
    }
}

public abstract class SqliteColumnTypeDefinition<T> : SqliteColumnTypeDefinition, ISqlColumnTypeDefinition<T>
    where T : notnull
{
    protected SqliteColumnTypeDefinition(SqliteDataType dataType, T defaultValue, Expression<Func<SqliteDataReader, int, T>> outputMapping)
        : base( dataType, (SqlLiteralNode)SqlNode.Literal( defaultValue ), outputMapping ) { }

    public new SqlLiteralNode<T> DefaultValue => ReinterpretCast.To<SqlLiteralNode<T>>( base.DefaultValue );

    public new Expression<Func<SqliteDataReader, int, T>> OutputMapping =>
        ReinterpretCast.To<Expression<Func<SqliteDataReader, int, T>>>( base.OutputMapping );

    public sealed override Type RuntimeType => typeof( T );

    [Pure]
    public abstract string ToDbLiteral(T value);

    [Pure]
    public abstract object ToParameterValue(T value);

    [Pure]
    public sealed override string? TryToDbLiteral(object value)
    {
        return value is T t ? ToDbLiteral( t ) : null;
    }

    [Pure]
    public sealed override object? TryToParameterValue(object value)
    {
        return value is T t ? ToParameterValue( t ) : null;
    }
}
