using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql;

/// <inheritdoc />
public abstract class SqlColumnTypeDefinition : ISqlColumnTypeDefinition
{
    internal SqlColumnTypeDefinition(ISqlDataType dataType, SqlLiteralNode defaultValue, LambdaExpression outputMapping)
    {
        DataType = dataType;
        DefaultValue = defaultValue;
        OutputMapping = outputMapping;
    }

    /// <inheritdoc />
    public ISqlDataType DataType { get; }

    /// <inheritdoc />
    public SqlLiteralNode DefaultValue { get; }

    /// <inheritdoc />
    public LambdaExpression OutputMapping { get; }

    /// <inheritdoc />
    public abstract Type RuntimeType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlColumnTypeDefinition"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public sealed override string ToString()
    {
        return $"{RuntimeType.GetDebugString()} <=> {DataType}, {nameof( DefaultValue )}: [{DefaultValue}]";
    }

    /// <inheritdoc />
    [Pure]
    public abstract string? TryToDbLiteral(object value);

    /// <inheritdoc />
    [Pure]
    public abstract object? TryToParameterValue(object value);

    /// <inheritdoc />
    public abstract void SetParameterInfo(IDbDataParameter parameter, bool isNullable);
}

/// <inheritdoc cref="ISqlColumnTypeDefinition{T}" />
public abstract class SqlColumnTypeDefinition<T> : SqlColumnTypeDefinition, ISqlColumnTypeDefinition<T>
    where T : notnull
{
    internal SqlColumnTypeDefinition(ISqlDataType dataType, T defaultValue, LambdaExpression outputMapping)
        : base( dataType, ( SqlLiteralNode )SqlNode.Literal( defaultValue ), outputMapping ) { }

    /// <inheritdoc />
    public new SqlLiteralNode<T> DefaultValue => ReinterpretCast.To<SqlLiteralNode<T>>( base.DefaultValue );

    /// <inheritdoc />
    public sealed override Type RuntimeType => typeof( T );

    /// <inheritdoc />
    [Pure]
    public abstract string ToDbLiteral(T value);

    /// <inheritdoc />
    [Pure]
    public abstract object ToParameterValue(T value);

    /// <inheritdoc />
    [Pure]
    public sealed override string? TryToDbLiteral(object value)
    {
        return value is T t ? ToDbLiteral( t ) : null;
    }

    /// <inheritdoc />
    [Pure]
    public sealed override object? TryToParameterValue(object value)
    {
        return value is T t ? ToParameterValue( t ) : null;
    }
}

/// <summary>
/// Represents a generic definition of a column type.
/// </summary>
/// <typeparam name="T">Underlying .NET type.</typeparam>
/// <typeparam name="TDataRecord">DB data record type.</typeparam>
/// <typeparam name="TParameter">DB parameter type.</typeparam>
public abstract class SqlColumnTypeDefinition<T, TDataRecord, TParameter> : SqlColumnTypeDefinition<T>
    where T : notnull
    where TDataRecord : IDataRecord
    where TParameter : IDbDataParameter
{
    /// <summary>
    /// Creates a new <see cref="SqlColumnTypeDefinition{T,TDataRecord,TParameter}"/> instance.
    /// </summary>
    /// <param name="dataType">Underlying DB data type.</param>
    /// <param name="defaultValue">Specifies the default value for this type.</param>
    /// <param name="outputMapping">
    /// Specifies the mapping of values read by <see cref="IDataReader"/> to objects
    /// of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </param>
    protected SqlColumnTypeDefinition(ISqlDataType dataType, T defaultValue, Expression<Func<TDataRecord, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    /// <inheritdoc cref="ISqlColumnTypeDefinition.OutputMapping" />
    public new Expression<Func<TDataRecord, int, T>> OutputMapping =>
        ReinterpretCast.To<Expression<Func<TDataRecord, int, T>>>( base.OutputMapping );

    /// <inheritdoc cref="ISqlColumnTypeDefinition.SetParameterInfo(IDbDataParameter,Boolean)" />
    public virtual void SetParameterInfo(TParameter parameter, bool isNullable)
    {
        parameter.DbType = DataType.DbType;
    }

    /// <inheritdoc />
    [EditorBrowsable( EditorBrowsableState.Never )]
    public sealed override void SetParameterInfo(IDbDataParameter parameter, bool isNullable)
    {
        if ( parameter is TParameter p )
            SetParameterInfo( p, isNullable );
        else
            parameter.DbType = DataType.DbType;
    }
}
