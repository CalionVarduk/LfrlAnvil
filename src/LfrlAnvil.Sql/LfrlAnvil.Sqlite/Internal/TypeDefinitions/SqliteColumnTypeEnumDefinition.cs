using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Internal;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeEnumDefinition<TEnum, T> : SqliteColumnTypeDefinition<TEnum, T>
    where TEnum : struct, Enum
    where T : unmanaged
{
    internal SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<T> @base)
        : base( @base, FindDefaultValue(), CreateOutputExpression( @base.OutputMapping ) ) { }

    [Pure]
    protected override T MapToBaseType(TEnum value)
    {
        return (T)(object)value;
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
