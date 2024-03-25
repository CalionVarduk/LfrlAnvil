using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Internal.TypeDefinitions;

internal sealed class PostgreSqlColumnTypeDefinitionObject : PostgreSqlColumnTypeDefinition<object>
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _provider;

    internal PostgreSqlColumnTypeDefinitionObject(
        PostgreSqlColumnTypeDefinitionProvider provider,
        PostgreSqlColumnTypeDefinitionByteArray @base)
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

    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.ResetDbType();
        if ( isNullable )
            parameter.DbType = DbType.Object;
    }
}
