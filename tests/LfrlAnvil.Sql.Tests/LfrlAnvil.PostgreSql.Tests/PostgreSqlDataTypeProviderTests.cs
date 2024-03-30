using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new PostgreSqlDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnBoolean()
    {
        var result = _sut.GetBool();
        result.Should().BeSameAs( PostgreSqlDataType.Boolean );
    }

    [Fact]
    public void GetInt8_ShouldReturnInt2()
    {
        var result = _sut.GetInt8();
        result.Should().BeSameAs( PostgreSqlDataType.Int2 );
    }

    [Fact]
    public void GetInt16_ShouldReturnInt2()
    {
        var result = _sut.GetInt16();
        result.Should().BeSameAs( PostgreSqlDataType.Int2 );
    }

    [Fact]
    public void GetInt32_ShouldReturnInt4()
    {
        var result = _sut.GetInt32();
        result.Should().BeSameAs( PostgreSqlDataType.Int4 );
    }

    [Fact]
    public void GetInt64_ShouldReturnInt8()
    {
        var result = _sut.GetInt64();
        result.Should().BeSameAs( PostgreSqlDataType.Int8 );
    }

    [Fact]
    public void GetUInt8_ShouldReturnInt2()
    {
        var result = _sut.GetUInt8();
        result.Should().BeSameAs( PostgreSqlDataType.Int2 );
    }

    [Fact]
    public void GetUInt16_ShouldReturnInt4()
    {
        var result = _sut.GetUInt16();
        result.Should().BeSameAs( PostgreSqlDataType.Int4 );
    }

    [Fact]
    public void GetUInt32_ShouldReturnInt8()
    {
        var result = _sut.GetUInt32();
        result.Should().BeSameAs( PostgreSqlDataType.Int8 );
    }

    [Fact]
    public void GetUInt64_ShouldReturnInt8()
    {
        var result = _sut.GetUInt64();
        result.Should().BeSameAs( PostgreSqlDataType.Int8 );
    }

    [Fact]
    public void GetFloat_ShouldReturnFloat4()
    {
        var result = _sut.GetFloat();
        result.Should().BeSameAs( PostgreSqlDataType.Float4 );
    }

    [Fact]
    public void GetDouble_ShouldReturnFloat8()
    {
        var result = _sut.GetDouble();
        result.Should().BeSameAs( PostgreSqlDataType.Float8 );
    }

    [Fact]
    public void GetDecimal_ShouldReturnDecimal()
    {
        var result = _sut.GetDecimal();
        result.Should().BeSameAs( PostgreSqlDataType.Decimal );
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
        var sut = (PostgreSqlDataType)_sut.GetDecimal( precision, scale );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( NpgsqlDbType.Numeric );
            sut.DbType.Should().Be( DbType.Decimal );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( precision, scale );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( PostgreSqlDataType.Decimal.ParameterDefinitions.ToArray() );
        }
    }

    [Theory]
    [InlineData( -1, 0 )]
    [InlineData( 1001, 0 )]
    [InlineData( 29, -1001 )]
    [InlineData( 29, 1001 )]
    public void GetDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => _sut.GetDecimal( precision, scale ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetGuid_ShouldReturnUuid()
    {
        var result = _sut.GetGuid();
        result.Should().BeSameAs( PostgreSqlDataType.Uuid );
    }

    [Fact]
    public void GetString_ShouldReturnVarChar()
    {
        var result = _sut.GetString();
        result.Should().BeSameAs( PostgreSqlDataType.VarChar );
    }

    [Fact]
    public void GetString_WithMaxLength_ShouldReturnVarChar_WhenMaxLengthIsTooLarge()
    {
        var sut = _sut.GetString( 10485761 );
        sut.Should().BeSameAs( PostgreSqlDataType.VarChar );
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void GetString_WithMaxLength_ShouldReturnCorrectVarChar_WhenMaxLengthIsInVarCharBounds(int maxLength, string expected)
    {
        var sut = (PostgreSqlDataType)_sut.GetString( maxLength );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( NpgsqlDbType.Varchar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( maxLength );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( PostgreSqlDataType.VarChar.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetString_WithMaxLength_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetString( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetFixedString_ShouldReturnVarChar()
    {
        var result = _sut.GetFixedString();
        result.Should().BeSameAs( PostgreSqlDataType.VarChar );
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldReturnVarChar_WhenLengthIsTooLarge()
    {
        var sut = _sut.GetFixedString( 10485761 );
        sut.Should().BeSameAs( PostgreSqlDataType.VarChar );
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void GetFixedString_WithLength_ShouldReturnCorrectVarChar_WhenLengthIsInVarCharBounds(int length, string expected)
    {
        var sut = (PostgreSqlDataType)_sut.GetFixedString( length );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( NpgsqlDbType.Varchar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( length );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( PostgreSqlDataType.VarChar.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetFixedString_WithLength_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedString( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnInt8()
    {
        var result = _sut.GetTimestamp();
        result.Should().BeSameAs( PostgreSqlDataType.Int8 );
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnTimestampTz()
    {
        var result = _sut.GetUtcDateTime();
        result.Should().BeSameAs( PostgreSqlDataType.TimestampTz );
    }

    [Fact]
    public void GetDateTime_ShouldReturnTimestamp()
    {
        var result = _sut.GetDateTime();
        result.Should().BeSameAs( PostgreSqlDataType.Timestamp );
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnInt8()
    {
        var result = _sut.GetTimeSpan();
        result.Should().BeSameAs( PostgreSqlDataType.Int8 );
    }

    [Fact]
    public void GetDate_ShouldReturnDate()
    {
        var result = _sut.GetDate();
        result.Should().BeSameAs( PostgreSqlDataType.Date );
    }

    [Fact]
    public void GetTime_ShouldReturnTime()
    {
        var result = _sut.GetTime();
        result.Should().BeSameAs( PostgreSqlDataType.Time );
    }

    [Fact]
    public void GetBinary_ShouldReturnBytea()
    {
        var result = _sut.GetBinary();
        result.Should().BeSameAs( PostgreSqlDataType.Bytea );
    }

    [Fact]
    public void GetBinary_WithMaxLength_ShouldReturnBytea()
    {
        var sut = _sut.GetBinary( 10485761 );
        sut.Should().BeSameAs( PostgreSqlDataType.Bytea );
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBytea()
    {
        var result = _sut.GetFixedBinary();
        result.Should().BeSameAs( PostgreSqlDataType.Bytea );
    }

    [Fact]
    public void GetFixedBinary_WithLength_ShouldReturnBytea()
    {
        var sut = _sut.GetFixedBinary( 10485761 );
        sut.Should().BeSameAs( PostgreSqlDataType.Bytea );
    }
}
