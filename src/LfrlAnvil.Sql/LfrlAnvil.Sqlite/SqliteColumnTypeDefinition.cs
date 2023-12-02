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
    public SqliteColumnTypeDefinition<TTarget> Extend<TTarget>(
        Func<TTarget, T> mapper,
        Expression<Func<T, TTarget>> outputMapper,
        TTarget defaultValue)
        where TTarget : notnull
    {
        return new SqliteColumnTypeDefinitionLambda<TTarget, T>( this, defaultValue, mapper, outputMapper );
    }

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

    [Pure]
    ISqlColumnTypeDefinition<TTarget> ISqlColumnTypeDefinition<T>.Extend<TTarget>(
        Func<TTarget, T> mapper,
        Expression<Func<T, TTarget>> outputMapper,
        TTarget defaultValue)
    {
        return Extend( mapper, outputMapper, defaultValue );
    }
}

public abstract class SqliteColumnTypeDefinition<T, TBase> : SqliteColumnTypeDefinition<T>
    where T : notnull
    where TBase : notnull
{
    protected SqliteColumnTypeDefinition(
        SqliteColumnTypeDefinition<TBase> @base,
        T defaultValue,
        Expression<Func<SqliteDataReader, int, T>> outputMapping)
        : base( @base.DataType, defaultValue, outputMapping )
    {
        Base = @base;
    }

    protected SqliteColumnTypeDefinition<TBase> Base { get; }

    [Pure]
    public sealed override string ToDbLiteral(T value)
    {
        var baseValue = MapToBaseType( value );
        return Base.ToDbLiteral( baseValue );
    }

    [Pure]
    public sealed override object ToParameterValue(T value)
    {
        var baseValue = MapToBaseType( value );
        return Base.ToParameterValue( baseValue );
    }

    [Pure]
    protected abstract TBase MapToBaseType(T value);
}

internal sealed class SqliteColumnTypeDefinitionLambda<T, TBase> : SqliteColumnTypeDefinition<T, TBase>
    where T : notnull
    where TBase : notnull
{
    private readonly Func<T, TBase> _mapper;

    internal SqliteColumnTypeDefinitionLambda(
        SqliteColumnTypeDefinition<TBase> @base,
        T defaultValue,
        Func<T, TBase> mapper,
        Expression<Func<TBase, T>> outputMapper)
        : base( @base, defaultValue, CreateOutputExpression( @base.OutputMapping, outputMapper ) )
    {
        _mapper = mapper;
    }

    [Pure]
    protected override TBase MapToBaseType(T value)
    {
        return _mapper( value );
    }

    [Pure]
    private static Expression<Func<SqliteDataReader, int, T>> CreateOutputExpression(
        Expression<Func<SqliteDataReader, int, TBase>> baseOutputMapping,
        Expression<Func<TBase, T>> outputMapper)
    {
        var body = outputMapper.Body.ReplaceParameter( outputMapper.Parameters[0], baseOutputMapping.Body );
        return Expression.Lambda<Func<SqliteDataReader, int, T>>( body, baseOutputMapping.Parameters );
    }
}
