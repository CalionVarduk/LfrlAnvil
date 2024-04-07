using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionProviderBuilderTests : TestsBase
{
    private readonly SqlColumnTypeDefinitionProviderBuilder _sut = new SqlColumnTypeDefinitionProviderBuilderMock();

    [Fact]
    public void Register_ShouldAddNewDefinition()
    {
        var definition = new SqlColumnTypeDefinitionMock<uint>( SqlDataTypeMock.Integer, 0U );
        var result = (( ISqlColumnTypeDefinitionProviderBuilder )_sut).Register( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.Contains( definition.RuntimeType ).Should().BeTrue();
        }
    }

    [Fact]
    public void Register_ShouldOverrideExistingDefinition()
    {
        var definition = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var result = (( ISqlColumnTypeDefinitionProviderBuilder )_sut).Register( definition );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( _sut );
            _sut.Contains( definition.RuntimeType ).Should().BeTrue();
        }
    }

    [Fact]
    public void Build_ShouldInvokeImplementation()
    {
        var definition = new SqlColumnTypeDefinitionMock<uint>( SqlDataTypeMock.Integer, 0 );
        _sut.Register( definition );

        var result = (( ISqlColumnTypeDefinitionProviderBuilder )_sut).Build();

        var typeDefinitions = result.GetTypeDefinitions();
        var dataTypeDefinitions = result.GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            typeDefinitions.Count.Should().Be( 10 );
            typeDefinitions.Select( t => (t.DataType, t.RuntimeType) )
                .Should()
                .BeEquivalentTo(
                    (SqlDataTypeMock.Integer, typeof( int )),
                    (SqlDataTypeMock.Boolean, typeof( bool )),
                    (SqlDataTypeMock.Real, typeof( double )),
                    (SqlDataTypeMock.Text, typeof( string )),
                    (SqlDataTypeMock.Binary, typeof( byte[] )),
                    (SqlDataTypeMock.Object, typeof( object )),
                    (SqlDataTypeMock.Integer, typeof( long )),
                    (SqlDataTypeMock.Real, typeof( float )),
                    (SqlDataTypeMock.Integer, typeof( uint )),
                    (SqlDataTypeMock.Text, typeof( DateTime )) );

            dataTypeDefinitions.Count.Should().Be( 6 );
            dataTypeDefinitions.Select( t => t.DataType )
                .Should()
                .BeEquivalentTo(
                    SqlDataTypeMock.Integer,
                    SqlDataTypeMock.Boolean,
                    SqlDataTypeMock.Real,
                    SqlDataTypeMock.Text,
                    SqlDataTypeMock.Binary,
                    SqlDataTypeMock.Object );
        }
    }

    [Fact]
    public void Register_ShouldThrowArgumentException_WhenDefinitionDialectIsInvalid()
    {
        var definition = new InvalidTypeDefinitionMock<int>( 0 );
        var action = Lambda.Of( () => (( ISqlColumnTypeDefinitionProviderBuilder )_sut).Register( definition ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( byte ), false )]
    public void Contains_ShouldReturnTrue_WhenDefinitionExists(Type type, bool expected)
    {
        var result = _sut.Contains( type );
        result.Should().Be( expected );
    }

    public sealed class InvalidTypeDefinitionMock<T> : SqlColumnTypeDefinition<T, DbDataRecord, DbParameter>
        where T : notnull
    {
        public InvalidTypeDefinitionMock(T defaultValue)
            : base( CreateDataType(), defaultValue, (r, i) => ( T )r.GetValue( i ) ) { }

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

        [Pure]
        private static ISqlDataType CreateDataType()
        {
            var result = Substitute.For<ISqlDataType>();
            result.Dialect.Returns( new SqlDialect( "foo" ) );
            return result;
        }
    }
}
