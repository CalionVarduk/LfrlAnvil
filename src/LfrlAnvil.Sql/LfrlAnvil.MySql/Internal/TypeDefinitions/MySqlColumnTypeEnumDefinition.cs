using System;
using LfrlAnvil.Sql;
using MySqlConnector;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

/// <summary>
/// Represents a generic definition of an <see cref="Enum"/> column type for <see cref="MySqlDialect"/>.
/// </summary>
/// <typeparam name="TEnum">Underlying .NET <see cref="Enum"/> type.</typeparam>
/// <typeparam name="TUnderlying">.NET type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
public sealed class MySqlColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, MySqlDataReader, MySqlParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    /// <summary>
    /// Creates a new <see cref="MySqlColumnTypeEnumDefinition{TEnum,TUnderlying}"/> instance.
    /// </summary>
    /// <param name="base">Column type definition associated with the underlying type.</param>
    public MySqlColumnTypeEnumDefinition(MySqlColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new MySqlDataType DataType => ReinterpretCast.To<MySqlDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        parameter.MySqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
