using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Internal.TypeDefinitions;

internal sealed class MySqlColumnTypeDefinitionObject : MySqlColumnTypeDefinition<object>
{
    private readonly MySqlColumnTypeDefinitionProvider _provider;

    internal MySqlColumnTypeDefinitionObject(MySqlColumnTypeDefinitionProvider provider, MySqlColumnTypeDefinitionByteArray @base)
        : base( @base.DataType, @base.DefaultValue.GetValue(), static (reader, ordinal) => reader.GetValue( ordinal ) )
    {
        _provider = provider;
    }

    [Pure]
    public override string ToDbLiteral(object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        var result = definition is not null && definition.RuntimeType != typeof( object ) ? definition.TryToDbLiteral( value ) : null;
        if ( result is not null )
            return result;

        throw new ArgumentException( ExceptionResources.ValueCannotBeConvertedToDbLiteral( typeof( object ) ), nameof( value ) );
    }

    [Pure]
    public override object ToParameterValue(object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        return definition is not null && definition.RuntimeType != typeof( object )
            ? definition.TryToParameterValue( value ) ?? value
            : value;
    }

    public override void SetParameterInfo(MySqlParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.ResetDbType();
        if ( isNullable )
            parameter.DbType = DbType.Object;
    }
}
