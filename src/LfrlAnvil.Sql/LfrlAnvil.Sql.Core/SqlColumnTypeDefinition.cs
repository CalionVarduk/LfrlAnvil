using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql;

public abstract class SqlColumnTypeDefinition : ISqlColumnTypeDefinition
{
    internal SqlColumnTypeDefinition(ISqlDataType dataType, SqlLiteralNode defaultValue, LambdaExpression outputMapping)
    {
        DataType = dataType;
        DefaultValue = defaultValue;
        OutputMapping = outputMapping;
    }

    public ISqlDataType DataType { get; }
    public SqlLiteralNode DefaultValue { get; }
    public LambdaExpression OutputMapping { get; }
    public abstract Type RuntimeType { get; }

    [Pure]
    public sealed override string ToString()
    {
        return $"{RuntimeType.GetDebugString()} <=> {DataType}, {nameof( DefaultValue )}: [{DefaultValue}]";
    }

    [Pure]
    public abstract string? TryToDbLiteral(object value);

    [Pure]
    public abstract object? TryToParameterValue(object value);

    public abstract void SetParameterInfo(IDbDataParameter parameter, bool isNullable);
}

public abstract class SqlColumnTypeDefinition<T> : SqlColumnTypeDefinition, ISqlColumnTypeDefinition<T>
    where T : notnull
{
    internal SqlColumnTypeDefinition(ISqlDataType dataType, T defaultValue, LambdaExpression outputMapping)
        : base( dataType, ( SqlLiteralNode )SqlNode.Literal( defaultValue ), outputMapping ) { }

    public new SqlLiteralNode<T> DefaultValue => ReinterpretCast.To<SqlLiteralNode<T>>( base.DefaultValue );
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

public abstract class SqlColumnTypeDefinition<T, TDataRecord, TParameter> : SqlColumnTypeDefinition<T>
    where T : notnull
    where TDataRecord : IDataRecord
    where TParameter : IDbDataParameter
{
    protected SqlColumnTypeDefinition(ISqlDataType dataType, T defaultValue, Expression<Func<TDataRecord, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    public new Expression<Func<TDataRecord, int, T>> OutputMapping =>
        ReinterpretCast.To<Expression<Func<TDataRecord, int, T>>>( base.OutputMapping );

    public virtual void SetParameterInfo(TParameter parameter, bool isNullable)
    {
        parameter.DbType = DataType.DbType;
    }

    [EditorBrowsable( EditorBrowsableState.Never )]
    public sealed override void SetParameterInfo(IDbDataParameter parameter, bool isNullable)
    {
        if ( parameter is TParameter p )
            SetParameterInfo( p, isNullable );
        else
            parameter.DbType = DataType.DbType;
    }
}
