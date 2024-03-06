using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionProviderBuilderTests : TestsBase
{
    private readonly SqliteColumnTypeDefinitionProviderBuilder _sut = new SqliteColumnTypeDefinitionProviderBuilder();

    [Fact]
    public void Register_ShouldAddNewDefinition()
    {
        var definition = new TypeDefinition<StringBuilder>( SqliteDataType.Text, new StringBuilder() );
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
        var definition = new TypeDefinition<int>( SqliteDataType.Integer, 0 );
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
        var definition = new TypeDefinition<StringBuilder>( SqliteDataType.Text, new StringBuilder() );
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
                    (SqliteDataType.Integer, typeof( long )),
                    (SqliteDataType.Real, typeof( double )),
                    (SqliteDataType.Text, typeof( string )),
                    (SqliteDataType.Integer, typeof( bool )),
                    (SqliteDataType.Blob, typeof( byte[] )),
                    (SqliteDataType.Integer, typeof( byte )),
                    (SqliteDataType.Integer, typeof( sbyte )),
                    (SqliteDataType.Integer, typeof( ushort )),
                    (SqliteDataType.Integer, typeof( short )),
                    (SqliteDataType.Integer, typeof( uint )),
                    (SqliteDataType.Integer, typeof( int )),
                    (SqliteDataType.Integer, typeof( ulong )),
                    (SqliteDataType.Integer, typeof( TimeSpan )),
                    (SqliteDataType.Real, typeof( float )),
                    (SqliteDataType.Text, typeof( DateTime )),
                    (SqliteDataType.Text, typeof( DateTimeOffset )),
                    (SqliteDataType.Text, typeof( DateOnly )),
                    (SqliteDataType.Text, typeof( TimeOnly )),
                    (SqliteDataType.Text, typeof( decimal )),
                    (SqliteDataType.Text, typeof( char )),
                    (SqliteDataType.Blob, typeof( Guid )),
                    (SqliteDataType.Text, typeof( StringBuilder )),
                    (SqliteDataType.Any, typeof( object )) );

            dataTypeDefinitions.Count.Should().Be( 5 );
            dataTypeDefinitions.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    SqliteDataType.Any,
                    SqliteDataType.Integer,
                    SqliteDataType.Real,
                    SqliteDataType.Text,
                    SqliteDataType.Blob );
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

    public sealed class TypeDefinition<T> : SqliteColumnTypeDefinition<T>
        where T : notnull
    {
        public TypeDefinition(SqliteDataType dataType, T defaultValue)
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
