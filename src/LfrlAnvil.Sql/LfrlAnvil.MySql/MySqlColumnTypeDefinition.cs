using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public abstract class MySqlColumnTypeDefinition : ISqlColumnTypeDefinition
{
    internal MySqlColumnTypeDefinition(MySqlDataType dataType, SqlLiteralNode defaultValue, LambdaExpression outputMapping)
    {
        DataType = dataType;
        DefaultValue = defaultValue;
        OutputMapping = outputMapping;
    }

    public MySqlDataType DataType { get; }
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

    public virtual void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.MySqlDbType = DataType.Value;
    }

    void ISqlColumnTypeDefinition.SetParameterInfo(IDbDataParameter parameter, bool isNullable)
    {
        if ( parameter is MySqlParameter mySql )
            SetParameterInfo( mySql, isNullable );
        else
            parameter.DbType = DataType.DbType;
    }
}

public abstract class MySqlColumnTypeDefinition<T> : MySqlColumnTypeDefinition, ISqlColumnTypeDefinition<T>
    where T : notnull
{
    protected MySqlColumnTypeDefinition(MySqlDataType dataType, T defaultValue, Expression<Func<MySqlDataReader, int, T>> outputMapping)
        : base( dataType, (SqlLiteralNode)SqlNode.Literal( defaultValue ), outputMapping ) { }

    public new SqlLiteralNode<T> DefaultValue => ReinterpretCast.To<SqlLiteralNode<T>>( base.DefaultValue );

    public new Expression<Func<MySqlDataReader, int, T>> OutputMapping =>
        ReinterpretCast.To<Expression<Func<MySqlDataReader, int, T>>>( base.OutputMapping );

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
