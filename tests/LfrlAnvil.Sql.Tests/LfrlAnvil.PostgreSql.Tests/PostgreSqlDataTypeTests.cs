using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDataTypeTests : TestsBase
{
    [Fact]
    public void Boolean_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Boolean;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BOOLEAN" );
            sut.Value.Should().Be( NpgsqlDbType.Boolean );
            sut.DbType.Should().Be( DbType.Boolean );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Int2_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int2;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INT2" );
            sut.Value.Should().Be( NpgsqlDbType.Smallint );
            sut.DbType.Should().Be( DbType.Int16 );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Int4_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int4;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INT4" );
            sut.Value.Should().Be( NpgsqlDbType.Integer );
            sut.DbType.Should().Be( DbType.Int32 );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Int8_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Int8;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "INT8" );
            sut.Value.Should().Be( NpgsqlDbType.Bigint );
            sut.DbType.Should().Be( DbType.Int64 );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Float4_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Float4;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "FLOAT4" );
            sut.Value.Should().Be( NpgsqlDbType.Real );
            sut.DbType.Should().Be( DbType.Single );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Float8_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Float8;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "FLOAT8" );
            sut.Value.Should().Be( NpgsqlDbType.Double );
            sut.DbType.Should().Be( DbType.Double );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Decimal_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Decimal;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DECIMAL(29, 10)" );
            sut.Value.Should().Be( NpgsqlDbType.Numeric );
            sut.DbType.Should().Be( DbType.Decimal );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 29, 10 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo(
                    new SqlDataTypeParameter( "PRECISION", Bounds.Create( 0, 1000 ) ),
                    new SqlDataTypeParameter( "SCALE", Bounds.Create( -1000, 1000 ) ) );
        }
    }

    [Fact]
    public void VarChar_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.VarChar;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "VARCHAR" );
            sut.Value.Should().Be( NpgsqlDbType.Varchar );
            sut.DbType.Should().Be( DbType.String );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( 10485760 );
            sut.ParameterDefinitions.ToArray()
                .Should()
                .BeSequentiallyEqualTo( new SqlDataTypeParameter( "MAX_LENGTH", Bounds.Create( 0, 10485760 ) ) );
        }
    }

    [Fact]
    public void Uuid_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Uuid;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "UUID" );
            sut.Value.Should().Be( NpgsqlDbType.Uuid );
            sut.DbType.Should().Be( DbType.Guid );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Bytea_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Bytea;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "BYTEA" );
            sut.Value.Should().Be( NpgsqlDbType.Bytea );
            sut.DbType.Should().Be( DbType.Binary );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Date_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Date;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "DATE" );
            sut.Value.Should().Be( NpgsqlDbType.Date );
            sut.DbType.Should().Be( DbType.Date );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Time_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Time;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TIME" );
            sut.Value.Should().Be( NpgsqlDbType.Time );
            sut.DbType.Should().Be( DbType.Time );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void Timestamp_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.Timestamp;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TIMESTAMP" );
            sut.Value.Should().Be( NpgsqlDbType.Timestamp );
            sut.DbType.Should().Be( DbType.DateTime2 );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void TimestampTz_ShouldHaveCorrectProperties()
    {
        var sut = PostgreSqlDataType.TimestampTz;

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "TIMESTAMPTZ" );
            sut.Value.Should().Be( NpgsqlDbType.TimestampTz );
            sut.DbType.Should().Be( DbType.DateTime );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void CreateDecimal_ShouldReturnStaticDecimal_WhenPrecisionAndScaleAreDefault()
    {
        var sut = PostgreSqlDataType.CreateDecimal( 29, 10 );
        sut.Should().BeSameAs( PostgreSqlDataType.Decimal );
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
    public void CreateDecimal_ShouldThrowSqlDataTypeException_WhenPrecisionOrScaleAreInvalid(int precision, int scale)
    {
        var action = Lambda.Of( () => PostgreSqlDataType.CreateDecimal( precision, scale ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void CreateVarChar_ShouldReturnStaticVarChar_WhenMaxLengthIsTooLarge()
    {
        var sut = PostgreSqlDataType.CreateVarChar( 10485761 );
        sut.Should().BeSameAs( PostgreSqlDataType.VarChar );
    }

    [Theory]
    [InlineData( 0, "VARCHAR(0)" )]
    [InlineData( 500, "VARCHAR(500)" )]
    [InlineData( 10485760, "VARCHAR(10485760)" )]
    public void CreateVarChar_ShouldReturnNewVarChar_WhenMaxLengthIsNotDefault(int maxLength, string expected)
    {
        var sut = PostgreSqlDataType.CreateVarChar( maxLength );

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
    public void CreateVarChar_ShouldThrowSqlDataTypeException_WhenMaxLengthIsLessThanZero()
    {
        var action = Lambda.Of( () => PostgreSqlDataType.CreateVarChar( -1 ) );
        action.Should().ThrowExactly<SqlDataTypeException>();
    }

    [Fact]
    public void Custom_ShouldCreateNewType()
    {
        var sut = PostgreSqlDataType.Custom( "FOO", NpgsqlDbType.Geometry, DbType.Object );

        using ( new AssertionScope() )
        {
            sut.Name.Should().Be( "FOO" );
            sut.Value.Should().Be( NpgsqlDbType.Geometry );
            sut.DbType.Should().Be( DbType.Object );
            sut.Dialect.Should().BeSameAs( PostgreSqlDialect.Instance );
            sut.Parameters.ToArray().Should().BeEmpty();
            sut.ParameterDefinitions.ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDataType.Decimal;
        var result = sut.ToString();
        result.Should().Be( "'DECIMAL(29, 10)' (Numeric)" );
    }

    [Fact]
    public void GetHashCode_ShouldReturnCorrectResult()
    {
        var sut = PostgreSqlDataType.Decimal;
        var expected = HashCode.Combine( sut.Value, sut.Name );
        var result = sut.GetHashCode();
        result.Should().Be( expected );
    }

    [Fact]
    public void NpgsqlDbTypeConversionOperator_ShouldReturnValue()
    {
        var sut = PostgreSqlDataType.Decimal;
        var result = (NpgsqlDbType)sut;
        result.Should().Be( sut.Value );
    }

    [Theory]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Uuid, true )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "bar", NpgsqlDbType.Uuid, false )]
    [InlineData( "foo", NpgsqlDbType.Uuid, "foo", NpgsqlDbType.Smallint, false )]
    public void Equals_ShouldCompareByValueThenName(string name1, NpgsqlDbType value1, string name2, NpgsqlDbType value2, bool expected)
    {
        var result = PostgreSqlDataType.Custom( name1, value1, DbType.Object )
            .Equals( PostgreSqlDataType.Custom( name2, value2, DbType.Object ) );

        result.Should().Be( expected );
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
        result.Should().Be( expected );
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
        result.Should().Be( expected );
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

        Math.Sign( result ).Should().Be( expectedSign );
    }

    [Fact]
    public void CompareTo_ShouldReturnOne_WhenOtherIsNull()
    {
        var result = PostgreSqlDataType.Custom( "foo", NpgsqlDbType.Uuid, DbType.Object ).CompareTo( null );
        result.Should().Be( 1 );
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
        result.Should().Be( expected );
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
        result.Should().Be( expected );
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
        result.Should().Be( expected );
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
        result.Should().Be( expected );
    }
}
