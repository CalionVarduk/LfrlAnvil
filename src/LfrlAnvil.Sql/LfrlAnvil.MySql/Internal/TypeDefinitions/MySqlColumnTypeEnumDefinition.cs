using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Internal;
using MySqlConnector;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeEnumDefinition<TEnum, T> : MySqlColumnTypeDefinition<TEnum>
    where TEnum : struct, Enum
    where T : unmanaged
{
    private readonly MySqlColumnTypeDefinition<T> _base;

    internal MySqlColumnTypeEnumDefinition(MySqlColumnTypeDefinition<T> @base)
        : base( @base.DataType, FindDefaultValue(), CreateOutputExpression( @base.OutputMapping ) )
    {
        _base = @base;
    }

    [Pure]
    public override string ToDbLiteral(TEnum value)
    {
        return _base.ToDbLiteral( (T)(object)value );
    }

    [Pure]
    public override object ToParameterValue(TEnum value)
    {
        return _base.ToParameterValue( (T)(object)value );
    }

    [Pure]
    private static TEnum FindDefaultValue()
    {
        var values = Enum.GetValues<TEnum>();
        foreach ( var value in values )
        {
            if ( Generic<TEnum>.IsDefault( value ) )
                return value;
        }

        return values.Length > 0 ? values[0] : default;
    }

    [Pure]
    private static Expression<Func<MySqlDataReader, int, TEnum>> CreateOutputExpression(
        Expression<Func<MySqlDataReader, int, T>> baseOutputMapping)
    {
        var body = Expression.Convert( baseOutputMapping.Body, typeof( TEnum ) );
        return Expression.Lambda<Func<MySqlDataReader, int, TEnum>>( body, baseOutputMapping.Parameters );
    }
}
