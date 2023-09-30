using System;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

internal sealed class SqliteColumnTypeDefinitionObject : SqliteColumnTypeDefinition<object>
{
    private readonly SqliteColumnTypeDefinitionProvider _provider;

    internal SqliteColumnTypeDefinitionObject(SqliteColumnTypeDefinitionProvider provider)
        : base( SqliteDataType.Any, provider.GetDefaultForDataType( SqliteDataType.Blob ).DefaultValue.GetValue() )
    {
        _provider = provider;
    }

    [Pure]
    public override string ToDbLiteral(object value)
    {
        return TryToDbLiteral( value ) ??
            throw new ArgumentException(
                ExceptionResources.ValueCannotBeConvertedToDbLiteral( typeof( object ) ),
                nameof( value ) );
    }

    [Pure]
    public override string? TryToDbLiteral(object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        return definition is not null && definition.RuntimeType != typeof( object ) ? definition.TryToDbLiteral( value ) : null;
    }

    public override void SetParameter(IDbDataParameter parameter, object value)
    {
        if ( ! TrySetParameter( parameter, value ) )
            throw new ArgumentException( ExceptionResources.ValueCannotBeUsedInParameter( typeof( object ) ), nameof( value ) );
    }

    public override bool TrySetParameter(IDbDataParameter parameter, object value)
    {
        var definition = _provider.TryGetByType( value.GetType() );
        return definition is not null && definition.RuntimeType != typeof( object ) && definition.TrySetParameter( parameter, value );
    }
}
