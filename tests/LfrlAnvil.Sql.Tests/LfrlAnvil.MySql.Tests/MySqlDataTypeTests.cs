using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDataTypeTests : TestsBase
{
    [Fact]
    public void Bool_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Bool;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BOOL" );
            sut.Value.Should().Be( MySqlDbType.Bool );
            sut.DbType.Should().Be( DbType.Boolean );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void TinyInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.TinyInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TINYINT" );
            sut.Value.Should().Be( MySqlDbType.Byte );
            sut.DbType.Should().Be( DbType.SByte );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void UnsignedTinyInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedTinyInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TINYINT UNSIGNED" );
            sut.Value.Should().Be( MySqlDbType.UByte );
            sut.DbType.Should().Be( DbType.Byte );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void SmallInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.SmallInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "SMALLINT" );
            sut.Value.Should().Be( MySqlDbType.Int16 );
            sut.DbType.Should().Be( DbType.Int16 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void UnsignedSmallInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedSmallInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "SMALLINT UNSIGNED" );
            sut.Value.Should().Be( MySqlDbType.UInt16 );
            sut.DbType.Should().Be( DbType.UInt16 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Int_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Int;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INT" );
            sut.Value.Should().Be( MySqlDbType.Int32 );
            sut.DbType.Should().Be( DbType.Int32 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void UnsignedInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INT UNSIGNED" );
            sut.Value.Should().Be( MySqlDbType.UInt32 );
            sut.DbType.Should().Be( DbType.UInt32 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void BigInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.BigInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BIGINT" );
            sut.Value.Should().Be( MySqlDbType.Int64 );
            sut.DbType.Should().Be( DbType.Int64 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void UnsignedBigInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedBigInt;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BIGINT UNSIGNED" );
            sut.Value.Should().Be( MySqlDbType.UInt64 );
            sut.DbType.Should().Be( DbType.UInt64 );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Float_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Float;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "FLOAT" );
            sut.Value.Should().Be( MySqlDbType.Float );
            sut.DbType.Should().Be( DbType.Single );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Double_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Double;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DOUBLE" );
            sut.Value.Should().Be( MySqlDbType.Double );
            sut.DbType.Should().Be( DbType.Double );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Decimal_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Decimal;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DECIMAL(29, 10)" );
            sut.Value.Should().Be( MySqlDbType.NewDecimal );
            sut.DbType.Should().Be( DbType.Decimal );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 29, 10 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 65 ) ),
                    new SqlDataTypeParameter( "SCALE", Bounds.Create( 0, 30 ) ) );
        }
    }

    [Fact]
    public void Char_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Char;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "CHAR(255)" );
            sut.Value.Should().Be( MySqlDbType.String );
            sut.DbType.Should().Be( DbType.StringFixedLength );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 255 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo( new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) );
        }
    }

    [Fact]
    public void Binary_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Binary;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BINARY(255)" );
            sut.Value.Should().Be( MySqlDbType.Binary );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 255 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo( new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) );
        }
    }

    [Fact]
    public void VarChar_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.VarChar;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "VARCHAR(65535)" );
            sut.Value.Should().Be( MySqlDbType.VarChar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 65535 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo( new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) );
        }
    }

    [Fact]
    public void VarBinary_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.VarBinary;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "VARBINARY(65535)" );
            sut.Value.Should().Be( MySqlDbType.VarBinary );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 65535 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo( new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) );
        }
    }

    [Fact]
    public void Blob_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Blob;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "LONGBLOB" );
            sut.Value.Should().Be( MySqlDbType.LongBlob );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Text_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Text;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "LONGTEXT" );
            sut.Value.Should().Be( MySqlDbType.LongText );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Date_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Date;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DATE" );
            sut.Value.Should().Be( MySqlDbType.Newdate );
            sut.DbType.Should().Be( DbType.Date );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Time_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Time;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TIME" );
            sut.Value.Should().Be( MySqlDbType.Time );
            sut.DbType.Should().Be( DbType.Time );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void DateTime_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.DateTime;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DATETIME" );
            sut.Value.Should().Be( MySqlDbType.DateTime );
            sut.DbType.Should().Be( DbType.DateTime );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void CreateDecimal_ShouldReturnStaticDecimal_WhenPrecisionAndScaleAreDefault()
    {
        var sut = MySqlDataType.CreateDecimal( 29, 10 );
        sut.Should().BeSameAs( MySqlDataType.Decimal );
    }

    [Theory]
    [InlineData( 10, 0, "DECIMAL(10, 0)" )]
    [InlineData( 29, 11, "DECIMAL(29, 11)" )]
    [InlineData( 28, 10, "DECIMAL(28, 10)" )]
    [InlineData( 65, 30, "DECIMAL(65, 30)" )]
    public void CreateDecimal_ShouldReturnNewDecimal_WhenPrecisionOrScaleAreNotDefault(int precision, int scale, string expected)
    {
        var sut = MySqlDataType.CreateDecimal( precision, scale );

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
    public void CreateDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateDecimal( precision, scale ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void CreateChar_ShouldReturnStaticChar_WhenLengthIsDefault()
    {
        var sut = MySqlDataType.CreateChar( 255 );
        sut.Should().BeSameAs( MySqlDataType.Char );
    }

    [Theory]
    [InlineData( 0, "CHAR(0)" )]
    [InlineData( 50, "CHAR(50)" )]
    [InlineData( 254, "CHAR(254)" )]
    public void CreateChar_ShouldReturnNewChar_WhenLengthIsNotDefault(int length, string expected)
    {
        var sut = MySqlDataType.CreateChar( length );

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
    [InlineData( -1 )]
    [InlineData( 256 )]
    public void CreateChar_ShouldThrowSqlDataTypeException_WhenLengthIsInvalid(int length)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateChar( length ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void CreateBinary_ShouldReturnStaticBinary_WhenLengthIsDefault()
    {
        var sut = MySqlDataType.CreateBinary( 255 );
        sut.Should().BeSameAs( MySqlDataType.Binary );
    }

    [Theory]
    [InlineData( 0, "BINARY(0)" )]
    [InlineData( 50, "BINARY(50)" )]
    [InlineData( 254, "BINARY(254)" )]
    public void CreateBinary_ShouldReturnNewBinary_WhenLengthIsNotDefault(int length, string expected)
    {
        var sut = MySqlDataType.CreateBinary( length );

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
    [InlineData( -1 )]
    [InlineData( 256 )]
    public void CreateBinary_ShouldThrowSqlDataTypeException_WhenLengthIsInvalid(int length)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateBinary( length ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void CreateVarChar_ShouldReturnStaticVarChar_WhenMaxLengthIsDefault()
    {
        var sut = MySqlDataType.CreateVarChar( 65535 );
        sut.Should().BeSameAs( MySqlDataType.VarChar );
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 65534, "VARCHAR(65534)" )]
    public void CreateVarChar_ShouldReturnNewVarChar_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = MySqlDataType.CreateVarChar( maxLength );

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

    [Theory]
    [InlineData( -1 )]
    [InlineData( 65536 )]
    public void CreateVarChar_ShouldThrowSqlDataTypeException_WhenMaxLengthIsInvalid(int maxLength)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateVarChar( maxLength ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void CreateVarBinary_ShouldReturnStaticVarBinary_WhenMaxLengthIsDefault()
    {
        var sut = MySqlDataType.CreateVarBinary( 65535 );
        sut.Should().BeSameAs( MySqlDataType.VarBinary );
    }

    [Theory]
    [InlineData( 0, "VARBINARY(0)" )]
    [InlineData( 500, "VARBINARY(500)" )]
    [InlineData( 65534, "VARBINARY(65534)" )]
    public void CreateVarBinary_ShouldReturnNewVarBinary_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = MySqlDataType.CreateVarBinary( maxLength );

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

    [Theory]
    [InlineData( -1 )]
    [InlineData( 65536 )]
    public void CreateVarBinary_ShouldThrowSqlDataTypeException_WhenMaxLengthIsInvalid(int maxLength)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateVarBinary( maxLength ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void Custom_ShouldCreateNewType()
    {
        var sut = MySqlDataType.Custom( "FOO", MySqlDbType.Geometry, DbType.Object );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "FOO" );
            sut.Value.Should().Be( MySqlDbType.Geometry );
            sut.DbType.Should().Be( DbType.Object );
            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = MySqlDataType.Decimal;
        var result = sut.ToString();
        result.Should().Be( "'DECIMAL(29, 10)' (NewDecimal)" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = MySqlDataType.Decimal;
        var expected = HashCode.Combine( sut.Value, sut.Name );
        var result = sut.GetHashCode();
        result.Should().Be( expected );
    }

    [Fact]
    public void MySqlDbTypeConversionOperator_ShouldReturnValue()
    {
        var sut = MySqlDataType.Decimal;
        var result = (MySqlDbType)sut;
        result.Should().Be( sut.Value );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, false )]
    public void Equals_ShouldCompareByValueThenName(string name1, MySqlDbType value1, string name2, MySqlDbType value2, bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ).Equals( MySqlDataType.Custom( name2, value2, DbType.Object ) );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, false )]
    public void EqualityOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) == MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, true )]
    public void InequalityOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) != MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, 0 )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, 1 )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, 1 )]
    [InlineData( "bar", MySqlDbType.Byte, "foo", MySqlDbType.Byte, -1 )]
    [InlineData( "foo", MySqlDbType.Decimal, "foo", MySqlDbType.Byte, -1 )]
    public void CompareTo_ShouldCompareByValueThenName(string name1, MySqlDbType value1, string name2, MySqlDbType value2, int expectedSign)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ).CompareTo( MySqlDataType.Custom( name2, value2, DbType.Object ) );
        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void CompareTo_ShouldReturnOne_WhenOtherIsNull()
    {
        var result = MySqlDataType.Custom( "foo", MySqlDbType.Byte, DbType.Object ).CompareTo( null );
        result.Should().Be( 1 );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, true )]
    [InlineData( "bar", MySqlDbType.Byte, "foo", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Decimal, "foo", MySqlDbType.Byte, false )]
    public void GreaterThanOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) > MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, true )]
    [InlineData( "bar", MySqlDbType.Byte, "foo", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Decimal, "foo", MySqlDbType.Byte, false )]
    public void GreaterThanOrEqualToOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) >= MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, false )]
    [InlineData( "bar", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Decimal, "foo", MySqlDbType.Byte, true )]
    public void LessThanOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) < MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, false )]
    [InlineData( "bar", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Decimal, "foo", MySqlDbType.Byte, true )]
    public void LessThanOrEqualToOperator_ShouldCompareByValueThenName(
        string name1,
        MySqlDbType value1,
        string name2,
        MySqlDbType value2,
        bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ) <= MySqlDataType.Custom( name2, value2, DbType.Object );
        result.Should().Be( expected );
    }
}
