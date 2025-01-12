using System.Text;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Tests.EncodeableTextTests;

public class EncodeableTextTests : TestsBase
{
    [Fact]
    public void Default_ShouldContainEmptyString()
    {
        var sut = default( EncodeableText );
        using ( new AssertionScope() )
        {
            sut.ByteCount.Should().Be( 0 );
            sut.Value.ToArray().Should().BeEmpty();
            sut.Encoding.Should().BeSameAs( Encoding.UTF8 );
        }
    }

    [Fact]
    public void Create_ShouldReturnValidResult_WhenEncoderSuccessfullyRecoversByteCount()
    {
        var result = EncodeableText.Create( Encoding.Unicode, "foobar" );

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            result.Value.ByteCount.Should().Be( 12 );
            result.Value.Value.ToString().Should().Be( "foobar" );
            result.Value.Encoding.Should().BeSameAs( Encoding.Unicode );
        }
    }

    [Fact]
    public void Create_ShouldReturnInvalidResult_WhenEncoderFailsToRecoverByteCount()
    {
        var result = EncodeableText.Create( new InvalidEncoding(), "foobar" );
        result.Exception.Should().BeOfType<NotSupportedException>();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var sut = EncodeableText.Create( Encoding.UTF8, "foobar" ).Value;
        var result = sut.ToString();
        result.Should().Be( "[Encoding: Unicode (UTF-8), ByteCount: 6] 'foobar'" );
    }

    [Fact]
    public void Encode_ShouldReturnValidResult_WhenEncoderPerformsOperationSuccessfully()
    {
        var target = new byte[10];
        var sut = EncodeableText.Create( Encoding.Unicode, "foo" ).Value;

        var result = sut.Encode( target );

        using ( new AssertionScope() )
        {
            result.Exception.Should().BeNull();
            target.Should().BeSequentiallyEqualTo<byte>( 102, 0, 111, 0, 111, 0, 0, 0, 0, 0 );
        }
    }

    [Fact]
    public void Encode_ShouldReturnInvalidResult_WhenEncoderFailsToEncode()
    {
        var target = new byte[4];
        var sut = EncodeableText.Create( Encoding.Unicode, "foo" ).Value;

        var result = sut.Encode( target );

        result.Exception.Should().BeOfType<ArgumentException>();
    }

    private sealed class InvalidEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotSupportedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            throw new NotSupportedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotSupportedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotSupportedException();
        }
    }
}
