using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new PostgreSqlDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnBoolean()
    {
        var result = _sut.GetBool();
        result.TestRefEquals( PostgreSqlDataType.Boolean ).Go();
    }

    [Fact]
    public void GetInt8_ShouldReturnInt2()
    {
        var result = _sut.GetInt8();
        result.TestRefEquals( PostgreSqlDataType.Int2 ).Go();
    }

    [Fact]
    public void GetInt16_ShouldReturnInt2()
    {
        var result = _sut.GetInt16();
        result.TestRefEquals( PostgreSqlDataType.Int2 ).Go();
    }

    [Fact]
    public void GetInt32_ShouldReturnInt4()
    {
        var result = _sut.GetInt32();
        result.TestRefEquals( PostgreSqlDataType.Int4 ).Go();
    }

    [Fact]
    public void GetInt64_ShouldReturnInt8()
    {
        var result = _sut.GetInt64();
        result.TestRefEquals( PostgreSqlDataType.Int8 ).Go();
    }

    [Fact]
    public void GetUInt8_ShouldReturnInt2()
    {
        var result = _sut.GetUInt8();
        result.TestRefEquals( PostgreSqlDataType.Int2 ).Go();
    }

    [Fact]
    public void GetUInt16_ShouldReturnInt4()
    {
        var result = _sut.GetUInt16();
        result.TestRefEquals( PostgreSqlDataType.Int4 ).Go();
    }

    [Fact]
    public void GetUInt32_ShouldReturnInt8()
    {
        var result = _sut.GetUInt32();
        result.TestRefEquals( PostgreSqlDataType.Int8 ).Go();
    }

    [Fact]
    public void GetUInt64_ShouldReturnInt8()
    {
        var result = _sut.GetUInt64();
        result.TestRefEquals( PostgreSqlDataType.Int8 ).Go();
    }

    [Fact]
    public void GetFloat_ShouldReturnFloat4()
    {
        var result = _sut.GetFloat();
        result.TestRefEquals( PostgreSqlDataType.Float4 ).Go();
    }

    [Fact]
    public void GetDouble_ShouldReturnFloat8()
    {
        var result = _sut.GetDouble();
        result.TestRefEquals( PostgreSqlDataType.Float8 ).Go();
    }

    [Fact]
    public void GetDecimal_ShouldReturnDecimal()
    {
        var result = _sut.GetDecimal();
        result.TestRefEquals( PostgreSqlDataType.Decimal ).Go();
    }

    [Theory]
    [InlineData( 10, 0, "DECIMAL(10, 0)" )]
    [InlineData( 29, 10, "DECIMAL(29, 10)" )]
    [InlineData( 29, 11, "DECIMAL(29, 11)" )]
    [InlineData( 28, 10, "DECIMAL(28, 10)" )]
    [InlineData( 1000, 1000, "DECIMAL(1000, 1000)" )]
    [InlineData( 1000, -1000, "DECIMAL(1000, -1000)" )]
    public void GetDecimal_WithParameters_ShouldReturnCorrectDecimal(int precision, int scale, string expected)
    {
        var sut = ( PostgreSqlDataType )_sut.GetDecimal( precision, scale );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( NpgsqlDbType.Numeric ),
                sut.DbType.TestEquals( DbType.Decimal ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.TestSequence( [ precision, scale ] ),
                sut.ParameterDefinitions.TestSequence( PostgreSqlDataType.Decimal.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1001, 0 )]
    [InlineData( 29, -1001 )]
    [InlineData( 29, 1001 )]
    public void GetDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => _sut.GetDecimal( precision, scale ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetGuid_ShouldReturnUuid()
    {
        var result = _sut.GetGuid();
        result.TestRefEquals( PostgreSqlDataType.Uuid ).Go();
    }

    [Fact]
    public void GetString_ShouldReturnVarChar()
    {
        var result = _sut.GetString();
        result.TestRefEquals( PostgreSqlDataType.VarChar ).Go();
    }

    [Fact]
    public void GetString_WithMaxLength_ShouldReturnVarChar_WhenMaxLengthIsTooLarge()
    {
        var sut = _sut.GetString( 10485761 );
        sut.TestRefEquals( PostgreSqlDataType.VarChar ).Go();
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void GetString_WithMaxLength_ShouldReturnCorrectVarChar_WhenMaxLengthIsInVarCharBounds(int maxLength, string expected)
    {
        var sut = ( PostgreSqlDataType )_sut.GetString( maxLength );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( NpgsqlDbType.Varchar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.TestSequence( [ maxLength ] ),
                sut.ParameterDefinitions.TestSequence( PostgreSqlDataType.VarChar.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetString_WithMaxLength_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetString( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetFixedString_ShouldReturnVarChar()
    {
        var result = _sut.GetFixedString();
        result.TestRefEquals( PostgreSqlDataType.VarChar ).Go();
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldReturnVarChar_WhenLengthIsTooLarge()
    {
        var sut = _sut.GetFixedString( 10485761 );
        sut.TestRefEquals( PostgreSqlDataType.VarChar ).Go();
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void GetFixedString_WithLength_ShouldReturnCorrectVarChar_WhenLengthIsInVarCharBounds(int length, string expected)
    {
        var sut = ( PostgreSqlDataType )_sut.GetFixedString( length );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( NpgsqlDbType.Varchar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.TestSequence( [ length ] ),
                sut.ParameterDefinitions.TestSequence( PostgreSqlDataType.VarChar.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedString( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnInt8()
    {
        var result = _sut.GetTimestamp();
        result.TestRefEquals( PostgreSqlDataType.Int8 ).Go();
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnTimestampTz()
    {
        var result = _sut.GetUtcDateTime();
        result.TestRefEquals( PostgreSqlDataType.TimestampTz ).Go();
    }

    [Fact]
    public void GetDateTime_ShouldReturnTimestamp()
    {
        var result = _sut.GetDateTime();
        result.TestRefEquals( PostgreSqlDataType.Timestamp ).Go();
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnInt8()
    {
        var result = _sut.GetTimeSpan();
        result.TestRefEquals( PostgreSqlDataType.Int8 ).Go();
    }

    [Fact]
    public void GetDate_ShouldReturnDate()
    {
        var result = _sut.GetDate();
        result.TestRefEquals( PostgreSqlDataType.Date ).Go();
    }

    [Fact]
    public void GetTime_ShouldReturnTime()
    {
        var result = _sut.GetTime();
        result.TestRefEquals( PostgreSqlDataType.Time ).Go();
    }

    [Fact]
    public void GetBinary_ShouldReturnBytea()
    {
        var result = _sut.GetBinary();
        result.TestRefEquals( PostgreSqlDataType.Bytea ).Go();
    }

    [Fact]
    public void GetBinary_WithMaxLength_ShouldReturnBytea()
    {
        var sut = _sut.GetBinary( 10485761 );
        sut.TestRefEquals( PostgreSqlDataType.Bytea ).Go();
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBytea()
    {
        var result = _sut.GetFixedBinary();
        result.TestRefEquals( PostgreSqlDataType.Bytea ).Go();
    }

    [Fact]
    public void GetFixedBinary_WithLength_ShouldReturnBytea()
    {
        var sut = _sut.GetFixedBinary( 10485761 );
        sut.TestRefEquals( PostgreSqlDataType.Bytea ).Go();
    }
}
