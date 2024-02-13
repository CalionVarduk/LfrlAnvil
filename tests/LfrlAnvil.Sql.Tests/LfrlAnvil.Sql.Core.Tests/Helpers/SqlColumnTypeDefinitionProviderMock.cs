using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class SqlColumnTypeDefinitionProviderMock : SqlColumnTypeDefinitionProvider
{
    private readonly Dictionary<SqlDataTypeMock, SqlColumnTypeDefinition> _definitionsByDataType;

    public SqlColumnTypeDefinitionProviderMock()
    {
        var definitions = new SqlColumnTypeDefinition[]
        {
            new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 ),
            new SqlColumnTypeDefinitionMock<bool>( SqlDataTypeMock.Boolean, false ),
            new SqlColumnTypeDefinitionMock<double>( SqlDataTypeMock.Real, 0.0 ),
            new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty ),
            new SqlColumnTypeDefinitionMock<byte[]>( SqlDataTypeMock.Binary, Array.Empty<byte>() ),
            new SqlColumnTypeDefinitionMock<object>( SqlDataTypeMock.Object, Array.Empty<byte>() )
        };

        _definitionsByDataType = new Dictionary<SqlDataTypeMock, SqlColumnTypeDefinition>();

        foreach ( var d in definitions )
        {
            _definitionsByDataType.Add( ReinterpretCast.To<SqlDataTypeMock>( d.DataType ), d );
            TryAddDefinition( d );
        }

        TryAddDefinition( new SqlColumnTypeDefinitionMock<long>( SqlDataTypeMock.Integer, 0L ) );
        TryAddDefinition( new SqlColumnTypeDefinitionMock<float>( SqlDataTypeMock.Real, 0.0f ) );
    }

    [Pure]
    public override IReadOnlyCollection<SqlColumnTypeDefinition> GetDataTypeDefinitions()
    {
        return _definitionsByDataType.Values;
    }

    [Pure]
    public override SqlColumnTypeDefinition GetByDataType(ISqlDataType type)
    {
        return _definitionsByDataType[SqlHelpers.CastOrThrow<SqlDataTypeMock>( SqlDialectMock.Instance, type )];
    }
}
