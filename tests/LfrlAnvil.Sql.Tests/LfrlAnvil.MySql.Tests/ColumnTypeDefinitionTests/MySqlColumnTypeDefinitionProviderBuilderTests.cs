using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.MySql.Tests.ColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionProviderBuilderTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProviderBuilder _sut = new MySqlColumnTypeDefinitionProviderBuilder();

    [Fact]
    public void Register_ShouldAddNewDefinition()
    {
        var definition = new TypeDefinition<StringBuilder>( MySqlDataType.Text, new StringBuilder() );
        var result = _sut.Register( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.Contains( definition.RuntimeType ).Should().BeTrue();
        }
    }

    [Fact]
    public void Register_ShouldOverrideExistingDefinition()
    {
        var definition = new TypeDefinition<int>( MySqlDataType.Int, 0 );
        var result = _sut.Register( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.Contains( definition.RuntimeType ).Should().BeTrue();
        }
    }

    [Fact]
    public void Build_ShouldInvokeImplementation()
    {
        var definition = new TypeDefinition<StringBuilder>( MySqlDataType.Text, new StringBuilder() );
        _sut.Register( definition );

        var result = _sut.Build();

        var typeDefinitions = result.GetTypeDefinitions();
        var dataTypeDefinitions = result.GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            typeDefinitions.Count.Should().Be( 23 );
            typeDefinitions.Select( t => (t.DataType, t.RuntimeType) )
                .Should()
                .BeEquivalentTo(
                    (MySqlDataType.Bool, typeof( bool )),
                    (MySqlDataType.TinyInt, typeof( sbyte )),
                    (MySqlDataType.UnsignedTinyInt, typeof( byte )),
                    (MySqlDataType.SmallInt, typeof( short )),
                    (MySqlDataType.UnsignedSmallInt, typeof( ushort )),
                    (MySqlDataType.Int, typeof( int )),
                    (MySqlDataType.UnsignedInt, typeof( uint )),
                    (MySqlDataType.BigInt, typeof( long )),
                    (MySqlDataType.UnsignedBigInt, typeof( ulong )),
                    (MySqlDataType.Float, typeof( float )),
                    (MySqlDataType.Double, typeof( double )),
                    (MySqlDataType.Decimal, typeof( decimal )),
                    (MySqlDataType.Text, typeof( string )),
                    (MySqlDataType.Blob, typeof( byte[] )),
                    (MySqlDataType.Date, typeof( DateOnly )),
                    (MySqlDataType.Time, typeof( TimeOnly )),
                    (MySqlDataType.DateTime, typeof( DateTime )),
                    (MySqlDataType.BigInt, typeof( TimeSpan )),
                    (MySqlDataType.CreateChar( 33 ), typeof( DateTimeOffset )),
                    (MySqlDataType.CreateChar( 1 ), typeof( char )),
                    (MySqlDataType.CreateBinary( 16 ), typeof( Guid )),
                    (MySqlDataType.Text, typeof( StringBuilder )),
                    (MySqlDataType.Blob, typeof( object )) );

            dataTypeDefinitions.Count.Should().Be( 21 );
            dataTypeDefinitions.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    MySqlDataType.Bool,
                    MySqlDataType.TinyInt,
                    MySqlDataType.UnsignedTinyInt,
                    MySqlDataType.SmallInt,
                    MySqlDataType.UnsignedSmallInt,
                    MySqlDataType.Int,
                    MySqlDataType.UnsignedInt,
                    MySqlDataType.BigInt,
                    MySqlDataType.UnsignedBigInt,
                    MySqlDataType.Float,
                    MySqlDataType.Double,
                    MySqlDataType.Decimal,
                    MySqlDataType.Text,
                    MySqlDataType.Char,
                    MySqlDataType.VarChar,
                    MySqlDataType.Blob,
                    MySqlDataType.Binary,
                    MySqlDataType.VarBinary,
                    MySqlDataType.Date,
                    MySqlDataType.Time,
                    MySqlDataType.DateTime );
        }
    }

    [Fact]
    public void Register_ShouldThrowArgumentException_WhenDefinitionDialectIsInvalid()
    {
        var definition = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var action = Lambda.Of( () => ((ISqlColumnTypeDefinitionProviderBuilder)_sut).Register( definition ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( StringBuilder ), false )]
    public void Contains_ShouldReturnTrue_WhenDefinitionExists(Type type, bool expected)
    {
        var result = _sut.Contains( type );
        result.Should().Be( expected );
    }

    public sealed class TypeDefinition<T> : MySqlColumnTypeDefinition<T>
        where T : notnull
    {
        public TypeDefinition(MySqlDataType dataType, T defaultValue)
            : base( dataType, defaultValue, (r, i) => (T)r.GetValue( i ) ) { }

        [Pure]
        public override string ToDbLiteral(T value)
        {
            return value.ToString() ?? string.Empty;
        }

        [Pure]
        public override object ToParameterValue(T value)
        {
            return value;
        }
    }
}
