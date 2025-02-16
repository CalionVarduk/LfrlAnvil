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
        var result = (( ISqlColumnTypeDefinitionProvider )_sut).GetTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 9 ),
                result.Select( t => (t.DataType, t.RuntimeType) )
                    .TestSetEqual(
                    [
                        (SqlDataTypeMock.Integer, typeof( int )),
                        (SqlDataTypeMock.Boolean, typeof( bool )),
                        (SqlDataTypeMock.Real, typeof( double )),
                        (SqlDataTypeMock.Text, typeof( string )),
                        (SqlDataTypeMock.Binary, typeof( byte[] )),
                        (SqlDataTypeMock.Object, typeof( object )),
                        (SqlDataTypeMock.Integer, typeof( long )),
                        (SqlDataTypeMock.Real, typeof( float )),
                        (SqlDataTypeMock.Text, typeof( DateTime ))
                    ] ) )
            .Go();
    }

    [Fact]
    public void GetDataTypeDefinitions_ShouldReturnGetDataTypeDefinitionsImplementationResult()
    {
        var result = (( ISqlColumnTypeDefinitionProvider )_sut).GetDataTypeDefinitions();

        Assertion.All(
                result.Count.TestEquals( 6 ),
                result.Select( t => t.DataType )
                    .TestSetEqual(
                    [
                        SqlDataTypeMock.Integer,
                        SqlDataTypeMock.Boolean,
                        SqlDataTypeMock.Real,
                        SqlDataTypeMock.Text,
                        SqlDataTypeMock.Binary,
                        SqlDataTypeMock.Object
                    ] ) )
            .Go();
    }

    [Fact]
    public void TryGetByType_ShouldReturnCorrectTypeDefinition_WhenTypeDefinitionForRuntimeTypeExists()
    {
        var result = (( ISqlColumnTypeDefinitionProvider )_sut).TryGetByType<long>();

        Assertion.All(
                result.TestNotNull(),
                (result?.RuntimeType).TestEquals( typeof( long ) ),
                (result?.DataType).TestRefEquals( SqlDataTypeMock.Integer ) )
            .Go();
    }

    [Fact]
    public void TryGetByType_ShouldReturnNull_WhenTypeDefinitionForRuntimeTypeDoesNotExist()
    {
        var result = _sut.TryGetByType<uint>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetByType_ShouldReturnCorrectTypeDefinition_WhenTypeDefinitionForRuntimeTypeExists()
    {
        var result = (( ISqlColumnTypeDefinitionProvider )_sut).GetByType<long>();

        Assertion.All(
                result.RuntimeType.TestEquals( typeof( long ) ),
                result.DataType.TestRefEquals( SqlDataTypeMock.Integer ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenTypeDefinitionForRuntimeTypeDoesNotExist()
    {
        var action = Lambda.Of( () => _sut.GetByType<uint>() );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenRuntimeTypeDefinitionExists()
    {
        var result = _sut.Contains( _sut.GetByType<long>() );
        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenDataTypeDefinitionExists()
    {
        var result = _sut.Contains( (( ISqlColumnTypeDefinitionProvider )_sut).GetByDataType( SqlDataTypeMock.Integer ) );
        result.TestTrue().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenDifferentRuntimeTypeDefinitionExists()
    {
        var result = _sut.Contains( new SqlColumnTypeDefinitionMock<long>( SqlDataTypeMock.Integer, 0L ) );
        result.TestFalse().Go();
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenDifferentDataTypeDefinitionExists()
    {
        var result = _sut.Contains( new SqlColumnTypeDefinitionMock<byte>( SqlDataTypeMock.Integer, 0 ) );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( SqlDataTypeMock.Integer ),
                result.DefaultValue.Value.TestEquals( EnumWithDefault.B ),
                result.RuntimeType.TestEquals( typeof( EnumWithDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutDefaultZeroValue()
    {
        var result = _sut.GetByType<EnumWithoutDefault>();

        Assertion.All(
                result.DataType.TestRefEquals( SqlDataTypeMock.Integer ),
                result.DefaultValue.Value.TestEquals( EnumWithoutDefault.A ),
                result.RuntimeType.TestEquals( typeof( EnumWithoutDefault ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldAutomaticallyCreateMissingEnumDefinitionWithoutAnyValues()
    {
        var result = _sut.GetByType<EmptyEnum>();

        Assertion.All(
                result.DataType.TestRefEquals( SqlDataTypeMock.Integer ),
                result.DefaultValue.Value.TestEquals( default( EmptyEnum ) ),
                result.RuntimeType.TestEquals( typeof( EmptyEnum ) ) )
            .Go();
    }

    [Fact]
    public void GetByType_ShouldReturnPreviouslyCreatedEnumDefinition_WhenCalledMoreThanOnce()
    {
        var expected = _sut.GetByType<EnumWithDefault>();
        var result = _sut.GetByType<EnumWithDefault>();
        expected.TestRefEquals( result ).Go();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenEnumTypeDefinitionCouldNotBeCreatedDueToMissingUnderlyingTypeDefinition()
    {
        var action = Lambda.Of( () => _sut.GetByType<EnumWithNonExistingUnderlyingType>() );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenEnumTypeDefinitionDoesNotExistAndProviderIsLocked()
    {
        _sut.Lock();
        var action = Lambda.Of( () => _sut.GetByType<EnumWithDefault>() );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetByType_ShouldThrowKeyNotFoundException_WhenCustomUnknownDefinitionIsNotCreated()
    {
        var action = Lambda.Of( () => _sut.GetByType<Guid>() );
        action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
    }

    [Fact]
    public void GetByType_ShouldReturnPreviouslyCreatedCustomUnknownDefinition_WhenCalledMoreThanOnce()
    {
        var expected = _sut.GetByType<byte>();
        var result = _sut.GetByType<byte>();
        expected.TestRefEquals( result ).Go();
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
