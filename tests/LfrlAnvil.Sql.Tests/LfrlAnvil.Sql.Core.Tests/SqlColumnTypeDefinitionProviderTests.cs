using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlColumnTypeDefinitionProviderTests : TestsBase
{
    private readonly SqlColumnTypeDefinitionProvider _sut =
        new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() );

    [Fact]
    public void GetTypeDefinitions_ShouldReturnAllTypeDefinitionsRegisteredByRuntimeType()
    {
        var result = ((ISqlColumnTypeDefinitionProvider)_sut).GetTypeDefinitions();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 9 );
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
                    (SqlDataTypeMock.Real, typeof( float )),
                    (SqlDataTypeMock.Text, typeof( DateTime )) );
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
        var result = _sut.TryGetByType<uint>();
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
        var action = Lambda.Of( () => _sut.GetByType<uint>() );
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

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        using ( new AssertionScope() )
        {
            result.DataType.Should().BeSameAs( SqlDataTypeMock.Integer );
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
            result.DataType.Should().BeSameAs( SqlDataTypeMock.Integer );
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
            result.DataType.Should().BeSameAs( SqlDataTypeMock.Integer );
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
    public void GetByType_ShouldThrowKeyNotFoundException_WhenEnumTypeDefinitionCouldNotBeCreatedDueToMissingUnderlyingTypeDefinition()
    {
        var action = Lambda.Of( () => _sut.GetByType<EnumWithNonExistingUnderlyingType>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenEnumTypeDefinitionDoesNotExistAndProviderIsLocked()
    {
        _sut.Lock();
        var action = Lambda.Of( () => _sut.GetByType<EnumWithDefault>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenCustomUnknownDefinitionIsNotCreated()
    {
        var action = Lambda.Of( () => _sut.GetByType<Guid>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Fact]
    public void GetByType_ShouldReturnPreviouslyCreatedCustomUnknownDefinition_WhenCalledMoreThanOnce()
    {
        var expected = _sut.GetByType<byte>();
        var result = _sut.GetByType<byte>();
        expected.Should().BeSameAs( result );
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

    public enum EnumWithNonExistingUnderlyingType : uint { }
}
