using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;

namespace LfrlAnvil.Sqlite;

public class SqliteColumnTypeDefinitionProviderBuilder : SqlColumnTypeDefinitionProviderBuilder
{
    internal readonly SqliteColumnTypeDefinitionInt64 DefaultInteger;
    internal readonly SqliteColumnTypeDefinitionDouble DefaultReal;
    internal readonly SqliteColumnTypeDefinitionString DefaultText;
    internal readonly SqliteColumnTypeDefinitionByteArray DefaultBlob;

    public SqliteColumnTypeDefinitionProviderBuilder()
        : base( SqliteDialect.Instance )
    {
        DefaultInteger = new SqliteColumnTypeDefinitionInt64();
        DefaultReal = new SqliteColumnTypeDefinitionDouble();
        DefaultText = new SqliteColumnTypeDefinitionString();
        DefaultBlob = new SqliteColumnTypeDefinitionByteArray();

        AddOrUpdate( DefaultInteger );
        AddOrUpdate( DefaultReal );
        AddOrUpdate( DefaultText );
        AddOrUpdate( DefaultBlob );

        AddOrUpdate( new SqliteColumnTypeDefinitionBool() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt8() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt8() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt16() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt16() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt32() );
        AddOrUpdate( new SqliteColumnTypeDefinitionInt32() );
        AddOrUpdate( new SqliteColumnTypeDefinitionUInt64() );
        AddOrUpdate( new SqliteColumnTypeDefinitionTimeSpan() );
        AddOrUpdate( new SqliteColumnTypeDefinitionFloat() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateTime() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateTimeOffset() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDateOnly() );
        AddOrUpdate( new SqliteColumnTypeDefinitionTimeOnly() );
        AddOrUpdate( new SqliteColumnTypeDefinitionDecimal() );
        AddOrUpdate( new SqliteColumnTypeDefinitionChar() );
        AddOrUpdate( new SqliteColumnTypeDefinitionGuid() );
    }

    [Pure]
    public sealed override SqliteColumnTypeDefinitionProvider Build()
    {
        return new SqliteColumnTypeDefinitionProvider( this );
    }
}
