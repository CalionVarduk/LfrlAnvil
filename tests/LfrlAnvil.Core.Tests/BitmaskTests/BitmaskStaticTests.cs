using LfrlAnvil.Functional;

namespace LfrlAnvil.Tests.BitmaskTests;

public class BitmaskStaticTests
{
    [Fact]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsNull()
    {
        var result = Bitmask.GetUnderlyingType( null );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( int ) )]
    [InlineData( typeof( IEquatable<int> ) )]
    [InlineData( typeof( IEquatable<> ) )]
    public void GetUnderlyingType_ShouldReturnNull_WhenTypeIsIncorrect(Type type)
    {
        var result = Bitmask.GetUnderlyingType( type );
        result.Should().BeNull();
    }

    [Theory]
    [InlineData( typeof( Bitmask<int> ), typeof( int ) )]
    [InlineData( typeof( Bitmask<decimal> ), typeof( decimal ) )]
    [InlineData( typeof( Bitmask<double> ), typeof( double ) )]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrect(Type type, Type expected)
    {
        var result = Bitmask.GetUnderlyingType( type );
        result.Should().Be( expected );
    }

    [Fact]
    public void GetUnderlyingType_ShouldReturnCorrectType_WhenTypeIsCorrectAndOpen()
    {
        var expected = typeof( Bitmask<> ).GetGenericArguments()[0];
        var result = Bitmask.GetUnderlyingType( typeof( Bitmask<> ) );
        result.Should().Be( expected );
    }

    [Fact]
    public void StaticCtor_ShouldThrowTypeInitializationException_WhenAttemptingToUseEnumWithoutFlagsAttribute()
    {
        var action = Lambda.Of( () => Bitmask<EnumWithoutFlagsAttribute>.BitCount );
        action.Should().ThrowExactly<TypeInitializationException>();
    }

    [Fact]
    public void StaticCtor_ShouldThrowTypeInitializationException_WhenAttemptingToUseEnumWithoutZeroValueMember()
    {
        var action = Lambda.Of( () => Bitmask<EnumWithoutZeroValueMember>.BitCount );
        action.Should().ThrowExactly<TypeInitializationException>();
    }

    [Fact]
    public void StaticCtor_ShouldThrowTypeInitializationException_WhenAttemptingToUseStructThatDoesntSupportConversionToUInt64()
    {
        var action = Lambda.Of( () => Bitmask<InvalidStructA>.BitCount );
        action.Should().ThrowExactly<TypeInitializationException>();
    }

    [Fact]
    public void StaticCtor_ShouldThrowTypeInitializationException_WhenAttemptingToUseStructThatDoesntSupportConversionFromUInt64()
    {
        var action = Lambda.Of( () => Bitmask<InvalidStructB>.BitCount );
        action.Should().ThrowExactly<TypeInitializationException>();
    }
}

public enum EnumWithoutFlagsAttribute { A = 0 }

[Flags]
public enum EnumWithoutZeroValueMember { A = 1 }

public readonly struct InvalidStructA : IConvertible, IComparable
{
    public TypeCode GetTypeCode()
    {
        return TypeCode.Empty;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return false;
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return 0;
    }

    public char ToChar(IFormatProvider? provider)
    {
        return '\0';
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return DateTime.UnixEpoch;
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return 0;
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return 0;
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return 0;
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return 0;
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return 0;
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return 0;
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return 0;
    }

    public string ToString(IFormatProvider? provider)
    {
        return string.Empty;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return 0;
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return 0;
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return 0;
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return 0;
    }

    public int CompareTo(object? obj)
    {
        return 0;
    }

    public static implicit operator ulong(InvalidStructA a)
    {
        return 0;
    }
}

public readonly struct InvalidStructB : IConvertible, IComparable
{
    public TypeCode GetTypeCode()
    {
        return TypeCode.Empty;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return false;
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return 0;
    }

    public char ToChar(IFormatProvider? provider)
    {
        return '\0';
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return DateTime.UnixEpoch;
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return 0;
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return 0;
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return 0;
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return 0;
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return 0;
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return 0;
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return 0;
    }

    public string ToString(IFormatProvider? provider)
    {
        return string.Empty;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return 0;
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return 0;
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return 0;
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return 0;
    }

    public int CompareTo(object? obj)
    {
        return 0;
    }

    public static implicit operator InvalidStructB(ulong a)
    {
        return new InvalidStructB();
    }
}
