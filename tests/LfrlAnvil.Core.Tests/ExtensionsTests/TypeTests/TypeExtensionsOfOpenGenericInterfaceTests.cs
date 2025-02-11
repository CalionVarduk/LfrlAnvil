using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class OpenGenericInterfaceTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.GetImplementation<IDirect>();
        result.TestEquals( typeof( IDirect ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.TestEquals( typeof( IIndirectFromInterface ) ).Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.GetImplementation<INotImplemented>();
        result.TestNull().Go();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
        result.TestNull().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.Implements<IDirect>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.Implements<IIndirectFromInterface>();
        result.TestTrue().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.Implements<INotImplemented>();
        result.TestFalse().Go();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.Implements( typeof( IBaseGeneric<> ) );
        result.TestFalse().Go();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
    {
        var sut = typeof( IGenericInterface<> );
        var expected = new[] { typeof( IBaseGeneric<> ).MakeGenericType( sut.GetGenericArguments()[0] ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.TestSetEqual( expected ).Go();
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( IGenericInterface<> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.TestTrue().Go();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( IGenericInterface<> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.TestSetEqual( expected ).Go();
    }
}
