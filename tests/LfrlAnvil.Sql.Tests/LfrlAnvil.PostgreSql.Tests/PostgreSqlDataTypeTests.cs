using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDataTypeTests : TestsBase
{
    [Fact]
    public void Boolean_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Boolean;

        Assertion.All(
                sut.Name.TestEquals( "BOOLEAN" ),
                sut.Value.TestEquals( NpgsqlDbType.Boolean ),
                sut.DbType.TestEquals( DbType.Boolean ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Int2_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int2;

        Assertion.All(
                sut.Name.TestEquals( "INT2" ),
                sut.Value.TestEquals( NpgsqlDbType.Smallint ),
                sut.DbType.TestEquals( DbType.Int16 ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Int4_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int4;

        Assertion.All(
                sut.Name.TestEquals( "INT4" ),
                sut.Value.TestEquals( NpgsqlDbType.Integer ),
                sut.DbType.TestEquals( DbType.Int32 ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Int8_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int8;

        Assertion.All(
                sut.Name.TestEquals( "INT8" ),
                sut.Value.TestEquals( NpgsqlDbType.Bigint ),
                sut.DbType.TestEquals( DbType.Int64 ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Float4_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Float4;

        Assertion.All(
                sut.Name.TestEquals( "FLOAT4" ),
                sut.Value.TestEquals( NpgsqlDbType.Real ),
                sut.DbType.TestEquals( DbType.Single ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Float8_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Float8;

        Assertion.All(
                sut.Name.TestEquals( "FLOAT8" ),
                sut.Value.TestEquals( NpgsqlDbType.Double ),
                sut.DbType.TestEquals( DbType.Double ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Decimal_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Decimal;

        Assertion.All(
                sut.Name.TestEquals( "DECIMAL(29, 10)" ),
                sut.Value.TestEquals( NpgsqlDbType.Numeric ),
                sut.DbType.TestEquals( DbType.Decimal ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 29, 10 ] ),
                sut.ParameterDefinitions.ToArray()
                    .TestSequence(
                    [
                        new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 1000 ) ),
                        new SqlDataTypeParameter( "SCALE", Bounds.Create( -1000, 1000 ) )
                    ] ) )
            .Go();
    }

    [Fact]
    public void VarChar_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.VarChar;

        Assertion.All(
                sut.Name.TestEquals( "VARCHAR" ),
                sut.Value.TestEquals( NpgsqlDbType.Varchar ),
                sut.DbType.TestEquals( DbType.String ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestSequence( [ 10485760 ] ),
                sut.ParameterDefinitions.ToArray()
                    .TestSequence( [ new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 10485760 ) ) ] ) )
            .Go();
    }

    [Fact]
    public void Uuid_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Uuid;

        Assertion.All(
                sut.Name.TestEquals( "UUID" ),
                sut.Value.TestEquals( NpgsqlDbType.Uuid ),
                sut.DbType.TestEquals( DbType.Guid ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Bytea_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Bytea;

        Assertion.All(
                sut.Name.TestEquals( "BYTEA" ),
                sut.Value.TestEquals( NpgsqlDbType.Bytea ),
                sut.DbType.TestEquals( DbType.Binary ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Date_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Date;

        Assertion.All(
                sut.Name.TestEquals( "DATE" ),
                sut.Value.TestEquals( NpgsqlDbType.Date ),
                sut.DbType.TestEquals( DbType.Date ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Time_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Time;

        Assertion.All(
                sut.Name.TestEquals( "TIME" ),
                sut.Value.TestEquals( NpgsqlDbType.Time ),
                sut.DbType.TestEquals( DbType.Time ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void Timestamp_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Timestamp;

        Assertion.All(
                sut.Name.TestEquals( "TIMESTAMP" ),
                sut.Value.TestEquals( NpgsqlDbType.Timestamp ),
                sut.DbType.TestEquals( DbType.DateTime2 ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void TimestampTz_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.TimestampTz;

        Assertion.All(
                sut.Name.TestEquals( "TIMESTAMPTZ" ),
                sut.Value.TestEquals( NpgsqlDbType.TimestampTz ),
                sut.DbType.TestEquals( DbType.DateTime ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void CreateDecimal_ShouldReturnStaticDecimal_WhenPrecisionAndScaleAreDefault()
    {
        var sut = PostgreSqlDataType.CreateDecimal( 29, 10 );
        sut.TestRefEquals( PostgreSqlDataType.Decimal ).Go();
    }

    [Theory]
    [InlineData( 10, 0, "DECIMAL(10, 0)" )]
    [InlineData( 29, 11, "DECIMAL(29, 11)" )]
    [InlineData( 28, 10, "DECIMAL(28, 10)" )]
    [InlineData( 1000, 1000, "DECIMAL(1000, 1000)" )]
    [InlineData( 1000, -1000, "DECIMAL(1000, -1000)" )]
    public void CreateDecimal_ShouldReturnNewDecimal_WhenPrecisionOrScaleAreNotDefault(int precision, int scale, string expected)
    {
        var sut = PostgreSqlDataType.CreateDecimal( precision, scale );

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
    public void CreateDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => PostgreSqlDataType.CreateDecimal( precision, scale ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void CreateVarChar_ShouldReturnStaticVarChar_WhenMaxLengthIsTooLarge()
    {
        var sut = PostgreSqlDataType.CreateVarChar( 10485761 );
        sut.TestRefEquals( PostgreSqlDataType.VarChar ).Go();
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void CreateVarChar_ShouldReturnNewVarChar_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = PostgreSqlDataType.CreateVarChar( maxLength );

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
    public void CreateVarChar_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => PostgreSqlDataType.CreateVarChar( -1 ) );
        action.Test( exc => exc.TestType().Exact<SqlDataTypeException>() ).Go();
    }

    [Fact]
    public void Custom_ShouldCreateNewType()
    {
        var sut = PostgreSqlDataType.Custom( "FOO", NpgsqlDbType.Geometry, DbType.Object );

        Assertion.All(
                sut.Name.TestEquals( "FOO" ),
                sut.Value.TestEquals( NpgsqlDbType.Geometry ),
                sut.DbType.TestEquals( DbType.Object ),
                sut.Dialect.TestRefEquals( PostgreSqlDialect.Instance ),
                sut.Parameters.ToArray().TestEmpty(),
                sut.ParameterDefinitions.ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDataType.Decimal;
        var result = sut.ToString();
        result.TestEquals( "'DECIMAL(29, 10)' (Numeric)" ).Go();
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDataType.Decimal;
        var expected = HashCode.Combine( sut.Value, sut.Name );
        var result = sut.GetHashCode();
        result.TestEquals( expected ).Go();
    }

    [Fact]
    public void NpgsqlDbTypeConversionOperator_ShouldReturnValue()
    {
        var sut = PostgreSqlDataType.Decimal;
        var result = ( NpgsqlDbType )sut;
        result.TestEquals( sut.Value ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, false )]
    public void Equals_ShouldCompareByValueThenName(string name1, NpgsqlDbType value1, string name2, NpgsqlDbType value2, bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object )
            .Equals( PostgreSqlDataType.Custom( name2, value2, DbType.Object ) );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, false )]
    public void EqualityOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) == PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, true )]
    public void InequalityOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) != PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, 0 )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, 1 )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, 1 )]
    [InlineData( "bar", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, -1 )]
    [InlineData( "foo", NpgsqlDbType.Smallint, "foo", NpgsqlDbType.Uuid, -1 )]
    public void CompareTo_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        int expectedSign)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object )
            .CompareTo( PostgreSqlDataType.Custom( name2, value2, DbType.Object ) );

        Math.Sign( result ).TestEquals( expectedSign ).Go();
    }

    [Fact]
    public void CompareTo_ShouldReturnOne_WhenOtherIsNull()
    {
        var result = PostgreSqlDataType.Custom( "foo", NpgsqlDbType.Uuid, DbType.Object ).CompareTo( null );
        result.TestEquals( 1 ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, true )]
    [InlineData( "bar", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Smallint, "foo", NpgsqlDbType.Uuid, false )]
    public void GreaterThanOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) > PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, true )]
    [InlineData( "bar", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Smallint, "foo", NpgsqlDbType.Uuid, false )]
    public void GreaterThanOrEqualToOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) >= PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, false )]
    [InlineData( "bar", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Smallint, "foo", NpgsqlDbType.Uuid, true )]
    public void LessThanOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) < PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, false )]
    [InlineData( "bar", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Smallint, "foo", NpgsqlDbType.Uuid, true )]
    public void LessThanOrEqualToOperator_ShouldCompareByValueThenName(
        string name1,
        NpgsqlDbType value1,
        string name2,
        NpgsqlDbType value2,
        bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object ) <= PostgreSqlDataType.Custom( name2, value2, DbType.Object );
        result.TestEquals( expected ).Go();
    }
}
