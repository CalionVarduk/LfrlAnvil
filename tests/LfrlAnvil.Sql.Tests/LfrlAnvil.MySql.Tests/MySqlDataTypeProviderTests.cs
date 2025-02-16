using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new MySqlDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnBool()
    {
        var result = _sut.GetBool();
        result.TestRefEquals( MySqlDataType.Bool ).Go();
    }

    [Fact]
    public void GetInt8_ShouldReturnTinyInt()
    {
        var result = _sut.GetInt8();
        result.TestRefEquals( MySqlDataType.TinyInt ).Go();
    }

    [Fact]
    public void GetInt16_ShouldReturnSmallInt()
    {
        var result = _sut.GetInt16();
        result.TestRefEquals( MySqlDataType.SmallInt ).Go();
    }

    [Fact]
    public void GetInt32_ShouldReturnInt()
    {
        var result = _sut.GetInt32();
        result.TestRefEquals( MySqlDataType.Int ).Go();
    }

    [Fact]
    public void GetInt64_ShouldReturnBigInt()
    {
        var result = _sut.GetInt64();
        result.TestRefEquals( MySqlDataType.BigInt ).Go();
    }

    [Fact]
    public void GetUInt8_ShouldReturnUnsignedTinyInt()
    {
        var result = _sut.GetUInt8();
        result.TestRefEquals( MySqlDataType.UnsignedTinyInt ).Go();
    }

    [Fact]
    public void GetUInt16_ShouldReturnUnsignedSmallInt()
    {
        var result = _sut.GetUInt16();
        result.TestRefEquals( MySqlDataType.UnsignedSmallInt ).Go();
    }

    [Fact]
    public void GetUInt32_ShouldReturnUnsignedInt()
    {
        var result = _sut.GetUInt32();
        result.TestRefEquals( MySqlDataType.UnsignedInt ).Go();
    }

    [Fact]
    public void GetUInt64_ShouldReturnUnsignedBigInt()
    {
        var result = _sut.GetUInt64();
        result.TestRefEquals( MySqlDataType.UnsignedBigInt ).Go();
    }

    [Fact]
    public void GetFloat_ShouldReturnFloat()
    {
        var result = _sut.GetFloat();
        result.TestRefEquals( MySqlDataType.Float ).Go();
    }

    [Fact]
    public void GetDouble_ShouldReturnDouble()
    {
        var result = _sut.GetDouble();
        result.TestRefEquals( MySqlDataType.Double ).Go();
    }

    [Fact]
    public void GetDecimal_ShouldReturnDecimal()
    {
        var result = _sut.GetDecimal();
        result.TestRefEquals( MySqlDataType.Decimal ).Go();
    }

    [Theory]
    [InlineData( 10, 0, "DECIMAL(10, 0)" )]
    [InlineData( 29, 10, "DECIMAL(29, 10)" )]
    [InlineData( 29, 11, "DECIMAL(29, 11)" )]
    [InlineData( 28, 10, "DECIMAL(28, 10)" )]
    [InlineData( 65, 30, "DECIMAL(65, 30)" )]
    public void GetDecimal_WithParameters_ShouldReturnCorrectDecimal(int precision, int scale, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetDecimal( precision, scale );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.NewDecimal ),
                sut.DbType.TestEquals( DbType.Decimal ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ precision, scale ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.Decimal.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1, -1 )]
    [InlineData( 66, 0 )]
    [InlineData( 65, 31 )]
    [InlineData( 66, 31 )]
    public void GetDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => _sut.GetDecimal( precision, scale ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetGuid_ShouldReturnBinary16()
    {
        var result = _sut.GetGuid();
        result.TestEquals( MySqlDataType.CreateBinary( 16 ) ).Go();
    }

    [Fact]
    public void GetString_ShouldReturnVarChar()
    {
        var result = _sut.GetString();
        result.TestRefEquals( MySqlDataType.VarChar ).Go();
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 65534, "VARCHAR(65534)" )]
    [InlineData( 65535, "VARCHAR(65535)" )]
    public void GetString_WithMaxLength_ShouldReturnCorrectVarChar_WhenMaxLengthIsInVarCharBounds(int maxLength, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetString( maxLength );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarChar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ maxLength ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarChar.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetString_ShouldReturnText_WhenMaxLengthIsGreaterThanVarCharBoundsAllow()
    {
        var result = _sut.GetString( 65536 );
        result.TestRefEquals( MySqlDataType.Text ).Go();
    }

    [Fact]
    public void GetString_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetString( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetFixedString_ShouldReturnChar()
    {
        var result = _sut.GetFixedString();
        result.TestRefEquals( MySqlDataType.Char ).Go();
    }

    [Theory]
    [InlineData( 0, "CHAR(0)" )]
    [InlineData( 50, "CHAR(50)" )]
    [InlineData( 254, "CHAR(254)" )]
    [InlineData( 255, "CHAR(255)" )]
    public void GetFixedString_WithLength_ShouldReturnCorrectChar_WhenLengthIsInCharBounds(int length, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedString( length );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.String ),
                sut.DbType.TestEquals( DbType.StringFixedLength ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ length ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.Char.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( 256, "VARCHAR(256)" )]
    [InlineData( 65534, "VARCHAR(65534)" )]
    [InlineData( 65535, "VARCHAR(65535)" )]
    public void GetFixedString_WithLength_ShouldReturnCorrectVarChar_WhenLengthIsGreaterThanCharBoundsAllowButIsInVarCharBounds(
        int length,
        string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedString( length );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarChar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ length ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarChar.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetFixedString_ShouldReturnText_WhenLengthIsGreaterThanVarCharBoundsAllow()
    {
        var result = _sut.GetFixedString( 65536 );
        result.TestRefEquals( MySqlDataType.Text ).Go();
    }

    [Fact]
    public void GetFixedString_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedString( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnBigInt()
    {
        var result = _sut.GetTimestamp();
        result.TestRefEquals( MySqlDataType.BigInt ).Go();
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnDateTime()
    {
        var result = _sut.GetUtcDateTime();
        result.TestRefEquals( MySqlDataType.DateTime ).Go();
    }

    [Fact]
    public void GetDateTime_ShouldReturnDateTime()
    {
        var result = _sut.GetDateTime();
        result.TestRefEquals( MySqlDataType.DateTime ).Go();
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnBigInt()
    {
        var result = _sut.GetTimeSpan();
        result.TestRefEquals( MySqlDataType.BigInt ).Go();
    }

    [Fact]
    public void GetDate_ShouldReturnDate()
    {
        var result = _sut.GetDate();
        result.TestRefEquals( MySqlDataType.Date ).Go();
    }

    [Fact]
    public void GetTime_ShouldReturnTime()
    {
        var result = _sut.GetTime();
        result.TestRefEquals( MySqlDataType.Time ).Go();
    }

    [Fact]
    public void GetBinary_ShouldReturnVarBinary()
    {
        var result = _sut.GetBinary();
        result.TestRefEquals( MySqlDataType.VarBinary ).Go();
    }

    [Theory]
    [InlineData( 0, "VARBINARY(0)" )]
    [InlineData( 500, "VARBINARY(500)" )]
    [InlineData( 65534, "VARBINARY(65534)" )]
    [InlineData( 65535, "VARBINARY(65535)" )]
    public void GetBinary_WithMaxLength_ShouldReturnCorrectVarBinary_WhenMaxLengthIsInVarBinaryBounds(int maxLength, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetBinary( maxLength );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarBinary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ maxLength ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarBinary.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetBinary_ShouldReturnBlob_WhenMaxLengthIsGreaterThanVarBinaryBoundsAllow()
    {
        var result = _sut.GetBinary( 65536 );
        result.TestRefEquals( MySqlDataType.Blob ).Go();
    }

    [Fact]
    public void GetBinary_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetBinary( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBinary()
    {
        var result = _sut.GetFixedBinary();
        result.TestRefEquals( MySqlDataType.Binary ).Go();
    }

    [Theory]
    [InlineData( 0, "BINARY(0)" )]
    [InlineData( 50, "BINARY(50)" )]
    [InlineData( 254, "BINARY(254)" )]
    [InlineData( 255, "BINARY(255)" )]
    public void GetFixedBinary_WithLength_ShouldReturnCorrectBinary_WhenLengthIsInBinaryBounds(int length, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedBinary( length );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.Binary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ length ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.Binary.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( 256, "VARBINARY(256)" )]
    [InlineData( 65534, "VARBINARY(65534)" )]
    [InlineData( 65535, "VARBINARY(65535)" )]
    public void GetFixedBinary_WithLength_ShouldReturnCorrectVarBinary_WhenLengthIsGreaterThanBinaryBoundsAllowButIsInVarBinaryBounds(
        int length,
        string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedBinary( length );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarBinary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ length ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarBinary.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBlob_WhenLengthIsGreaterThanVarBinaryBoundsAllow()
    {
        var result = _sut.GetFixedBinary( 65536 );
        result.TestRefEquals( MySqlDataType.Blob ).Go();
    }

    [Fact]
    public void GetFixedBinary_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedBinary( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }
}
