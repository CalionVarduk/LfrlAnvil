using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDataTypeTests : TestsBase
{
    [Fact]
    public void Bool_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Bool;

        Assertion.All(
                sut.Name.TestEquals( "BOOL" ),
                sut.Value.TestEquals( MySqlDbType.Bool ),
                sut.DbType.TestEquals( DbType.Boolean ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void TinyInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.TinyInt;

        Assertion.All(
                sut.Name.TestEquals( "TINYINT" ),
                sut.Value.TestEquals( MySqlDbType.Byte ),
                sut.DbType.TestEquals( DbType.SByte ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void UnsignedTinyInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedTinyInt;

        Assertion.All(
                sut.Name.TestEquals( "TINYINT UNSIGNED" ),
                sut.Value.TestEquals( MySqlDbType.UByte ),
                sut.DbType.TestEquals( DbType.Byte ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void SmallInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.SmallInt;

        Assertion.All(
                sut.Name.TestEquals( "SMALLINT" ),
                sut.Value.TestEquals( MySqlDbType.Int16 ),
                sut.DbType.TestEquals( DbType.Int16 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void UnsignedSmallInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedSmallInt;

        Assertion.All(
                sut.Name.TestEquals( "SMALLINT UNSIGNED" ),
                sut.Value.TestEquals( MySqlDbType.UInt16 ),
                sut.DbType.TestEquals( DbType.UInt16 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Int_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Int;

        Assertion.All(
                sut.Name.TestEquals( "INT" ),
                sut.Value.TestEquals( MySqlDbType.Int32 ),
                sut.DbType.TestEquals( DbType.Int32 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void UnsignedInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedInt;

        Assertion.All(
                sut.Name.TestEquals( "INT UNSIGNED" ),
                sut.Value.TestEquals( MySqlDbType.UInt32 ),
                sut.DbType.TestEquals( DbType.UInt32 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void BigInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.BigInt;

        Assertion.All(
                sut.Name.TestEquals( "BIGINT" ),
                sut.Value.TestEquals( MySqlDbType.Int64 ),
                sut.DbType.TestEquals( DbType.Int64 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void UnsignedBigInt_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.UnsignedBigInt;

        Assertion.All(
                sut.Name.TestEquals( "BIGINT UNSIGNED" ),
                sut.Value.TestEquals( MySqlDbType.UInt64 ),
                sut.DbType.TestEquals( DbType.UInt64 ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Float_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Float;

        Assertion.All(
                sut.Name.TestEquals( "FLOAT" ),
                sut.Value.TestEquals( MySqlDbType.Float ),
                sut.DbType.TestEquals( DbType.Single ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Double_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Double;

        Assertion.All(
                sut.Name.TestEquals( "DOUBLE" ),
                sut.Value.TestEquals( MySqlDbType.Double ),
                sut.DbType.TestEquals( DbType.Double ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Decimal_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Decimal;

        Assertion.All(
                sut.Name.TestEquals( "DECIMAL(29, 10)" ),
                sut.Value.TestEquals( MySqlDbType.NewDecimal ),
                sut.DbType.TestEquals( DbType.Decimal ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 29, 10 ] ),
                sut.ParameterDefinitions.ToArray()
                    .TestSequence(
                    [
                        new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 65 ) ),
                        new SqlDataTypeParameter( "SCALE", Bounds.Create( 0, 30 ) )
                    ] ) )
            .Go();
    }

    [Fact]
    public void Char_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Char;

        Assertion.All(
                sut.Name.TestEquals( "CHAR(255)" ),
                sut.Value.TestEquals( MySqlDbType.String ),
                sut.DbType.TestEquals( DbType.StringFixedLength ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 255 ] ),
                sut.ParameterDefinitions.ToArray().TestSequence( [ new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) ] ) )
            .Go();
    }

    [Fact]
    public void Binary_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Binary;

        Assertion.All(
                sut.Name.TestEquals( "BINARY(255)" ),
                sut.Value.TestEquals( MySqlDbType.Binary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 255 ] ),
                sut.ParameterDefinitions.ToArray().TestSequence( [ new SqlDataTypeParameter( "LENGTH", Bounds.Create( 0, 255 ) ) ] ) )
            .Go();
    }

    [Fact]
    public void VarChar_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.VarChar;

        Assertion.All(
                sut.Name.TestEquals( "VARCHAR(65535)" ),
                sut.Value.TestEquals( MySqlDbType.VarChar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 65535 ] ),
                sut.ParameterDefinitions.ToArray().TestSequence( [ new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) ] ) )
            .Go();
    }

    [Fact]
    public void VarBinary_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.VarBinary;

        Assertion.All(
                sut.Name.TestEquals( "VARBINARY(65535)" ),
                sut.Value.TestEquals( MySqlDbType.VarBinary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 65535 ] ),
                sut.ParameterDefinitions.ToArray().TestSequence( [ new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 65535 ) ) ] ) )
            .Go();
    }

    [Fact]
    public void Blob_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Blob;

        Assertion.All(
                sut.Name.TestEquals( "LONGBLOB" ),
                sut.Value.TestEquals( MySqlDbType.LongBlob ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Text_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Text;

        Assertion.All(
                sut.Name.TestEquals( "LONGTEXT" ),
                sut.Value.TestEquals( MySqlDbType.LongText ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Date_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Date;

        Assertion.All(
                sut.Name.TestEquals( "DATE" ),
                sut.Value.TestEquals( MySqlDbType.Newdate ),
                sut.DbType.TestEquals( DbType.Date ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Time_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.Time;

        Assertion.All(
                sut.Name.TestEquals( "TIME(6)" ),
                sut.Value.TestEquals( MySqlDbType.Time ),
                sut.DbType.TestEquals( DbType.Time ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void DateTime_ShouldHaveCorrectProperties()
    {
        var sut = MySqlDataType.DateTime;

        Assertion.All(
                sut.Name.TestEquals( "DATETIME(6)" ),
                sut.Value.TestEquals( MySqlDbType.DateTime ),
                sut.DbType.TestEquals( DbType.DateTime ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void CreateDecimal_ShouldReturnStaticDecimal_WhenPrecisionAndScaleAreDefault()
    {
        var sut = MySqlDataType.CreateDecimal( 29, 10 );
        sut.TestRefEquals( MySqlDataType.Decimal ).Go();
    }

    [Theory]
    [InlineData( 10, 0, "DECIMAL(10, 0)" )]
    [InlineData( 29, 11, "DECIMAL(29, 11)" )]
    [InlineData( 28, 10, "DECIMAL(28, 10)" )]
    [InlineData( 65, 30, "DECIMAL(65, 30)" )]
    public void CreateDecimal_ShouldReturnNewDecimal_WhenPrecisionOrScaleAreNotDefault(int precision, int scale, string expected)
    {
        var sut = MySqlDataType.CreateDecimal( precision, scale );

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
    public void CreateDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateDecimal( precision, scale ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void CreateChar_ShouldReturnStaticChar_WhenLengthIsDefault()
    {
        var sut = MySqlDataType.CreateChar( 255 );
        sut.TestRefEquals( MySqlDataType.Char ).Go();
    }

    [Theory]
    [InlineData( 0, "CHAR(0)" )]
    [InlineData( 50, "CHAR(50)" )]
    [InlineData( 254, "CHAR(254)" )]
    public void CreateChar_ShouldReturnNewChar_WhenLengthIsNotDefault(int length, string expected)
    {
        var sut = MySqlDataType.CreateChar( length );

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
    [InlineData( -1 )]
    [InlineData( 256 )]
    public void CreateChar_ShouldThrowSqlDataTypeException_WhenLengthIsInvalid(int length)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateChar( length ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void CreateBinary_ShouldReturnStaticBinary_WhenLengthIsDefault()
    {
        var sut = MySqlDataType.CreateBinary( 255 );
        sut.TestRefEquals( MySqlDataType.Binary ).Go();
    }

    [Theory]
    [InlineData( 0, "BINARY(0)" )]
    [InlineData( 50, "BINARY(50)" )]
    [InlineData( 254, "BINARY(254)" )]
    public void CreateBinary_ShouldReturnNewBinary_WhenLengthIsNotDefault(int length, string expected)
    {
        var sut = MySqlDataType.CreateBinary( length );

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
    [InlineData( -1 )]
    [InlineData( 256 )]
    public void CreateBinary_ShouldThrowSqlDataTypeException_WhenLengthIsInvalid(int length)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateBinary( length ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void CreateVarChar_ShouldReturnStaticVarChar_WhenMaxLengthIsDefault()
    {
        var sut = MySqlDataType.CreateVarChar( 65535 );
        sut.TestRefEquals( MySqlDataType.VarChar ).Go();
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 65534, "VARCHAR(65534)" )]
    public void CreateVarChar_ShouldReturnNewVarChar_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = MySqlDataType.CreateVarChar( maxLength );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarChar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ maxLength ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarChar.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 65536 )]
    public void CreateVarChar_ShouldThrowSqlDataTypeException_WhenMaxLengthIsInvalid(int maxLength)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateVarChar( maxLength ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void CreateVarBinary_ShouldReturnStaticVarBinary_WhenMaxLengthIsDefault()
    {
        var sut = MySqlDataType.CreateVarBinary( 65535 );
        sut.TestRefEquals( MySqlDataType.VarBinary ).Go();
    }

    [Theory]
    [InlineData( 0, "VARBINARY(0)" )]
    [InlineData( 500, "VARBINARY(500)" )]
    [InlineData( 65534, "VARBINARY(65534)" )]
    public void CreateVarBinary_ShouldReturnNewVarBinary_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = MySqlDataType.CreateVarBinary( maxLength );

        Assertion.All(
                sut.Name.TestEquals( expected ),
                sut.Value.TestEquals( MySqlDbType.VarBinary ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.TestSequence( [ maxLength ] ),
                sut.ParameterDefinitions.TestSequence( MySqlDataType.VarBinary.ParameterDefinitions.ToArray() ) )
            .Go();
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 65536 )]
    public void CreateVarBinary_ShouldThrowSqlDataTypeException_WhenMaxLengthIsInvalid(int maxLength)
    {
        var action = Lambda.Of( () => MySqlDataType.CreateVarBinary( maxLength ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void Custom_ShouldCreateNewType()
    {
        var sut = MySqlDataType.Custom( "FOO", MySqlDbType.Geometry, DbType.Object );

        Assertion.All(
                sut.Name.TestEquals( "FOO" ),
                sut.Value.TestEquals( MySqlDbType.Geometry ),
                sut.DbType.TestEquals( DbType.Object ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = MySqlDataType.Decimal;
        var result = sut.ToString();
        result.TestEquals( "'DECIMAL(29, 10)' (NewDecimal)" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = MySqlDataType.Decimal;
        var expected = HashCode.Combine( sut.Value, sut.Name );
        var result = sut.GetHashCode();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void MySqlDbTypeConversionOperator_ShouldReturnValue()
    {
        var sut = MySqlDataType.Decimal;
        var result = ( MySqlDbType )sut;
        result.TestEquals( sut.Value ).Go();
    }

    [Theory]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Byte, true )]
    [InlineData( "foo", MySqlDbType.Byte, "bar", MySqlDbType.Byte, false )]
    [InlineData( "foo", MySqlDbType.Byte, "foo", MySqlDbType.Decimal, false )]
    public void Equals_ShouldCompareByValueThenName(string name1, MySqlDbType value1, string name2, MySqlDbType value2, bool expected)
    {
        var result = MySqlDataType.Custom( name1, value1, DbType.Object ).Equals( MySqlDataType.Custom( name2, value2, DbType.Object ) );
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Fact]
    public void CompareTo_ShouldReturnOne_WhenOtherIsNull()
    {
        var result = MySqlDataType.Custom( "foo", MySqlDbType.Byte, DbType.Object ).CompareTo( null );
        result.TestEquals( 1 ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
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
        result.TestEquals( expected ).Go();
    }
}
