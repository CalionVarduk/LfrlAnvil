using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class ColumnTypeDefinitionProviderMock
{
    [Pure]
    public static ISqlColumnTypeDefinitionProvider Create(params ISqlColumnTypeDefinition[] definitions)
    {
        var result = Substitute.For<ISqlColumnTypeDefinitionProvider>();
        result.GetTypeDefinitions().Returns( definitions );
        result.GetByType( Arg.Any<Type>() ).Returns( i => definitions.Single( d => d.RuntimeType == i.ArgAt<Type>( 0 ) ) );
        result.GetByDataType( Arg.Any<ISqlDataType>() )
            .Returns( i => definitions.Single( d => ReferenceEquals( d.DataType, i.ArgAt<ISqlDataType>( 0 ) ) ) );

        return result;
    }

    [Pure]
    public static ISqlColumnTypeDefinitionProvider Default(SqlDialect dialect)
    {
        var intDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "INT", DbType.Int32 ),
            (r, o) => r.GetInt32( o ) );

        var doubleDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "DOUBLE", DbType.Double ),
            (r, o) => r.GetDouble( o ) );

        var boolDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "BOOL", DbType.Boolean ),
            (r, o) => r.GetBoolean( o ) );

        var stringDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "STRING", DbType.String ),
            (r, o) => r.GetString( o ),
            string.Empty );

        var binaryDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "BINARY", DbType.Binary ),
            (r, o) => r.GetBytes( o ),
            Array.Empty<byte>() );

        var objDef = ColumnTypeDefinitionMock.Create(
            DataTypeMock.Create( dialect, "OBJECT" ),
            (r, o) => r.GetValue( o ),
            Array.Empty<byte>() );

        return Create( intDef, doubleDef, boolDef, stringDef, binaryDef, objDef );
    }
}
