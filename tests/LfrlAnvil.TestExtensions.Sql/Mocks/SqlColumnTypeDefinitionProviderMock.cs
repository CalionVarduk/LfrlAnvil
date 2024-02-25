using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnTypeDefinitionProviderMock : SqlColumnTypeDefinitionProvider
{
    private readonly Dictionary<SqlDataTypeMock, SqlColumnTypeDefinition> _definitionsByDataType;

    public SqlColumnTypeDefinitionProviderMock(SqlColumnTypeDefinitionProviderBuilderMock builder)
        : base( builder )
    {
        _definitionsByDataType = new Dictionary<SqlDataTypeMock, SqlColumnTypeDefinition>();
        foreach ( var definition in builder.DefaultDefinitions )
            _definitionsByDataType.Add( (SqlDataTypeMock)definition.DataType, definition );
    }

    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _definitionsByDataType.Values;
    }

    [Pure]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return _definitionsByDataType[SqlHelpers.CastOrThrow<SqlDataTypeMock>( Dialect, type )];
    }

    [Pure]
    protected override SqlColumnTypeDefinition<TEnum> CreateEnumTypeDefinition<TEnum, TUnderlying>(
        SqlColumnTypeDefinition<TUnderlying> underlyingTypeDefinition)
    {
        return new SqlColumnTypeEnumDefinitionMock<TEnum, TUnderlying>(
            ReinterpretCast.To<SqlColumnTypeDefinitionMock<TUnderlying>>( underlyingTypeDefinition ) );
    }

    [Pure]
    protected override SqlColumnTypeDefinition? TryCreateUnknownTypeDefinition(Type type)
    {
        return type == typeof( byte )
            ? new SqlColumnTypeDefinitionMock<byte>( SqlDataTypeMock.Integer, 0 )
            : base.TryCreateUnknownTypeDefinition( type );
    }
}
