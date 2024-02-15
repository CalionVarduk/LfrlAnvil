using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql;

public abstract class SqlColumnTypeEnumDefinition<TEnum, TUnderlying, TDataRecord, TParameter>
    : SqlColumnTypeDefinition<TEnum, TDataRecord, TParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
    where TDataRecord : IDataRecord
    where TParameter : IDbDataParameter
{
    private readonly SqlColumnTypeDefinition<TUnderlying, TDataRecord, TParameter> _base;

    protected SqlColumnTypeEnumDefinition(SqlColumnTypeDefinition<TUnderlying, TDataRecord, TParameter> @base)
        : base( @base.DataType, FindDefaultValue(), CreateOutputExpression( @base.OutputMapping ) )
    {
        _base = @base;
    }

    [Pure]
    public override string ToDbLiteral(TEnum value)
    {
        return _base.ToDbLiteral( (TUnderlying)(object)value );
    }

    [Pure]
    public override object ToParameterValue(TEnum value)
    {
        return _base.ToParameterValue( (TUnderlying)(object)value );
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
    private static Expression<Func<TDataRecord, int, TEnum>> CreateOutputExpression(
        Expression<Func<TDataRecord, int, TUnderlying>> baseOutputMapping)
    {
        var body = Expression.Convert( baseOutputMapping.Body, typeof( TEnum ) );
        return Expression.Lambda<Func<TDataRecord, int, TEnum>>( body, baseOutputMapping.Parameters );
    }
}
