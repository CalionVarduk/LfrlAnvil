using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Internal.TypeDefinitions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.MySql;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public class MySqlColumnTypeDefinitionProviderBuilder : SqlColumnTypeDefinitionProviderBuilder
{
    internal readonly MySqlColumnTypeDefinitionBool DefaultBool;
    internal readonly MySqlColumnTypeDefinitionInt8 DefaultTinyInt;
    internal readonly MySqlColumnTypeDefinitionUInt8 DefaultUnsignedTinyInt;
    internal readonly MySqlColumnTypeDefinitionInt16 DefaultSmallInt;
    internal readonly MySqlColumnTypeDefinitionUInt16 DefaultUnsignedSmallInt;
    internal readonly MySqlColumnTypeDefinitionInt32 DefaultInt;
    internal readonly MySqlColumnTypeDefinitionUInt32 DefaultUnsignedInt;
    internal readonly MySqlColumnTypeDefinitionInt64 DefaultBigInt;
    internal readonly MySqlColumnTypeDefinitionUInt64 DefaultUnsignedBigInt;
    internal readonly MySqlColumnTypeDefinitionFloat DefaultFloat;
    internal readonly MySqlColumnTypeDefinitionDouble DefaultDouble;
    internal readonly MySqlColumnTypeDefinitionDecimal DefaultDecimal;
    internal readonly MySqlColumnTypeDefinitionString DefaultText;
    internal readonly MySqlColumnTypeDefinitionString DefaultChar;
    internal readonly MySqlColumnTypeDefinitionString DefaultVarChar;
    internal readonly MySqlColumnTypeDefinitionByteArray DefaultBlob;
    internal readonly MySqlColumnTypeDefinitionByteArray DefaultBinary;
    internal readonly MySqlColumnTypeDefinitionByteArray DefaultVarBinary;
    internal readonly MySqlColumnTypeDefinitionDateOnly DefaultDate;
    internal readonly MySqlColumnTypeDefinitionTimeOnly DefaultTime;
    internal readonly MySqlColumnTypeDefinitionDateTime DefaultDateTime;

    /// <summary>
    /// Creates a new <see cref="MySqlColumnTypeDefinitionProviderBuilder"/> instance.
    /// </summary>
    public MySqlColumnTypeDefinitionProviderBuilder()
        : base( MySqlDialect.Instance )
    {
        DefaultBool = new MySqlColumnTypeDefinitionBool();
        DefaultTinyInt = new MySqlColumnTypeDefinitionInt8();
        DefaultUnsignedTinyInt = new MySqlColumnTypeDefinitionUInt8();
        DefaultSmallInt = new MySqlColumnTypeDefinitionInt16();
        DefaultUnsignedSmallInt = new MySqlColumnTypeDefinitionUInt16();
        DefaultInt = new MySqlColumnTypeDefinitionInt32();
        DefaultUnsignedInt = new MySqlColumnTypeDefinitionUInt32();
        DefaultBigInt = new MySqlColumnTypeDefinitionInt64();
        DefaultUnsignedBigInt = new MySqlColumnTypeDefinitionUInt64();
        DefaultFloat = new MySqlColumnTypeDefinitionFloat();
        DefaultDouble = new MySqlColumnTypeDefinitionDouble();
        DefaultDecimal = new MySqlColumnTypeDefinitionDecimal();
        DefaultText = new MySqlColumnTypeDefinitionString();
        DefaultChar = new MySqlColumnTypeDefinitionString( DefaultText, MySqlDataType.Char );
        DefaultVarChar = new MySqlColumnTypeDefinitionString( DefaultText, MySqlDataType.VarChar );
        DefaultBlob = new MySqlColumnTypeDefinitionByteArray();
        DefaultBinary = new MySqlColumnTypeDefinitionByteArray( DefaultBlob, MySqlDataType.Binary );
        DefaultVarBinary = new MySqlColumnTypeDefinitionByteArray( DefaultBlob, MySqlDataType.VarBinary );
        DefaultDate = new MySqlColumnTypeDefinitionDateOnly();
        DefaultTime = new MySqlColumnTypeDefinitionTimeOnly();
        DefaultDateTime = new MySqlColumnTypeDefinitionDateTime();

        AddOrUpdate( DefaultBool );
        AddOrUpdate( DefaultTinyInt );
        AddOrUpdate( DefaultUnsignedTinyInt );
        AddOrUpdate( DefaultSmallInt );
        AddOrUpdate( DefaultUnsignedSmallInt );
        AddOrUpdate( DefaultInt );
        AddOrUpdate( DefaultUnsignedInt );
        AddOrUpdate( DefaultBigInt );
        AddOrUpdate( DefaultUnsignedBigInt );
        AddOrUpdate( DefaultFloat );
        AddOrUpdate( DefaultDouble );
        AddOrUpdate( DefaultDecimal );
        AddOrUpdate( DefaultText );
        AddOrUpdate( DefaultBlob );
        AddOrUpdate( DefaultDate );
        AddOrUpdate( DefaultTime );
        AddOrUpdate( DefaultDateTime );

        AddOrUpdate( new MySqlColumnTypeDefinitionTimeSpan() );
        AddOrUpdate( new MySqlColumnTypeDefinitionDateTimeOffset() );
        AddOrUpdate( new MySqlColumnTypeDefinitionChar() );
        AddOrUpdate( new MySqlColumnTypeDefinitionGuid() );
    }

    /// <inheritdoc cref="SqlColumnTypeDefinitionProviderBuilder.Register(SqlColumnTypeDefinition)" />
    public new MySqlColumnTypeDefinitionProviderBuilder Register(SqlColumnTypeDefinition definition)
    {
        base.Register( definition );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public sealed override MySqlColumnTypeDefinitionProvider Build()
    {
        return new MySqlColumnTypeDefinitionProvider( this );
    }

    [Pure]
    internal Dictionary<string, SqlColumnTypeDefinition> CreateDataTypeDefinitionsByName()
    {
        return new Dictionary<string, SqlColumnTypeDefinition>( capacity: 21, comparer: SqlHelpers.NameComparer )
        {
            { DefaultBool.DataType.Name, DefaultBool },
            { DefaultTinyInt.DataType.Name, DefaultTinyInt },
            { DefaultUnsignedTinyInt.DataType.Name, DefaultUnsignedTinyInt },
            { DefaultSmallInt.DataType.Name, DefaultSmallInt },
            { DefaultUnsignedSmallInt.DataType.Name, DefaultUnsignedSmallInt },
            { DefaultInt.DataType.Name, DefaultInt },
            { DefaultUnsignedInt.DataType.Name, DefaultUnsignedInt },
            { DefaultBigInt.DataType.Name, DefaultBigInt },
            { DefaultUnsignedBigInt.DataType.Name, DefaultUnsignedBigInt },
            { DefaultFloat.DataType.Name, DefaultFloat },
            { DefaultDouble.DataType.Name, DefaultDouble },
            { DefaultDecimal.DataType.Name, DefaultDecimal },
            { DefaultText.DataType.Name, DefaultText },
            { DefaultChar.DataType.Name, DefaultChar },
            { DefaultVarChar.DataType.Name, DefaultVarChar },
            { DefaultBlob.DataType.Name, DefaultBlob },
            { DefaultBinary.DataType.Name, DefaultBinary },
            { DefaultVarBinary.DataType.Name, DefaultVarBinary },
            { DefaultDate.DataType.Name, DefaultDate },
            { DefaultTime.DataType.Name, DefaultTime },
            { DefaultDateTime.DataType.Name, DefaultDateTime }
        };
    }
}
