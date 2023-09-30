using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Internal.TypeDefinitions;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly ISqlColumnTypeDefinitionProvider _sut = new SqliteColumnTypeDefinitionProvider();

    [Fact]
    public void GetDefaultForDataType_ShouldReturnInt64ForInteger()
    {
        var result = _sut.GetDefaultForDataType( SqliteDataType.Integer );

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.GetValue().Should().Be( 0L );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetDefaultForDataType_ShouldReturnDoubleForReal()
    {
        var result = _sut.GetDefaultForDataType( SqliteDataType.Real );

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Real );
            result.DefaultValue.GetValue().Should().Be( 0.0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetDefaultForDataType_ShouldReturnStringForText()
    {
        var result = _sut.GetDefaultForDataType( SqliteDataType.Text );

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetDefaultForDataType_ShouldReturnByteArrayForBlob()
    {
        var result = _sut.GetDefaultForDataType( SqliteDataType.Blob );

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Blob );
            result.DefaultValue.GetValue().Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetDefaultForDataType_ShouldReturnObjectForAny()
    {
        var result = _sut.GetDefaultForDataType( SqliteDataType.Any );

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Any );
            result.DefaultValue.GetValue().Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( object ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForBool()
    {
        var result = _sut.GetByType<bool>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().BeFalse();
            result.RuntimeType.Should().Be( typeof( bool ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForUInt8()
    {
        var result = _sut.GetByType<byte>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( byte ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForInt8()
    {
        var result = _sut.GetByType<sbyte>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( sbyte ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForUInt16()
    {
        var result = _sut.GetByType<ushort>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( ushort ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForInt16()
    {
        var result = _sut.GetByType<short>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( short ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForUInt32()
    {
        var result = _sut.GetByType<uint>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( uint ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForInt32()
    {
        var result = _sut.GetByType<int>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForUInt64()
    {
        var result = _sut.GetByType<ulong>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( ulong ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForInt64()
    {
        var result = _sut.GetByType<long>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnIntegerBasedDefinitionForTimeSpan()
    {
        var result = _sut.GetByType<TimeSpan>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
            result.DefaultValue.Value.Should().Be( TimeSpan.Zero );
            result.RuntimeType.Should().Be( typeof( TimeSpan ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnRealBasedDefinitionForFloat()
    {
        var result = _sut.GetByType<float>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Real );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( float ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnRealBasedDefinitionForDouble()
    {
        var result = _sut.GetByType<double>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Real );
            result.DefaultValue.Value.Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForDateTime()
    {
        var result = _sut.GetByType<DateTime>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForDateTimeOffset()
    {
        var result = _sut.GetByType<DateTimeOffset>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( DateTimeOffset.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTimeOffset ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForDateOnly()
    {
        var result = _sut.GetByType<DateOnly>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( DateOnly.FromDateTime( DateTime.UnixEpoch ) );
            result.RuntimeType.Should().Be( typeof( DateOnly ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForTimeOnly()
    {
        var result = _sut.GetByType<TimeOnly>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( TimeOnly.MinValue );
            result.RuntimeType.Should().Be( typeof( TimeOnly ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForDecimal()
    {
        var result = _sut.GetByType<decimal>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnTextBasedDefinitionForChar()
    {
        var result = _sut.GetByType<char>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
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
            result.DbType.Should().BeSameAs( SqliteDataType.Text );
            result.DefaultValue.Value.Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnBlobBasedDefinitionForGuid()
    {
        var result = _sut.GetByType<Guid>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Blob );
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
            result.DbType.Should().BeSameAs( SqliteDataType.Blob );
            result.DefaultValue.Value.Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByType_ShouldReturnAnyBasedDefinitionForObject()
    {
        var result = _sut.GetByType<object>();

        using ( new AssertionScope() )
        {
            result.DbType.Should().BeSameAs( SqliteDataType.Any );
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
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
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
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
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
            result.DbType.Should().BeSameAs( SqliteDataType.Integer );
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
    public void GetAll_ShouldReturnAllRegisteredDefinitions()
    {
        var sut = (SqliteColumnTypeDefinitionProvider)_sut;
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
                sut.GetByType<object>() );
    }

    [Fact]
    public void RegisterDefinition_ShouldAddNewTypeDefinition()
    {
        var baseDefinition = _sut.GetByType<string>();
        var definition = baseDefinition.Extend( c => c.Value, new Code( string.Empty ) );
        var result = _sut.RegisterDefinition( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            definition.DbType.Should().BeSameAs( baseDefinition.DbType );
            definition.RuntimeType.Should().Be( typeof( Code ) );
            definition.DefaultValue.Value.Should().Be( new Code( string.Empty ) );
            _sut.GetByType( typeof( Code ) ).Should().BeSameAs( definition );
        }
    }

    [Fact]
    public void RegisterDefinition_ShouldOverrideExistingTypeDefinition()
    {
        var baseDefinition = _sut.GetByType<double>();
        var definition = baseDefinition.Extend( v => (double)v, 1m );
        var result = _sut.RegisterDefinition( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            definition.DbType.Should().BeSameAs( baseDefinition.DbType );
            definition.RuntimeType.Should().Be( typeof( decimal ) );
            definition.DefaultValue.Value.Should().Be( 1m );
            _sut.GetByType( typeof( decimal ) ).Should().BeSameAs( definition );
        }
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowInvalidOperationException_WhenAttemptingToOverrideInt64()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( new SqliteColumnTypeDefinitionInt64() ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowInvalidOperationException_WhenAttemptingToOverrideDouble()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( new SqliteColumnTypeDefinitionDouble() ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowInvalidOperationException_WhenAttemptingToOverrideString()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( new SqliteColumnTypeDefinitionString() ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowInvalidOperationException_WhenAttemptingToOverrideByteArray()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( new SqliteColumnTypeDefinitionByteArray() ) );
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowInvalidOperationException_WhenAttemptingToOverrideObject()
    {
        var action = Lambda.Of(
            () => _sut.RegisterDefinition( new SqliteColumnTypeDefinitionObject( (SqliteColumnTypeDefinitionProvider)_sut ) ) );

        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void RegisterDefinition_ShouldThrowSqliteObjectCastException_WhenTypeDefinitionTypeIsInvalid()
    {
        var action = Lambda.Of( () => _sut.RegisterDefinition( Substitute.For<ISqlColumnTypeDefinition<Code>>() ) );
        action.Should().ThrowExactly<SqliteObjectCastException>();
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
