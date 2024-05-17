using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc cref="SqlColumnTypeDefinition{T}" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public abstract class PostgreSqlColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, NpgsqlDataReader, NpgsqlParameter>
    where T : notnull
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlColumnTypeDefinition{T}"/> instance.
    /// </summary>
    /// <param name="dataType">Underlying DB data type.</param>
    /// <param name="defaultValue">Specifies the default value for this type.</param>
    /// <param name="outputMapping">
    /// Specifies the mapping of values read by <see cref="NpgsqlDataReader"/> to objects
    /// of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </param>
    protected PostgreSqlColumnTypeDefinition(
        PostgreSqlDataType dataType,
        T defaultValue,
        Expression<Func<NpgsqlDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new PostgreSqlDataType DataType => ReinterpretCast.To<PostgreSqlDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.NpgsqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
