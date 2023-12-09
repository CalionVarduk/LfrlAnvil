using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Internal;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeEnumDefinition<TEnum, T> : SqliteColumnTypeDefinition<TEnum>
    where TEnum : struct, Enum
    where T : unmanaged
{
    private readonly SqliteColumnTypeDefinition<T> _base;

    internal SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<T> @base)
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
    private static Expression<Func<SqliteDataReader, int, TEnum>> CreateOutputExpression(
        Expression<Func<SqliteDataReader, int, T>> baseOutputMapping)
    {
        var body = Expression.Convert( baseOutputMapping.Body, typeof( TEnum ) );
        return Expression.Lambda<Func<SqliteDataReader, int, TEnum>>( body, baseOutputMapping.Parameters );
    }
}
