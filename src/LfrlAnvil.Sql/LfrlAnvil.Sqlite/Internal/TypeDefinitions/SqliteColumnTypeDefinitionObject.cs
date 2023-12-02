using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionObject : SqliteColumnTypeDefinition<object>
{
    private readonly SqliteColumnTypeDefinitionProvider _provider;

    internal SqliteColumnTypeDefinitionObject(SqliteColumnTypeDefinitionProvider provider)
        : base(
            SqliteDataType.Any,
            provider.GetDefaultForDataType( SqliteDataType.Blob ).DefaultValue.GetValue(),
            static (reader, ordinal) => reader.GetValue( ordinal ) )
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

    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        parameter.IsNullable = isNullable;
        parameter.ResetSqliteType();
        parameter.DbType = DataType.DbType;
    }
}
