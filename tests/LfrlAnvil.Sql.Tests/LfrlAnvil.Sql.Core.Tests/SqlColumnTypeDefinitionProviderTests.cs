using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly SqlColumnTypeDefinitionProvider _sut = new SqlColumnTypeDefinitionProviderMock();

    [Fact]
    public void GetTypeDefinitions_ShouldReturnAllTypeDefinitionsRegisteredByRuntimeType()
    {
        var result = ((ISqlColumnTypeDefinitionProvider)_sut).GetTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 8 );
            result.Select( t => (t.DataType, t.RuntimeType) )
                .Should()
                .BeEquivalentTo(
                    (SqlDataTypeMock.Integer, typeof( int )),
                    (SqlDataTypeMock.Boolean, typeof( bool )),
                    (SqlDataTypeMock.Real, typeof( double )),
                    (SqlDataTypeMock.Text, typeof( string )),
                    (SqlDataTypeMock.Binary, typeof( byte[] )),
                    (SqlDataTypeMock.Object, typeof( object )),
                    (SqlDataTypeMock.Integer, typeof( long )),
                    (SqlDataTypeMock.Real, typeof( float )) );
        }
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = ((ISqlColumnTypeDefinitionProvider)_sut).GetDataTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 6 );
            result.Select( t => t.DataType )
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
    public void TryGetByType_ShouldReturnCorrectTypeDefinition_WhenTypeDefinitionForRuntimeTypeExists()
    {
        var result = ((ISqlColumnTypeDefinitionProvider)_sut).TryGetByType<long>();

        using ( new AssertionScope() )
        {
            result.Should().NotBeNull();
            (result?.RuntimeType).Should().Be<long>();
            (result?.DataType).Should().BeSameAs( SqlDataTypeMock.Integer );
        }
    }

    [Fact]
    public void TryGetByType_ShouldReturnNull_WhenTypeDefinitionForRuntimeTypeDoesNotExist()
    {
        var result = _sut.TryGetByType<byte>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetByType_ShouldReturnCorrectTypeDefinition_WhenTypeDefinitionForRuntimeTypeExists()
    {
        var result = ((ISqlColumnTypeDefinitionProvider)_sut).GetByType<long>();

        using ( new AssertionScope() )
        {
            result.RuntimeType.Should().Be<long>();
            result.DataType.Should().BeSameAs( SqlDataTypeMock.Integer );
        }
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenTypeDefinitionForRuntimeTypeDoesNotExist()
    {
        var action = Lambda.Of( () => _sut.GetByType<byte>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenRuntimeTypeDefinitionExists()
    {
        var result = _sut.Contains( _sut.GetByType<long>() );
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenDataTypeDefinitionExists()
    {
        var result = _sut.Contains( ((ISqlColumnTypeDefinitionProvider)_sut).GetByDataType( SqlDataTypeMock.Integer ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenDifferentRuntimeTypeDefinitionExists()
    {
        var result = _sut.Contains( new SqlColumnTypeDefinitionMock<long>( SqlDataTypeMock.Integer, 0L ) );
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenDifferentDataTypeDefinitionExists()
    {
        var result = _sut.Contains( new SqlColumnTypeDefinitionMock<byte>( SqlDataTypeMock.Integer, 0 ) );
        result.Should().BeFalse();
    }
}
