using System.Data;
using System.Linq;
using LfrlAnvil.Sql.Extensions;
using NpgsqlTypes;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProvider _sut =
        new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );

    [Fact]
    public void GetByDataType_ShouldReturnBoolForBoolean()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Boolean );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Boolean );
            result.DefaultValue.GetValue().Should().Be( false );
            result.RuntimeType.Should().Be( typeof( bool ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt16ForInt2()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int2 );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int2 );
            result.DefaultValue.GetValue().Should().Be( ( short )0 );
            result.RuntimeType.Should().Be( typeof( short ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnUInt32ForInt4()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int4 );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int4 );
            result.DefaultValue.GetValue().Should().Be( 0 );
            result.RuntimeType.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnInt64ForInt8()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Int8 );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int8 );
            result.DefaultValue.GetValue().Should().Be( ( long )0 );
            result.RuntimeType.Should().Be( typeof( long ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnFloatForFloat4()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Float4 );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Float4 );
            result.DefaultValue.GetValue().Should().Be( 0.0F );
            result.RuntimeType.Should().Be( typeof( float ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDoubleForFloat8()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Float8 );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Float8 );
            result.DefaultValue.GetValue().Should().Be( 0.0 );
            result.RuntimeType.Should().Be( typeof( double ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDecimalForDecimal()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Decimal );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Decimal );
            result.DefaultValue.GetValue().Should().Be( 0m );
            result.RuntimeType.Should().Be( typeof( decimal ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnStringForVarChar()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.VarChar );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.VarChar );
            result.DefaultValue.GetValue().Should().Be( string.Empty );
            result.RuntimeType.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnByteArrayForBytea()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Bytea );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Bytea );
            result.DefaultValue.GetValue().Should().BeSameAs( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( byte[] ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnGuidForUuid()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Uuid );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Uuid );
            result.DefaultValue.GetValue().Should().Be( Guid.Empty );
            result.RuntimeType.Should().Be( typeof( Guid ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateOnlyForDate()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Date );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Date );
            result.DefaultValue.GetValue().Should().Be( DateOnly.FromDateTime( DateTime.UnixEpoch ) );
            result.RuntimeType.Should().Be( typeof( DateOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnTimeOnlyForTime()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Time );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Time );
            result.DefaultValue.GetValue().Should().Be( TimeOnly.MinValue );
            result.RuntimeType.Should().Be( typeof( TimeOnly ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForTimestamp()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Timestamp );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Timestamp );
            result.DefaultValue.GetValue().Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnDateTimeForTimestampTz()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.TimestampTz );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.TimestampTz );
            result.DefaultValue.GetValue().Should().Be( DateTime.UnixEpoch );
            result.RuntimeType.Should().Be( typeof( DateTime ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnObjectForUnknown()
    {
        var result = _sut.GetByDataType( PostgreSqlDataType.Custom( "NULL", NpgsqlDbType.Unknown, DbType.Object ) );

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Bytea );
            result.DefaultValue.GetValue().Should().BeEquivalentTo( Array.Empty<byte>() );
            result.RuntimeType.Should().Be( typeof( object ) );
        }
    }

    [Fact]
    public void GetByDataType_ShouldReturnCustomDecimalForCustomNumeric()
    {
        var type = PostgreSqlDataType.Custom( "DECIMAL(10, 0)", NpgsqlDbType.Numeric, DbType.Decimal );
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
    [InlineData( NpgsqlDbType.Text )]
    [InlineData( NpgsqlDbType.Char )]
    [InlineData( NpgsqlDbType.Varchar )]
    public void GetByDataType_ShouldReturnCustomStringForCustomChar(NpgsqlDbType dbType)
    {
        var type = PostgreSqlDataType.Custom( "STRING", dbType, DbType.String );
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

    [Fact]
    public void GetTypeDefinitions_ShouldReturnAllTypeDefinitionsRegisteredByRuntimeType()
    {
        var result = _sut.GetTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 22 );
            result.Select( t => (t.DataType, t.RuntimeType) )
                .Should()
                .BeEquivalentTo(
                    (PostgreSqlDataType.Boolean, typeof( bool )),
                    (PostgreSqlDataType.Int2, typeof( short )),
                    (PostgreSqlDataType.Int4, typeof( int )),
                    (PostgreSqlDataType.Int8, typeof( long )),
                    (PostgreSqlDataType.Float4, typeof( float )),
                    (PostgreSqlDataType.Float8, typeof( double )),
                    (PostgreSqlDataType.Decimal, typeof( decimal )),
                    (PostgreSqlDataType.VarChar, typeof( string )),
                    (PostgreSqlDataType.Bytea, typeof( byte[] )),
                    (PostgreSqlDataType.Uuid, typeof( Guid )),
                    (PostgreSqlDataType.Date, typeof( DateOnly )),
                    (PostgreSqlDataType.Time, typeof( TimeOnly )),
                    (PostgreSqlDataType.Timestamp, typeof( DateTime )),
                    (PostgreSqlDataType.CreateVarChar( 1 ), typeof( char )),
                    (PostgreSqlDataType.CreateVarChar( 33 ), typeof( DateTimeOffset )),
                    (PostgreSqlDataType.Int2, typeof( sbyte )),
                    (PostgreSqlDataType.Int8, typeof( TimeSpan )),
                    (PostgreSqlDataType.Int2, typeof( byte )),
                    (PostgreSqlDataType.Int4, typeof( ushort )),
                    (PostgreSqlDataType.Int8, typeof( uint )),
                    (PostgreSqlDataType.Int8, typeof( ulong )),
                    (PostgreSqlDataType.Bytea, typeof( object )) );
        }
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = _sut.GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 14 );
            result.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    PostgreSqlDataType.Boolean,
                    PostgreSqlDataType.Int2,
                    PostgreSqlDataType.Int4,
                    PostgreSqlDataType.Int8,
                    PostgreSqlDataType.Float4,
                    PostgreSqlDataType.Float8,
                    PostgreSqlDataType.Decimal,
                    PostgreSqlDataType.VarChar,
                    PostgreSqlDataType.Bytea,
                    PostgreSqlDataType.Uuid,
                    PostgreSqlDataType.Date,
                    PostgreSqlDataType.Time,
                    PostgreSqlDataType.Timestamp,
                    PostgreSqlDataType.TimestampTz );
        }
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int8 );
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
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int4 );
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
            result.DataType.Should().BeSameAs( PostgreSqlDataType.Int4 );
            result.DefaultValue.Value.Should().Be( default( EmptyEnum ) );
            result.RuntimeType.Should().Be( typeof( EmptyEnum ) );
        }
    }

    public enum EmptyEnum { }

    public enum EnumWithoutDefault
    {
        A = 5,
        B = 10,
        C = 20
    }

    public enum EnumWithDefault : long
    {
        A = -1,
        B = 0,
        C = 1
    }
}
