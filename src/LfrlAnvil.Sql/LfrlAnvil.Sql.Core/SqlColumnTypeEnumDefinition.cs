using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a generic definition of an <see cref="Enum"/> column type.
/// </summary>
/// <typeparam name="TEnum">Underlying .NET <see cref="Enum"/> type.</typeparam>
/// <typeparam name="TUnderlying">.NET type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
/// <typeparam name="TDataRecord">DB data record type.</typeparam>
/// <typeparam name="TParameter">DB parameter type.</typeparam>
public abstract class SqlColumnTypeEnumDefinition<TEnum, TUnderlying, TDataRecord, TParameter>
    : SqlColumnTypeDefinition<TEnum, TDataRecord, TParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
    where TDataRecord : IDataRecord
    where TParameter : IDbDataParameter
{
    private readonly SqlColumnTypeDefinition<TUnderlying, TDataRecord, TParameter> _base;

    /// <summary>
    /// Creates a new <see cref="SqlColumnTypeEnumDefinition{TEnum,TUnderlying,TDataRecord,TParameter}"/> instance.
    /// </summary>
    /// <param name="base">Column type definition associated with the underlying type.</param>
    protected SqlColumnTypeEnumDefinition(SqlColumnTypeDefinition<TUnderlying, TDataRecord, TParameter> @base)
        : base( @base.DataType, FindDefaultValue(), CreateOutputExpression( @base.OutputMapping ) )
    {
        _base = @base;
    }

    /// <inheritdoc />
    [Pure]
    public override string ToDbLiteral(TEnum value)
    {
        return _base.ToDbLiteral( ( TUnderlying )( object )value );
    }

    /// <inheritdoc />
    [Pure]
    public override object ToParameterValue(TEnum value)
    {
        return _base.ToParameterValue( ( TUnderlying )( object )value );
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
