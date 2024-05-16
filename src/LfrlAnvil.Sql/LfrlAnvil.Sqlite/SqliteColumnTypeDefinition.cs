using System;
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc cref="SqlColumnTypeDefinition{T}" />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public abstract class SqliteColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, SqliteDataReader, SqliteParameter>
    where T : notnull
{
    /// <summary>
    /// Creates a new <see cref="SqliteColumnTypeDefinition{T}"/> instance.
    /// </summary>
    /// <param name="dataType">Underlying DB data type.</param>
    /// <param name="defaultValue">Specifies the default value for this type.</param>
    /// <param name="outputMapping">
    /// Specifies the mapping of values read by <see cref="SqliteDataReader"/> to objects
    /// of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </param>
    protected SqliteColumnTypeDefinition(SqliteDataType dataType, T defaultValue, Expression<Func<SqliteDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new SqliteDataType DataType => ReinterpretCast.To<SqliteDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.SqliteType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
