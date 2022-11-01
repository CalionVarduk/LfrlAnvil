using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Tests.ExtensionsTests.TypeTests;

public class TypeExtensionsOfClosedGenericClassTests : TypeTestsBase
{
    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IDirect>();
        result.Should().Be( typeof( IDirect ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IIndirectFromInterface>();
        result.Should().Be( typeof( IIndirectFromInterface ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IIndirectFromType>();
        result.Should().Be( typeof( IIndirectFromType ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnCorrectResult_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<IBaseGeneric<int>>();
        result.Should().Be( typeof( IBaseGeneric<int> ) );
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation<INotImplemented>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetImplementation_ShouldReturnNull_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetImplementation( typeof( IBaseGeneric<> ) );
        result.Should().BeNull();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsDirectlyImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IDirect>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughOtherInterface()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IIndirectFromInterface>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenInterfaceIsImplementedThroughBaseType()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IIndirectFromType>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnTrue_WhenImplementedInterfaceIsGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<IBaseGeneric<int>>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsNotImplemented()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements<INotImplemented>();
        result.Should().BeFalse();
    }

    [Fact]
    public void Implements_ShouldReturnFalse_WhenInterfaceIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Implements( typeof( IBaseGeneric<> ) );
        result.Should().BeFalse();
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( IBaseGeneric<int> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<string> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetOpenGenericImplementations_ShouldReturnCorrectResult_ForMultiWithTheSameGenericArgs()
    {
        var sut = typeof( MultiGenericClass<int, int> );
        var expected = new[] { typeof( IBaseGeneric<int> ), typeof( IBaseGeneric<int> ) }.AsEnumerable();
        var result = sut.GetOpenGenericImplementations( typeof( IBaseGeneric<> ) );
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void ImplementsOpenGeneric_ShouldReturnTrue_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var result = sut.ImplementsOpenGeneric( typeof( IBaseGeneric<> ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllImplementedGenericDefinitions_ShouldReturnCorrectResult_ForMulti()
    {
        var sut = typeof( MultiGenericClass<int, string> );
        var expected = new[] { typeof( IBaseGeneric<> ) }.AsEnumerable();
        var result = sut.GetAllImplementedGenericDefinitions();
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsDirectParent()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<BaseGenericClass<int>>();
        result.Should().Be( typeof( BaseGenericClass<int> ) );
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectResult_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<BaseClass>();
        result.Should().Be( typeof( BaseClass ) );
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<NotExtended>();
        result.Should().BeNull();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension( typeof( BaseGenericClass<> ) );
        result.Should().BeNull();
    }

    [Fact]
    public void GetExtension_ShouldReturnNull_WhenTypeIsSelf()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetExtension<GenericClass<int>>();
        result.Should().BeNull();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsDirectParent()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<BaseGenericClass<int>>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Extends_ShouldReturnTrue_WhenTypeIsIndirectAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<BaseClass>();
        result.Should().BeTrue();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsNotAnAncestor()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<NotExtended>();
        result.Should().BeFalse();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsOpenGeneric()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends( typeof( BaseGenericClass<> ) );
        result.Should().BeFalse();
    }

    [Fact]
    public void Extends_ShouldReturnFalse_WhenTypeIsSelf()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.Extends<GenericClass<int>>();
        result.Should().BeFalse();
    }

    [Fact]
    public void GetOpenGenericExtension_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.GetOpenGenericExtension( typeof( BaseGenericClass<> ) );
        result.Should().Be( typeof( BaseGenericClass<int> ) );
    }

    [Fact]
    public void ExtendsOpenGeneric_ShouldReturnTrue()
    {
        var sut = typeof( GenericClass<int> );
        var result = sut.ExtendsOpenGeneric( typeof( BaseGenericClass<> ) );
        result.Should().BeTrue();
    }

    [Fact]
    public void GetAllExtendedGenericDefinitions_ShouldReturnCorrectResult()
    {
        var sut = typeof( GenericClass<int> );
        var expected = new[] { typeof( BaseGenericClass<> ) }.AsEnumerable();
        var result = sut.GetAllExtendedGenericDefinitions();
        result.Should().BeEquivalentTo( expected );
    }
}
