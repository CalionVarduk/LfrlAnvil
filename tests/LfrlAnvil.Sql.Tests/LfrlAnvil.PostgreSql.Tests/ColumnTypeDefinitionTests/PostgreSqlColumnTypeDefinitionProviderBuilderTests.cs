using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionProviderBuilderTests : TestsBase
{
    private readonly PostgreSqlColumnTypeDefinitionProviderBuilder _sut = new PostgreSqlColumnTypeDefinitionProviderBuilder();

    [Fact]
    public void Register_ShouldAddNewDefinition()
    {
        var definition = new TypeDefinition<StringBuilder>( PostgreSqlDataType.VarChar, new StringBuilder() );
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
        var definition = new TypeDefinition<int>( PostgreSqlDataType.Int4, 0 );
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
        var definition = new TypeDefinition<StringBuilder>( PostgreSqlDataType.VarChar, new StringBuilder() );
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
                    (PostgreSqlDataType.VarChar, typeof( StringBuilder )),
                    (PostgreSqlDataType.Bytea, typeof( object )) );

            dataTypeDefinitions.Count.Should().Be( 14 );
            dataTypeDefinitions.Select( t => t.DataType )
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

    public sealed class TypeDefinition<T> : PostgreSqlColumnTypeDefinition<T>
        where T : notnull
    {
        public TypeDefinition(PostgreSqlDataType dataType, T defaultValue)
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
