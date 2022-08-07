using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class ClosedGenericInterfaceTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation<IDirect>();
        result.Should().Be( typeof( IDirect ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.Should().Be( typeof( IIndirectFromInterface ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation<IBaseGeneric<int>>();
        result.Should().Be( typeof( IBaseGeneric<int> ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation<INotImplemented>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsSelf()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation<IGenericInterface<int>>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
        result.Should().BeNull();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements<IDirect>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements<IIndirectFromInterface>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements<IBaseGeneric<int>>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements<INotImplemented>();
        result.Should().BeFalse();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsSelf()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements<IGenericInterface<int>>();
        result.Should().BeFalse();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.Implements( typeof( IBaseGeneric<> ) );
        result.Should().BeFalse();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
    {
        var sut = typeof( IGenericInterface<int> );
        var expected = new[] { typeof( IBaseGeneric<int> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( IGenericInterface<int> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( IGenericInterface<int> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( IMultiGenericClosedInterface );
        var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<string> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
    {
        var sut = typeof( IMultiGenericClosedInterface );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( IMultiGenericClosedInterface );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.Should().BeEquivalentTo( expected );
    }
}
