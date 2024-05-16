using System;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

/// <summary>
/// Represents a generic definition of an <see cref="Enum"/> column type for <see cref="SqliteDialect"/>.
/// </summary>
/// <typeparam name="TEnum">Underlying .NET <see cref="Enum"/> type.</typeparam>
/// <typeparam name="TUnderlying">.NET type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
public sealed class SqliteColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, SqliteDataReader, SqliteParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    /// <summary>
    /// Creates a new <see cref="SqliteColumnTypeEnumDefinition{TEnum,TUnderlying}"/> instance.
    /// </summary>
    /// <param name="base">Column type definition associated with the underlying type.</param>
    public SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

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
