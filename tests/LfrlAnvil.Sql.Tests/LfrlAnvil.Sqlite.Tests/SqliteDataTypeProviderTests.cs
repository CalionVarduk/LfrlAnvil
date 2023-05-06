using LfrlAnvil.Functional;
using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteDataTypeProviderTests : TestsBase
{
    private readonly ISqlDataTypeProvider _sut = new SqliteDataTypeProvider();

    [Fact]
    public void GetBool_ShouldReturnInteger()
    {
        var result = _sut.GetBool();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt8_ShouldReturnInteger()
    {
        var result = _sut.GetInt8();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt16_ShouldReturnInteger()
    {
        var result = _sut.GetInt16();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt32_ShouldReturnInteger()
    {
        var result = _sut.GetInt32();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetInt64_ShouldReturnInteger()
    {
        var result = _sut.GetInt64();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt8_ShouldReturnInteger()
    {
        var result = _sut.GetUInt8();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt16_ShouldReturnInteger()
    {
        var result = _sut.GetUInt16();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt32_ShouldReturnInteger()
    {
        var result = _sut.GetUInt32();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetUInt64_ShouldReturnInteger()
    {
        var result = _sut.GetUInt64();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetFloat_ShouldReturnReal()
    {
        var result = _sut.GetFloat();
        result.Should().BeSameAs( SqliteDataType.Real );
    }

    [Fact]
    public void GetDouble_ShouldReturnReal()
    {
        var result = _sut.GetDouble();
        result.Should().BeSameAs( SqliteDataType.Real );
    }

    [Fact]
    public void GetDecimal_ShouldReturnText()
    {
        var result = _sut.GetDecimal();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Theory]
    [InlineData( 1, 0 )]
    [InlineData( 1, 1 )]
    [InlineData( 10, 0 )]
    [InlineData( 10, 5 )]
    [InlineData( 10, 10 )]
    public void GetDecimal_WithParameters_ShouldReturnText(int precision, int scale)
    {
        var result = _sut.GetDecimal( precision, scale );
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void GetDecimal_WithParameters_ShouldThrowArgumentOutOfRangeException_WhenPrecisionIsLessThanOne(int precision)
    {
        var action = Lambda.Of( () => _sut.GetDecimal( precision, 0 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetDecimal_WithParameters_ShouldThrowArgumentOutOfRangeException_WhenScaleIsLessThanZero()
    {
        var action = Lambda.Of( () => _sut.GetDecimal( 1, -1 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetDecimal_WithParameters_ShouldThrowArgumentOutOfRangeException_WhenScaleIsGreaterThanPrecision()
    {
        var action = Lambda.Of( () => _sut.GetDecimal( 1, 2 ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetGuid_ShouldReturnBlob()
    {
        var result = _sut.GetGuid();
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Fact]
    public void GetString_ShouldReturnText()
    {
        var result = _sut.GetString();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 100 )]
    public void GetString_WithLength_ShouldReturnText(int length)
    {
        var result = _sut.GetString( length );
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void GetString_WithLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanOne(int length)
    {
        var action = Lambda.Of( () => _sut.GetString( length ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetTimestamp_ShouldReturnInteger()
    {
        var result = _sut.GetTimestamp();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetDateTime_ShouldReturnText()
    {
        var result = _sut.GetDateTime();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetTimeSpan_ShouldReturnInteger()
    {
        var result = _sut.GetTimeSpan();
        result.Should().BeSameAs( SqliteDataType.Integer );
    }

    [Fact]
    public void GetDate_ShouldReturnText()
    {
        var result = _sut.GetDate();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetTime_ShouldReturnText()
    {
        var result = _sut.GetTime();
        result.Should().BeSameAs( SqliteDataType.Text );
    }

    [Fact]
    public void GetBinary_ShouldReturnBlob()
    {
        var result = _sut.GetBinary();
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Theory]
    [InlineData( 1 )]
    [InlineData( 10 )]
    [InlineData( 100 )]
    public void GetBinary_WithLength_ShouldReturnBlob(int length)
    {
        var result = _sut.GetBinary( length );
        result.Should().BeSameAs( SqliteDataType.Blob );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( -1 )]
    public void GetBinary_WithLength_ShouldThrowArgumentOutOfRangeException_WhenLengthIsLessThanOne(int length)
    {
        var action = Lambda.Of( () => _sut.GetBinary( length ) );
        action.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetAny_ShouldReturnAny()
    {
        var result = ((SqliteDataTypeProvider)_sut).GetAny();
        result.Should().BeSameAs( SqliteDataType.Any );
    }
}
