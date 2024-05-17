using System;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

/// <summary>
/// Represents a generic definition of an <see cref="Enum"/> column type for <see cref="PostgreSqlDialect"/>.
/// </summary>
/// <typeparam name="TEnum">Underlying .NET <see cref="Enum"/> type.</typeparam>
/// <typeparam name="TUnderlying">.NET type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
public sealed class PostgreSqlColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, NpgsqlDataReader, NpgsqlParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlColumnTypeEnumDefinition{TEnum,TUnderlying}"/> instance.
    /// </summary>
    /// <param name="base">Column type definition associated with the underlying type.</param>
    public PostgreSqlColumnTypeEnumDefinition(PostgreSqlColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new PostgreSqlDataType DataType => ReinterpretCast.To<PostgreSqlDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.NpgsqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
