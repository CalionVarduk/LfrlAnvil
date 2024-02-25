using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlColumnTypeDefinitionProviderBuilderMock : SqlColumnTypeDefinitionProviderBuilder
{
    internal readonly SqlColumnTypeDefinition[] DefaultDefinitions;

    public SqlColumnTypeDefinitionProviderBuilderMock()
        : base( SqlDialectMock.Instance )
    {
        DefaultDefinitions = new SqlColumnTypeDefinition[]
        {
            new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 ),
            new SqlColumnTypeDefinitionMock<bool>( SqlDataTypeMock.Boolean, false ),
            new SqlColumnTypeDefinitionMock<double>( SqlDataTypeMock.Real, 0.0 ),
            new SqlColumnTypeDefinitionMock<string>( SqlDataTypeMock.Text, string.Empty ),
            new SqlColumnTypeDefinitionMock<byte[]>( SqlDataTypeMock.Binary, Array.Empty<byte>() ),
            new SqlColumnTypeDefinitionMock<object>( SqlDataTypeMock.Object, Array.Empty<byte>() )
        };

        foreach ( var definition in DefaultDefinitions )
            AddOrUpdate( definition );

        AddOrUpdate( new SqlColumnTypeDefinitionMock<long>( SqlDataTypeMock.Integer, 0L ) );
        AddOrUpdate( new SqlColumnTypeDefinitionMock<float>( SqlDataTypeMock.Real, 0.0f ) );
        AddOrUpdate( new SqlColumnTypeDefinitionMock<DateTime>( SqlDataTypeMock.Text, DateTime.UnixEpoch ) );
    }

    [Pure]
    public override SqlColumnTypeDefinitionProviderMock Build()
    {
        return new SqlColumnTypeDefinitionProviderMock( this );
    }
}
