using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.PostgreSql;

public class PostgreSqlColumnTypeDefinitionProviderBuilder : SqlColumnTypeDefinitionProviderBuilder
{
    internal readonly PostgreSqlColumnTypeDefinitionBool DefaultBoolean;
    internal readonly PostgreSqlColumnTypeDefinitionInt16 DefaultInt2;
    internal readonly PostgreSqlColumnTypeDefinitionInt32 DefaultInt4;
    internal readonly PostgreSqlColumnTypeDefinitionInt64 DefaultInt8;
    internal readonly PostgreSqlColumnTypeDefinitionFloat DefaultFloat4;
    internal readonly PostgreSqlColumnTypeDefinitionDouble DefaultFloat8;
    internal readonly PostgreSqlColumnTypeDefinitionDecimal DefaultDecimal;
    internal readonly PostgreSqlColumnTypeDefinitionString DefaultVarChar;
    internal readonly PostgreSqlColumnTypeDefinitionByteArray DefaultBytea;
    internal readonly PostgreSqlColumnTypeDefinitionGuid DefaultUuid;
    internal readonly PostgreSqlColumnTypeDefinitionDateOnly DefaultDate;
    internal readonly PostgreSqlColumnTypeDefinitionTimeOnly DefaultTime;
    internal readonly PostgreSqlColumnTypeDefinitionDateTime DefaultTimestamp;
    internal readonly PostgreSqlColumnTypeDefinitionDateTime DefaultTimestampTz;

    public PostgreSqlColumnTypeDefinitionProviderBuilder()
        : base( PostgreSqlDialect.Instance )
    {
        DefaultBoolean = new PostgreSqlColumnTypeDefinitionBool();
        DefaultInt2 = new PostgreSqlColumnTypeDefinitionInt16();
        DefaultInt4 = new PostgreSqlColumnTypeDefinitionInt32();
        DefaultInt8 = new PostgreSqlColumnTypeDefinitionInt64();
        DefaultFloat4 = new PostgreSqlColumnTypeDefinitionFloat();
        DefaultFloat8 = new PostgreSqlColumnTypeDefinitionDouble();
        DefaultDecimal = new PostgreSqlColumnTypeDefinitionDecimal();
        DefaultVarChar = new PostgreSqlColumnTypeDefinitionString();
        DefaultBytea = new PostgreSqlColumnTypeDefinitionByteArray();
        DefaultUuid = new PostgreSqlColumnTypeDefinitionGuid();
        DefaultDate = new PostgreSqlColumnTypeDefinitionDateOnly();
        DefaultTime = new PostgreSqlColumnTypeDefinitionTimeOnly();
        DefaultTimestamp = new PostgreSqlColumnTypeDefinitionDateTime( PostgreSqlDataType.Timestamp );
        DefaultTimestampTz = new PostgreSqlColumnTypeDefinitionDateTime( PostgreSqlDataType.TimestampTz );

        AddOrUpdate( DefaultBoolean );
        AddOrUpdate( DefaultInt2 );
        AddOrUpdate( DefaultInt4 );
        AddOrUpdate( DefaultInt8 );
        AddOrUpdate( DefaultFloat4 );
        AddOrUpdate( DefaultFloat8 );
        AddOrUpdate( DefaultDecimal );
        AddOrUpdate( DefaultVarChar );
        AddOrUpdate( DefaultBytea );
        AddOrUpdate( DefaultUuid );
        AddOrUpdate( DefaultDate );
        AddOrUpdate( DefaultTime );
        AddOrUpdate( DefaultTimestamp );

        AddOrUpdate( new PostgreSqlColumnTypeDefinitionChar() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionDateTimeOffset() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionInt8() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionTimeSpan() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionUInt8() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionUInt16() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionUInt32() );
        AddOrUpdate( new PostgreSqlColumnTypeDefinitionUInt64() );
    }

    public new PostgreSqlColumnTypeDefinitionProviderBuilder Register(SqlColumnTypeDefinition definition)
    {
        base.Register( definition );
        return this;
    }

    [Pure]
    public sealed override PostgreSqlColumnTypeDefinitionProvider Build()
    {
        return new PostgreSqlColumnTypeDefinitionProvider( this );
    }

    [Pure]
    internal Dictionary<string, SqlColumnTypeDefinition> CreateDataTypeDefinitionsByName()
    {
        return new Dictionary<string, SqlColumnTypeDefinition>( capacity: 14, comparer: SqlHelpers.NameComparer )
        {
            { DefaultBoolean.DataType.Name, DefaultBoolean },
            { DefaultInt2.DataType.Name, DefaultInt2 },
            { DefaultInt4.DataType.Name, DefaultInt4 },
            { DefaultInt8.DataType.Name, DefaultInt8 },
            { DefaultFloat4.DataType.Name, DefaultFloat4 },
            { DefaultFloat8.DataType.Name, DefaultFloat8 },
            { DefaultDecimal.DataType.Name, DefaultDecimal },
            { DefaultVarChar.DataType.Name, DefaultVarChar },
            { DefaultBytea.DataType.Name, DefaultBytea },
            { DefaultUuid.DataType.Name, DefaultUuid },
            { DefaultDate.DataType.Name, DefaultDate },
            { DefaultTime.DataType.Name, DefaultTime },
            { DefaultTimestamp.DataType.Name, DefaultTimestamp },
            { DefaultTimestampTz.DataType.Name, DefaultTimestampTz }
        };
    }
}
