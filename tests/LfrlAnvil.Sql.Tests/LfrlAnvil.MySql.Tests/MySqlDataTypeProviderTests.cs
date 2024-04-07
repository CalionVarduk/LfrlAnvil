using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new MySqlDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnBool()
    {
        var result = _sut.GetBool();
        result.Should().BeSameAs( MySqlDataType.Bool );
    }

    [Fact]
    public void GetInt8_ShouldReturnTinyInt()
    {
        var result = _sut.GetInt8();
        result.Should().BeSameAs( MySqlDataType.TinyInt );
    }

    [Fact]
    public void GetInt16_ShouldReturnSmallInt()
    {
        var result = _sut.GetInt16();
        result.Should().BeSameAs( MySqlDataType.SmallInt );
    }

    [Fact]
    public void GetInt32_ShouldReturnInt()
    {
        var result = _sut.GetInt32();
        result.Should().BeSameAs( MySqlDataType.Int );
    }

    [Fact]
    public void GetInt64_ShouldReturnBigInt()
    {
        var result = _sut.GetInt64();
        result.Should().BeSameAs( MySqlDataType.BigInt );
    }

    [Fact]
    public void GetUInt8_ShouldReturnUnsignedTinyInt()
    {
        var result = _sut.GetUInt8();
        result.Should().BeSameAs( MySqlDataType.UnsignedTinyInt );
    }

    [Fact]
    public void GetUInt16_ShouldReturnUnsignedSmallInt()
    {
        var result = _sut.GetUInt16();
        result.Should().BeSameAs( MySqlDataType.UnsignedSmallInt );
    }

    [Fact]
    public void GetUInt32_ShouldReturnUnsignedInt()
    {
        var result = _sut.GetUInt32();
        result.Should().BeSameAs( MySqlDataType.UnsignedInt );
    }

    [Fact]
    public void GetUInt64_ShouldReturnUnsignedBigInt()
    {
        var result = _sut.GetUInt64();
        result.Should().BeSameAs( MySqlDataType.UnsignedBigInt );
    }

    [Fact]
    public void GetFloat_ShouldReturnFloat()
    {
        var result = _sut.GetFloat();
        result.Should().BeSameAs( MySqlDataType.Float );
    }

    [Fact]
    public void GetDouble_ShouldReturnDouble()
    {
        var result = _sut.GetDouble();
        result.Should().BeSameAs( MySqlDataType.Double );
    }

    [Fact]
    public void GetDecimal_ShouldReturnDecimal()
    {
        var result = _sut.GetDecimal();
        result.Should().BeSameAs( MySqlDataType.Decimal );
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

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.NewDecimal );
            sut.DbType.Should().Be( DbType.Decimal );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( precision, scale );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.Decimal.ParameterDefinitions.ToArray() );
        }
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
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetGuid_ShouldReturnBinary16()
    {
        var result = _sut.GetGuid();
        result.Should().BeEquivalentTo( MySqlDataType.CreateBinary( 16 ) );
    }

    [Fact]
    public void GetString_ShouldReturnVarChar()
    {
        var result = _sut.GetString();
        result.Should().BeSameAs( MySqlDataType.VarChar );
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 65534, "VARCHAR(65534)" )]
    [InlineData( 65535, "VARCHAR(65535)" )]
    public void GetString_WithMaxLength_ShouldReturnCorrectVarChar_WhenMaxLengthIsInVarCharBounds(int maxLength, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetString( maxLength );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.VarChar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( maxLength );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.VarChar.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetString_ShouldReturnText_WhenMaxLengthIsGreaterThanVarCharBoundsAllow()
    {
        var result = _sut.GetString( 65536 );
        result.Should().BeSameAs( MySqlDataType.Text );
    }

    [Fact]
    public void GetString_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetString( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetFixedString_ShouldReturnChar()
    {
        var result = _sut.GetFixedString();
        result.Should().BeSameAs( MySqlDataType.Char );
    }

    [Theory]
    [InlineData( 0, "CHAR(0)" )]
    [InlineData( 50, "CHAR(50)" )]
    [InlineData( 254, "CHAR(254)" )]
    [InlineData( 255, "CHAR(255)" )]
    public void GetFixedString_WithLength_ShouldReturnCorrectChar_WhenLengthIsInCharBounds(int length, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedString( length );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.String );
            sut.DbType.Should().Be( DbType.StringFixedLength );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( length );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.Char.ParameterDefinitions.ToArray() );
        }
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

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.VarChar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( length );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.VarChar.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetFixedString_ShouldReturnText_WhenLengthIsGreaterThanVarCharBoundsAllow()
    {
        var result = _sut.GetFixedString( 65536 );
        result.Should().BeSameAs( MySqlDataType.Text );
    }

    [Fact]
    public void GetFixedString_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedString( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnBigInt()
    {
        var result = _sut.GetTimestamp();
        result.Should().BeSameAs( MySqlDataType.BigInt );
    }

    [Fact]
    public void GetUtcDateTime_ShouldReturnDateTime()
    {
        var result = _sut.GetUtcDateTime();
        result.Should().BeSameAs( MySqlDataType.DateTime );
    }

    [Fact]
    public void GetDateTime_ShouldReturnDateTime()
    {
        var result = _sut.GetDateTime();
        result.Should().BeSameAs( MySqlDataType.DateTime );
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnBigInt()
    {
        var result = _sut.GetTimeSpan();
        result.Should().BeSameAs( MySqlDataType.BigInt );
    }

    [Fact]
    public void GetDate_ShouldReturnDate()
    {
        var result = _sut.GetDate();
        result.Should().BeSameAs( MySqlDataType.Date );
    }

    [Fact]
    public void GetTime_ShouldReturnTime()
    {
        var result = _sut.GetTime();
        result.Should().BeSameAs( MySqlDataType.Time );
    }

    [Fact]
    public void GetBinary_ShouldReturnVarBinary()
    {
        var result = _sut.GetBinary();
        result.Should().BeSameAs( MySqlDataType.VarBinary );
    }

    [Theory]
    [InlineData( 0, "VARBINARY(0)" )]
    [InlineData( 500, "VARBINARY(500)" )]
    [InlineData( 65534, "VARBINARY(65534)" )]
    [InlineData( 65535, "VARBINARY(65535)" )]
    public void GetBinary_WithMaxLength_ShouldReturnCorrectVarBinary_WhenMaxLengthIsInVarBinaryBounds(int maxLength, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetBinary( maxLength );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.VarBinary );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( maxLength );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.VarBinary.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetBinary_ShouldReturnBlob_WhenMaxLengthIsGreaterThanVarBinaryBoundsAllow()
    {
        var result = _sut.GetBinary( 65536 );
        result.Should().BeSameAs( MySqlDataType.Blob );
    }

    [Fact]
    public void GetBinary_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetBinary( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBinary()
    {
        var result = _sut.GetFixedBinary();
        result.Should().BeSameAs( MySqlDataType.Binary );
    }

    [Theory]
    [InlineData( 0, "BINARY(0)" )]
    [InlineData( 50, "BINARY(50)" )]
    [InlineData( 254, "BINARY(254)" )]
    [InlineData( 255, "BINARY(255)" )]
    public void GetFixedBinary_WithLength_ShouldReturnCorrectBinary_WhenLengthIsInBinaryBounds(int length, string expected)
    {
        var sut = ( MySqlDataType )_sut.GetFixedBinary( length );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.Binary );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( length );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.Binary.ParameterDefinitions.ToArray() );
        }
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

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( expected );
            sut.Value.Should().Be( MySqlDbType.VarBinary );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( length );
            sut.ParameterDefinitions.ToArray().Should().BeSequentiallyEqualTo( MySqlDataType.VarBinary.ParameterDefinitions.ToArray() );
        }
    }

    [Fact]
    public void GetFixedBinary_ShouldReturnBlob_WhenLengthIsGreaterThanVarBinaryBoundsAllow()
    {
        var result = _sut.GetFixedBinary( 65536 );
        result.Should().BeSameAs( MySqlDataType.Blob );
    }

    [Fact]
    public void GetFixedBinary_ShouldThrowSqlDataTypeException_WhenLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetFixedBinary( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }
}
