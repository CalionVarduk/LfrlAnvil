using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <inheritdoc cref="SqlColumnTypeDefinition{T}" />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public abstract class MySqlColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, MySqlDataReader, MySqlParameter>
    where T : notnull
{
    /// <summary>
    /// Creates a new <see cref="MySqlColumnTypeDefinition{T}"/> instance.
    /// </summary>
    /// <param name="dataType">Underlying DB data type.</param>
    /// <param name="defaultValue">Specifies the default value for this type.</param>
    /// <param name="outputMapping">
    /// Specifies the mapping of values read by <see cref="MySqlDataReader"/> to objects
    /// of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </param>
    protected MySqlColumnTypeDefinition(MySqlDataType dataType, T defaultValue, Expression<Func<MySqlDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new MySqlDataType DataType => ReinterpretCast.To<MySqlDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        parameter.MySqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
