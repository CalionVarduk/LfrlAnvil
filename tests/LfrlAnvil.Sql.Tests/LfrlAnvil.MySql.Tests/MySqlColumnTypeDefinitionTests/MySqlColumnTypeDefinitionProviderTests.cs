using System.Data;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.MySqlColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _sut = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );

    [Fact]
    public void GetByDataType_ShouldReturnBoolForBool()
    {
        var result = _sut.GetByDataType( MySqlDataType.Bool );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Bool );
            result.DefaultValue.GetValue().Should().Be( false );
            result.RuntimeType.Should().Be( typeof( bool ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt8ForTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.TinyInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.TinyInt );
            result.DefaultValue.GetValue().Should().Be( (sbyte)0 );
            result.RuntimeType.Should().Be( typeof( sbyte ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt8ForUnsignedTinyInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedTinyInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedTinyInt );
            result.DefaultValue.GetValue().Should().Be( (byte)0 );
            result.RuntimeType.Should().Be( typeof( byte ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt16ForSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.SmallInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.SmallInt );
            result.DefaultValue.GetValue().Should().Be( (short)0 );
            result.RuntimeType.Should().Be( typeof( short ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt16ForUnsignedSmallInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedSmallInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedSmallInt );
            result.DefaultValue.GetValue().Should().Be( (ushort)0 );
            result.RuntimeType.Should().Be( typeof( ushort ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt32ForInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.Int );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
            result.DefaultValue.GetValue().Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt32ForUnsignedInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedInt );
            result.DefaultValue.GetValue().Should().Be( (uint)0 );
            result.RuntimeType.Should().Be( typeof( uint ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.BigInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.BigInt );
            result.DefaultValue.GetValue().Should().Be( 0L );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt64ForUnsignedBigInt()
    {
        var result = _sut.GetByDataType( MySqlDataType.UnsignedBigInt );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedBigInt );
            result.DefaultValue.GetValue().Should().Be( 0UL );
            result.RuntimeType.Should().Be( typeof( ulong ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnFloatForFloat()
    {
        var result = _sut.GetByDataType( MySqlDataType.Float );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Float );
            result.DefaultValue.GetValue().Should().Be( (float)0.0 );
            result.RuntimeType.Should().Be( typeof( float ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForDouble()
    {
        var result = _sut.GetByDataType( MySqlDataType.Double );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Double );
            result.DefaultValue.GetValue().Should().Be( 0.0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDecimalForDecimal()
    {
        var result = _sut.GetByDataType( MySqlDataType.Decimal );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Decimal );
            result.DefaultValue.GetValue().Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.Char );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Char );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.Binary );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Binary );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForVarChar()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarChar );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.VarChar );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForVarBinary()
    {
        var result = _sut.GetByDataType( MySqlDataType.VarBinary );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.VarBinary );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetByDataType( MySqlDataType.Blob );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetByDataType( MySqlDataType.Text );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Text );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateOnlyForDate()
    {
        var result = _sut.GetByDataType( MySqlDataType.Date );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Date );
            result.DefaultValue.GetValue().Should().Be( DateOnly.FromDateTime( DateTime.UnixEpoch ) );
            result.RuntimeType.Should().Be( typeof( DateOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnTimeOnlyForTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.Time );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Time );
            result.DefaultValue.GetValue().Should().Be( TimeOnly.MinValue );
            result.RuntimeType.Should().Be( typeof( TimeOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForDateTime()
    {
        var result = _sut.GetByDataType( MySqlDataType.DateTime );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.DateTime );
            result.DefaultValue.GetValue().Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnObjectForUnknown()
    {
        var result = _sut.GetByDataType( MySqlDataType.Custom( "NULL", MySqlDbType.Null, DbType.Object ) );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.GetValue().Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( object ) );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.Decimal )]
    [InlineData( MySqlDbType.NewDecimal )]
    public void GetByDataType_ShouldReturnCustomDecimalForCustomDecimal(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "DECIMAL(10, 0)", dbType, DbType.Decimal );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
            secondResult.Should().BeSameAs( result );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.String )]
    [InlineData( MySqlDbType.VarChar )]
    [InlineData( MySqlDbType.VarString )]
    [InlineData( MySqlDbType.TinyText )]
    [InlineData( MySqlDbType.Text )]
    [InlineData( MySqlDbType.MediumText )]
    [InlineData( MySqlDbType.LongText )]
    public void GetByDataType_ShouldReturnCustomStringForCustomText(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "STRING", dbType, DbType.String );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
            secondResult.Should().BeSameAs( result );
        }
    }

    [Theory]
    [InlineData( MySqlDbType.Binary )]
    [InlineData( MySqlDbType.VarBinary )]
    [InlineData( MySqlDbType.TinyBlob )]
    [InlineData( MySqlDbType.Blob )]
    [InlineData( MySqlDbType.MediumBlob )]
    [InlineData( MySqlDbType.LongBlob )]
    public void GetByDataType_ShouldReturnCustomByteArrayForCustomBlob(MySqlDbType dbType)
    {
        var type = MySqlDataType.Custom( "BYTEARRAY", dbType, DbType.Binary );
        var result = _sut.GetByDataType( type );
        var secondResult = _sut.GetByDataType( type );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( type );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
            secondResult.Should().BeSameAs( result );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBoolBasedDefinitionForBool()
    {
        var result = _sut.GetByType<bool>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Bool );
            result.DefaultValue.Value.Should().BeFalse();
            result.RuntimeType.Should().Be( typeof( bool ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnUnsignedTinyIntBasedDefinitionForUInt8()
    {
        var result = _sut.GetByType<byte>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedTinyInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( byte ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTinyIntBasedDefinitionForInt8()
    {
        var result = _sut.GetByType<sbyte>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.TinyInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( sbyte ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnUnsignedSmallIntBasedDefinitionForUInt16()
    {
        var result = _sut.GetByType<ushort>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedSmallInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( ushort ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnSmallIntBasedDefinitionForInt16()
    {
        var result = _sut.GetByType<short>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.SmallInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( short ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnUnsignedIntBasedDefinitionForUInt32()
    {
        var result = _sut.GetByType<uint>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( uint ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntBasedDefinitionForInt32()
    {
        var result = _sut.GetByType<int>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnUnsignedBigIntBasedDefinitionForUInt64()
    {
        var result = _sut.GetByType<ulong>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.UnsignedBigInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( ulong ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBigIntBasedDefinitionForInt64()
    {
        var result = _sut.GetByType<long>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.BigInt );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBigIntBasedDefinitionForTimeSpan()
    {
        var result = _sut.GetByType<TimeSpan>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.BigInt );
            result.DefaultValue.Value.Should().Be( TimeSpan.Zero );
            result.RuntimeType.Should().Be( typeof( TimeSpan ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnFloatBasedDefinitionForFloat()
    {
        var result = _sut.GetByType<float>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Float );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( float ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnDoubleBasedDefinitionForDouble()
    {
        var result = _sut.GetByType<double>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Double );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnDateTimeBasedDefinitionForDateTime()
    {
        var result = _sut.GetByType<DateTime>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.DateTime );
            result.DefaultValue.Value.Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnCharBasedDefinitionForDateTimeOffset()
    {
        var result = _sut.GetByType<DateTimeOffset>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeEquivalentTo( MySqlDataType.CreateChar( 33 ) );
            result.DefaultValue.Value.Should().Be( DateTimeOffset.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTimeOffset ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnDateBasedDefinitionForDateOnly()
    {
        var result = _sut.GetByType<DateOnly>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Date );
            result.DefaultValue.Value.Should().Be( DateOnly.FromDateTime( DateTime.UnixEpoch ) );
            result.RuntimeType.Should().Be( typeof( DateOnly ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTimeBasedDefinitionForTimeOnly()
    {
        var result = _sut.GetByType<TimeOnly>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Time );
            result.DefaultValue.Value.Should().Be( TimeOnly.MinValue );
            result.RuntimeType.Should().Be( typeof( TimeOnly ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnDecimalBasedDefinitionForDecimal()
    {
        var result = _sut.GetByType<decimal>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Decimal );
            result.DefaultValue.Value.Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnCharBasedDefinitionForChar()
    {
        var result = _sut.GetByType<char>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeEquivalentTo( MySqlDataType.CreateChar( 1 ) );
            result.DefaultValue.Value.Should().Be( '0' );
            result.RuntimeType.Should().Be( typeof( char ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForString()
    {
        var result = _sut.GetByType<string>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Text );
            result.DefaultValue.Value.Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBinaryBasedDefinitionForGuid()
    {
        var result = _sut.GetByType<Guid>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeEquivalentTo( MySqlDataType.CreateBinary( 16 ) );
            result.DefaultValue.Value.Should().Be( Guid.Empty );
            result.RuntimeType.Should().Be( typeof( Guid ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBlobBasedDefinitionForByteArray()
    {
        var result = _sut.GetByType<byte[]>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.Value.Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnObjectBasedDefinitionForObject()
    {
        var result = _sut.GetByType<object>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Blob );
            result.DefaultValue.Value.Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( object ) );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.TinyInt );
            result.DefaultValue.Value.Should().Be( EnumWithDefault.B );
            result.RuntimeType.Should().Be( typeof( EnumWithDefault ) );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.SmallInt );
            result.DefaultValue.Value.Should().Be( EnumWithoutDefault.A );
            result.RuntimeType.Should().Be( typeof( EnumWithoutDefault ) );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( MySqlDataType.Int );
            result.DefaultValue.Value.Should().Be( default( EmptyEnum ) );
            result.RuntimeType.Should().Be( typeof( EmptyEnum ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnPreviouslyCreatedEnumDefinition_WhenCalledMoreThanOnce()
    {
        var expected = _sut.GetByType<EnumWithDefault>();
        var result = _sut.GetByType<EnumWithDefault>();
        expected.Should().BeSameAs( result );
    }

    [Fact]
    public void RegisterDefinition_ShouldAddNewTypeDefinition()
    {
        var definition = new CodeTypeDefinition();
        var result = _sut.RegisterDefinition( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.GetByType( typeof( Code ) ).Should().BeSameAs( definition );
        }
    }

    [Fact]
    public void RegisterDefinition_ShouldOverrideExistingTypeDefinition()
    {
        var previousDefinition = _sut.GetByType<long>();
        var definition = new NewInt64TypeDefinition();
        var result = _sut.RegisterDefinition( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.GetByType( typeof( long ) ).Should().BeSameAs( definition );
            _sut.GetByDataType( MySqlDataType.BigInt ).Should().BeSameAs( previousDefinition );
        }
    }

    [Fact]
    public void GetAll_ShouldReturnAllRegisteredDefinitions()
    {
        var sut = (MySqlColumnTypeDefinitionProvider)_sut;
        var result = _sut.GetAll();

        result.Should()
            .BeEquivalentTo(
                sut.GetByType<bool>(),
                sut.GetByType<byte>(),
                sut.GetByType<sbyte>(),
                sut.GetByType<ushort>(),
                sut.GetByType<short>(),
                sut.GetByType<uint>(),
                sut.GetByType<int>(),
                sut.GetByType<ulong>(),
                sut.GetByType<long>(),
                sut.GetByType<TimeSpan>(),
                sut.GetByType<float>(),
                sut.GetByType<double>(),
                sut.GetByType<DateTime>(),
                sut.GetByType<DateTimeOffset>(),
                sut.GetByType<DateOnly>(),
                sut.GetByType<TimeOnly>(),
                sut.GetByType<decimal>(),
                sut.GetByType<char>(),
                sut.GetByType<string>(),
                sut.GetByType<Guid>(),
                sut.GetByType<byte[]>(),
                sut.GetByType<object>(),
                sut.GetByDataType( MySqlDataType.Char ),
                sut.GetByDataType( MySqlDataType.VarChar ),
                sut.GetByDataType( MySqlDataType.Binary ),
                sut.GetByDataType( MySqlDataType.VarBinary ) );
    }

    [Fact]
    public void GetAll_ShouldReturnAllRegisteredDefinitions_WhenBaseTypeDefinitionWasChanged()
    {
        var baseDefinition = _sut.GetByType<long>();
        _sut.RegisterDefinition( new NewInt64TypeDefinition() );
        var sut = (MySqlColumnTypeDefinitionProvider)_sut;
        var result = _sut.GetAll();

        result.Should()
            .BeEquivalentTo(
                sut.GetByType<bool>(),
                sut.GetByType<byte>(),
                sut.GetByType<sbyte>(),
                sut.GetByType<ushort>(),
                sut.GetByType<short>(),
                sut.GetByType<uint>(),
                sut.GetByType<int>(),
                sut.GetByType<ulong>(),
                sut.GetByType<long>(),
                sut.GetByType<TimeSpan>(),
                sut.GetByType<float>(),
                sut.GetByType<double>(),
                sut.GetByType<DateTime>(),
                sut.GetByType<DateTimeOffset>(),
                sut.GetByType<DateOnly>(),
                sut.GetByType<TimeOnly>(),
                sut.GetByType<decimal>(),
                sut.GetByType<char>(),
                sut.GetByType<string>(),
                sut.GetByType<Guid>(),
                sut.GetByType<byte[]>(),
                sut.GetByType<object>(),
                sut.GetByDataType( MySqlDataType.Char ),
                sut.GetByDataType( MySqlDataType.VarChar ),
                sut.GetByDataType( MySqlDataType.Binary ),
                sut.GetByDataType( MySqlDataType.VarBinary ),
                baseDefinition );
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowSqlObjectCastException_WhenTypeDefinitionTypeIsInvalid()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( Substitute.For<ISqlColumnTypeDefinition<Code>>() ) );
        action.Should().ThrowExactly<SqlObjectCastException>();
    }

    private sealed class NewInt64TypeDefinition : MySqlColumnTypeDefinition<long>
    {
        internal NewInt64TypeDefinition()
            : base( MySqlDataType.Text, 0L, static (reader, ordinal) => long.Parse( reader.GetString( ordinal ) ) ) { }

        public override string ToDbLiteral(long value)
        {
            return $"'{value}'";
        }

        public override object ToParameterValue(long value)
        {
            return value.ToString();
        }
    }

    private sealed class CodeTypeDefinition : MySqlColumnTypeDefinition<Code>
    {
        internal CodeTypeDefinition()
            : base(
                MySqlDataType.Text,
                new Code( string.Empty ),
                static (reader, ordinal) => new Code( reader.GetString( ordinal ) ) ) { }

        public override string ToDbLiteral(Code value)
        {
            return $"'{value.Value}'";
        }

        public override object ToParameterValue(Code value)
        {
            return value.Value;
        }
    }
}

public readonly record struct Code(string Value);

public enum EmptyEnum { }

public enum EnumWithoutDefault : short
{
    A = 5,
    B = 10,
    C = 20
}

public enum EnumWithDefault : sbyte
{
    A = -1,
    B = 0,
    C = 1
}
